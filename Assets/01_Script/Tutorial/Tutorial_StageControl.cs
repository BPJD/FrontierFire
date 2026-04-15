using UnityEngine;
using System.Collections;

public class Tutorial_StageControl : MonoBehaviour
{
    [SerializeField] UnitStatus[] units;
    [SerializeField] GameObject[] bonusUnits;

    [SerializeField] int allkillClearStepTarget = 11;

    Direction_TutorialTeller teller;

    Data_AudioClips clipData;
    GameObject data;
    GameSoundPlayer soundPlayer;
    Direction_BGMPlay bgmPlayer;

    [SerializeField] Transform portalPoint;
    [SerializeField] GameObject portalObj;
    [SerializeField] GameSoundPlayer.SoundType portalSoundType = GameSoundPlayer.SoundType.SFX;

    bool isPortalGenerated = false;
    bool isBunusUnitActivated = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        teller = GetComponent<Direction_TutorialTeller>();
        StartCoroutine(AllKillClearCheck());
        
        data = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag);
        soundPlayer = GameObject.FindGameObjectWithTag("Sound").GetComponent<GameSoundPlayer>();
        bgmPlayer = soundPlayer.gameObject.GetComponent<Direction_BGMPlay>();
        data = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag);
        clipData = soundPlayer.gameObject.GetComponent<Data_AudioClips>();

    }


    IEnumerator AllKillClearCheck()
    {
        while (!isPortalGenerated)
        {
            bool allClear = CheckAllClear();

            if (allClear && !isPortalGenerated)
            {
                teller.TutorialStepSuccess(allkillClearStepTarget);
                PortalGenerate();
                isPortalGenerated = true;
            }

            if (allkillClearStepTarget + 1 == teller.tutorialStepTarget && !isBunusUnitActivated)
            {
                for (int i = 0; i < bonusUnits.Length; i++)
                {
                    bonusUnits[i].SetActive(true);
                }
                isBunusUnitActivated = true;
            }

            yield return new WaitForSeconds(1.11f);
        }
    }

    void PortalGenerate()
    {
        AudioClip _clip = clipData.GetPortalSoundClipByPortalType(0);

        _clip = clipData.GetPortalSoundClipByPortalType(2);
        portalObj.SetActive(true);
        soundPlayer.GameSoundPlayByType(_clip, portalSoundType);
        bgmPlayer.StopBGM(2f);
    }

    bool CheckAllClear()
    {
        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] != null)
            {
                if (units[i].hpCur > 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

}
