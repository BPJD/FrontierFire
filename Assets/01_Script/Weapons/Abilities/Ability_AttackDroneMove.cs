using UnityEngine;
using System.Collections;

public class Ability_AttackDroneMove : MonoBehaviour
{
    Transform tr;

    [Header("Base Offset")]
    [SerializeField] Vector3 baseVector = new Vector3(0.5f, 0.75f, -0.333f);
    Vector3 leftPoint;
    Vector3 rightPoint;

    [Header("Move")]
    [SerializeField] float moveTime = 0.25f;

    [Header("Hover")]
    [SerializeField] float hoverAmplitude = 0.06f;   // 위아래 폭(로컬 y)
    [SerializeField] float hoverFrequency = 1.5f;    // 초당 왕복 느낌(Hz)
    [SerializeField] float hoverPhaseRandom = 1f;    // 개체마다 위상 랜덤

    Coroutine moveRoutine;
    bool? lastIsLeft = null;

    // “부유가 더해지기 전” 순수 오프셋
    Vector3 baseOffset;

    float phase;

    void Start()
    {
        tr = transform;

        rightPoint = baseVector;
        leftPoint = new Vector3(-baseVector.x, baseVector.y, baseVector.z);

        baseOffset = tr.localPosition;

        phase = (hoverPhaseRandom <= 0f) ? 0f : Random.value * Mathf.PI * 2f;
    }

    void LateUpdate()
    {
        // baseOffset + hover 를 합쳐서 최종 localPosition 적용
        Vector3 p = baseOffset;

        float hover = Mathf.Sin((Time.time * hoverFrequency * Mathf.PI * 2f) + phase) * hoverAmplitude;
        p.y += hover;

        tr.localPosition = p;
    }

    public void PlayerTurned(bool isLeft)
    {
        // 방향 변화 없으면 무시
        if (lastIsLeft.HasValue && lastIsLeft.Value == isLeft) return;
        lastIsLeft = isLeft;

        Vector3 target = isLeft ? leftPoint : rightPoint;
        MoveTo(target);
    }

    void MoveTo(Vector3 target)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine(target, moveTime));
    }

    IEnumerator MoveRoutine(Vector3 target, float duration)
    {
        Vector3 start = baseOffset;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            // Ease Out (처음 빠르고 끝에서 느려짐)
            float easeT = 1f - Mathf.Pow(1f - t, 2f);

            // baseOffset만 갱신 (hover는 LateUpdate에서 별도 적용)
            baseOffset = Vector3.Lerp(start, target, easeT);

            yield return null;
        }

        baseOffset = target;
        moveRoutine = null;
    }
}
