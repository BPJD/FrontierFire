using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] GameObject playerArms;
    [SerializeField] PlayerAnimatorLook animatorLookSystem;
    [SerializeField] PlayerMove moveSystem;
    [SerializeField] PlayerLookMouse lookMouseSystem;
    [SerializeField] PlayerWeaponController weaponControllerSystem;
    [SerializeField] Animator playerAni;


    private void OnEnable()
    {
        playerArms.SetActive(false);
        animatorLookSystem.enabled = false;
        moveSystem.enabled = false;
        lookMouseSystem.enabled = false;
        weaponControllerSystem.enabled = false;

    }

    public void DeathAnimationPlay(int _hp, int _damage)
    {
        if(_damage >= (int)_hp * 0.2f)
        {
            playerAni.SetTrigger("Death_Explosive");
        }
        else if(_damage >= (int)_hp * 0.1f)
        {
            playerAni.SetTrigger("Death_High");
        }
        else if(_damage >= (int)_hp * 0.5f)
        {
            playerAni.SetTrigger("Death_Med");
        }
        else
        {
            playerAni.SetTrigger("Death_Low");
        }
    }
}
