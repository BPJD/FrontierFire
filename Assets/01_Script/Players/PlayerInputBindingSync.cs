using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputBindingSync : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        var loader = MainUI_KeyMapLoader.GetOrFind();
        if (loader == null || playerInput == null || playerInput.actions == null)
            return;

        loader.ApplyToPlayerInput(playerInput, true);
    }
}