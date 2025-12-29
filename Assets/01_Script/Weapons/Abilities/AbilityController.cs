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
    [SerializeField] int[] abilityStacks;

    Transform tr;
    int ballCount = 0;
    [SerializeField] float ballRadius = 1.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
        abilityStacks = new int[abilityObjs.Length];
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
        abilityStacks[id] += 1;

        switch (id)
        {
            case 0:
                GameObject _drone = Instantiate(abilityObjs[id], tr);

                droneAtkSystems.Add(_drone.GetComponentInChildren<ObjectPool_PlayerDrone>());
                droneMoveSystems.Add(_drone.GetComponent<Ability_AttackDroneMove>());
                break;
            case 1:
                PlayerGetTurningBall();
                break;
            case 2:
                Instantiate(abilityObjs[id], tr);
                break;
            case 3:
                shieldManager.ShieldUpgrade();
                break;
            case 4:
                Instantiate(abilityObjs[id], tr);
                break;
            case 5:
                dashManager.DashLevelUp();
                break;
            case 6:
                Instantiate(abilityObjs[id], tr);
                break;
            case 7:
                Instantiate(abilityObjs[id], tr);
                break;
            case 8:
                Instantiate(abilityObjs[id], tr);
                break;
            case 9:
                Instantiate(abilityObjs[id], tr);
                break;
            case 10:
                Instantiate(abilityObjs[id], tr);
                break;
            default:
                break;

        }
    }

    void CheckItemInAbilityAndUpgrade(int id)
    {

    }



    


    
}
