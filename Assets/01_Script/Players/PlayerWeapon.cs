using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] Transform fireTr;
    ObjectPool bulletPool;
    Animator playerAnimator;
    PlayerLookMouse playerPointer;
    PlayerShootingStat shootingStat;
    public CameraMovingSystem camRange { get; private set; }
    PlayerAnimatorLook gunAnimator;
    PlayerWeaponController weaponController;
    PlayerInputController inputController;

    public float fireRate = 0.5f;
    float fireRateCur = 0f;
    bool isReloading = false;

    [SerializeField] Transform LhandIK;
    [SerializeField] Transform RhandIK;
    Transform tr;
    Gun_Parent_Data parData;
    Data_BulletPrafabs shootingDatas;
    public GameObject laserScope { get; private set; }

    
    string aniShootStr = "Shoot";
    string aniReloadStr = "Reload";

    WeaponStatus weaponStat;

    string[] weaponDrawAniStrs = {"PistolDraw", "ARDraw", "SRDraw", "RocketDraw"};

    UI_Weapon uiWeapon;
    public int magCur;
    public int magMax;
    public int ammoCur;
    public int pickCount;

    public int bulletCount = 1;

    public float bullet_angleError = 3f;

    float reloadSpeedCur = 0f;

    public bool isDefaultWeapon = false;

    bool isShooting = false;


    ParticleSystem eft_Muzzle;

    WeaponSoundPlay soundPlayer;

    private void Awake()
    {
        if (GetComponentInParent<PlayerMove>() == null)
        {
            this.enabled = false;
        }

        camRange = GameObject.Find("CameraAimPoint").GetComponent<CameraMovingSystem>();
        weaponStat = GetComponent<WeaponStatus>();
        bulletPool = GetComponent<ObjectPool>();
        playerAnimator = GetComponentInParent<Animator>();
        playerPointer = GetComponentInParent<PlayerLookMouse>();
        shootingStat = GetComponentInParent<PlayerShootingStat>();
        gunAnimator = GetComponentInParent<PlayerAnimatorLook>();
        parData = GetComponentInParent<Gun_Parent_Data>();
        uiWeapon = GameObject.Find("UI").GetComponent<UI_Weapon>();
        eft_Muzzle = GetComponentInChildren<ParticleSystem>();
        tr = transform;
        reloadSpeedCur = weaponStat.reloadSpeed;
        weaponController = GetComponentInParent<PlayerWeaponController>();
        inputController = GetComponentInParent<PlayerInputController>();
        soundPlayer = GetComponent<WeaponSoundPlay>();
        shootingDatas = GameObject.FindGameObjectWithTag("Data").GetComponent<Data_BulletPrafabs>();
        
    }

    void OnEnable()
    {
        playerAnimator.Rebind();
        gunAnimator.IKPositionSet(LhandIK, RhandIK);
        gunAnimator.GunPositionSet(parData.shoulder, parData.gunPos, parData.gunPar);
        gunAnimator.GunReload(false);
        isReloading = false;

        playerAnimator.SetTrigger(weaponDrawAniStrs[weaponStat.animationType]);
        camRange.CamControlSet(weaponStat._isCamRangeUp);

        foreach (var behaviour in playerAnimator.GetBehaviours<ReloadStateBehaviour>())
        {
            behaviour.OnReloadComplete = OnReloadComplete;
        }

        bulletCount = weaponStat.GetWeaponType() == WeaponParamsSO.WeaponTypes.Shotgun ? 8 : 1;
        inputController.Requested_WeaponReady(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (isShooting && fireRateCur < 0f && !isReloading)
        {
            Shoot();
            fireRateCur = fireRate;
        }
        /*
        if (Input.GetButton("Fire1") && fireRateCur < 0f && !isReloading)
        {
            Shoot();
            fireRateCur = fireRate;

        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            Reload();
        }
        */

        fireRateCur -= Time.deltaTime;


        MagUISet();
    }

    private void Start()
    {
        gunAnimator.IKPositionSet(LhandIK, RhandIK);
        gunAnimator.GunPositionSet(parData.shoulder, parData.gunPos, parData.gunPar);
        playerAnimator.SetTrigger(weaponDrawAniStrs[weaponStat.animationType]);
        //AmmoLoad();
    }

    public void Input_Shoot(bool isShoot)
    {
        isShooting = isShoot;
    }

    public void Input_Reload()
    {
        if (!isReloading)
        {
            Reload();
        }
    }

    void Shoot()
    {
        if(magCur > 0)
        {
            if (!playerPointer.isAimClose)
            {
                float _distance = (fireTr.position - playerPointer.targetPos).sqrMagnitude;
                playerAnimator.Play(aniShootStr, -1, 0f);

                if(bulletCount > 1)
                {
                    Shoot_ShotGun();
                }
                else
                {
                    Shoot_Normal();
                }

                soundPlayer.PlaySoundFire();
                eft_Muzzle.Play(true);

                magCur--;
            }
        }
        else
        {
            if(weaponStat.ammoCur > 0)
            {
                Reload();
            }
            
        }
    }

    void Shoot_Normal()
    {
        GameObject bullet = bulletPool.GetObject();
        //bullet.GetComponent<Bullet>().SetBulletStatus(weaponStat.bulletAtk, weaponStat.bulletRange, 0f);
        Transform bulletTr = bullet.transform;
        bulletTr.position = new Vector3(fireTr.position.x, fireTr.position.y, 0f);

        // 방향 설정
        //Vector3 direction = (playerPointer.targetPos - bulletTr.position).normalized;
        Vector3 direction = (playerPointer.targetPos - tr.position).normalized;
        Quaternion baseRotation = Quaternion.LookRotation(direction);

        // X축 회전 수정
        float _angleError = Random.Range(-bullet_angleError, bullet_angleError);
        Vector3 eulerAngles = baseRotation.eulerAngles;
        eulerAngles.x += Random.Range(-_angleError, _angleError); // X축에 _accuracy 값 추가
        bulletTr.rotation = Quaternion.Euler(eulerAngles);
    }

    void Shoot_ShotGun()
    {
        for (int i = 0; i < bulletCount; i++)
        {
            GameObject bullet = bulletPool.GetObject();
            //bullet.GetComponent<Bullet>().SetBulletStatus(weaponStat.bulletAtk, weaponStat.bulletRange, 0f);
            Transform bulletTr = bullet.transform;
            bulletTr.position = new Vector3(fireTr.position.x, fireTr.position.y, 0f);

            // 방향 설정
            //Vector3 direction = (playerPointer.targetPos - bulletTr.position).normalized;
            Vector3 direction = (playerPointer.targetPos - tr.position).normalized;
            Quaternion baseRotation = Quaternion.LookRotation(direction);

            // X축 회전 수정
            float _angleError = Random.Range(-bullet_angleError, bullet_angleError) * 3f;
            Vector3 eulerAngles = baseRotation.eulerAngles;
            eulerAngles.x += Random.Range(-_angleError, _angleError); // X축에 _accuracy 값 추가
            bulletTr.rotation = Quaternion.Euler(eulerAngles);
        }
    }

    public void Reload()
    {
        if(magMax > magCur && weaponStat.ammoCur > 0)
        {
            isReloading = true;
            // 1. 재장전 속도 값 갱신

            reloadSpeedCur = weaponStat.reloadSpeed;

            float _aniSpeed = shootingStat.reloadAniClips[weaponStat.animationType].length / reloadSpeedCur;
            playerAnimator.SetFloat("ReloadSpeed", _aniSpeed);

            // 2. 트리거로 애니메이션 시작
            playerAnimator.SetTrigger(aniReloadStr);

            // 3. 총기 애니메이션 핸들링 (별도 처리)
            gunAnimator.GunReload(true);
            soundPlayer.PlaySoundReload(true);


            
        }
    }

    void OnReloadComplete()
    {
        soundPlayer.PlaySoundReload(false);

        StartCoroutine(AmmoLoad());
    }

    IEnumerator AmmoLoad()
    {
        yield return new WaitForSeconds(0.3f);

        gunAnimator.GunReload(false);
        isReloading = false;

        if (magCur >= magMax || weaponStat.ammoCur <= 0)
        {
            //Debug.Log("재장전 불가능");
        }
        else
        {
            int neededAmmo = magMax - magCur; // 채워야 할 탄약
            int ammoToLoad = Mathf.Min(neededAmmo, weaponStat.ammoCur); // 실제로 로드할 탄약 수

            magCur += ammoToLoad;
            weaponStat.ammoCur -= ammoToLoad;
            weaponController.CheckAmmoFull();
        }
    }

    void MagUISet()
    {
        if (uiWeapon != null)
        {
            if (isDefaultWeapon)
            {
                uiWeapon.textMesh.text = "999+" + '/' + magCur.ToString();
            }
            else
            {
                uiWeapon.textMesh.text = weaponStat.ammoCur.ToString() + '/' + magCur.ToString();
            }
            
        }
    }

    public int GetAmmoCur()
    {
        return weaponStat.ammoCur;
    }

    public void SetLaserScope(int code)
    {

        GameObject _newScope = shootingDatas.GetLaserScopePrefab(code);

        if(laserScope != null)
        {
            Destroy(laserScope);
        }

        if(_newScope != null)
        {
            laserScope = Instantiate(_newScope, fireTr.transform);
        }
    }

    public void ScopeControl(bool isAiming)
    {
        laserScope.SetActive(isAiming);
    }

}