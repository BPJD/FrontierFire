using UnityEngine;

public class Ability_Hovering : MonoBehaviour
{
    PlayerMove playerMove;
    Rigidbody rb;

    bool isHoverActive = false;

    float defaultDamp = 0f;
    [SerializeField] float hoveringDamp = 1f;




    void Start()
    {
        playerMove = GetComponentInParent<PlayerMove>();
        rb = GetComponentInParent<Rigidbody>();
        defaultDamp = rb.linearDamping;
    }


    private void FixedUpdate()
    {
        if (playerMove != null)
        {
            if (playerMove.moveDir_y >= 0.3f && playerMove.isJumping)
            {
                if (rb.linearVelocity.y <= -0.5f)
                {
                    HoverActive();
                }
            }
            else
            {
                HoverDeActive();
            }
        }
    }

    void HoverActive()
    {
        if (isHoverActive && rb == null) return;

        isHoverActive = true;
        rb.linearDamping = hoveringDamp;
    }

    void HoverDeActive()
    {
        if (!isHoverActive && rb == null) return;

        isHoverActive = false;
        rb.linearDamping = defaultDamp;
    }


}
