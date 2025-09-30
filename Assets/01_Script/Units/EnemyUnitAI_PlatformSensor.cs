using Unity.VisualScripting;
using UnityEngine;

public class EnemyUnitAI_PlatformSensor : MonoBehaviour
{
    EnemyUnitMove moveCon;
    Transform thisTr;
    EnemyUnitAI_Controller aniCon;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveCon = GetComponentInParent<EnemyUnitMove>();
        thisTr = moveCon.gameObject.transform;
        aniCon = GetComponentInParent<EnemyUnitAI_Controller>();
    }


    private void OnTriggerExit(Collider other)
    {
        if(aniCon.state == EnemyUnitAI_Controller.UnitState.Patrol)
        {
            if (moveCon.isMoveForward)
            {
                thisTr.rotation = Quaternion.LookRotation(Vector3.right);
                moveCon.isMoveForward = false;
            }
            else
            {
                thisTr.rotation = Quaternion.LookRotation(Vector3.left);
                moveCon.isMoveForward = true;
            }
        }

    }

}
