using System.Collections.Generic;
using UnityEngine;

public class Item_Weapon : MonoBehaviour, IInteractable
{
    GameObject player;
    PlayerWeaponData pWeaponData;
    
    [SerializeField] Transform propTr;

    int weaponArray;

    [SerializeField] int weaponID;

    public int propMagCur = 0;
    public int propAmmoCur = 1;
    public int weaponPickCount = 0;

    [SerializeField] bool isWeaponDropped = false;


    private List<int> upgradesCur = new List<int>();

    Item_ToolTip toolTip;

    void Start()
    {
        if (!isWeaponDropped)
        {
            WeaponDropItem();
        }
    }


    void SetComponent()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        pWeaponData = GameObject.FindGameObjectWithTag("Data").GetComponent<PlayerWeaponData>();
    }

    public void WeaponDropItem()
    {
        isWeaponDropped = true;
        SetComponent();
        weaponArray = Random.Range(0, pWeaponData.GetWeaponCount());
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
            WeaponParamsSO weaponParams = pWeaponData.GetWeaponStatSO(id);
            toolTip.title = weaponParams.w_name;
            toolTip.subTitle = weaponParams.w_type.ToString();
            toolTip.description = weaponParams.w_desc;
            toolTip.weaponStat[0] = weaponParams.w_atkType.ToString();
            toolTip.weaponStat[1] = weaponParams.w_usingAmmo.ToString();
            toolTip.weaponStat[2] = weaponParams.w_atk.ToString();
            toolTip.weaponStat[3] = weaponParams.w_rpm.ToString();
            toolTip.weaponStat[4] = weaponParams.w_accuracy.ToString();
            toolTip.weaponStat[5] = weaponParams.w_range.ToString();
            toolTip.weaponStat[6] = weaponParams.w_reloadTime.ToString();
            toolTip.weaponStat[7] = weaponParams.w_magSize.ToString();

            toolTip.UpdateToolTipUI();
        }
    }


    public void Interact()
    {
        PlayerWeaponController weaponController = player.GetComponent<PlayerWeaponController>();
        if(pWeaponData.GetWeaponStatSO(weaponID).w_type != WeaponParamsSO.WeaponTypes.Default && weaponController.weaponCur == 2 && weaponController.isSlotFull)
        {
            Debug.Log("보조무기만 착용 가능한 슬롯입니다.");
        }
        else if (pWeaponData.GetWeaponStatSO(weaponID).w_type == WeaponParamsSO.WeaponTypes.Default && weaponController.weaponCur != 2)
        {
            Debug.Log("보조무기 슬롯에만 착용 가능합니다.");
        }
        else
        {
            Debug.Log($"{gameObject} 아이템 획득");

            player.GetComponent<PlayerWeaponController>().GetWeapon(weaponID, propAmmoCur, propMagCur, weaponPickCount, upgradesCur);

            ItemSelector selector = GetComponentInParent<ItemSelector>();

            if (selector != null)
            {
                selector.ItemSelected();
            }
            else
            {
                Destroy(gameObject);
            }
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

    public void UpgradeSet(List<int> ids)
    {
        upgradesCur = (ids != null) ? new List<int>(ids) : new List<int>();
    }



}
