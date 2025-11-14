using UnityEngine;

public class Stage_EnemySpawnPoint : MonoBehaviour
{
    public int mobCode;
    public bool isLookingRight = false;
    public bool isPatrol = false;
    public bool isNotMoving = false;

    GameObject idleEftObj;
    ParticleSystem idleEft;
    Transform tr;


    private void Start()
    {
        idleEftObj = GetComponentInParent<StageModule>().GetIdleParticleObj();

        if(idleEftObj != null)
        {
            TransformCheck();
            idleEftObj = Instantiate(idleEftObj, tr);
            idleEft = idleEftObj.GetComponent<ParticleSystem>();
        }
    }

    void TransformCheck()
    {
        if(tr == null)
        {
            tr = transform;
        }
    }

    public void SpawnEftPlay(GameObject obj)
    {
        if (obj != null)
        {
            TransformCheck();

            Instantiate(obj, tr);

            obj.GetComponent<ParticleSystem>().Play(true);
            if(idleEft != null)
            {
                idleEft.Stop(true);
            }
        }
    }

}
