using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class MainInputProbe : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current?.f2Key.wasPressedThisFrame != true) return;

        var es = EventSystem.current;
        var ui = es ? es.GetComponent<InputSystemUIInputModule>() : null;
        var pi = FindFirstObjectByType<PlayerInput>();

        Debug.Log(
            $"[Probe] timeScale={Time.timeScale}\n" +
            $"EventSystem={(es ? "OK" : "NULL")} enabled={(es ? es.isActiveAndEnabled : false)}\n" +
            $"UIInputModule={(ui ? "OK" : "NULL")} actionsAsset={(ui && ui.actionsAsset ? ui.actionsAsset.name : "NULL")}\n" +
            $"PlayerInput={(pi ? "OK" : "NULL")} enabled={(pi ? pi.enabled : false)} map={(pi ? pi.currentActionMap?.name : "null")}\n" +
            $"Selected={(es && es.currentSelectedGameObject ? es.currentSelectedGameObject.name : "null")}"
        );
    }
}