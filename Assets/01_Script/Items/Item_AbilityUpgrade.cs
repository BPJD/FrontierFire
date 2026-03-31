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
            player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);

        if (!upgradeProp)
        {
            var dataObj = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag);
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

        if (UpgradeTextDB.I == null || !UpgradeTextDB.I.IsReady)
        {
            Debug.LogWarning("[Item_AbilityUpgrade] UpgradeTextDB not ready");
            return;
        }

        if (!UpgradeTextDB.I.TryGet(100 + upStatID, out var row))
        {
            Debug.LogWarning($"[Item_AbilityUpgrade] No CSV row for ID={100 + upStatID}");
            return;
        }

        toolTip.title = row.name ?? string.Empty;
        toolTip.subTitle = row.desc ?? string.Empty;
        toolTip.description = row.effect ?? string.Empty;

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
