using UnityEngine;
using UnityEngine.InputSystem;

public static class KeymapDebugUtil
{
    public static void DumpBinding(InputActionAsset actions, string actionName, int bindingIndex)
    {
        var act = actions.FindAction(actionName, throwIfNotFound: false);
        if (act == null)
        {
            Debug.LogError($"[KeyMapDump] Action not found: {actionName}");
            return;
        }

        if (bindingIndex < 0 || bindingIndex >= act.bindings.Count)
        {
            Debug.LogError($"[KeyMapDump] Invalid bindingIndex={bindingIndex} for {actionName}. Count={act.bindings.Count}");
            return;
        }

        var b = act.bindings[bindingIndex];

        Debug.Log(
            $"[KeyMapDump] {actionName}[{bindingIndex}] " +
            $"path={b.path} overridePath={b.overridePath} effectivePath={b.effectivePath}"
        );
    }
}
