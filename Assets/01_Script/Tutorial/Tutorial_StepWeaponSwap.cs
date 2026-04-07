using UnityEngine;

public class Tutorial_StepWeaponSwap : MonoBehaviour
{
    PlayerWeaponController playerWeapon;
    [SerializeField] int stepTarget = 5;
    Direction_TutorialTeller tutorial;

    int beforeWeapon;

    [SerializeField] GameObject wall;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetComponent();
    }

    void SetComponent()
    {
        GameObject _player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
        if (_player != null)
        {
            playerWeapon = _player.GetComponent<PlayerWeaponController>();
        }
        tutorial = GetComponentInParent<Direction_TutorialTeller>();

    }

    // Update is called once per frame
    void Update()
    {
        if(playerWeapon != null)
        {

            if(playerWeapon.weaponCur != beforeWeapon && tutorial.tutorialStepTarget == stepTarget)
            {
                tutorial.TutorialStepSuccess(stepTarget);
                wall.SetActive(false);
                this.enabled = false;
            }
        }
        else
        {
            SetComponent();
        }
    }
}
