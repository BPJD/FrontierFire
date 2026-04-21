using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerDashManager : MonoBehaviour
{
    Transform tr;
    PlayerLookMouse playerLook;
    
    [SerializeField] PlayerMove_Dash[] dashSkills;
    [SerializeField] int dashLevel = 0;

    [SerializeField] float dashCoolDown = 2f;
    float cooldownCur = 3f;
    bool isDashUsable = true;

    static float cooldownTick = 0.02f;
    WaitForSeconds cooldown = new WaitForSeconds(cooldownTick);

    Image dashCooldownIcon;

    AudioSource audioSource;
    [SerializeField] AudioClip dashisCooldowning;
    [SerializeField] AudioClip dashisReady;


    private void Start()
    {
        playerLook = GetComponentInParent<PlayerLookMouse>();
        audioSource = GetComponent<AudioSource>();
        tr = transform;

        if(dashCooldownIcon == null)
        {
            GetDashIcon();
        }

        SetSkill();
    }


    public void DashLevelUp()
    {
        dashLevel = Mathf.Clamp(dashLevel + 1, 0, dashSkills.Length - 1);
        
        SetSkill();
    }

    public void DashActive(Vector3 inputDirWorld)
    {
        if (!isDashUsable)
        {
            if (audioSource != null && dashisCooldowning != null)
            {
                audioSource.PlayOneShot(dashisCooldowning);
            }
            return;
        }
            

        if (dashCooldownIcon == null)
        {
            GetDashIcon();
        }

        inputDirWorld.z = 0f;

        if (inputDirWorld.sqrMagnitude < 0.0001f)
        {
            if (playerLook == null)
                playerLook = GetComponentInParent<PlayerLookMouse>();

            inputDirWorld = playerLook.targetPos - tr.position;
            inputDirWorld.z = 0f;
        }

        dashSkills[dashLevel].TryDash(inputDirWorld);

        StartCoroutine(CoolDown());
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

        if (dashCooldownIcon != null)
        {
            dashCooldownIcon.gameObject.SetActive(true);
            dashCooldownIcon.fillAmount = 0f;
        }

        while (cooldownCur >= 0f)
        {
            cooldownCur -= cooldownTick;

            if (dashCooldownIcon != null)
            {
                dashCooldownIcon.fillAmount = 1f - (cooldownCur / dashCoolDown);
            }

            yield return cooldown;
        }

        cooldownCur = dashCoolDown;

        if (dashCooldownIcon != null)
        {
            dashCooldownIcon.gameObject.SetActive(false);
        }

        isDashUsable = true;

        if (audioSource != null && dashisReady != null)
        {
            audioSource.PlayOneShot(dashisReady);
        }
    }

    void GetDashIcon()
    {
        dashCooldownIcon = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag).GetComponent<Data_UI>().GetPlayerDashCooltime();
    }

    public void ResetDashCooldown()
    {
        dashCoolDown = -1f;
    }

}
