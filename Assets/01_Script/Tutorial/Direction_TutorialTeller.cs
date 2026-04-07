using UnityEngine;
using System.Collections;
using Michsky.UI.Heat;

public class Direction_TutorialTeller : MonoBehaviour
{

    [SerializeField] Animator tellerAnicon;
    [SerializeField] LocalizedObject tellerText;

    const string TELLER_ANICON_BOOL = "Teller";
    const string LOCALIZE_TEXT_KEY = "Tutorial_Text_";
    int tellerTextIndexCur = 0;
    int tellerTextIndexTarget = 2;

    WaitForSeconds tellerDelay = new WaitForSeconds(4f);
    WaitForSeconds nextTellerDelay = new WaitForSeconds(1f);

    int tutorialStepCur = 0;
    public int tutorialStepTarget { get; private set; } = 1;
    int tutorialStepClearedMax = 0;

    [SerializeField] Transform healer;
    GameObject player;
    UnitStatus playerStat;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(TellerCoroutine());

        TutorialStepSet(tutorialStepTarget);

        player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
        playerStat = player.GetComponent<UnitStatus>();
        playerStat.unitParams.u_immunePer -= 0.7f;
    }

    IEnumerator TellerCoroutine()
    {
        while (true)
        {
            if(tellerTextIndexCur < tellerTextIndexTarget)
            {
                string _localizeKey = LOCALIZE_TEXT_KEY + tellerTextIndexCur;

                tellerText.localizationKey = _localizeKey;
                tellerText.UpdateItem();

                tellerAnicon.SetBool(TELLER_ANICON_BOOL, true);
                yield return tellerDelay;

                tellerAnicon.SetBool(TELLER_ANICON_BOOL, false);

                yield return nextTellerDelay;
                tellerTextIndexCur++;

                yield return null;
            }



            yield return new WaitForSeconds(0.5f);

            //healer.position = player.transform.position;
        }
    }


    public void TutorialStepSet(int step)
    {
        tutorialStepTarget = step;
        switch (step)
        {
            case 0:
                break;
            case 1:
                tellerTextIndexTarget = 3;
                break;
            case 2:
                tellerTextIndexTarget = 5;
                break;
            case 3:
                tellerTextIndexTarget = 6;
                break;
            case 4:
                tellerTextIndexTarget = 7;
                break;
            case 5:
                tellerTextIndexTarget = 8;
                break;
            case 6:
                tellerTextIndexTarget = 9;
                break;
            case 7:
                tellerTextIndexTarget = 10;
                break;
            case 8:
                tellerTextIndexTarget = 11;
                break;
            case 9:
                tellerTextIndexTarget = 12;
                break;
            case 10:
                tellerTextIndexTarget = 13;
                break;
            case 11:
                tellerTextIndexTarget = 14;
                break;
            case 12:
                tellerTextIndexTarget = 16;
                break;
            default:
                break;
        }
    }

    public void TutorialStepSuccess(int step)
    {
        if(step == tutorialStepTarget)
        {
            tutorialStepCur = step;
            TutorialStepSet(tutorialStepTarget + 1);
        }
    }
}
