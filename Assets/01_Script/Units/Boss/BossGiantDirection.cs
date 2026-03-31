using UnityEngine;
using System.Collections;

public class BossGiantDirection : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;

    [SerializeField] GameObject directionUnit;
    [SerializeField] GameObject bossUnit;
    [SerializeField] Vector3 camOffset = Vector3.down;

    [SerializeField] Transform camPoint;

    GameObject gameController;

    bool isAnimated = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        gameController = GameObject.FindGameObjectWithTag("GameController");
        Direction_BossStage bossStage = gameController.GetComponent<Direction_BossStage>();
        bossStage.BossStageEntry(directionUnit, bossUnit, camOffset, camPoint);

        StartCoroutine(Direction());

    }


    IEnumerator Direction()
    {
        yield return new WaitForSeconds(2f);
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;



    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain") && !isAnimated)
        {
            animator.SetTrigger("Land");
            isAnimated = true;
            gameController.GetComponent<Direction_Camera>().Direction_Shake(3f, 2f);
        }
    }
}
