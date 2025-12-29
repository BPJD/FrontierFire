using UnityEngine;

public class Item_AbilityUpgrade : MonoBehaviour, IInteractable
{
    GameObject player;

    Data_UpgradeModels upgradeProp;

    [SerializeField] bool isDebug = false;
    [SerializeField] int upStatID;

    Item_ToolTip toolTip;

    [SerializeField] Transform ItemTr;
    [SerializeField] AudioClip[] sounds_itemGet;

    int upgradesCount = 0;

    void Awake()
    {
        SetComponent();
    }

    void Start()
    {
        if (!isDebug)
        {
            SetAbilityUpgradeID();
        }
        else
        {
            // 디버그 모드에서도 동일한 흐름
            AbilitySystemIDSet();
        }
    }

    void SetComponent()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player");

        if (!upgradeProp)
        {
            var dataObj = GameObject.FindGameObjectWithTag("Data");
            if (dataObj)
                upgradeProp = dataObj.GetComponent<Data_UpgradeModels>();
        }

        toolTip = GetComponent<Item_ToolTip>();
    }

    // =====================================================
    // 1) 업그레이드 ID 확정 + 모델 생성
    // =====================================================
    void SetAbilityUpgradeID()
    {
        if (!upgradeProp) SetComponent();

        upgradesCount = upgradeProp.GetAbilityModelCount();
        upStatID = Random.Range(0, upgradesCount);

        Instantiate(
            upgradeProp.GetAbilityModel(upStatID),
            ItemTr
        );

        AbilitySystemIDSet();
    }

    // =====================================================
    // 2) 툴팁 세팅 (StatUpgrade와 동일 패턴)
    // =====================================================
    void AbilitySystemIDSet()
    {
        if (toolTip == null)
            toolTip = GetComponent<Item_ToolTip>();

        if (toolTip == null)
            return;

        // CSV DB 준비 확인
        if (UpgradeTextDB.I == null || !UpgradeTextDB.I.IsReady)
        {
            Debug.LogWarning("[Item_AbilityUpgrade] UpgradeTextDB not ready");
            return;
        }

        if (!UpgradeTextDB.I.TryGet(100 + upStatID, out var row))
        {
            Debug.LogWarning($"[Item_AbilityUpgrade] No CSV row for ID={upStatID}");
            return;
        }

        // ---- 여기부터 StatUpgrade와 동일한 책임 ----
        toolTip.title = row.name;
        toolTip.subTitle = row.name;

        if (!string.IsNullOrEmpty(row.desc) && !string.IsNullOrEmpty(row.effect))
        {
            toolTip.description = $"{row.desc}\n{row.effect}";
        }
        else
        {
            toolTip.description = string.IsNullOrEmpty(row.desc)
                ? row.effect
                : row.desc;
        }

        toolTip.UpdateToolTipUI();
    }

    // =====================================================
    // 3) 상호작용
    // =====================================================
    public bool TryInteract()
    {
        player.GetComponentInChildren<AbilityController>().PlayerGetItem(upStatID);

        InteractComplete();
        return true;
    }

    void InteractComplete()
    {
        var legacySelector = GetComponentInParent<ItemSelector>();
        if (legacySelector != null)
        {
            legacySelector.ItemSelected();
            return;
        }

        Destroy(gameObject);
    }
}
