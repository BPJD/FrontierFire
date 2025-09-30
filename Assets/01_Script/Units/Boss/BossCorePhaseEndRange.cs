using UnityEngine;
using System.Collections;

public class BossCorePhaseEndRange : MonoBehaviour
{
    [SerializeField] Rigidbody playerRb;

    [SerializeField] Vector3 throwVelocity = new Vector3(-20f, 10f, 0f);

    [SerializeField] SphereCollider bossCol;
    [SerializeField] BoxCollider crystalCol;
    
    
    UnitStatus crystalStat;
    MeshRenderer crystalMesh;

    private void Start()
    {
        crystalStat = crystalCol.gameObject.GetComponent<UnitStatus>();
        crystalMesh = crystalCol.gameObject.GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.gameObject.GetComponent<Rigidbody>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = null;
        }
    }

    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("플레이어 사출");
            PlayerThrow();
        }
    }
    */



    public void PlayerThrow()
    {
        if (playerRb != null)
        {
            StartCoroutine(ColliderReset());
        }
    }

    IEnumerator ColliderReset()
    {
        bossCol.enabled = false;
        crystalCol.enabled = false;

        yield return new WaitForSeconds(0.01f);
        playerRb.linearVelocity = throwVelocity;

        yield return new WaitForSeconds(1.5f);

        crystalStat.HP_Reset();
        crystalMesh.enabled = true;
        bossCol.enabled = true;
        crystalCol.enabled = true;
    }


}
