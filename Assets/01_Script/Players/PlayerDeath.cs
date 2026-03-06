using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] GameObject playerArms;
    [SerializeField] PlayerAnimatorLook animatorLookSystem;
    [SerializeField] PlayerMove moveSystem;
    [SerializeField] PlayerLookMouse lookMouseSystem;
    [SerializeField] PlayerWeaponController weaponControllerSystem;
    [SerializeField] PlayerInputController inputSystem;
    [SerializeField] Animator playerAni;
    Direction_SceneChanger sceneChanger;

    private void OnEnable()
    {
        playerArms.SetActive(false);
        animatorLookSystem.enabled = false;
        moveSystem.enabled = false;
        lookMouseSystem.enabled = false;
        weaponControllerSystem.enabled = false;
        inputSystem.PlayerDead();

        GameObject.FindGameObjectWithTag("Module").GetComponent<Direction_GameOver>().PlayerDead();
        sceneChanger = GameObject.FindGameObjectWithTag("GameController").GetComponent<Direction_SceneChanger>();
        sceneChanger.player = gameObject;
    }

    public void DeathAnimationPlay(int _hp, int _damage)
    {
        if(_damage >= (int)_hp * 0.3f)
        {
            playerAni.SetTrigger("Death_Explosive");
        }
        else if(_damage >= (int)_hp * 0.15f)
        {
            playerAni.SetTrigger("Death_High");
        }
        else if(_damage >= (int)_hp * 0.05f)
        {
            playerAni.SetTrigger("Death_Med");
        }
        else
        {
            playerAni.SetTrigger("Death_Low");
        }
    }

}
