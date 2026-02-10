using UnityEngine.InputSystem;

public static class BindingIndexUtil
{
    public static int FindFirstBindingIndexByGroup(InputAction action, string group)
    {
        // action.bindings 전체에서
        // - composite 자체(binding.isComposite)는 제외
        // - composite의 part(binding.isPartOfComposite)는 "원하면" 포함/제외 가능
        // 여기서는 일반적인 버튼 액션 기준으로 part도 포함 가능하지만,
        // 일단 안전하게 part도 제외(이동 같은 composite 리바인딩은 별도 처리 권장)
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];
            if (b.isComposite) continue;
            if (b.isPartOfComposite) continue;

            if (BindingHasGroup(b, group))
                return i;
        }
        return -1;
    }

    private static bool BindingHasGroup(InputBinding binding, string group)
    {
        if (string.IsNullOrEmpty(binding.groups)) return false;

        // groups는 "Keyboard&Mouse;Gamepad" 처럼 들어갈 수 있음
        // 정확한 토큰 매칭
        var tokens = binding.groups.Split(';');
        for (int t = 0; t < tokens.Length; t++)
        {
            if (tokens[t].Trim() == group)
                return true;
        }
        return false;
    }
}
