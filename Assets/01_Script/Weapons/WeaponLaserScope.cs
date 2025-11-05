using UnityEngine;

public class WeaponLaserScope : MonoBehaviour
{
    WeaponStatus weaponStat;
    LineRenderer line;
    Transform tr;
    [SerializeField] LayerMask blockMask;     // 막히는 지형 레이어


    private void Start()
    {
        weaponStat = GetComponentInParent<WeaponStatus>();
        line = GetComponent<LineRenderer>();
        tr = transform;
    }

    void FixedUpdate()
    {
        float distance = weaponStat.bulletRange;

        if (Physics.Raycast(tr.position, tr.forward, out RaycastHit hit, distance, blockMask))
            distance = hit.distance;

        // 로컬 Z축 기준으로만 라인 표시
        line.SetPosition(1, new Vector3(0f, 0f, distance));
    }
}
