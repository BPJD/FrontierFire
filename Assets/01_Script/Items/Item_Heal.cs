using UnityEngine;

public class Item_HealPack : MonoBehaviour
{
    [SerializeField] float healHP_percent = 0.05f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Data_Strings.playerTag))
        {
            UnitStatus playerStatus = other.gameObject.GetComponent<UnitStatus>();
            int _healHP = Mathf.RoundToInt(playerStatus.unitParams.u_hp * healHP_percent);

            playerStatus.GetComponent<UnitStatus>().UnitGetHeal(_healHP, true);

            Destroy(this.gameObject);

        }
    }
}
