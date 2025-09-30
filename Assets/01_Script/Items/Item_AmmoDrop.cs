using UnityEngine;

public class Item_AmmoDrop : MonoBehaviour
{
    [SerializeField] WeaponParamsSO.Ammos ammos = WeaponParamsSO.Ammos.Infantry;
    int ammoCode = 0;

    private void Start()
    {
        ammoCode = ammos == WeaponParamsSO.Ammos.Armor ? 1 : 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerWeaponController pWeaponCon = other.gameObject.GetComponent<PlayerWeaponController>();
            

            if(pWeaponCon.isAmmoTypeUsing[ammoCode] == true && !pWeaponCon.isAmmoFullbyType[ammoCode])
            {
                pWeaponCon.AmmoGet(ammos);
                Destroy(gameObject);
            }


        }
    }
}
