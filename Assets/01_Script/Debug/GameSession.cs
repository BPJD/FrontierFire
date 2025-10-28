using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    public static GameSession I { get; private set; }

    [Header("Player")]
    [SerializeField] GameObject playerPrefab;
    GameObject player;

    void Awake()
    {
        // 싱글톤 보장
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        TryCachePlayer();  // ← 추가
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬에서 스폰 포인트 찾아서 위치 이동
        //var spawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (player)
        {
            /*
            player.transform.position = spawn.transform.position;
            player.transform.rotation = spawn.transform.rotation;

            */
            player.transform.position = Vector3.zero;
            player.transform.rotation = Quaternion.Euler(Vector3.zero);

            // 물리 초기화
            var rb = player.GetComponent<Rigidbody>();
            if (rb) rb.linearVelocity = Vector3.zero;
        }
    }

    void TryCachePlayer()
    {
        // 씬에 이미 플레이어가 존재하는지 확인
        var existing = GameObject.FindGameObjectWithTag("Player");
        if (existing != null)
        {
            player = existing;
            DontDestroyOnLoad(player);
            return;
        }

        // 없으면 새로 생성
        if (playerPrefab != null)
        {
            player = Instantiate(playerPrefab);
            player.tag = "Player"; // 안전하게 태그 지정
            DontDestroyOnLoad(player);
        }
        else
        {
            Debug.LogWarning("[GameSession] 플레이어 프리팹이 설정되지 않았습니다.");
        }
    }

    public GameObject Player => player;
}
