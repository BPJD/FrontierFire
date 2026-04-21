using UnityEngine;
using System.Collections;

public class BossCoreDealPhase : MonoBehaviour
{
    public UnitStatus bossStat;
    BossCoreAttackControl attackControl;
    BossCoreCrystalAttack crystalAttack;

    [SerializeField] BossCorePhaseEnd endPhase;

    [SerializeField] ParticleSystem shieldEft;
    [SerializeField] ParticleSystem shieldBreakEft;
    [SerializeField] ParticleSystem crystalBreakEft;
    [SerializeField] ParticleSystem crystalGenEft;

    MeshRenderer mesh;
    BoxCollider col;
    Transform tr;





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attackControl = bossStat.gameObject.GetComponent<BossCoreAttackControl>();
        mesh = GetComponent<MeshRenderer>();
        col = GetComponent<BoxCollider>();
        tr = transform;
        crystalAttack = GetComponent<BossCoreCrystalAttack>();
    }

    // Update is called once per frame
    void Update()
    {
        tr.Rotate(Vector3.up * 10f * Time.deltaTime);
    }

    public void NeutralUnitDead()
    {
        crystalBreakEft.Play(true);
        crystalAttack.CrystalAttackReady(false);


        StartCoroutine(DealPhase());
    }


    void DealPhaseOn(bool isOn)
    {
        if (bossStat != null)
        {
            if (isOn)
            {
                mesh.enabled = false;
                col.enabled = false;
                shieldEft.Stop(true);
                shieldBreakEft.Play(true);
                attackControl.isAttackReady = true;
                bossStat.immunePer = 1.5f;
            }
            else
            {
                gameObject.layer = 15;
                gameObject.tag = "Unit";
                shieldEft.Play(true);
                attackControl.isAttackReady = false;
                crystalAttack.CrystalAttackReady(true);
                bossStat.immunePer = 0.05f;
                endPhase.EndPhaseShot();
            }
        }
        
    }

    IEnumerator DealPhase()
    {
        DealPhaseOn(true);

        yield return new WaitForSeconds(20f);

        DealPhaseOn(false);
    }

}
