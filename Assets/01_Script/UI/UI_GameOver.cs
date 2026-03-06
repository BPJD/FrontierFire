using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_GameOver : MonoBehaviour
{
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField] private float BGfadeDuration = 10f;
    [SerializeField] private float UIfadeDuration = 1f;

    [SerializeField] private float BGfadeTarget = 0.97f;
    [SerializeField] private float UIfadeTarget = 1f;

    [SerializeField] private float BGfadeDelay = 1f;
    [SerializeField] private float UIfadeDelay = 2f;

    [SerializeField] private CanvasGroup canvasGroupBG;
    [SerializeField] private CanvasGroup canvasGroupUI;

    private Coroutine coBG;
    private Coroutine coUI;

    [SerializeField] private Direction_SceneChanger sceneChanger;
    [SerializeField] GameObject firstButton;

    private void OnEnable()
    {
        // 중복 실행 방지(Enable/Disable 반복 대비)
        if (coUI != null) StopCoroutine(coUI);
        if (coBG != null) StopCoroutine(coBG);

        coUI = StartCoroutine(FadeRoutine(canvasGroupUI, UIfadeDelay, UIfadeDuration, UIfadeTarget));
        coBG = StartCoroutine(FadeRoutine(canvasGroupBG, BGfadeDelay, BGfadeDuration, BGfadeTarget));


        EventSystem.current.SetSelectedGameObject(firstButton);
    }

    private IEnumerator FadeRoutine(CanvasGroup group, float delay, float duration, float target)
    {
        // 게임오버/일시정지 대응
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;

        // 시작값 명시 (재활성화 시 상태 꼬임 방지)
        group.alpha = 0f;

        // duration 0 방어
        if (duration <= 0f)
        {
            group.alpha = target;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);      // 0~1
            float eased = fadeCurve.Evaluate(t);              // 0~1 (커브 설계에 따름)

            group.alpha = Mathf.LerpUnclamped(0f, target, eased); // 0→target
            yield return null;
        }

        group.alpha = target;
    }


    public void ButtonReport()
    {

    }

    public void ButtonRestart()
    {
        sceneChanger.GameRestart();
    }

    public void ButtonToMain()
    {
        sceneChanger.ToMainMenu();
    }
}