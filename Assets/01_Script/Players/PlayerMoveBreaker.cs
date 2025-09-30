using UnityEngine;

public class PlayerMoveBreaker : MonoBehaviour
{
    [SerializeField] bool isRight = false;
    bool terrainCollided = false;

    PlayerMove playerMove;

    float noCollisionTimer = 0f;
    float noCollisionThreshold = 0.1f; // 0.1초 이상 충돌 없음 → 미감지 처리

    private void Start()
    {
        playerMove = GetComponentInParent<PlayerMove>();
    }

    private void Update()
    {
        // 충돌 없을 때 타이머 증가
        if (!terrainCollided)
        {
            noCollisionTimer += Time.deltaTime;

            if (noCollisionTimer >= noCollisionThreshold)
            {
                // 충돌 없음 상태에서 하고 싶은 행동
                SwitchBlock(false);
            }
        }
        else
        {
            noCollisionTimer = 0f;
        }

        // 상태 리셋 (다음 프레임용)
        terrainCollided = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!terrainCollided)
        {
            SwitchBlock(true);
        }

        terrainCollided = true;
    }

    void SwitchBlock(bool _switch)
    {
        if (isRight)
        {
            playerMove.isRightBlocked = _switch;
        }
        else
        {
            playerMove.isLeftBlocked = _switch;
        }
    }
}
