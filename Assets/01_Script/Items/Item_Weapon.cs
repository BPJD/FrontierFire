using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Item_Weapon : MonoBehaviour, IInteractable
{
    GameObject player;
    PlayerWeaponData pWeaponData;
    Data_WeaponStatUpgrades upgradeData;
    Data_ItemTierColor itemTierColorData;
    public WeaponParams itemWeaponParams { get; set; }

    [SerializeField] Transform propTr;

    int weaponArray;

    [SerializeField] int weaponID;

    public int propMagCur = 0;
    public int propAmmoCur = 1;
    public int weaponPickCount = 0;
    public int quality = 60;

    [SerializeField] int qualityMax = 100, qualityMin = 0;

    [SerializeField] bool isWeaponDropped = false;


    private List<int> upgradesCur = new List<int>();

    Item_ToolTip toolTip;

    void Start()
    {
        if (!isWeaponDropped)
        {
            WeaponDropItem(qualityMin, qualityMax);
        }


    }


    void SetComponent()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        pWeaponData = GameObject.FindGameObjectWithTag("Data").GetComponent<PlayerWeaponData>();
        itemTierColorData = GameObject.FindGameObjectWithTag("Data").GetComponent<Data_ItemTierColor>();
    }

    public void WeaponDropItem(int min, int max)
    {
        isWeaponDropped = true;
        SetComponent();
        weaponArray = Random.Range(0, pWeaponData.GetWeaponCount());
        quality = Random.Range(min, max);
        WeaponPropPrint(pWeaponData.GetWeaponIDbyList(weaponArray));
    }

    public void WeaponDropItem(int weaponID)
    {
        isWeaponDropped = true;
        SetComponent();
        WeaponPropPrint(weaponID);
    }

    void WeaponPropPrint(int id)
    {
        weaponID = id;
        GameObject prop = Instantiate(pWeaponData.GetpWeaponPrefab(id), propTr);
        prop.transform.localPosition = Vector3.zero;
        prop.transform.localRotation = Quaternion.Euler(Vector3.zero);
        prop.GetComponent<WeaponStatus>().weaponDataSource = pWeaponData.GetWeaponStatSO(id);
        prop.SetActive(true);

        toolTip = GetComponent<Item_ToolTip>();


        if (toolTip != null)
        {
            // --- 0) 기본 + 품질 보정 ---
            WeaponParamsSO weaponParamsSO = pWeaponData.GetWeaponStatSO(id);
            WeaponParams baseParam = new WeaponParams(
                WeaponStatRevisionByQuality.GetRevisedParams(weaponParamsSO, quality)
            );

            // --- 1) 이 스크립트가 들고 있는 Upgrade List 사용 ---
            // Data_WeaponStatUpgrades 가져오기 (WeaponUpgrade의 GetUpgradeData 비슷하게)
            if (upgradeData == null)
            {
                var dataGO = GameObject.FindGameObjectWithTag("Data");
                if (dataGO)
                    upgradeData = dataGO.GetComponent<Data_WeaponStatUpgrades>();
            }

            WeaponParams _param = baseParam;

            if (upgradeData != null && upgradesCur != null && upgradesCur.Count > 0)
            {
                var add = new Dictionary<int, float>();
                var mult = new Dictionary<int, float>();

                // 리스트에 들어 있는 업그레이드 ID들을 전부 누적
                foreach (int packId in upgradesCur)
                {
                    var pack = upgradeData.GetAllUpgrades(packId);
                    if (pack == null) continue;

                    foreach (var so in pack)
                    {
                        WeaponUpgradeUtil.ApplyUpgradeToDict(
                            so.up_type,   // 0=가산, 1=계수
                            so.up_stat,   // CSV up_stat
                            so.up_value,  // 값
                            add, mult
                        );
                    }
                }

                // 누적된 add/mult를 이용해 최종 스탯 계산
                _param = WeaponUpgradeUtil.BuildParamsWithUpgrades(baseParam, add, mult);
                itemWeaponParams = _param;
            }

            // --- 2) 최종 스탯으로 툴팁 채우기 ---
            toolTip.title = _param.w_name;
            toolTip.subTitle = _param.w_type.ToString();
            toolTip.titleColor = itemTierColorData.GetItemTierColor(quality, true);
            toolTip.description = _param.w_desc;

            toolTip.weaponStat[0] = _param.w_atkType.ToString();
            toolTip.weaponStat[1] = _param.w_usingAmmo.ToString();
            toolTip.weaponStat[2] = _param.w_atk.ToString("F0");
            toolTip.weaponStat[3] = _param.w_rpm.ToString("F0");
            toolTip.weaponStat[4] = _param.w_accuracy.ToString("F0");
            toolTip.weaponStat[5] = _param.w_range.ToString("F0");
            toolTip.weaponStat[6] = _param.w_reloadTime.ToString("F1");
            toolTip.weaponStat[7] = _param.w_magSize.ToString();
            toolTip.weaponStat[8] = _param.e_quality.ToString();
            toolTip.weaponStat[9] = _param.w_dps.ToString();

            toolTip.thisItemWeaponParams = _param;
            toolTip.UpdateToolTipUI();

        }
    }


    public bool TryInteract()
    {
        PlayerWeaponController weaponController = player.GetComponent<PlayerWeaponController>();
        if(pWeaponData.GetWeaponStatSO(weaponID).w_type != WeaponParamsSO.WeaponTypes.Default && weaponController.weaponCur == 2 && weaponController.isSlotFull)
        {
            Debug.Log("보조무기만 착용 가능한 슬롯입니다.");
            return false;
        }
        else if (pWeaponData.GetWeaponStatSO(weaponID).w_type == WeaponParamsSO.WeaponTypes.Default && weaponController.weaponCur != 2)
        {
            Debug.Log("보조무기 슬롯에만 착용 가능합니다.");
            return false;
        }
        else
        {
            player.GetComponent<PlayerWeaponController>().GetWeapon(weaponID, propAmmoCur, propMagCur, weaponPickCount, upgradesCur, quality);

            ItemSelector selector = GetComponentInParent<ItemSelector>();

            if (selector != null)
            {
                selector.ItemSelected();
            }
            else
            {
                Destroy(gameObject);
            }
            return true;
        }

            
    }

    public void ammoSet(int ammo, int mag, int pickCount)
    {
        propAmmoCur = ammo;
        propMagCur = mag;
        weaponPickCount = pickCount;

        toolTip = GetComponent<Item_ToolTip>();
        if (toolTip != null)
        {
            toolTip.UpdateToolTipUI();
        }
    }

    public void UpgradeSet(List<int> ids, int _quality)
    {
        quality = _quality;
        upgradesCur = (ids != null) ? new List<int>(ids) : new List<int>();
    }




}
