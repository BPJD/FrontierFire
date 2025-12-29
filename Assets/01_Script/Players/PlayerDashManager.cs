using UnityEngine;
using System.Collections;

public class PlayerDashManager : MonoBehaviour
{
    Transform tr;
    PlayerLookMouse playerLook;
    
    [SerializeField] PlayerMove_Dash[] dashSkills;
    [SerializeField] int dashLevel = 0;

    [SerializeField] float dashCoolDown = 3f;
    float cooldownCur = 3f;
    bool isDashUsable = true;

    static float cooldownTick = 0.1f;
    WaitForSeconds cooldown = new WaitForSeconds(cooldownTick);

    private void Start()
    {
        playerLook = GetComponentInParent<PlayerLookMouse>();
        tr = transform;

        SetSkill();
    }

    public void DashLevelUp()
    {
        dashLevel = Mathf.Clamp(dashLevel + 1, 0, dashSkills.Length - 1);
        
        SetSkill();
    }

    public void DashActive()
    {
        if (isDashUsable)
        {
            if(playerLook == null)
            {
                playerLook = GetComponentInParent<PlayerLookMouse>();
            }
            dashSkills[dashLevel].TryDash(playerLook.targetPos - tr.position);
            StartCoroutine(CoolDown());
        }
    }

    void SetSkill()
    {
        for(int i = 0; i < dashSkills.Length; i++)
        {
            dashSkills[i].gameObject.SetActive(i == dashLevel);
        }
    }

    IEnumerator CoolDown()
    {
        isDashUsable = false;
        while(cooldownCur >= 0f)
        {
            cooldownCur -= cooldownTick;
            yield return cooldown;
        }
        cooldownCur = dashCoolDown;
        isDashUsable = true;
    }


}
