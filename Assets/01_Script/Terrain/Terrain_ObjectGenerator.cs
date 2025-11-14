using UnityEngine;
using System.Collections;

public class Terrain_ObjectGenerator : MonoBehaviour
{
    [SerializeField] bool isGeneratable = true;

    [Header("Parent / Prefabs")]
    [SerializeField] Transform objParent;
    [SerializeField] GameObject[] prefabs;
    [SerializeField] bool isRotate = true;

    [Header("Distribution")]
    [SerializeField] int instancePerBlock = 1;   // 스트라이프당 생성 개수
    [SerializeField] int spawnPerFrame = 50;     // 프레임당 생성 개수(스파이크 완화)
    [SerializeField] float spawnRate = 100f;

    [Header("Raycast")]
    [SerializeField] float raycastHeight = 40f;  // 위에서 쏠 높이
    [SerializeField] float surfaceOffset = 0.02f;

    Transform tr;
    Collider selfCollider;
    int prefabCount;
    int stripeCount;
    float halfX, halfZ;
    Coroutine spawnRoutine;

    public enum TerrainType { Surface, Cliff, Ground };
    public TerrainType type = TerrainType.Surface;

    void Start()
    {
        if (isGeneratable)
        {
            tr = transform;
            if (!objParent) objParent = GameObject.FindGameObjectWithTag("Prop").transform;

            selfCollider = GetComponent<Collider>();
            prefabCount = prefabs?.Length ?? 0;
            if (!selfCollider || prefabCount == 0)
            {
                Debug.LogWarning("[Terrain_ObjectGenerator] Collider 또는 Prefabs가 없습니다.");
                return;
            }

            // 분포 범위(스케일은 범위 계산용으로만 사용)
            halfX = tr.localScale.x * 0.5f;
            halfZ = tr.localScale.z * 0.5f;
            stripeCount = Mathf.Max(0, (int)tr.localScale.x);

            spawnRoutine = StartCoroutine(SpawnRoutine());
        }
    }

    IEnumerator SpawnRoutine()
    {
        int spawnedThisFrame = 0;

        for (int i = 0; i < stripeCount; i++)
        {
            for (int j = 0; j < instancePerBlock; j++)
            {
                float rate = Random.Range(0f, 100f);
                if(rate >= spawnRate)
                {
                    continue;
                }

                // 균등 스트라이프 분포
                float rx = -halfX + i + Random.Range(0f, 1f);
                float rz = Random.Range(-halfZ, halfZ);

                // 회전만 반영(스케일 영향 X)
                Vector3 baseWorld = tr.position + tr.right * rx + tr.forward * rz;

                // 자기 자신 콜라이더에만 맞는 레이캐스트
                if (RaycastDownSelf(selfCollider, baseWorld + Vector3.up * raycastHeight, raycastHeight * 2f, out var hit))
                {
                    Vector3 pos;

                    if (type == TerrainType.Surface)
                    {
                        pos = new Vector3(baseWorld.x, hit.point.y + surfaceOffset, baseWorld.z);
                    }
                    else
                    {
                        float randY;
                        if (type == TerrainType.Ground)
                        {
                            if (tr.localScale.y < 4f)
                            {
                                continue;
                            }
                            float yMin = tr.position.y;
                            randY = Mathf.Min(Random.Range(-tr.localScale.y * 0.5f, tr.localScale.y * 0.3f), yMin);
                        }
                        else
                        {
                            randY = Random.Range(-tr.localScale.y * 0.5f, tr.localScale.y * 0.4f);
                        }
                            float z = -tr.localScale.z * 0.5f;
                        pos = new Vector3(baseWorld.x, baseWorld.y + randY, z);
                    }

                    float rotAngle = isRotate ? 359f : 5f;
                    Quaternion rot = Quaternion.Euler(0f, Random.Range(-5f, rotAngle), 0f);

                    var prefab = prefabs[Random.Range(0, prefabCount)];

                    Instantiate(prefab, pos, rot, objParent);
                }

                // 프레임 분할
                if (++spawnedThisFrame >= spawnPerFrame)
                {
                    spawnedThisFrame = 0;
                    yield return null; // 다음 프레임까지 양보
                }
            }
        }
    }

    // 자기 자신(단일 콜라이더)에만 맞는 단순 레이캐스트
    static bool RaycastDownSelf(Collider self, Vector3 origin, float maxDistance, out RaycastHit hit)
    {
        if (self && self.enabled)
            return self.Raycast(new Ray(origin, Vector3.down), out hit, maxDistance);

        hit = default;
        return false;
    }

    void OnDisable()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }
}
