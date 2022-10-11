using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// Within a script, reference an instance of this Scriptable Object as an asset in order to give
/// that script access to the user's inputs.
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/Input Event Channel")]
public class InputEventChannel : ScriptableObject, GameControls.IGameplayActions
{
    public event UnityAction attackInputEvent = delegate { };
    public event UnityAction interactInputEvent = delegate { };
    public event UnityAction jumpInputEvent = delegate { };
    public event UnityAction<Vector2> lookInputEvent = delegate { };
    public event UnityAction<Vector2> moveInputEvent = delegate { };
    public event UnityAction quaffInputEvent = delegate { };



    private GameControls _gameControls;

    private void OnEnable()
    {
        if (_gameControls == null)
        {
            _gameControls = new GameControls();
            //_gameControls.Dialogue.SetCallbacks(this);
            //_gameControls.Menu.SetCallbacks(this);
            _gameControls.Gameplay.SetCallbacks(this);
        }
        EnableGameplayInputs();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            attackInputEvent?.Invoke();
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            interactInputEvent?.Invoke();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            jumpInputEvent?.Invoke();
    }

    public void OnLook(InputAction.CallbackContext context) 
        => lookInputEvent?.Invoke(context.ReadValue<Vector2>());

    public void OnMove(InputAction.CallbackContext context)
        => moveInputEvent?.Invoke(context.ReadValue<Vector2>());

    public void OnQuaff(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            quaffInputEvent?.Invoke();
    }


    public void EnableGameplayInputs()
    {
        //_gameControls.Dialogue.Disable();
        _gameControls.Gameplay.Enable();
        //_gameControls.Menu.Disable();
    }
}
