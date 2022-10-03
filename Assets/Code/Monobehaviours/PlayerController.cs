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
    private CharacterController characterController;
    private State state;

    // physics & movement
    private Vector2 moveDirection;
    private Vector3 velocity;
    private float gravity = 9.81f;
    private float jumpHeight = 2f;
    private float jumpSpeedUpwards;
    private float moveSpeed = 5f;

    // camera
    private Vector2 lookDirection;
    private float cameraPitch;


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
    }

    private void ReactToAttackInput()
    {

    }

    private void ReactToJumpInput()
    {
        if (state == State.Grounded)
            velocity.y = jumpSpeedUpwards;
    }

    private void ReactToLookInput(Vector2 lookInputDirection)
        => lookDirection = lookInputDirection;

    private void ReactToMoveInput(Vector2 moveInputDirection)
        => moveDirection = moveInputDirection.normalized;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        jumpSpeedUpwards = 2f * jumpHeight / Mathf.Sqrt(2f * jumpHeight / gravity);
        state = State.Airborne;
    }

    private void OnDisable()
    {
        inputs.attackInputEvent -= ReactToAttackInput;
        inputs.jumpInputEvent -= ReactToJumpInput;
        inputs.lookInputEvent -= ReactToLookInput;
        inputs.moveInputEvent -= ReactToMoveInput;
    }

    private void FixedUpdate()
    {
        UpdateHorizontalVelocity();
        UpdateVerticalVelocity();
        characterController.Move(velocity * Time.fixedDeltaTime);
        UpdateState();
    }

    private void UpdateCamera()
    {

    }
    
    private void UpdateHorizontalVelocity()
    {
        bool forwardMoveInputPressed = (moveDirection.y > 0);
        bool backwardMoveInputPressed = (moveDirection.y < 0);
        bool rightMoveInputPressed = (moveDirection.x > 0);
        bool leftMoveInputPressed = (moveDirection.x < 0);

        velocity.x = 0f;
        velocity.z = 0f;
        if (forwardMoveInputPressed)
            velocity.z = moveSpeed;
        else if (backwardMoveInputPressed)
            velocity.z = -moveSpeed;
        if (rightMoveInputPressed)
            velocity.x = moveSpeed;
        else if (leftMoveInputPressed)
            velocity.x = -moveSpeed;
    }

    private void UpdateVerticalVelocity()
    {
        velocity.y -= gravity * Time.fixedDeltaTime;
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
