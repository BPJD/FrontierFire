using System.Collections.Generic;
using UnityEngine;

public class PlayerFoot : MonoBehaviour
{
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float coyoteTime = 0.08f;

    private readonly HashSet<Collider> groundSet = new HashSet<Collider>();
    private float leaveAt = -1f;

    public bool IsGroundedNow => groundSet.Count > 0;
    public bool IsCoyoteAvailable => IsGroundedNow || (leaveAt > 0f && Time.time < leaveAt);

    void Reset()
    {
        if (!playerMove) playerMove = GetComponentInParent<PlayerMove>();
    }

    void Start()
    {
        if (!playerMove) playerMove = GetComponentInParent<PlayerMove>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsGround(other)) return;

        if (groundSet.Add(other))
        {
            leaveAt = -1f;

            if (!playerMove.isGrounded)
                playerMove.GroundCheck();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsGround(other)) return;

        if (!groundSet.Contains(other))
            groundSet.Add(other);

        leaveAt = -1f;

        if (!playerMove.isGrounded)
            playerMove.GroundCheck();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsGround(other)) return;

        groundSet.Remove(other);

        if (groundSet.Count == 0)
            leaveAt = Time.time + coyoteTime;
    }

    void FixedUpdate()
    {
        if (groundSet.Count == 0 && leaveAt > 0f && Time.time >= leaveAt)
        {
            if (playerMove.isGrounded)
                playerMove.PlayerFalling();

            leaveAt = -1f;
        }
    }

    bool IsGround(Collider col)
    {
        return (groundMask.value & (1 << col.gameObject.layer)) != 0;
    }
}