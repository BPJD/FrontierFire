using UnityEngine;
using System.Collections;
using Michsky.UI.Heat;

public class Direction_TutorialTeller : MonoBehaviour
{
    Direction_SceneChanger sceneChanger;

    [SerializeField] Animator tellerAnicon;
    [SerializeField] LocalizedObject tellerText;



    const string TELLER_ANICON_BOOL = "Teller";
    const string LOCALIZE_TEXT_KEY = "Tutorial_Text_";
    [SerializeField] int tellerTextIndexCur = 0;
    [SerializeField] int tellerTextIndexTarget = 2;
    

    WaitForSeconds tellerDelay = new WaitForSeconds(2.75f);
    WaitForSeconds nextTellerDelay = new WaitForSeconds(0.75f);

    [SerializeField] int tutorialStepCur = 0;
    [SerializeField] public int tutorialStepTarget = 1; //{ get; private set; } = 1;
    int tutorialStepClearedMax = 0;

    [SerializeField] Transform healer;
    GameObject player;
    UnitStatus playerStat;

    [SerializeField] Tutorial_KeyPanel keyPanel;

    public bool isTutorialEnd = false;

    [SerializeField] GameObject navigator;
    [SerializeField] Transform[] navigatorTargetTrs;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sceneChanger = GameObject.FindGameObjectWithTag("GameController").GetComponent<Direction_SceneChanger>();
        StartCoroutine(TellerCoroutine());

        TutorialStepSet(tutorialStepTarget);

        player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
        playerStat = player.GetComponent<UnitStatus>();
        playerStat.unitParams.u_immunePer -= 0.8f;

    }

    IEnumerator TellerCoroutine()
    {
        while (true)
        {
            if(tellerTextIndexCur < tellerTextIndexTarget)
            {
                keyPanel.StepChanged(0);

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

            if(tellerTextIndexCur == tellerTextIndexTarget)
            {
                keyPanel.StepChanged(tutorialStepTarget);

                if(isTutorialEnd)
                {
                    sceneChanger.ToMainMenu();
                    ES3.Save<bool>("isStartInLobby", true);
                    yield return new WaitForSeconds(50f);
                }

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
            case 0: //시작
                NavigatorSet(navigatorTargetTrs[0]);
                break;
            case 1: //왼쪽 이동
                tellerTextIndexTarget = 3;
                NavigatorSet(navigatorTargetTrs[1]);
                break;
            case 2: //오른쪽 이동
                tellerTextIndexTarget = 5;
                NavigatorSet(navigatorTargetTrs[2]);
                break;
            case 3: //점프
                tellerTextIndexTarget = 6;
                NavigatorSet(navigatorTargetTrs[3]);
                break;
            case 4: //상자열기
                tellerTextIndexTarget = 7;
                NavigatorSet(navigatorTargetTrs[0]);
                break;
            case 5: //무기 집기
                tellerTextIndexTarget = 7;
                break;
            case 6: //무기 전환
                tellerTextIndexTarget = 8;
                break;
            case 7: //하단점프
                tellerTextIndexTarget = 9;
                NavigatorSet(navigatorTargetTrs[4]);
                break;
            case 8: //교전
                tellerTextIndexTarget = 10;
                NavigatorSet(navigatorTargetTrs[5]);
                break;
            case 9: //대형드론
                tellerTextIndexTarget = 11;
                NavigatorSet(navigatorTargetTrs[0]);
                break;
            case 10: //스탯강화
                tellerTextIndexTarget = 12;
                break;
            case 11: //모두처치
                tellerTextIndexTarget = 13;
                break;
            case 12: //보스전
                tellerTextIndexTarget = 14;
                break;
            case 13: //보스끝
                tellerTextIndexTarget = 15;
                break;
            case 14: //튜토리얼 종료
                tellerTextIndexTarget = 17;
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

    void NavigatorSet(Transform tr)
    {
        navigator.transform.position = tr.position;
    }
}
