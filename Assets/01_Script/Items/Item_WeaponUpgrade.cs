using UnityEngine;
using System.Collections;
using static WeaponStatUpgradesSO;

public class Item_WeaponUpgrade : MonoBehaviour, IInteractable
{
    GameObject player;
    PlayerWeaponController weaponController;
    WeaponStatUpgrade weaponStatUp;
    Data_WeaponStatUpgrades upgradeData;
    Data_WeaponUpgradeModels upgradeModelData;
    Data_ItemTierColor itemTierColor;

    [SerializeField] bool isDebug = false;

    WeaponStatUpgradesSO upgradeSO;

    [SerializeField] int upStatID;

    Item_ToolTip toolTip;

    [SerializeField] Transform itemTr;
    [SerializeField] AudioClip[] sounds_itemGet;

    [SerializeField]
    private WeaponRarityWeight[] rarityWeights = new WeaponRarityWeight[]
    {
        new WeaponRarityWeight{ itemTier = WeaponItemTier.D,  weight = 0f },
        new WeaponRarityWeight{ itemTier = WeaponItemTier.C,  weight = 3f },
        new WeaponRarityWeight{ itemTier = WeaponItemTier.B,  weight = 12f },
        new WeaponRarityWeight{ itemTier = WeaponItemTier.A,  weight = 18f },
        new WeaponRarityWeight{ itemTier = WeaponItemTier.S,  weight = 12f  },
        new WeaponRarityWeight{ itemTier = WeaponItemTier.SS, weight = 5f  }
    };


    void Awake()
    {
        // 항상 의존성 세팅 (디버그와 무관)
        SetComponent();
    }

    void Start()
    {
        if (!isDebug)
        {
            if(upStatID == 0)
            {
                SetStatUpgradeID();
            }
        }
    }

    void SetComponent()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);

        weaponController = player.GetComponent<PlayerWeaponController>();

        if (!upgradeData || !upgradeModelData)
        {
            var dataObj = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag);
            if (dataObj)
            {
                upgradeData = dataObj.GetComponent<Data_WeaponStatUpgrades>();
                upgradeModelData = dataObj.GetComponent <Data_WeaponUpgradeModels>();
                itemTierColor = dataObj.GetComponent<Data_ItemTierColor>();
            }
        }
    }

    void SetStatUpgradeID()
    {
        if (!upgradeData || !upgradeModelData) SetComponent();

        upStatID = upgradeData.GetRandomIdByRolledRarity(rarityWeights);

        // 패키지 우선
        var pack = upgradeData.GetAllUpgrades(upStatID);

        // 대표 SO 결정: pack[0] 우선, 없으면 단일
        upgradeSO = (pack != null && pack.Count > 0) ? pack[0] : upgradeData.GetUpgrade(upStatID);

        if (upgradeSO == null)
        {
            Debug.LogWarning($"[Item_WeaponUpgrade] upStatID {upStatID} 에 해당하는 SO가 없습니다.");
            return;
        }

        // 대표 SO 기준으로 모델/티어 처리
        upgradeModelData.InstanceStatUpObj(itemTr, upgradeSO.up_model, upgradeSO.up_tier);

        StatSystemIDSet();
    }

    void StatSystemIDSet()
    {
        toolTip = GetComponent<Item_ToolTip>();

        if (toolTip == null || upgradeData == null || upgradeSO == null)
            return;

        var pack = upgradeData.GetAllUpgrades(upStatID);

        if (pack == null || pack.Count == 0)
            return;

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

            if(type == 1)
            {
                value = Mathf.Floor(value * 100f);
            }
        }

        if (value > 0 && type != 2)
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
            case 13:
                return true;
        }
        return false;
    }

    public bool TryInteract()
    {
        // 지연 초기화 안전망
        if (!weaponStatUp) SetComponent();

        GameObject weaponCur = weaponController.playersWeapons[weaponController.weaponCur];

        if (weaponCur == null)
        {
            Debug.Log("소지한 무기가 없습니다.");
            return false;
        }

        weaponStatUp = weaponCur.GetComponent<WeaponStatUpgrade>();

        if (weaponStatUp)
        {
            // 패키지 단위 적용
            weaponStatUp.UpgradeStatPackage(upStatID);
            weaponStatUp.upgradesCur.Add(upStatID);
            weaponStatUp.WeaponEffectApply();
        }

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

        GameObject _getEft = Instantiate(upgradeModelData.GetClassGetEft(upgradeSO.up_tier), player.transform.position + Vector3.up, Quaternion.identity, player.transform);
        Destroy(_getEft, 5f);

        // 1) 기존의 부모 ItemSelector 알림 (네 프로젝트 고유)
        var legacySelector = GetComponentInParent<ItemSelector>(); // 기존 스크립트
        if (legacySelector != null)
        {
            legacySelector.ItemSelected(); // 기존 동작 유지
            return; // 이 경우 파괴/정리 로직은 legacySelector가 처리한다고 가정
        }
        Destroy(gameObject);
    }
}
