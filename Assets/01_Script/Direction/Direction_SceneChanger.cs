using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Direction_SceneChanger : MonoBehaviour
{
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float fadeTarget = 1f;

    [SerializeField] private CanvasGroup canvasGroup;

    public GameObject player { private get; set; }


    public void GameRestart()
    {
        string _sceneName = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag).
            GetComponent<Data_Scenes>().stageScenes[1];

        StartCoroutine(FadeRoutine(fadeDuration, fadeTarget, _sceneName, true));

    }

    public void ToMainMenu()
    {
        string _sceneName = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag).
            GetComponent<Data_Scenes>().stageScenes[0];

        StartCoroutine(FadeRoutine(fadeDuration, fadeTarget, _sceneName, true));
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(FadeRoutine(fadeDuration, fadeTarget, sceneName, false));
    }

    private IEnumerator FadeRoutine(float duration, float target, string sceneName, bool isPlayerReset)
    {

        float elapsed = 0f;

        // 시작값 명시 (재활성화 시 상태 꼬임 방지)
        canvasGroup.alpha = 0f;

        // duration 0 방어
        if (duration <= 0f)
        {
            canvasGroup.alpha = target;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);      // 0~1
            float eased = fadeCurve.Evaluate(t);              // 0~1 (커브 설계에 따름)

            canvasGroup.alpha = Mathf.LerpUnclamped(0f, target, eased); // 0→target
            yield return null;
        }

        canvasGroup.alpha = target;

        if(isPlayerReset)
        {
            Debug.Log(player + " Player Destroyed");

            Destroy(player);
        }
        yield return new WaitForSecondsRealtime(0.5f); // 씬 전환 전 잠깐 대기

        Debug.Log("Scene Changed to " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}
