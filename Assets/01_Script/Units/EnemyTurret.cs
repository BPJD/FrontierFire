using System.Collections;
using UnityEngine;

public class EnemyTurret : MonoBehaviour
{

    [SerializeField] GameObject[] dropTable;
    [Range(0f, 1f)]
    [SerializeField] float armorAmmoWeight = 0.5f;

    [Range(0f, 100f)]
    [SerializeField] float ammoDropRate = 5f;

    [Range(0f, 100f)]
    [SerializeField] float itemDropRate = 5f;

    [SerializeField] Transform turretTr;
    Rigidbody turretRb;

    [SerializeField] ParticleSystem deathExplosion;


    [Range(0f, 2f)]
    [SerializeField] float deathAnimationPower = 1f;

    [SerializeField] bool isDrone = false;
    bool isDead = false;
    bool isEftPlayed = false;

    public bool isGravityReverse = false;

    private void Start()
    {
        itemDropRate = GetComponent<TurretAttackSystem>().unitAIDataSource.ai_dropRate;

        if (isGravityReverse)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 180f);
        }

    }

    private void Update()
    {
        if (isGravityReverse)
        {
            GetComponent<Rigidbody>().linearVelocity = Vector3.up * 3f;
        }
    }

    public void TurretDown()
    {
        ItemDrop();
        GetComponentInParent<StageModule>()?.EnemyCountDown();
        DeathAnimationPlay();
    }

    void DroneDeath()
    {
        turretRb = GetComponent<Rigidbody>();
        turretRb.useGravity = true;

        float _randX = Random.Range(-120f, 120f);
        float _randY = Random.Range(150f, 250f);
        float _randZ = Random.Range(30f, 50f);

        if(isGravityReverse)
        {
            _randY = Random.Range(-50f, -100f);
        }

        Vector3 _randVector = new Vector3(_randX, _randY, _randZ) * deathAnimationPower;

        turretRb.AddForce(_randVector);
    }

    void TurretDeath()
    {
        DeathExplode();
        turretTr.parent = null;

        turretTr.gameObject.AddComponent<Rigidbody>();
        turretRb = turretTr.gameObject.GetComponent<Rigidbody>();

        float _randX = Random.Range(-4f, 4f);
        float _randY = Random.Range(8f, 12f);
        float _randZ = Random.Range(1f, 3f);

        Vector3 _randVector = new Vector3(_randX, _randY, _randZ) * deathAnimationPower;

        turretRb.linearVelocity = _randVector;
        turretRb.angularVelocity = _randVector * 1.5f;
    }

    void DeathAnimationPlay()
    {
        isDead = true;
        switch (isDrone)
        {
            case true:
                DroneDeath();
                break;
                case false:
                TurretDeath();
                break;
        }
    }

    void ItemDrop()
    {
        Data_DropAmmo data = GameObject.FindGameObjectWithTag("Data").GetComponent<Data_DropAmmo>();
        PlayerShootingStat playerAmmoStat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerShootingStat>();

        float ammoValue = Random.Range(0f, 100f);
        float ammoDropRateFinal = Mathf.Clamp((ammoDropRate * data.GameDropRate) + playerAmmoStat.playerItemDropRate, 0f, 100f);

        if (ammoValue <= ammoDropRateFinal)
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
            float itemdropRateFinal = Mathf.Clamp((itemDropRate * data.GameDropRate) + playerAmmoStat.playerItemDropRate, 0f, 100f);

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

    private void OnCollisionEnter(Collision collision)
    {
        if(isDead && isDrone && collision.gameObject.CompareTag(Data_Strings.terrainTag) || collision.gameObject.CompareTag("Untagged"))
        {
            DeathExplode();
        }
    }


    void DeathExplode()
    {
        if (!isEftPlayed)
        {
            Destroy(Instantiate(deathExplosion, transform.position, turretTr.rotation), 5f);
            if (isDrone)
            {
                GetComponent<MeshRenderer>().enabled = false;
                turretRb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                turretTr.tag = Data_Strings.DeadUnitTag;
                turretTr.gameObject.layer = 10;
            }
                isEftPlayed = true;
        }
        

    }


}
