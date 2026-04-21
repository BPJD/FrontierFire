using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Game_DifficultySelector : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GameObject portal;
    [SerializeField] CanvasGroup difficultySelectPanel;

    [Header("Input")]
    [SerializeField] string actionMapName = "Player";
    [SerializeField] string moveActionName = "Move";

    [Header("Difficulty")]
    [SerializeField] int selectedDifficulty = 1;   // 0: Easy, 1: Normal, 2: Hard, 3: Hard+
    [SerializeField] int minDifficulty = 0;
    [SerializeField] int maxDifficulty = 3;

    PlayerInteract playerInteract;
    PlayerInput playerInput;
    InputAction moveAction;

    bool isPortalSelected = false;
    bool axisInUse = false;

    public int SelectedDifficulty => selectedDifficulty;


    [SerializeField] RectTransform[] difficultyPanelTrs;
    [SerializeField] CanvasGroup[] difficultyGroups;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
        if (player == null)
        {
            Debug.LogWarning("[Game_DifficultySelector] Player not found.");
            enabled = false;
            return;
        }

        playerInteract = player.GetComponentInChildren<PlayerInteract>();
        playerInput = player.GetComponent<PlayerInput>();

        if (playerInteract == null)
        {
            Debug.LogWarning("[Game_DifficultySelector] PlayerInteract not found.");
        }

        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogWarning("[Game_DifficultySelector] PlayerInput or InputActions not found.");
            enabled = false;
            return;
        }

        InputActionMap map = null;

        if (!string.IsNullOrEmpty(actionMapName))
            map = playerInput.actions.FindActionMap(actionMapName, false);

        if (map != null)
            moveAction = map.FindAction(moveActionName, false);
        else
            moveAction = playerInput.actions.FindAction(moveActionName, false);

        if (moveAction == null)
        {
            Debug.LogWarning($"[Game_DifficultySelector] Move action not found. map={actionMapName}, action={moveActionName}");
            enabled = false;
            return;
        }

        difficultySelectPanel.alpha = 0f;
        selectedDifficulty = ES3.Load<int>(Data_Strings.gameDifficultyKey, 1);
        UpdateDifficultyUI();
    }

    private void Update()
    {
        if (playerInteract == null || portal == null)
            return;

        isPortalSelected = playerInteract.SelectedObj == portal;

        if (difficultySelectPanel != null)
            difficultySelectPanel.alpha = isPortalSelected ? 1f : 0f;

        if (!isPortalSelected)
        {
            axisInUse = false;
            return;
        }

        HandleDifficultyInput();
    }

    void HandleDifficultyInput()
    {
        if (moveAction == null)
            return;

        Vector2 move = moveAction.ReadValue<Vector2>();

        if (!axisInUse)
        {
            if (move.y > 0.5f)
            {
                ChangeDifficulty(1);
                axisInUse = true;
            }
            else if (move.y < -0.5f)
            {
                ChangeDifficulty(-1);
                axisInUse = true;
            }
        }

        if (Mathf.Abs(move.y) < 0.3f)
        {
            axisInUse = false;
        }
    }

    void ChangeDifficulty(int dir)
    {
        int prev = selectedDifficulty;
        selectedDifficulty = Mathf.Clamp(selectedDifficulty + dir, minDifficulty, maxDifficulty);

        if (prev != selectedDifficulty)
        {
            //Debug.Log($"[Game_DifficultySelector] Difficulty Changed : {selectedDifficulty}");
            ES3.Save(Data_Strings.gameDifficultyKey, selectedDifficulty);
            UpdateDifficultyUI();
        }
    }


    public void UpdateDifficultyUI()
    {
        StopAllCoroutines();
        StartCoroutine(DifficultyUIRoutine());
    }

    IEnumerator DifficultyUIRoutine()
    {
        float duration = 0.2f;
        float time = 0f;

        int count = difficultyGroups.Length;

        float[] startAlpha = new float[count];
        float[] targetAlpha = new float[count];

        Vector3[] startScale = new Vector3[count];
        Vector3[] targetScale = new Vector3[count];

        // 초기값 / 목표값 세팅
        for (int i = 0; i < count; i++)
        {
            startAlpha[i] = difficultyGroups[i].alpha;
            startScale[i] = difficultyPanelTrs[i].localScale;

            if (i == selectedDifficulty)
            {
                targetAlpha[i] = 1f;
                targetScale[i] = Vector3.one;
            }
            else
            {
                targetAlpha[i] = 0.5f;
                targetScale[i] = Vector3.one * 0.75f;
            }
        }

        // 보간
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // 부드러운 easing
            t = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < count; i++)
            {
                difficultyGroups[i].alpha = Mathf.Lerp(startAlpha[i], targetAlpha[i], t);
                difficultyPanelTrs[i].localScale = Vector3.Lerp(startScale[i], targetScale[i], t);
            }

            yield return null;
        }

        // 마지막 값 보정
        for (int i = 0; i < count; i++)
        {
            difficultyGroups[i].alpha = targetAlpha[i];
            difficultyPanelTrs[i].localScale = targetScale[i];
        }
    }
}