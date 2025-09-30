using Kamgam.HitMe;
using UnityEngine;

public class EnemyWeapon_Grenade : MonoBehaviour
{
    Transform source;
    public Transform Target;
    public GameObject ProjectilePrefab;
    public BallisticProjectileConfig Config;
    public Animator unitAnimator;

    EnemyAttackSystem attackSystem;

    bool hasEventTriggered = false;

    [SerializeField] float grenadeTiming = 0.6f;

    MeshRenderer grenadeMesh;

    void Start()
    {
        grenadeMesh = GetComponent<MeshRenderer>();
        attackSystem = GetComponentInParent<EnemyAttackSystem>();
        attackSystem.SetGrenadeComponent(this);
        source = transform;
    }

    // Update is called once per frame
    void Update()
    {
        
        AnimatorStateInfo stateInfo = unitAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Grenade"))
        {
            float normalizedTime = stateInfo.normalizedTime;

            // 예: 50% 지점에서 호출
            if (!hasEventTriggered && normalizedTime >= grenadeTiming)
            {
                hasEventTriggered = true;
                Target = attackSystem.target;
                ShootGrenade();
            }

            // 다음 루프 대비 리셋 (Looping 애니메이션 고려 시)
            if (normalizedTime >= 1f)
                hasEventTriggered = false;
        }
        
    }


    public void ShootGrenade()
    {
        BallisticProjectile.Spawn(ProjectilePrefab, source, Target, Config);
        grenadeMesh.enabled = false;
    }

    public void ResetGrenadeState()
    {
        hasEventTriggered = false;
        grenadeMesh.enabled = true;
    }
}
