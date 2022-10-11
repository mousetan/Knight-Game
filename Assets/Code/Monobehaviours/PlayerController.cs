using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Airborne,
        Grounded
    }

    [SerializeField] private InputEventChannel inputs;
    [SerializeField] public SfxClips playerSfx;
    private CharacterController characterController;
    private PlayerState state;

    // physics & movement
    private Vector2 moveDirection;
    private Vector3 localVelocity;
    private Vector3 worldVelocity;
    private float gravity = 9.81f;
    private float jumpHeight = 2f;
    private float moveSpeedForwards = 4f;
    private float moveSpeedSideways = 3.5f;
    private float moveSpeedBackwards = 3f;
    private float jumpSpeedUpwards;

    // footsteps
    private AudioSource audioSource;
    private float footstepTimer;
    private float forwardsFootstepTimerThreshold = 0.4f;
    private float backwardsFootstepTimerThreshold = 0.42f;
    private float sidewaysFootstepTimerThreshold = 0.44f;
    private int globalFootstepCounter;
    private int footstepCounter;
    private float firstFootstepPitch = 0.9f;
    private float lastStopTimer;
    private float lastStopTimerThreshold = 0.4f;

    // camera
    private Vector2 lookDirection;
    [SerializeField] private Transform eyeballs;
    private float cameraPitch;
    private float maxPitchUp = 90f;
    private float maxPitchDown = 70f; // don't let the player look at feet
    private float cameraSensitivity = 3f;

    // crosshair
    [SerializeField] private GameObject crosshair;

    // inventory
    private Stack<LootableItemData> potions = new Stack<LootableItemData>();
    private Stack<LootableItemData> grenades = new Stack<LootableItemData>();
    private Stack<LootableItemData> keys = new Stack<LootableItemData>();

    // attacks
    [SerializeField] private GameObject weapon;
    private bool canAttack;

    // health
    private float maxHealth = 100f;
    [HideInInspector] public float currentHealth;
    [SerializeField] private HealthBarController healthBar;

    public static PlayerController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

    private void OnEnable()
    {
        inputs.attackInputEvent += ReactToAttackInput;
        inputs.interactInputEvent += ReactToInteractInput;
        //inputs.jumpInputEvent += ReactToJumpInput;
        inputs.lookInputEvent += ReactToLookInput;
        inputs.moveInputEvent += ReactToMoveInput;
        inputs.quaffInputEvent += ReactToQuaffInput;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ReactToAttackInput()
    {
        if (canAttack)
            StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        weapon.GetComponent<Animator>().Play("Attack");
        weapon.GetComponent<AudioSource>().PlayOneShot(playerSfx.clips[0]);
        yield return new WaitForSeconds(weapon.GetComponent<WeaponBehaviour>().weaponData.attackCooldown);
        weapon.GetComponent<Animator>().Play("Idle");
        canAttack = true;
    }

    private void ReactToInteractInput()
    {
        bool somethingInRange = Physics.Raycast(eyeballs.position, eyeballs.forward, out var hitData, weapon.GetComponent<WeaponBehaviour>().weaponData.range);
        bool lootableInRange;
        if (somethingInRange)
            lootableInRange = (hitData.transform.gameObject.GetComponent<Item>() != null);
        else
            lootableInRange = false;
        if (lootableInRange)
        {
            if (hitData.transform.GetComponent<Item>().itemData.name == "Potion")
            {
                potions.Push(hitData.transform.GetComponent<Item>().itemData);
                audioSource.PlayOneShot(hitData.transform.GetComponent<Item>().itemData.pickupSound);
            }
            else if (hitData.transform.GetComponent<Item>().itemData.name == "Grenade")
            {
                grenades.Push(hitData.transform.GetComponent<Item>().itemData);
                audioSource.PlayOneShot(hitData.transform.GetComponent<Item>().itemData.pickupSound);
            }
            else if (hitData.transform.GetComponent<Item>().itemData.name == "Key")
            {
                keys.Push(hitData.transform.GetComponent<Item>().itemData);
                audioSource.PlayOneShot(hitData.transform.GetComponent<Item>().itemData.pickupSound);
            }
            Destroy(hitData.transform.gameObject);

        }
    }

    private void ReactToJumpInput()
    {
        if (state == PlayerState.Grounded)
            localVelocity.y = jumpSpeedUpwards;
    }

    private void ReactToLookInput(Vector2 lookInputDirection)
        => lookDirection = lookInputDirection;

    private void ReactToMoveInput(Vector2 moveInputDirection)
        => moveDirection = moveInputDirection.normalized;

    private void ReactToQuaffInput()
    {

    }

    private void Start()
    {
        eyeballs = this.transform.GetChild(0);
        characterController = GetComponent<CharacterController>();
        jumpSpeedUpwards = 2f * jumpHeight / Mathf.Sqrt(2f * jumpHeight / gravity);
        state = PlayerState.Airborne;
        canAttack = true;
        audioSource = GetComponent<AudioSource>();
        footstepCounter = 0;
        currentHealth = maxHealth;
        healthBar.UpdateMaxHP(maxHealth);
        healthBar.UpdateCurrentHP(currentHealth);
    }

    private void OnDisable()
    {
        inputs.attackInputEvent -= ReactToAttackInput;
        inputs.interactInputEvent -= ReactToInteractInput;
        //inputs.jumpInputEvent -= ReactToJumpInput;
        inputs.lookInputEvent -= ReactToLookInput;
        inputs.moveInputEvent -= ReactToMoveInput;
        inputs.interactInputEvent -= ReactToQuaffInput;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        cameraPitch -= lookDirection.y * cameraSensitivity * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxPitchUp, maxPitchDown);
        UpdateHorizontalVelocity();
        UpdateVerticalVelocity();
        worldVelocity = localVelocity.x * transform.right + localVelocity.y * transform.up + localVelocity.z * transform.forward;
        PlayFootsteps();
        UpdateCrosshair();
    }

    private void FixedUpdate()
    {
        RotatePlayerAndEyeballs();
        characterController.Move(worldVelocity * Time.deltaTime);
        UpdateState();
    }


    private void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.UpdateCurrentHP(currentHealth);
        if (currentHealth < 0f)
            Die();
    }
    private void Die()
    {

    }


    private void UpdateCrosshair()
    {
        if (Physics.Raycast(eyeballs.position, eyeballs.forward, out var hitboxInfo, weapon.GetComponent<WeaponBehaviour>().weaponData.range, 1 << 6))
        {
            crosshair.transform.GetChild(0).gameObject.SetActive(false);
            crosshair.transform.GetChild(1).gameObject.SetActive(false);
            crosshair.transform.GetChild(2).gameObject.SetActive(true);
        }
        else if (Physics.Raycast(eyeballs.position, eyeballs.forward, out var hitInfo, weapon.GetComponent<WeaponBehaviour>().weaponData.range))
        {
            if (hitInfo.transform.tag == "Item" || hitInfo.transform.tag == "Door")
            {
                crosshair.transform.GetChild(0).gameObject.SetActive(false);
                crosshair.transform.GetChild(1).gameObject.SetActive(true);
                crosshair.transform.GetChild(2).gameObject.SetActive(false);
            }
        }
        else
        {
            crosshair.transform.GetChild(0).gameObject.SetActive(true);
            crosshair.transform.GetChild(1).gameObject.SetActive(false);
            crosshair.transform.GetChild(2).gameObject.SetActive(false);
        }
    }


    // KNOWN BUG: PLAYER CAN SPAM LEFT/RIGHT OR UP/DOWN TO MAKE TONS OF NOISE, FIX IT BY TIMING THE LAST TIME SINCE THE PLAYER 
    // LAST STOPPED MOVING
    private void PlayFootsteps()
    {
        bool playerJustStartedMoving = (footstepTimer == 0f && footstepCounter == 0);
        //lastStopTimer += Time.deltaTime;

        if (moveDirection.y > 0)
        {
            if (playerJustStartedMoving) // USE A TIMER HERE TO FIX THE BUG
            {
                if (globalFootstepCounter % 2 == 0)
                    audioSource.pitch = firstFootstepPitch;
                else
                    audioSource.pitch = 1f;
                audioSource.PlayOneShot(playerSfx.clips[3]);
            }

            footstepTimer += Time.deltaTime;
            if (footstepTimer > forwardsFootstepTimerThreshold)
            {
                footstepTimer = 0f;
                globalFootstepCounter++;
                footstepCounter++;
                if (globalFootstepCounter % 2 == 0)
                    audioSource.pitch = firstFootstepPitch;
                else
                    audioSource.pitch = 1f;
                audioSource.PlayOneShot(playerSfx.clips[3]);
            }            
        }
        else if (moveDirection.x != 0)
        {
            if (playerJustStartedMoving)
            {
                if (globalFootstepCounter % 2 == 0)
                    audioSource.pitch = firstFootstepPitch;
                else
                    audioSource.pitch = 1f;
                audioSource.PlayOneShot(playerSfx.clips[3]);
            }

            footstepTimer += Time.deltaTime;
            if (footstepTimer > sidewaysFootstepTimerThreshold)
            {
                footstepTimer = 0f;
                globalFootstepCounter++;
                footstepCounter++;
                if (globalFootstepCounter % 2 == 0)
                    audioSource.pitch = firstFootstepPitch;
                else
                    audioSource.pitch = 1f;
                audioSource.PlayOneShot(playerSfx.clips[3]);
            }
        }
        else if (moveDirection.y < 0)
        {
            if (playerJustStartedMoving)
            {
                if (globalFootstepCounter % 2 == 0)
                    audioSource.pitch = firstFootstepPitch;
                else
                    audioSource.pitch = 1f;
                audioSource.PlayOneShot(playerSfx.clips[3]);
            }

            footstepTimer += Time.deltaTime;
            if (footstepTimer > backwardsFootstepTimerThreshold)
            {
                footstepTimer = 0f;
                globalFootstepCounter++;
                footstepCounter++;
                if (globalFootstepCounter % 2 == 0)
                    audioSource.pitch = firstFootstepPitch;
                else
                    audioSource.pitch = 1f;
                audioSource.PlayOneShot(playerSfx.clips[3]);
            }
        }
        else
        {
            footstepTimer = 0f;
            footstepCounter = 0;
            //lastStopTimer = 0f;
            // INCREMENT A TIMER HERE TO FIX THE BUG
        }
    }

    private void RotatePlayerAndEyeballs()
    {
        eyeballs.transform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
        this.transform.Rotate(Vector3.up * lookDirection.x * cameraSensitivity * Time.deltaTime);
    }

    private void UpdateHorizontalVelocity()
    {
        bool forwardMoveInputPressed = (moveDirection.y > 0);
        bool backwardMoveInputPressed = (moveDirection.y < 0);
        bool rightMoveInputPressed = (moveDirection.x > 0);
        bool leftMoveInputPressed = (moveDirection.x < 0);
        
        if (forwardMoveInputPressed)
            localVelocity.z = moveSpeedForwards;
        else if (backwardMoveInputPressed)
            localVelocity.z = -moveSpeedBackwards;
        else
            localVelocity.z = 0f;
        if (rightMoveInputPressed)
            localVelocity.x = moveSpeedSideways;
        else if (leftMoveInputPressed)
            localVelocity.x = -moveSpeedSideways;
        else
            localVelocity.x = 0f;

        if (localVelocity.z > 0)
            localVelocity.z = new Vector2(localVelocity.x, localVelocity.z).normalized.y * moveSpeedForwards;
        else
            localVelocity.z = new Vector2(localVelocity.x, localVelocity.z).normalized.y * moveSpeedBackwards;
        localVelocity.x = new Vector2(localVelocity.x, localVelocity.z).normalized.x* moveSpeedSideways;

    }

    private void UpdateVerticalVelocity()
    {
        localVelocity.y -= gravity * Time.deltaTime;
    }

    private void UpdateState()
    {
        switch (state)
        {
            case PlayerState.Airborne:
                if (characterController.isGrounded)
                    state = PlayerState.Grounded;
                break;
            case PlayerState.Grounded:
                if (!characterController.isGrounded)
                    state = PlayerState.Airborne;
                break;
        }
    }
}
