using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Ability_Hovering : MonoBehaviour, IAbilityUpgradable
{
    PlayerMove playerMove;
    Rigidbody rb;

    bool isHoverActive = false;

    float defaultDamp = 0f;
    [SerializeField] float hoveringDamp = 1f;


    [SerializeField] float hoverDuration = 5f;
    [SerializeField] AnimationCurve hoverDampCurve;

    float hoverTimer = 0f;


    void Start()
    {
        playerMove = GetComponentInParent<PlayerMove>();
        rb = GetComponentInParent<Rigidbody>();
        defaultDamp = rb.linearDamping;
    }


    private void FixedUpdate()
    {
        if (playerMove == null || rb == null)
            return;

        if (!playerMove.isJumping)
        {
            hoverTimer = 0f;
        }

        if (playerMove.moveDir_y >= 0.3f && playerMove.isJumping)
        {
            if (rb.linearVelocity.y <= -0.5f)
            {
                HoverActive();

                // 시간 증가
                hoverTimer += Time.fixedDeltaTime;

                float t = Mathf.Clamp01(hoverTimer / hoverDuration);

                // 커브 평가
                float curveValue = hoverDampCurve.Evaluate(t);

                // damping 적용
                rb.linearDamping = hoveringDamp * curveValue;

                return;
            }
        }

        HoverDeActive();
    }

    void HoverActive()
    {
        if (isHoverActive) return;

        isHoverActive = true;
        hoverTimer = 0f; // 시작 시 초기화
    }

    void HoverDeActive()
    {
        if (!isHoverActive) return;

        isHoverActive = false;
        rb.linearDamping = defaultDamp;
    }


    public void UpgradeAbility()
    {
        hoverDuration += 2f;
    }


}
