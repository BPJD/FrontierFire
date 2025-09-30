using UnityEngine;
using Michsky.UI.Heat; // Heat UI namespace

public class UI_UnitHPBar : MonoBehaviour
{
    [SerializeField] UnitStatus unitStatus;
    [SerializeField] private ProgressBar myBar;

    Transform camTr;
    Transform tr;


    void Start()
    {
        unitStatus = GetComponentInParent<UnitStatus>();
        myBar = GetComponent<ProgressBar>();
        unitStatus.OnHpChanged += UpdateHpBar;

        UpdateHpBar(unitStatus.hpCur, unitStatus.hpCur);

        tr = transform;
        camTr = Camera.main.transform;
    }

    void UpdateHpBar(int currentHp, int maxHp)
    {
        myBar.minValue = 0;
        myBar.maxValue = maxHp;

        myBar.SetValue(currentHp);
        myBar.UpdateUI();

        if(currentHp <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        unitStatus.OnHpChanged -= UpdateHpBar;
        
    }

    void LateUpdate()
    {
        // 항상 카메라를 바라보게 회전
        transform.forward = camTr.forward;
    }
}
