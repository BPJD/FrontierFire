using UnityEngine;
using System.Collections;

public class BossGiantAttackControl : MonoBehaviour
{
    public enum GiantPattern { None, Swing, Smash, RStomp, StoneStomp }

    GiantPattern usedPattern;
    public GiantPattern patternCur { get; private set; } = GiantPattern.None;

    BossControlSystem bossState;
    BossGiantMove giantMove;
    Animator aniCon;

    [SerializeField] int patternUseCount = 0;
    public bool isPatternUsing = false;

    string ani_swing = "P_Swing";
    string ani_smash = "P_Smash";
    string ani_Rstomp = "P_RStomp";
    string ani_StoneStomp = "P_StoneStomp";
    string ani_DealPhaseStart = "DealPhaseStart";
    string ani_DealPhaseEnd = "DealPhaseEnd";

    [SerializeField] float bossStunDuration = 10f;
    public bool isStun { get; private set; } = false;
    [SerializeField] float weakpointRevision = 1.35f;

    public GameObject[] stoneProjectiles;
    public GameObject[] dripStoneProjectiles;
    public GameObject[] spawnMobs;
    public Transform[] mobSpawnPoints;
    [SerializeField] UnitWeakPoint bossWeakPoint;

    // ★ 현재 돌아가는 코루틴 핸들 저장
    Coroutine patternRollRoutine;
    Coroutine patternCoolRoutine;

    void Start()
    {
        aniCon = GetComponent<Animator>();
        bossState = GetComponent<BossControlSystem>();
        giantMove = GetComponent<BossGiantMove>();

        patternRollRoutine = StartCoroutine(PatternRoll());
    }

    IEnumerator PatternRoll()
    {
        yield return new WaitForSeconds(2f);
        while (bossState.isBossLive)
        {
            // ★ 스턴 상태면 패턴 로직 완전 정지
            if (isStun)
            {
                giantMove.isMove = false;
                yield return null;
                continue;
            }

            // 이동 중 + 아직 패턴 사용 중이 아닐 때만 새로운 패턴 선택
            if (giantMove.isMove && !isPatternUsing)
            {
                patternCur = SetUsePattern();

                giantMove.isMove = false;
                isPatternUsing = true;

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
            case GiantPattern.None:
                // 아무 것도 하지 않음
                break;
        }

        // ★ 이전에 돌던 쿨타임 코루틴 정리
        if (patternCoolRoutine != null)
        {
            StopCoroutine(patternCoolRoutine);
        }
        patternCoolRoutine = StartCoroutine(PatternCoolDown());
    }

    GiantPattern SetUsePattern()
    {
        if (patternUseCount >= 3)
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
        float elapsed = 0f;
        float cooldown = 8f;

        while (elapsed < cooldown)
        {
            // ★ 스턴에 들어가면 쿨타임도 의미 없으니 바로 종료
            if (isStun)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        isPatternUsing = false;
        patternCoolRoutine = null;
    }

    public void BossGetStun()
    {
        if (isStun) return;

        isStun = true;

        // ★ 현재 패턴/쿨타임 정리
        if (patternCoolRoutine != null)
        {
            StopCoroutine(patternCoolRoutine);
            patternCoolRoutine = null;
        }

        isPatternUsing = false;
        patternCur = GiantPattern.None;
        usedPattern = GiantPattern.None;

        giantMove.isMove = false;

        // (필요하다면 공격 트리거들 리셋해도 좋음)
        // aniCon.ResetTrigger(ani_swing);
        // aniCon.ResetTrigger(ani_smash);
        // aniCon.ResetTrigger(ani_Rstomp);
        // aniCon.ResetTrigger(ani_StoneStomp);

        aniCon.SetTrigger(ani_DealPhaseStart);

        bossWeakPoint.isNormalDamagePoint = false;
        bossWeakPoint.addDamage = weakpointRevision;

        StartCoroutine(BossStunEnd());
    }

    IEnumerator BossStunEnd()
    {
        yield return new WaitForSeconds(bossStunDuration);

        aniCon.SetTrigger(ani_DealPhaseEnd);
        bossWeakPoint.isNormalDamagePoint = true;
        bossWeakPoint.addDamage = 1f;

        isStun = false;

        // ★ 스턴 끝났으니 다시 움직일 수 있게
        // (giantMove 쪽 로직에 따라 조정해도 됨)
        giantMove.isMove = true;
    }
}
