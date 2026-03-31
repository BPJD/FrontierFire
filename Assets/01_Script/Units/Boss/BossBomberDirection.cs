using UnityEngine;
using System.Collections;

public class BossBomberDirection : MonoBehaviour
{
    [SerializeField] Transform bomberMeshTr;
    [SerializeField] GameObject bossUnit;


    [SerializeField] Vector3 camOffset = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Direction_BossStage bossStage = GameObject.FindGameObjectWithTag("GameController").GetComponent<Direction_BossStage>();
        bossStage.BossStageEntry(gameObject, bossUnit, camOffset);

        StartCoroutine(MoveToPosition(bomberMeshTr, bossUnit.transform.position, 3f));
    }


    public IEnumerator MoveToPosition(Transform target, Vector3 endPos, float duration)
    {
        Vector3 startPos = target.position;
        Vector3 _endPos = endPos;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            target.position = Vector3.Lerp(startPos, _endPos, t);
            yield return null;
        }

        target.position = _endPos;
    }
}
