using System.Collections;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum State
    {
        Airborne,
        Grounded
    }

    [SerializeField] private InputEventChannel inputs;
    [SerializeField] private SfxClips playerSfx;
    private CharacterController characterController;
    private State state;

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
    [SerializeField] private GameObject Sword;
    private bool canAttack;
    private float attackCooldown = .417f; // AKA attack speed, lol hardcoded
    [HideInInspector] public int attackDamage = 1;

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
        inputs.jumpInputEvent += ReactToJumpInput;
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
        Sword.GetComponent<Animator>().Play("Attack");
        Sword.GetComponent<AudioSource>().PlayOneShot(playerSfx.clips[0]);
        yield return new WaitForSeconds(attackCooldown);
        Sword.GetComponent<Animator>().Play("Idle");
        canAttack = true;
    }

    private void ReactToJumpInput()
    {
        if (state == State.Grounded)
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
        state = State.Airborne;
        canAttack = true;
    }

    private void OnDisable()
    {
        inputs.attackInputEvent -= ReactToAttackInput;
        inputs.jumpInputEvent -= ReactToJumpInput;
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

        localVelocity.x = 0f;
        localVelocity.z = 0f;
        if (forwardMoveInputPressed)
            localVelocity.z = moveSpeed;
        else if (backwardMoveInputPressed)
            localVelocity.z = -moveSpeed;
        if (rightMoveInputPressed)
            localVelocity.x = moveSpeed;
        else if (leftMoveInputPressed)
            localVelocity.x = -moveSpeed;
    }

    private void UpdateVerticalVelocity()
    {
        localVelocity.y -= gravity * Time.deltaTime;
    }

    private void UpdateState()
    {
        switch (state)
        {
            case State.Airborne:
                if (characterController.isGrounded)
                    state = State.Grounded;
                break;
            case State.Grounded:
                if (!characterController.isGrounded)
                    state = State.Airborne;
                break;
        }
    }

}
