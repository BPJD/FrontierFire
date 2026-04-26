using UnityEngine;
using System.Collections;


public class Item_StatUpgrade : MonoBehaviour, IInteractable
{
    GameObject player;
    UnitStatUpgrade playerStatUp;
    Data_StatUpgrades upgradeData;
    Data_UpgradeModels upgradeProp;
    Data_ItemTierColor itemTierColor;


    StatUpgradesSO upgradeSO;

    public int upStatID = 0;
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
        SetStatUpgradeID();
    }

    void SetComponent()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);

        if (player && !playerStatUp)
            playerStatUp = player.GetComponent<UnitStatUpgrade>();

        if (!upgradeData || !upgradeProp)
        {
            var dataObj = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag);
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

        if(upStatID == 0)
        {
            int id = upgradeData.GetRandomIdByRolledRarity();
            upStatID = id;
        }

        upgradeSO = upgradeData.GetStatUp(upStatID);

        upgradeProp.InstanceStatUpObj(ItemTr, upgradeSO.up_category, upgradeSO.up_tier);

        StatSystemIDSet();
    }


    void StatSystemIDSet()
    {
        var pack = upgradeData.GetAllStatUps(upStatID);

        if (toolTip == null)
            toolTip = GetComponent<Item_ToolTip>();

        if (toolTip == null)
            return;

        if (pack != null && pack.Count > 0)
        {
            var first = pack[0];

            toolTip.title = first.up_name;
            toolTip.subTitle = itemTierColor.ReturnItemTier(first.up_tier, false);
            toolTip.titleColor = itemTierColor.GetItemTierColor(first.up_tier, false);
            toolTip.description = first.up_desc;

            toolTip.weaponStat.Clear();
            toolTip.weaponStatIds.Clear();

            for (int i = 0; i < pack.Count; i++)
            {
                string statName = upgradeData.localize_statUp + upgradeData.loczlize_Stats[pack[i].up_stat];
                string valueStr = ToolTipValue(pack[i].up_value, pack[i].up_type, pack[i].up_stat);

                toolTip.weaponStat.Add(statName);
                toolTip.weaponStat.Add(valueStr);
                toolTip.weaponStatIds.Add(pack[i].up_stat);
            }

            RefreshToolTip();
        }
    }

    void RefreshToolTip()
    {
        if (toolTip == null)
            return;

        toolTip.UpdateToolTipUI();
        StartCoroutine(RefreshToolTipNextFrame());
    }

    IEnumerator RefreshToolTipNextFrame()
    {
        yield return null;

        if (toolTip != null)
            toolTip.UpdateToolTipUI();
    }

    string ToolTipValue(float value, int type, int statID)
    {
        string _percentStr = "";
        string _plusStr = "";
        string _setStr = "";
        if (IsToolTipDescPrintPercent(type, statID))
        {
            _percentStr = "%";
        }

        if(value > 0 && type != 2)
        {
            _plusStr = "+";
        }

        if (type == 2)
        {
            _setStr = ": ";
        }

        return _plusStr + _setStr + value + _percentStr;
    }

    bool IsToolTipDescPrintPercent(int type, int statID)
    {
        if (type == 1)
        {
            return true;
        }
        switch (statID)
        {
            case 3:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
                return true;
        }
        return false;
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
