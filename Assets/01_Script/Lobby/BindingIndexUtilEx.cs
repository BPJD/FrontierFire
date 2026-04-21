using UnityEngine.InputSystem;

public static class BindingIndexUtilEx
{
    public static int FindBindingIndexByExactPath(InputAction action, string group, string exactPath)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];

            if (b.isComposite || b.isPartOfComposite)
                continue;

            if (!string.IsNullOrEmpty(group) && (b.groups == null || !b.groups.Contains(group)))
                continue;

            if (b.path == exactPath)
                return i;
        }

        return -1;
    }

    public static int FindCompositePartIndex(InputAction action, string group, string partName)
    {
        if (action == null) return -1;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];

            if (!b.isPartOfComposite)
                continue;

            if (!string.Equals(b.name, partName, System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.IsNullOrEmpty(group) && !BindingMatchesGroup(b, group))
                continue;

            return i;
        }

        return -1;
    }

    private static bool BindingMatchesGroup(InputBinding binding, string targetGroup)
    {
        if (string.IsNullOrEmpty(binding.groups) || string.IsNullOrEmpty(targetGroup))
            return false;

        var groups = binding.groups.Split(';');
        for (int i = 0; i < groups.Length; i++)
        {
            if (groups[i].Trim() == targetGroup)
                return true;
        }

        return false;
    }
}