using UnityEngine;
using TMPro; // TextMeshPro 쓴다면

public class ProximityTooltip : MonoBehaviour
{
    [Header("Tooltip Content")]
    public string title = "Item Name";
    [TextArea]public string description = "Item description...";

    [Header("Trigger / Visibility")]
    [SerializeField] Transform player;               // 없으면 Start에서 자동 탐색
    [SerializeField] float showDistance = 4f;        // 이 거리 이내면 표시
    [SerializeField] bool requireLineOfSight = true; // 시야 가림 체크
    [SerializeField] LayerMask losMask;              // 가리는 레이어
    [SerializeField] Vector3 worldOffset = new Vector3(0, 1.5f, 0);

    [Header("UI")]
    [SerializeField] RectTransform tooltipPrefab;    // TooltipUI 프리팹
    [SerializeField] Canvas rootCanvas;              // Screen Space Canvas
    [SerializeField] float appearSpeed = 10f;        // 페이드 속도
    [SerializeField] Vector2 screenClamp = new Vector2(8, 8); // 화면 경계 여백(px)

    Camera cam;
    RectTransform ui;        // 인스턴스
    CanvasGroup cg;          // 페이드
    TMP_Text titleTMP;
    TMP_Text descTMP;

    bool visibleTarget = false;  // 논리적 표시 조건(거리/시야)
    bool created = false;

    void Start()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        cam = Camera.main;

        // UI 인스턴스 만들기
        if (tooltipPrefab && rootCanvas)
        {
            ui = Instantiate(tooltipPrefab, rootCanvas.transform);
            created = true;

            cg = ui.GetComponent<CanvasGroup>();
            if (!cg) cg = ui.gameObject.AddComponent<CanvasGroup>();

            // 텍스트 바인딩
            titleTMP = ui.GetComponentInChildren<TMP_Text>(); // 첫 TMP_Text를 제목으로 쓰고…
            // 만약 제목/설명 따로라면 GetComponentsInChildren로 잡아서 배열[0]/[1] 등으로 매핑
            // 여기서는 간단히 두 개를 찾아봄
            var tmps = ui.GetComponentsInChildren<TMP_Text>();
            if (tmps.Length >= 2) { titleTMP = tmps[0]; descTMP = tmps[1]; }
            else if (tmps.Length == 1) { titleTMP = tmps[0]; }

            if (titleTMP) titleTMP.text = title;
            if (descTMP) descTMP.text = description;

            cg.alpha = 0f; // 시작은 숨김
        }
        else
        {
            Debug.LogWarning($"[ProximityTooltip] Prefab/Canvas 미지정: {name}");
        }
    }

    void LateUpdate()
    {
        if (!created || !player || !cam) return;

        // 1) 거리 체크 (sqrt 피하려면 sqr 비교)
        Vector3 worldPos = transform.position + worldOffset;
        float sqrDist = (player.position - worldPos).sqrMagnitude;
        bool closeEnough = sqrDist <= showDistance * showDistance;

        // 2) 카메라 후면 체크
        Vector3 camToTarget = worldPos - cam.transform.position;
        bool inFront = Vector3.Dot(cam.transform.forward, camToTarget) > 0f;

        // 3) LOS(가림) 체크 (선택)
        bool hasLOS = true;
        if (requireLineOfSight && inFront && closeEnough)
        {
            if (Physics.Raycast(cam.transform.position, camToTarget.normalized, out RaycastHit hit, Mathf.Sqrt(sqrDist), losMask))
            {
                // 아이템 자신이면 통과. 그 외에 맞으면 가려진 것으로 판단
                if (hit.transform != transform && hit.transform.IsChildOf(transform) == false)
                    hasLOS = false;
            }
        }

        visibleTarget = closeEnough && inFront && hasLOS;

        // 4) 표시/숨김 페이드
        float targetAlpha = visibleTarget ? 1f : 0f;
        cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, appearSpeed * Time.deltaTime);
        ui.gameObject.SetActive(cg.alpha > 0.01f); // 완전 0이면 비활성

        if (cg.alpha <= 0.01f) return;

        // 5) 월드→스크린 좌표 변환
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // 6) 오프스크린 클램프 + 뒤집힘 방지
        // (Z<0일 경우 화면 뒤라서 보이지 않게 처리했으니 여기선 x,y만 클램프)
        Vector2 clamped = new Vector2(
            Mathf.Clamp(screenPos.x, screenClamp.x, Screen.width - screenClamp.x),
            Mathf.Clamp(screenPos.y, screenClamp.y, Screen.height - screenClamp.y)
        );

        ui.position = clamped;
    }

    void OnDestroy()
    {
        if (created && ui)
            Destroy(ui.gameObject);
    }
}
