using UnityEngine;
using System.Text;

public class Item_WeaponUpgrade : MonoBehaviour, IInteractable
{
    GameObject player;
    PlayerWeaponController weaponController;
    WeaponStatUpgrade weaponStatUp;
    Data_WeaponStatUpgrades upgradeData;
    Data_WeaponUpgradeModels upgradeModelData;

    [SerializeField] bool isDebug = false;

    WeaponStatUpgradesSO upgradeSO;

    [SerializeField] int upStatID;

    Item_ToolTip toolTip;

    [SerializeField] Transform itemTr;
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

        weaponController = player.GetComponent<PlayerWeaponController>();

        if (!upgradeData || !upgradeModelData)
        {
            var dataObj = GameObject.FindGameObjectWithTag("Data");
            if (dataObj)
            {
                upgradeData = dataObj.GetComponent<Data_WeaponStatUpgrades>();
                upgradeModelData = dataObj.GetComponent <Data_WeaponUpgradeModels>();
            }
        }
    }

    void SetStatUpgradeID()
    {
        if (!upgradeData || !upgradeModelData) SetComponent();

        int upgradeCount = upgradeData.GetUpgradeCount();
        upStatID = upgradeData.GetRandomUpgradeID();

        // 단일 SO는 하위호환/디버그용으로 보관
        upgradeSO = upgradeData.GetUpgrade(upStatID);

        upgradeModelData.InstanceStatUpObj(itemTr, upgradeSO.up_model, upgradeSO.up_tier);

        StatSystemIDSet();
    }

    void StatSystemIDSet()
    {
        toolTip = GetComponent<Item_ToolTip>();
        if (toolTip == null) return;

        var pack = upgradeData.GetAllUpgrades(upStatID);
        if (pack == null || pack.Count == 0)
        {
            // fallback(단일)
            if (upgradeSO != null)
            {
                toolTip.title = upgradeSO.up_name;
                toolTip.subTitle = upgradeSO.up_desc;
                toolTip.description = upgradeSO.up_uiDesc;
                toolTip.UpdateToolTipUI();
            }
            return;
        }

        toolTip.title = pack[0].up_name; // 같은 ID면 동일 이름/설명 가정
        //toolTip.subTitle = pack[0].up_desc;
        toolTip.subTitle = "";

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
            // ★ 패키지 단위 적용
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
