using System.Text;
using UnityEngine;
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
            player = GameObject.FindGameObjectWithTag("Player");

        weaponController = player.GetComponent<PlayerWeaponController>();

        if (!upgradeData || !upgradeModelData)
        {
            var dataObj = GameObject.FindGameObjectWithTag("Data");
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
        if (toolTip == null || upgradeData == null || upgradeSO == null) return;

        var pack = upgradeData.GetAllUpgrades(upStatID);

        // 대표 기준(이미 upgradeSO가 대표)
        toolTip.title = upgradeSO.up_name;
        toolTip.subTitle = itemTierColor.ReturnItemTier(upgradeSO.up_tier, false);
        toolTip.titleColor = itemTierColor.GetItemTierColor(upgradeSO.up_tier, false);

        if (pack == null || pack.Count == 0)
        {
            toolTip.description = upgradeSO.up_uiDesc;
            toolTip.UpdateToolTipUI();
            return;
        }

        var sb = new StringBuilder();
        foreach (var so in pack)
        {
            if (!string.IsNullOrEmpty(so.up_uiDesc))
                sb.AppendLine(so.up_uiDesc);
        }
        toolTip.description = sb.ToString().TrimEnd();
        toolTip.UpdateToolTipUI();
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
