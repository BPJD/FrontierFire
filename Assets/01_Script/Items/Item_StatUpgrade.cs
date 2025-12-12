using UnityEngine;
using static StatUpgradesSO;


public class Item_StatUpgrade : MonoBehaviour, IInteractable
{
    GameObject player;
    UnitStatUpgrade playerStatUp;
    Data_StatUpgrades upgradeData;
    Data_UpgradeModels upgradeProp;
    Data_ItemTierColor itemTierColor;

    [SerializeField] bool isDebug = false;

    StatUpgradesSO upgradeSO;

    [SerializeField] int upStatID;
    [SerializeField] float statValue;

    Item_ToolTip toolTip;
    [SerializeField] Transform ItemTr;
    [SerializeField] AudioClip[] sounds_itemGet;


    void Awake()
    {
        // 항상 의존성 세팅 (디버그와 무관)
        SetComponent();
    }

    void Start()
    {
        if (!isDebug)
        {
            SetStatUpgradeID();
        }

    }

    void SetComponent()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player && !playerStatUp)
            playerStatUp = player.GetComponent<UnitStatUpgrade>();

        if (!upgradeData || !upgradeProp)
        {
            var dataObj = GameObject.FindGameObjectWithTag("Data");
            if (dataObj)
            {
                upgradeData = dataObj.GetComponent<Data_StatUpgrades>();
                upgradeProp = dataObj.GetComponent<Data_UpgradeModels>();
                itemTierColor = dataObj.GetComponent<Data_ItemTierColor>();
            }
        }
    }

    void SetStatUpgradeID()
    {
        if (!upgradeData || !upgradeProp) SetComponent();

        int id = upgradeData.GetRandomIdByRolledRarity();

        upStatID = id;
        upgradeSO = upgradeData.GetStatUp(id);

        upgradeProp.InstanceStatUpObj(ItemTr, upgradeSO.up_category, upgradeSO.up_tier);

        StatSystemIDSet();
    }

    void StatSystemIDSet()
    {
        // upStatID는 이제 "업그레이드 ID" 그대로 사용
        var pack = upgradeData.GetAllStatUps(upStatID);
        toolTip = GetComponent<Item_ToolTip>();
        if (toolTip == null) return;


        if (pack != null && pack.Count > 0)
        {
            toolTip.title = pack[0].up_name;   // 같은 ID면 이름/설명 동일 가정

            toolTip.subTitle = itemTierColor.ReturnItemTier(upgradeSO.up_tier, false);
            toolTip.titleColor = itemTierColor.GetItemTierColor(upgradeSO.up_tier, false);

            var sb = new System.Text.StringBuilder();
            foreach (var so in pack)
                if (!string.IsNullOrEmpty(so.up_uiDesc))
                {
                    sb.AppendLine(so.up_uiDesc);
                }
            toolTip.description = sb.ToString().TrimEnd();
            


            toolTip.UpdateToolTipUI();
        }
        else
        {
            // 폴백: 단일 SO 경로 (기존 그대로)
            // toolTip.title = upgradeSO.up_name; ...
        }
    }

    public bool TryInteract()
    {
        if (!playerStatUp) SetComponent();
        if (playerStatUp)
            playerStatUp.UpgradeStatPackageById(upStatID); // ★ 패키지 적용
        InteractComplete();

        return true;
    }

    AudioClip GetClip(int itemClass)
    {
        switch (itemClass)
        {
            case 0:
            case 1:
                return sounds_itemGet[0];
            case 2:
            case 3:
                return sounds_itemGet[1];
            case 4:
            case 5:
                return sounds_itemGet[2];
            default:
                return sounds_itemGet[0];
        }
    }

    void InteractComplete()
    {
        AudioClip _clip = GetClip(upgradeSO.up_tier);
        player.GetComponentInChildren<AudioSource>().PlayOneShot(_clip);

        Transform playerTr = player.transform;
        GameObject _getEft = Instantiate(upgradeProp.GetClassGetEft(upgradeSO.up_tier), playerTr.position + Vector3.up, Quaternion.identity, playerTr);
        Destroy(_getEft, 5f);

        // 1) 기존의 부모 ItemSelector 알림 (네 프로젝트 고유)
        // *클래스 이름 충돌을 피하려면, 내 선택 스캐너는 다른 이름 사용*
        var legacySelector = GetComponentInParent<ItemSelector>(); // 네 기존 스크립트
        if (legacySelector != null)
        {
            legacySelector.ItemSelected(); // 기존 동작 유지
            return; // 이 경우 파괴/정리 로직은 legacySelector가 처리한다고 가정
        }

        Destroy(gameObject);
    }
}
