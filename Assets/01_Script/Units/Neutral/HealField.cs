using UnityEngine;
using System.Collections;

public class HealField : MonoBehaviour
{
    [SerializeField] bool isPercentValue = false;

    [SerializeField] int healValue = 10;
    [SerializeField] float delay = 0.5f;
    WaitForSeconds _delay;

    [SerializeField] float healFieldDuration = 5f;
    float healFieldTimer;

    UnitStatus healUnit;
    Transform unitTr;

    AudioSource audioSource;
    [SerializeField] float audioFadeDuration = 1f;

    Coroutine audioFadeCo;

    [SerializeField] GameObject particle;

    [SerializeField] ParticleSystem fieldParticle;

    bool isHealing = true;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        _delay = new WaitForSeconds(delay);

        if (audioSource != null)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            FadeAudio(0.5f, audioFadeDuration); // 시작 시 1로 증가
        }

        StartCoroutine(Heal());
    }

    IEnumerator Heal()
    {
        while (isHealing)
        {
            if(healUnit != null && isHealing)
            {
                healUnit.UnitGetHeal(healValue, true, isPercentValue);

                GameObject _particle = Instantiate(particle, unitTr.position + Vector3.up, particle.transform.rotation);
                Destroy(_particle, 3f);

            }

            healFieldTimer += delay;
            if (healFieldTimer >= healFieldDuration)
            {
                isHealing = false;
                fieldParticle.Stop(true);
                FadeAudio(0f, audioFadeDuration); // 종료 시 0으로 감소
            }

            yield return _delay;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Data_Strings.playerTag))
        {
            healUnit = other.gameObject.GetComponent<UnitStatus>();
            unitTr = other.gameObject.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Data_Strings.playerTag))
        {
            healUnit = null;
            unitTr = null;
        }
    }

    void FadeAudio(float targetVolume, float duration)
    {
        if (audioSource == null) return;

        if (audioFadeCo != null)
            StopCoroutine(audioFadeCo);

        audioFadeCo = StartCoroutine(FadeAudioRoutine(targetVolume, duration));
    }

    IEnumerator FadeAudioRoutine(float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        audioSource.volume = targetVolume;

        // 완전히 꺼지면 Stop까지 처리
        if (targetVolume <= 0f)
            audioSource.Stop();

        audioFadeCo = null;
    }
}
