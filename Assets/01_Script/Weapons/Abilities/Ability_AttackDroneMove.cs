using UnityEngine;
using System.Collections;

public class Ability_AttackDroneMove : MonoBehaviour
{
    Transform tr;
    Vector3 leftPoint;
    Vector3 rightPoint;
    [SerializeField] float moveTime = 0.5f;

    private void Start()
    {
        tr = transform;
        rightPoint = tr.position;
        leftPoint = new Vector3(-tr.position.x, tr.position.y, -tr.position.z);
    }

    public void PlayerTurned(bool isLeft)
    {
        Vector3 _dir = isLeft ? leftPoint : rightPoint;
        MoveTo(_dir);

    }

    public void MoveTo(Vector3 target)
    {
        StartCoroutine(MoveRoutine(target, moveTime));
    }

    IEnumerator MoveRoutine(Vector3 target, float duration)
    {
        Vector3 start = tr.localPosition;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Ease Out (처음 빠르고 끝에서 느려짐)
            float easeT = 1f - Mathf.Pow(1f - t, 2f);

            tr.localPosition = Vector3.Lerp(start, target, easeT);
            yield return null;
        }

        tr.localPosition = target;
    }
}
