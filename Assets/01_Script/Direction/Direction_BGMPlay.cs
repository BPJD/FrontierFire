using UnityEngine;
using System.Collections;

public class Direction_BGMPlay : MonoBehaviour
{
    [Header("Start")]
    [SerializeField] private bool isPlayOnStart = true;
    [SerializeField] private AudioClip bgmClip;

    [Header("Fade")]
    [SerializeField] private float defaultFadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio Sources (2)")]
    [SerializeField] private AudioSource[] musicSources = new AudioSource[2];

    private int currentIndex = 0;
    private Coroutine fadeCo;
    private Coroutine volumeFadeCo;

    private void Awake()
    {
        if (musicSources == null || musicSources.Length < 2 || musicSources[0] == null || musicSources[1] == null)
        {
            Debug.LogError("[Direction_BGMPlay] AudioSource 2개를 할당해야 합니다.");
            enabled = false;
            return;
        }

        musicSources[0].volume = 0f;
        musicSources[1].volume = 0f;
    }

    private void Start()
    {
        if (isPlayOnStart && bgmClip != null)
            PlayBGM(bgmClip);
    }

    public void PlayBGM(AudioClip clip, float fadeDuration = -1f, bool forceImmediate = false, bool loop = true)
    {
        if (clip == null) return;

        if (fadeDuration < 0f)
            fadeDuration = defaultFadeDuration;

        AudioSource current = musicSources[currentIndex];

        if (!forceImmediate && current.isPlaying && current.clip == clip)
            return;

        int nextIndex = current.isPlaying ? 1 - currentIndex : currentIndex;
        AudioSource next = musicSources[nextIndex];

        next.clip = clip;
        next.loop = loop;
        next.volume = 0f;
        next.Play();

        if (fadeCo != null)
            StopCoroutine(fadeCo);

        if (forceImmediate || fadeDuration <= 0f)
        {
            if (current != next)
            {
                current.Stop();
                current.volume = 0f;
            }

            next.volume = 1f;
            currentIndex = nextIndex;
            return;
        }

        fadeCo = StartCoroutine(FadeRoutine(current.isPlaying && current != next ? current : null, next, fadeDuration, nextIndex));
    }

    public void StopBGM(float fadeDuration = -1f)
    {
        AudioSource current = musicSources[currentIndex];
        if (!current.isPlaying) return;

        if (fadeDuration < 0f)
            fadeDuration = defaultFadeDuration;

        if (fadeCo != null)
            StopCoroutine(fadeCo);

        if (fadeDuration <= 0f)
        {
            current.Stop();
            current.volume = 0f;
            return;
        }

        fadeCo = StartCoroutine(FadeRoutine(current, null, fadeDuration, currentIndex));
    }

    // 현재 재생 중인 음악의 볼륨을 targetVolume까지 페이드
    public void FadeCurrentVolume(float targetVolume, float duration = -1f)
    {
        AudioSource current = musicSources[currentIndex];
        if (!current.isPlaying) return;

        if (duration < 0f)
            duration = defaultFadeDuration;

        targetVolume = Mathf.Clamp01(targetVolume);

        if (volumeFadeCo != null)
            StopCoroutine(volumeFadeCo);

        if (duration <= 0f)
        {
            current.volume = targetVolume;
            return;
        }

        volumeFadeCo = StartCoroutine(FadeVolumeRoutine(current, targetVolume, duration));
    }

    private IEnumerator FadeVolumeRoutine(AudioSource source, float targetVolume, float duration)
    {
        float elapsed = 0f;
        float startVolume = source.volume;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = fadeCurve.Evaluate(t);

            source.volume = Mathf.LerpUnclamped(startVolume, targetVolume, eased);
            yield return null;
        }

        source.volume = targetVolume;
        volumeFadeCo = null;
    }

    private IEnumerator FadeRoutine(AudioSource from, AudioSource to, float duration, int nextIndex)
    {
        float elapsed = 0f;
        float fromStart = from != null ? from.volume : 0f;
        float toStart = to != null ? to.volume : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = fadeCurve.Evaluate(t);

            if (from != null)
                from.volume = Mathf.LerpUnclamped(fromStart, 0f, eased);

            if (to != null)
                to.volume = Mathf.LerpUnclamped(toStart, 1f, eased);

            yield return null;
        }

        if (from != null)
        {
            from.volume = 0f;
            from.Stop();
        }

        if (to != null)
        {
            to.volume = 1f;
            currentIndex = nextIndex;
        }

        fadeCo = null;
    }
}