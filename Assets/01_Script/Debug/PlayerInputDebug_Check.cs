using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputDebug_Check : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        PrintState("Awake");
    }

    private void Start()
    {
        PrintState("Start");
    }

    public void PrintState(string from)
    {
        if (playerInput == null)
        {
            Debug.Log($"[InputDebug] {from} / playerInput is NULL");
            return;
        }

        Debug.Log($"[InputDebug] {from}");
        Debug.Log($"[InputDebug] currentActionMap = {playerInput.currentActionMap?.name}");
        Debug.Log($"[InputDebug] currentControlScheme = {playerInput.currentControlScheme}");

        var move = playerInput.actions.FindAction("Move", false);
        var jump = playerInput.actions.FindAction("Jump", false);

        if (move != null)
        {
            Debug.Log($"[InputDebug] Move enabled = {move.enabled}");
            for (int i = 0; i < move.bindings.Count; i++)
            {
                Debug.Log($"[InputDebug] Move[{i}] name={move.bindings[i].name}, groups={move.bindings[i].groups}, effectivePath={move.bindings[i].effectivePath}");
            }
        }

        if (jump != null)
        {
            Debug.Log($"[InputDebug] Jump enabled = {jump.enabled}");
            for (int i = 0; i < jump.bindings.Count; i++)
            {
                Debug.Log($"[InputDebug] Jump[{i}] groups={jump.bindings[i].groups}, effectivePath={jump.bindings[i].effectivePath}");
            }
        }
    }
}