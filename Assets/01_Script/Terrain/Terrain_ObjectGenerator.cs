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
    [SerializeField] int instanceMax = 0;        // 최대 생성 개수 (0이면 무제한)
    int instanceCount = 0;

    [Header("Raycast")]
    [SerializeField] float raycastHeight = 40f;

    [Header("Side View Spawn")]
    [SerializeField] bool useFrontSideOnly = true;
    [SerializeField] bool useBackSideInstead = false;
    [SerializeField] float frontZOffset = 0f;

    [Header("Spawn Position Offset")]
    [SerializeField] Vector3 surfaceOffsetPos = new Vector3(0f, 0.02f, 0f);
    [SerializeField] Vector3 groundOffsetPos = new Vector3(0f, -0.5f, 0f);
    [SerializeField] Vector3 cliffOffsetPos = new Vector3(0f, -0.3f, 0f);

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
        if (!isGeneratable)
            return;

        tr = transform;

        if (!objParent)
            objParent = GameObject.FindGameObjectWithTag("Prop").transform;

        selfCollider = GetComponent<Collider>();
        prefabCount = prefabs?.Length ?? 0;

        if (!selfCollider || prefabCount == 0)
        {
            Debug.LogWarning("[Terrain_ObjectGenerator] Collider 또는 Prefabs가 없습니다.");
            return;
        }

        // 분포 범위 계산
        halfX = tr.localScale.x * 0.5f;
        halfZ = tr.localScale.z * 0.5f;
        stripeCount = Mathf.Max(0, (int)tr.localScale.x);

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        int spawnedThisFrame = 0;

        for (int i = 0; i < stripeCount; i++)
        {
            for (int j = 0; j < instancePerBlock; j++)
            {
                float rate = Random.Range(0f, 100f);
                if (rate >= spawnRate)
                    continue;

                float rx = -halfX + i + Random.Range(0f, 1f);
                float rz;

                if (useFrontSideOnly)
                {
                    rz = useBackSideInstead ? halfZ + frontZOffset : -halfZ + frontZOffset;
                }
                else
                {
                    rz = Random.Range(-halfZ, halfZ);
                }

                Vector3 baseWorld = tr.position + tr.right * rx + tr.forward * rz;

                if (instanceMax > 0)
                {
                    if (instanceCount >= instanceMax || tr.position.z != 0f)
                        yield break;
                }

                RaycastHit hit;
                bool hasHit = false;

                switch (type)
                {
                    case TerrainType.Ground:
                        // Ground는 아래 -> 위로 레이캐스트
                        hasHit = RaycastUpSelf(
                            selfCollider,
                            baseWorld + Vector3.down * raycastHeight,
                            raycastHeight * 2f,
                            out hit
                        );
                        break;

                    case TerrainType.Surface:
                    case TerrainType.Cliff:
                    default:
                        // Surface / Cliff는 위 -> 아래로 레이캐스트
                        hasHit = RaycastDownSelf(
                            selfCollider,
                            baseWorld + Vector3.up * raycastHeight,
                            raycastHeight * 2f,
                            out hit
                        );
                        break;
                }

                if (hasHit)
                {
                    GameObject prefab = prefabs[Random.Range(0, prefabCount)];

                    float rotAngle = isRotate ? 359f : 5f;
                    Quaternion rot = Quaternion.Euler(0f, Random.Range(-5f, rotAngle), 0f);

                    Vector3 pos;

                    switch (type)
                    {
                        case TerrainType.Surface:
                            pos = hit.point + surfaceOffsetPos;
                            break;

                        case TerrainType.Ground:
                            if (tr.localScale.y < 4f)
                                continue;

                            pos = hit.point + groundOffsetPos;
                            break;

                        case TerrainType.Cliff:
                            pos = hit.point + cliffOffsetPos;
                            break;

                        default:
                            pos = hit.point;
                            break;
                    }

                    // 사이드뷰 기준 앞면 고정
                    pos.z = baseWorld.z;

                    Instantiate(prefab, pos, rot, objParent);
                    instanceCount++;
                }

                if (++spawnedThisFrame >= spawnPerFrame)
                {
                    spawnedThisFrame = 0;
                    yield return null;
                }
            }
        }
    }

    static bool RaycastDownSelf(Collider self, Vector3 origin, float maxDistance, out RaycastHit hit)
    {
        if (self && self.enabled)
            return self.Raycast(new Ray(origin, Vector3.down), out hit, maxDistance);

        hit = default;
        return false;
    }

    static bool RaycastUpSelf(Collider self, Vector3 origin, float maxDistance, out RaycastHit hit)
    {
        if (self && self.enabled)
            return self.Raycast(new Ray(origin, Vector3.up), out hit, maxDistance);

        hit = default;
        return false;
    }

    void OnDisable()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = null;
    }
}