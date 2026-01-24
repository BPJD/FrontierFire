using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;


public class PlayerWeaponController : MonoBehaviour
{

    public GameObject[] playersWeapons;
    int[] playersWeaponIDs = {0, 0, 60000};
    PlayerWeaponData weaponData;
    Data_UI dataUI;
    UI_Weapon weaponUI;
    PlayerMove playerMove;

    [SerializeField] GameObject weaponProp;

    [SerializeField] Transform[] gunPointTr;

    public bool isSlotFull { get; private set; } = false;


    public int weaponCur { get; private set; } = 0;

    public bool[] isAmmoTypeUsing { get; private set; } = { false, false };
    public bool[] isAmmoFullbyType = { false, false };

    [SerializeField] AudioSource playerSound;
    [SerializeField] AudioClip sound_weaponGet, sound_weaponChange;
    [SerializeField] AudioClip sound_ammoGet;

    PlayerInteract playerInteract;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 바뀌면
        weaponUI = GameObject.FindGameObjectWithTag("UI").GetComponent<UI_Weapon>();
        playerMove = GetComponent<PlayerMove>();

        weaponData = GameObject.FindGameObjectWithTag("Data").GetComponent<PlayerWeaponData>();
        dataUI = weaponData.gameObject.GetComponent<Data_UI>();

        CheckWeaponUI();
        WeaponChangeRequested(2);

        for (int i = 0; i < playersWeapons.Length; i++)
        {
            if(playersWeapons[i] != null)
            {
                int weaponID = playersWeaponIDs[i];
                WeaponStatus weaponStat = playersWeapons[i].GetComponent<WeaponStatus>();
                int weaponType = (int)weaponData.GetWeaponStatSO(weaponID).w_type;
                Sprite _weaponIcon = dataUI.GetImageByWeaponType(weaponType, weaponStat.weaponIcon);

                weaponUI.SetImageIcon(i, _weaponIcon);
            }
            else
            {
                continue;
            }
        }
        
    }

    public WeaponStatus GetWeaponStatCur()
    {
        return playersWeapons[weaponCur].GetComponent<WeaponStatus>();
    }


    public AudioSource GetPlayerAudioSource()
    {
        return playerSound;
    }

    public void WeaponChangeRequested(int weaponSlot)
    {
        if (playersWeapons[weaponSlot] != null)
        {
            weaponCur = weaponSlot;
            WeaponReady(weaponSlot);
        }
        else
        {

            /*
            if (playersWeapons[weaponCur] != null)
            {
                playersWeapons[weaponCur].SetActive(false);
            }

            //Debug.Log("소지한 무기가 없습니다.");
            GetComponentInChildren<Animator>().SetTrigger("NoneDraw");
            GetComponentInChildren<PlayerAnimatorLook>().GunAnimationReady(false);
            weaponCur = weaponSlot;
            */
        }



    }


    public void GetWeapon(int weaponID, int ammoCur, int magCur, int pickCount, List<int> list, int quality)
    {
        WeaponParamsSO weaponSO = weaponData.GetWeaponStatSO(weaponID);
        if (weaponSO.w_usingAmmo == WeaponParamsSO.Ammos.Default)
        {
            weaponCur = 2;
        }
        else if (playersWeapons[0] == null)
        {
            weaponCur = 0;
        }
        else if (playersWeapons[1] == null)
        {
            weaponCur = 1;
            isSlotFull = true;
        }


        if (playersWeapons[weaponCur] != null)
        {
            WeaponThrow(weaponCur);
        }


        playersWeaponIDs[weaponCur] = weaponID;

        GameObject getWeapon = Instantiate(weaponData.GetpWeaponPrefab(weaponID));
        playersWeapons[weaponCur] = getWeapon;
        
        
        PlayerWeapon weaponSystem = playersWeapons[weaponCur].GetComponent<PlayerWeapon>();
        weaponSystem.magCur = magCur;

        WeaponStatus weaponStat = playersWeapons[weaponCur].GetComponent<WeaponStatus>();
        weaponStat.quality = quality;
        weaponStat.weaponDataSource = weaponSO;

        int weaponType = (int)weaponData.GetWeaponStatSO(weaponID).w_type;
        int weaponAniType = weaponStat.SetWeaponAniType(weaponType);


        playersWeapons[weaponCur].transform.SetParent(gunPointTr[weaponAniType], false);

        weaponStat.SetAmmoCurrent(ammoCur, pickCount);

        playersWeapons[weaponCur].GetComponent<WeaponStatUpgrade>().ApplyUpgradeByWeaponEquip(list);

        Sprite _weaponIcon = dataUI.GetImageByWeaponType(weaponType, weaponStat.weaponIcon);

        CheckWeaponUI();
        weaponUI.SetImageIcon(weaponCur, _weaponIcon);

        playerSound.PlayOneShot(sound_weaponGet);
        UsingWeaponCheck();
        WeaponReady(weaponCur);
        CheckAmmoFull();
    }

    void CheckWeaponUI()
    {
        if (weaponUI == null)
        {
            weaponUI = GameObject.FindGameObjectWithTag("UI").GetComponent<UI_Weapon>();
        }
        if(playerInteract == null)
        {
            playerInteract = GetComponentInChildren<PlayerInteract>();
        }
    }

    public void WeaponReady(int slot)
    {
        CheckWeaponUI();


        for (int i = 0; i < playersWeapons.Length; i++)
        {
            if (playersWeapons[i] != null)
            {
                bool isOn = i == slot;
                playersWeapons[i].SetActive(isOn);
                weaponUI.SetWeaponSelectedUI(i, isOn);
                playerSound.PlayOneShot(sound_weaponChange);
                playerInteract.RestoreOriginal(playersWeapons[i]);
            }
                
        }

    }


    public void WeaponThrow(int slot)
    {
        GameObject weaponObj = playersWeapons[slot];
        if (weaponObj == null) return;

        PlayerWeapon weaponStat = weaponObj.GetComponent<PlayerWeapon>();
        if (weaponStat == null) return;

        WeaponStatUpgrade weaponUpgradeStat = weaponObj.GetComponent<WeaponStatUpgrade>();

        int mag = weaponStat.magCur;
        int ammo = weaponStat.GetAmmoCur();

        float throwDir = playerMove.isLookingRight ? 5f : -5f;

        GameObject prop = Instantiate(weaponProp, transform.position + Vector3.up, Quaternion.identity);

        // Rigidbody가 있으면 던지기 속도 부여
        var rb = prop.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = new Vector3(throwDir, 5f, 0f); // 너의 프로젝트 표준을 유지

        Item_Weapon propData = prop.GetComponent<Item_Weapon>();
        if (propData == null) { Destroy(prop); return; }

        // 1) 업그레이드 리스트는 "복사본"으로 한 번만 전달
        List<int> upgradeCopy = (weaponUpgradeStat != null && weaponUpgradeStat.upgradesCur != null)
            ? new List<int>(weaponUpgradeStat.upgradesCur)
            : new List<int>();
        propData.UpgradeSet(upgradeCopy, weaponStat.quality);

        // 2) ID/탄약/픽카운트 등 나머지 정보 세팅
        propData.WeaponDropItem(playersWeaponIDs[slot]);
        propData.ammoSet(ammo, mag, weaponStat.pickCount);

        // 3) 기존 무기 제거 + 슬롯 비우기
        Destroy(weaponObj);
        playersWeapons[slot] = null;
        playersWeaponIDs[slot] = 0;
    }


    public void AmmoGet(WeaponParamsSO.Ammos ammoType)
    {
        float ammoGainStat = GetComponent<PlayerShootingStat>().playerAmmoGain;
        float getAmmoValue = Random.Range(0.12f, 0.24f) * ammoGainStat;

        for (int i = 0; i < playersWeapons.Length; i++)
        {
            if (playersWeapons[i] == null) {  continue; }

            WeaponStatus weaponStat = playersWeapons[i].GetComponent<WeaponStatus>();
            if(weaponStat.GetUsingAmmo() != ammoType) { continue; }

            weaponStat.ammoCur = Mathf.Clamp(weaponStat.ammoCur + Mathf.RoundToInt(weaponStat.ammoMax * getAmmoValue), 1, weaponStat.ammoMax);

        }
        playerSound.PlayOneShot(sound_ammoGet);
        CheckAmmoFull();
    }

    void UsingWeaponCheck()
    {
        isAmmoTypeUsing[0] = false;
        isAmmoTypeUsing[1] = false;

        for (int i = 0; i < playersWeapons.Length; i++)
        {
            if (playersWeapons[i] != null)
            {
                WeaponStatus weaponStat = playersWeapons[i].GetComponent<WeaponStatus>();
                WeaponParamsSO.Ammos type = weaponStat.GetUsingAmmo();
                //WeaponParamsSO.Ammos type = weaponData.GetWeaponStatSO(playersWeaponIDs[i]).w_usingAmmo;

                    switch (type)
                    {
                        case WeaponParamsSO.Ammos.Infantry:
                            isAmmoTypeUsing[0] = true;
                            break;
                        case WeaponParamsSO.Ammos.Armor:
                            isAmmoTypeUsing[1] = true;
                            break;
                        default:
                            continue;
                    }
            }
        }
    }

    public void CheckAmmoFull()
    {
        // 기본값 true (해당 탄약 타입 무기가 없으면 그대로 true 유지됨)
        isAmmoFullbyType[0] = true;
        isAmmoFullbyType[1] = true;

        for (int i = 0; i < playersWeapons.Length; i++)
        {
            if (playersWeapons[i] == null) continue;

            WeaponStatus weaponStat = playersWeapons[i].GetComponent<WeaponStatus>();
            int ammoType = 0;

            switch (weaponStat.GetUsingAmmo())
            {
                case WeaponParamsSO.Ammos.Infantry:
                    ammoType = 0;
                    break;
                case WeaponParamsSO.Ammos.Armor:
                    ammoType = 1;
                    break;
                default:
                    continue; // 다른 탄약 타입은 패스
            }

            // 하나라도 부족하면 false
            if (weaponStat.ammoCur < weaponStat.ammoMax)
            {
                isAmmoFullbyType[ammoType] = false;
            }
        }
    }

    public void ApplyUnitUpgrade()
    {
        for (int i = 0; i < playersWeapons.Length; i++)
        {
            if (playersWeapons[i] != null)
            {
                playersWeapons[i].GetComponent<WeaponStatus>().ApplyStatusInSystem();
            }
        }
    }

    public void UIRefresh()
    {
        CheckWeaponUI();

        for(int i = 0; i < playersWeapons.Length; i++)
        {
            if (playersWeapons[i] != null)
            {
                WeaponStatus weaponStat = playersWeapons[i].GetComponent<WeaponStatus>();

                int weaponType = (int)weaponData.GetWeaponStatSO(playersWeaponIDs[i]).w_type;

                Sprite _weaponIcon = dataUI.GetImageByWeaponType(weaponType, weaponStat.weaponIcon);

                weaponUI.SetImageIcon(weaponCur, _weaponIcon);
            }
            else
            {
                weaponUI.SetImageIcon(i, null);
            }
        }

    }

}
