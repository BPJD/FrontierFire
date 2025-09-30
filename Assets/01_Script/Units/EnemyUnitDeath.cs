using UnityEngine;

public class EnemyUnitDeath : MonoBehaviour
{

    Animator unitAniCon;

    EnemyUnitAI_Controller controller;
    [SerializeField] EnemyUnitAI_Sensor sensor;
    EnemyUnitMove move;

    [SerializeField] GameObject[] dropTable;
    [Range(0f, 1f)]
    [SerializeField] float armorAmmoWeight = 0.5f;

    [Range(0f, 100f)]
    [SerializeField] float ammoDropRate = 5f;

    [Range(0f, 100f)]
    [SerializeField] float itemDropRate = 5f;

    private void Start()
    {
        unitAniCon = GetComponent<Animator>();

        controller = GetComponent<EnemyUnitAI_Controller>();
        move = GetComponent<EnemyUnitMove>();
    }

    public void DeathAnimationPlay(int _hp, int _damage)
    {
        ItemDrop();

        if (_damage >= (int)_hp * 0.2f)
        {
            unitAniCon.SetTrigger("Death_Explosive");
        }
        else if (_damage >= (int)_hp * 0.1f)
        {
            unitAniCon.SetTrigger("Death_High");
        }
        else if (_damage >= (int)_hp * 0.5f)
        {
            unitAniCon.SetTrigger("Death_Med");
        }
        else
        {
            unitAniCon.SetTrigger("Death_Low");
        }
        controller.state = EnemyUnitAI_Controller.UnitState.Dead;
        sensor.enabled = false;
        move.enabled = false;

        GetComponentInParent<StageModule>().EnemyCountDown();
    }

    void ItemDrop()
    {
        Data_DropAmmo data = GameObject.FindGameObjectWithTag("Data").GetComponent<Data_DropAmmo>();
        float ammoValue = Random.Range(0f, 100f);
        float ammoDropRateFinal = Mathf.Clamp(ammoDropRate * data.GameDropRate, 0f, 100f);

        if(ammoValue <= ammoDropRateFinal)
        {
            GameObject ammo = Instantiate(data.GetAmmoType(armorAmmoWeight), transform.position, Quaternion.identity);

            float angle = Random.Range(-45f, 45f);
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up; // X-Y 평면 기준

            Rigidbody rb = ammo.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float randRange = Random.Range(4f, 10f);
                rb.AddForce(direction.normalized * randRange, ForceMode.Impulse);
            }
        }

        if (dropTable.Length > 0)
        {
            float itemDropvalue = Random.Range(0f, 100f);
            float itemdropRateFinal = Mathf.Clamp(itemDropRate * data.GameDropRate, 0f, 100f);

            if (itemDropvalue <= itemdropRateFinal)
            {
                int randValue = Random.Range(0, dropTable.Length);
                GameObject item = Instantiate(dropTable[randValue], transform.position, Quaternion.identity);
                
                float angle = Random.Range(-45f, 45f);
                Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up; // X-Y 평면 기준

                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float randRange = Random.Range(4f, 10f);
                    rb.AddForce(direction.normalized * randRange, ForceMode.Impulse);
                }
            }
        }
    }
}
