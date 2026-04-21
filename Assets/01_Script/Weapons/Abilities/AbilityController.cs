using UnityEngine;
using System.Collections.Generic;

public class AbilityController : MonoBehaviour
{

    public List<ObjectPool_PlayerDrone> droneAtkSystems = new List<ObjectPool_PlayerDrone>();
    public List<Ability_AttackDroneMove> droneMoveSystems = new List<Ability_AttackDroneMove>();

    [SerializeField] PlayerDashManager dashManager;
    [SerializeField] ShieldManager shieldManager;
    List<Transform> ballTrs = new List<Transform>();

    [SerializeField] Transform ballParentTr;
    [SerializeField] GameObject[] abilityObjs;
    private GameObject[] abilityInstances;

    [SerializeField] int[] abilityStacks;

    Transform tr;
    int ballCount = 0;
    [SerializeField] float ballRadius = 1.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
        abilityStacks = new int[abilityObjs.Length];
        abilityInstances = new GameObject[abilityObjs.Length];
    }

    public void PlayerWeaponChanged()
    {
        for (int i = 0; i < droneAtkSystems.Count; i++)
        {
            droneAtkSystems[i].SetWeaponStat();
        }
    }

    public void PlayerGetTurningBall()
    {
        if(ballCount == 0)
        {
            GameObject _addBall = Instantiate(abilityObjs[1], ballParentTr);
            ballTrs.Add(_addBall.transform);

            _addBall = Instantiate(abilityObjs[1], ballParentTr);
            ballTrs.Add(_addBall.transform);
        }

        GameObject _ball = Instantiate(abilityObjs[1], ballParentTr);
        ballTrs.Add(_ball.transform);
        ballCount++;

        ResetBallPosition();
    }

    void ResetBallPosition()
    {
        int n = ballTrs.Count;
        if (n == 0) return;

        float step = Mathf.PI * 2f / n;

        for (int i = 0; i < n; i++)
        {
            float t = step * i;
            Vector3 pos = new Vector3(
                Mathf.Cos(t),
                Mathf.Sin(t),
                0f
            ) * ballRadius;

            ballTrs[i].localPosition = pos;
            ballTrs[i].localRotation = Quaternion.identity; // 개별 회전은 의미 없음
        }
    }


    public void PlayerWeaponShooted()
    {
        for (int i = 0; i < droneAtkSystems.Count; i++)
        {
            droneAtkSystems[i].DroneWeaponShoot();
        }
    }

    public void PlayerTurned(bool isLeft)
    {
        for (int i = 0; i < droneAtkSystems.Count; i++)
        {
            droneMoveSystems[i].PlayerTurned(isLeft);
        }
    }

    public void PlayerGetItem(int id)
    {
        switch (id)
        {
            case 0:
                abilityStacks[id]++;

                GameObject _drone = Instantiate(abilityObjs[id], tr);

                droneAtkSystems.Add(_drone.GetComponentInChildren<ObjectPool_PlayerDrone>());
                droneMoveSystems.Add(_drone.GetComponent<Ability_AttackDroneMove>());
                break;

            case 1:
                abilityStacks[id]++;
                PlayerGetTurningBall();
                break;

            case 3:
                abilityStacks[id]++;
                shieldManager.ShieldUpgrade();
                break;

            case 5:
                abilityStacks[id]++;
                dashManager.DashLevelUp();
                break;

            default:
                CheckItemInAbilityAndUpgrade(id);
                break;
        }
    }

    void CheckItemInAbilityAndUpgrade(int id)
    {
        // 아직 능력이 없으면 생성
        if (abilityInstances[id] == null)
        {
            GameObject abilityObj = Instantiate(abilityObjs[id], tr);
            abilityInstances[id] = abilityObj;
            abilityStacks[id] = 1;
            return;
        }

        // 이미 있으면 업그레이드
        IAbilityUpgradable upgradable = abilityInstances[id].GetComponent<IAbilityUpgradable>();

        if (upgradable != null)
        {
            upgradable.UpgradeAbility();
            abilityStacks[id]++;
        }
        else
        {
            //Debug.LogWarning($"Ability ID {id} 오브젝트에 IAbilityUpgradable 구현이 없음");
        }
    }







}
