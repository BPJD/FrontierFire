using UnityEngine;

public class TerrainDownPlatform : MonoBehaviour
{
    bool _playerCollided;
    string playerTag = "Player";

    BoxCollider playerPlatform;
    Rigidbody playerRb;
    PlayerInputController inputController;

    [SerializeField] bool isOnlyUp = false;

    private void Start()
    {
        playerPlatform = GetComponent<BoxCollider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            _playerCollided = true;
            playerRb = collision.gameObject.GetComponent<Rigidbody>();
            inputController = collision.gameObject.GetComponent<PlayerInputController>();
            inputController.downPlatform = this;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            _playerCollided = false;
            playerRb = null;
            inputController = null;
        }
    }

    public void DownJumpRequested()
    {
        if (_playerCollided && !isOnlyUp)
        {
            playerPlatform.enabled = false;
            if (playerRb != null)
            {
                playerRb.AddForce(Vector3.down * 0.01f);
                _playerCollided = false;
            }
        }
    }
}
