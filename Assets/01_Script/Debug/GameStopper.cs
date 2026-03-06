using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameStopper : MonoBehaviour
{

    bool isStopped = false;
    [SerializeField] private PlayerInput playerInput;
    // Update is called once per frame

    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            isStopped = !isStopped;
            Time.timeScale = isStopped ? 0f : 1f;
            Debug.Log(isStopped ? "Game Stopped" : "Game Resumed");
        }


        if (playerInput == null) return;
        if (Keyboard.current?.f1Key.wasPressedThisFrame == true)
            Debug.Log($"[Input] currentActionMap = {playerInput.currentActionMap?.name}");
    }
    */
    void Update()
    {
        if (Keyboard.current?.f3Key.wasPressedThisFrame != true) return;

        var pis = Object.FindObjectsByType<PlayerInput>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        var sb = new StringBuilder();
        sb.AppendLine($"[Dump] PlayerInput count = {pis.Length}");

        foreach (var pi in pis)
        {
            sb.AppendLine($"- {pi.name} (activeInHierarchy={pi.gameObject.activeInHierarchy}, enabled={pi.enabled})");
            sb.AppendLine($"  map={pi.currentActionMap?.name}, scheme={pi.currentControlScheme}, behavior={pi.notificationBehavior}");

            if (pi.devices.Count > 0)
            {
                sb.Append("  devices=");
                for (int i = 0; i < pi.devices.Count; i++)
                {
                    var d = pi.devices[i];
                    sb.Append($"{d.displayName}({d.layout})");
                    if (i < pi.devices.Count - 1)
                        sb.Append(", ");
                }
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("  devices=(none)");
            }
        }

        Debug.Log(sb.ToString());
    }

}
