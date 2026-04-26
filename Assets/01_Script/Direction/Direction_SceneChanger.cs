using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Direction_SceneChanger : MonoBehaviour
{
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float fadeDuration = 2f;
    [SerializeField] private float fadeTarget = 1f;

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private string loadingSceneName = "Scene_Loading";

    Direction_BGMPlay bgmPlayer;

    public GameObject player { private get; set; }

    public void GameRestart()
    {
        string sceneName = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag)
            .GetComponent<Data_Scenes>().stageScenes[1];

        ChangeScene(sceneName, true);
    }

    public void ToMainMenu()
    {
        string sceneName = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag)
            .GetComponent<Data_Scenes>().stageScenes[0];

        ChangeScene(sceneName, true);
    }

    public void ChangeScene(string sceneName, bool isReset)
    {
        if(player != null)
        {
            player.GetComponentInChildren<PlayerDashManager>().ResetDashCooldown();
        }


        StartCoroutine(FadeRoutine(fadeDuration, fadeTarget, sceneName, isReset));
        bgmPlayer = GameObject.FindGameObjectWithTag("Sound").GetComponent<Direction_BGMPlay>();

        if(bgmPlayer != null)
        {
            bgmPlayer.StopBGM(fadeDuration);
        }
    }

    private IEnumerator FadeRoutine(float duration, float target, string sceneName, bool isPlayerReset)
    {
        float elapsed = 0f;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (duration <= 0f)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = target;
        }
        else
        {
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = fadeCurve.Evaluate(t);

                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.LerpUnclamped(0f, target, eased);

                yield return null;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = target;
        }

        if (isPlayerReset && player != null)
        {
            Destroy(player);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        LoadingSceneController.nextSceneName = sceneName;
        SceneManager.LoadScene(loadingSceneName);
    }
}