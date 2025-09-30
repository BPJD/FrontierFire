using UnityEngine;

public class EnemyWeapon_Trasnforms : MonoBehaviour
{
    public Transform bulletPoint;
    public Transform rightHandPos;
    public Transform leftHandPos;
    public GameObject bulletObj;

    public enum WeaponTypes
    {
        Knife,
        Sword,
        Pistol,
        Rifle,
        Rocket,
        Grenade
    }

    public WeaponTypes weaponAniType;

    private void Start()
    {
        if(weaponAniType != WeaponTypes.Grenade)
        {
            GetComponentInParent<EnemyUnitAI_WeaponLook>().SetWeaponHandPoint(rightHandPos, leftHandPos);
        }
        
    }
}
