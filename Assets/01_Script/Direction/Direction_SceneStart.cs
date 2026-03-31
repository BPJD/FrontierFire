using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Direction_SceneStart : MonoBehaviour
{

    [SerializeField] bool isSkip = false;
    [SerializeField] CanvasGroup fadeCanvas;


    [Header("Delay")]
    [SerializeField] private float inputLockDuration = 5f;

    [Header("Effect")]
    [SerializeField] private ParticleSystem startEffect;
    [SerializeField] private ParticleSystem idleEffect;
    AudioSource effect_Audio;

    private GameObject player;
    private PlayerInput playerInputComp;
    private PlayerInputController playerInputController;

    private bool isPlaying = false;

    private void Start()
    {
        if (!isSkip)
        {
            idleEffect.Play(true);
            StartCoroutine(FadeIn(fadeCanvas, 1.5f));
        }
    }

    private void Update()
    {
        if (isSkip) return;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);

            if (player != null)
            {
                playerInputComp = player.GetComponent<PlayerInput>();
                playerInputController = player.GetComponent<PlayerInputController>();
                PlaySequence();
            }
        }
    }

    /// <summary>
    /// 연출 시작:
    /// 일정 시간 동안 플레이어 입력을 막고,
    /// 시간이 지나면 입력을 다시 활성화하면서 파티클을 재생한다.
    /// </summary>
    public void PlaySequence()
    {
        if (isPlaying)
            return;

        StartCoroutine(Co_PlaySequence());
    }

    private IEnumerator Co_PlaySequence()
    {
        isPlaying = true;

        if (playerInputController != null)
        {
            playerInputController.playerModelObj.SetActive(false);
            playerInputController.playerWeaponObj.SetActive(false);
        }

        SetPlayerInputEnabled(false);

        yield return new WaitForSecondsRealtime(inputLockDuration);

        SetPlayerInputEnabled(true);

        if (idleEffect != null)
            idleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (startEffect != null)
            startEffect.Play(true);

        effect_Audio = startEffect.GetComponentInParent<AudioSource>();
        if (effect_Audio != null)
            effect_Audio.Play();

        if (playerInputController != null)
        {
            playerInputController.playerModelObj.SetActive(true);
            playerInputController.playerWeaponObj.SetActive(true);
        }

        isPlaying = false;
    }

    private void SetPlayerInputEnabled(bool enabledState)
    {
        if (playerInputComp != null)
            playerInputComp.enabled = enabledState;

        if (playerInputController != null)
            playerInputController.enabled = enabledState;
    }


    IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        float time = 0f;

        // 시작 상태 보장
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / duration);
            yield return null;
        }

        // 종료 상태 보정
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }


}

