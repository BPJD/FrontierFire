using UnityEngine;
using System.Collections;

public class BossGiantAttackControl : MonoBehaviour
{
    public enum GiantPattern { Swing, Smash, RStomp, StoneStomp }

    GiantPattern usedPattern = GiantPattern.Smash;
    public GiantPattern patternCur { get; private set; } = GiantPattern.Smash;
    //UnitStatus unitStat;
    BossControlSystem bossState;
    BossGiantMove giantMove;
    Animator aniCon;


    [SerializeField] int patternUseCount = 0;
    public bool isPatternUsing = false;

    string ani_swing = "P_Swing";
    string ani_smash = "P_Smash";
    string ani_Rstomp = "P_RStomp";
    string ani_StoneStomp = "P_StoneStomp";



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aniCon = GetComponent<Animator>();
        //unitStat = GetComponent<UnitStatus>();
        bossState = GetComponent<BossControlSystem>();
        giantMove = GetComponent<BossGiantMove>();
        StartCoroutine(PatternRoll());
    }

    IEnumerator PatternRoll()
    {
        while (bossState.isBossLive)
        {
            patternCur = SetUsePattern();

            if (!isPatternUsing)
            {
                isPatternUsing = true;
                giantMove.isMove = false;


                UsePattern(patternCur);
                patternUseCount++;

                
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void UsePattern(GiantPattern pattern)
    {
        usedPattern = pattern;

        switch (pattern)
        {
            case GiantPattern.Swing:
                aniCon.SetTrigger(ani_swing);
                break;
            case GiantPattern.Smash:
                aniCon.SetTrigger(ani_smash);

                break;
            case GiantPattern.RStomp:
                aniCon.SetTrigger(ani_Rstomp);

                break;
            case GiantPattern.StoneStomp:
                aniCon.SetTrigger(ani_StoneStomp);
                patternUseCount = 0;

                break;
        }

        StartCoroutine(PatternCoolDown());
    }


    GiantPattern SetUsePattern()
    {
        if(patternUseCount >= 3)
        {
            return GiantPattern.StoneStomp;
        }
        else
        {
            if (giantMove.isClose)
            {
                if (usedPattern == GiantPattern.Swing)
                {
                    return GiantPattern.RStomp;
                }
                else
                {
                    return GiantPattern.Swing;
                }
            }
            else
            {
                if (usedPattern == GiantPattern.Smash)
                {
                    return GiantPattern.RStomp;
                }
                else
                {
                    return GiantPattern.Smash;
                }
            }
        }
        
    }

    IEnumerator PatternCoolDown()
    {
        yield return new WaitForSeconds(10f);

        isPatternUsing = false;
    }
}
