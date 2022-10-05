using System.Collections;
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
    private float moveSpeed = 5f;
    private float jumpSpeedUpwards;

    // camera
    private Vector2 lookDirection;
    [SerializeField] private Transform eyeballs;
    private float cameraPitch;
    private float maxPitchUp = 90f;
    private float maxPitchDown = 60f; // don't let the player look at feet
    private float cameraSensitivity = 3f;

    // attacks
    [SerializeField] private GameObject weapon;
    private bool canAttack;

    // 

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
        //inputs.jumpInputEvent += ReactToJumpInput;
        inputs.lookInputEvent += ReactToLookInput;
        inputs.moveInputEvent += ReactToMoveInput;
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

    private void ReactToJumpInput()
    {
        if (state == PlayerState.Grounded)
            localVelocity.y = jumpSpeedUpwards;
    }

    private void ReactToLookInput(Vector2 lookInputDirection)
        => lookDirection = lookInputDirection;

    private void ReactToMoveInput(Vector2 moveInputDirection)
        => moveDirection = moveInputDirection.normalized;

    private void Start()
    {
        eyeballs = this.transform.GetChild(0);
        characterController = GetComponent<CharacterController>();
        jumpSpeedUpwards = 2f * jumpHeight / Mathf.Sqrt(2f * jumpHeight / gravity);
        state = PlayerState.Airborne;
        canAttack = true;
    }

    private void OnDisable()
    {
        inputs.attackInputEvent -= ReactToAttackInput;
        //inputs.jumpInputEvent -= ReactToJumpInput;
        inputs.lookInputEvent -= ReactToLookInput;
        inputs.moveInputEvent -= ReactToMoveInput;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        cameraPitch -= lookDirection.y * cameraSensitivity * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxPitchUp, maxPitchDown);
        UpdateHorizontalVelocity();
        UpdateVerticalVelocity();
        worldVelocity = localVelocity.x * transform.right + localVelocity.y * transform.up + localVelocity.z * transform.forward;
    }

    private void FixedUpdate()
    {
        RotatePlayerAndEyeballs();
        characterController.Move(worldVelocity * Time.deltaTime);
        UpdateState();
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
            localVelocity.z = moveSpeed;
        else if (backwardMoveInputPressed)
            localVelocity.z = -moveSpeed;
        else
            localVelocity.z = 0f;
        if (rightMoveInputPressed)
            localVelocity.x = moveSpeed;
        else if (leftMoveInputPressed)
            localVelocity.x = -moveSpeed;
        else
            localVelocity.x = 0f;
        localVelocity.z = new Vector2(localVelocity.x, localVelocity.z).normalized.y*moveSpeed;
        localVelocity.x = new Vector2(localVelocity.x, localVelocity.z).normalized.x*moveSpeed;
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
