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
    private Vector3 velocity;
    private float gravity = 9.81f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        state = State.Airborne;
        velocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        Debug.Log("Current State: " + state.ToString());
        velocity.y -= gravity * Time.fixedDeltaTime;
        characterController.Move(velocity * Time.fixedDeltaTime);
        UpdateState();
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
