using UnityEngine;
using System.Collections;

public class Direction_BossStage : MonoBehaviour
{
    [SerializeField] Animator bossStageAniCon;
    [SerializeField] CameraMovingSystem cameraMovingSystem;

    [SerializeField] CanvasGroup weaponSlotGroup;

    GameObject directionUnit;
    GameObject bossUnit;

    PlayerInputController playerInput;

    Transform bossTr;

    Vector3 offset = Vector3.zero;



    void ComponentCheck()
    {
        if (playerInput == null)
        {
            GameObject _player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
            playerInput = _player.GetComponent<PlayerInputController>();
        }


    }

    public void BossStageEntry(GameObject direction, GameObject boss, Vector3 camOffset, Transform camPos = null)
    {
        directionUnit = direction;
        bossUnit = boss;
        offset = camOffset;

        if (camPos != null)
        {
            bossTr = camPos;
        }
        else
        {
            bossTr = bossUnit.transform;
        }


            
        StartCoroutine(BossStageEntryCoroutine());

    }


    IEnumerator BossStageEntryCoroutine()
    {
        yield return null;

        ComponentCheck();

        directionUnit.SetActive(true);
        bossUnit.SetActive(false);
        weaponSlotGroup.alpha = 0f;

        bossStageAniCon.SetTrigger("Open");
        playerInput.SetInputLock(true);
        yield return new WaitForSeconds(1f);
        cameraMovingSystem.StartBossDirection(bossTr, offset);

        yield return new WaitForSeconds(4f);

        bossStageAniCon.SetTrigger("Close");
        cameraMovingSystem.EndBossDirection();
        playerInput.SetInputLock(false);
        weaponSlotGroup.alpha = 1f;

        yield return new WaitForSeconds(1f);
        directionUnit.SetActive(false);

        yield return null;
        bossUnit.SetActive(true);

    }
}
