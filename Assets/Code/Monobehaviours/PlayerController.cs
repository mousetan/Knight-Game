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

    private void Update()
    {
        velocity.y -= gravity * Time.fixedDeltaTime;
        CheckState();
    }

    private void CheckState()
    {
        switch (state)
        {
            case State.Airborne:
                Debug.Log("Player is Airborne!");
                break;
            case State.Grounded:
                Debug.Log("Player is Grounded!");
                break;
        }
    }

}
