using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Michsky.UI.Heat;

public class LoadingSceneManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("Option")]
    [SerializeField] private float minLoadTime = 1f;

    [TextArea]
    [SerializeField] private string[] tips;

    private void Start()
    {
        SetRandomTip();
        StartCoroutine(LoadSceneRoutine());
    }

    private void SetRandomTip()
    {
        if (tipText == null || tips == null || tips.Length == 0)
            return;

        int rand = Random.Range(0, tips.Length);
        tipText.text = tips[rand];
    }

    private IEnumerator LoadSceneRoutine()
    {
        string nextScene = LoadingSceneController.nextSceneName;

        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogWarning("LoadingSceneController.nextSceneName is null or empty.");
            yield break;
        }

        // 로딩 씬 UI가 실제로 한 번 그려질 시간을 확보
        yield return null;
        yield return new WaitForEndOfFrame();

        float timer = 0f;

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(op.progress / 0.9f);
            float progressPercent = progress * 100f;

            if (progressBar != null)
            {
                progressBar.currentValue = progressPercent;
                progressBar.UpdateUI();
            }

            if (progressText != null)
                progressText.text = $"Loading... {progressPercent:0}%";

            if (op.progress >= 0.9f && timer >= minLoadTime)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}