using UnityEngine;
using TMPro; // TextMeshPro 쓴다면
using UnityEngine.UI;

public class Item_ToolTip : MonoBehaviour
{
    [Header("Tooltip Content")]
    public string title = "Item Name";
    public string subTitle = "Item Name";
    [TextArea] public string description = "Item description...";

    [Header("Weapon ToolTip Content")]
    public string[] weaponStat = new string[8];

    [Header("Trigger / Visibility")]
    [SerializeField] Transform player;               // 없으면 Start에서 자동 탐색
    [SerializeField] bool requireLineOfSight = true; // 시야 가림 체크
    [SerializeField] LayerMask losMask;              // 가리는 레이어
    [SerializeField] Vector3 worldOffset = new Vector3(0, 1.5f, 0);

    [Header("UI")]
    [SerializeField] UI_ToolTip_Object.ObjectType tooltipType;
    RectTransform tooltipPrefab;    // TooltipUI 프리팹
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
    bool isComponentSelected = false;

    PlayerInteract interacter;
    UI_ToolTip_Object toolTip_Object;
    int interactCount = 0;
    int interactCountMax = 1;

    bool isSelectedColor = false;
    float deselectedAlpha = 0.7f;

    void Start()
    {
        if (!isComponentSelected)
        {
            ComponentSelect();
        }


    }

    void ComponentSelect()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        cam = Camera.main;
        interacter = player.gameObject.GetComponentInChildren<PlayerInteract>();

        if (rootCanvas == null)
        {
            rootCanvas = GameObject.FindGameObjectWithTag("ItemUICanvas").GetComponent<Canvas>();
        }

        tooltipPrefab = rootCanvas.GetComponent<DataToolTips>().GetToolTipData(tooltipType);

        // UI 인스턴스 만들기
        if (tooltipPrefab && rootCanvas)
        {
            ui = Instantiate(tooltipPrefab, rootCanvas.transform);
            created = true;

            cg = ui.GetComponent<CanvasGroup>();
            if (!cg) cg = ui.gameObject.AddComponent<CanvasGroup>();

            /*
            // 텍스트 바인딩
            titleTMP = ui.GetComponentInChildren<TMP_Text>(); // 첫 TMP_Text를 제목으로 쓰고…
            // 만약 제목/설명 따로라면 GetComponentsInChildren로 잡아서 배열[0]/[1] 등으로 매핑
            // 여기서는 간단히 두 개를 찾아봄
            var tmps = ui.GetComponentsInChildren<TMP_Text>();
            if (tmps.Length >= 2) { titleTMP = tmps[0]; descTMP = tmps[1]; }
            else if (tmps.Length == 1) { titleTMP = tmps[0]; }

            if (titleTMP) titleTMP.text = title;
            if (descTMP) descTMP.text = description;
            */

            toolTip_Object = ui.gameObject.GetComponent<UI_ToolTip_Object>();
            UpdateToolTipUI();

            cg.alpha = 0f; // 시작은 숨김


            isComponentSelected = true;
        }
        else
        {
            //Debug.LogWarning($"[ProximityTooltip] Prefab/Canvas 미지정: {name}");
        }
    }



    void LateUpdate()
    {
        if (!created || !player || !cam) return;

        bool isSelected = interacter /*&& interacter.SelectedObj == gameObject*/;

        // 선택된 경우에만 가시성 조건을 계산
        if (isSelected/* || tooltipType != UI_ToolTip_Object.ObjectType.Weapon*/)
        {
            Vector3 worldPos = transform.position + worldOffset;

            // 1) 거리 체크 (sqrt 피하기)
            float sqrDist = (player.position - worldPos).sqrMagnitude;
            bool closeEnough = sqrDist <= interacter.uiShowDistance * interacter.uiShowDistance;

            // 2) 카메라 전면 체크
            Vector3 camToTarget = worldPos - cam.transform.position;
            bool inFront = Vector3.Dot(cam.transform.forward, camToTarget) > 0f;

            // 3) 시야 가림(LOS) 체크
            bool hasLOS = true;
            if (requireLineOfSight && inFront && closeEnough)
            {
                if (Physics.Raycast(cam.transform.position, camToTarget.normalized,
                                    out RaycastHit hit, Mathf.Sqrt(sqrDist), losMask))
                {
                    if (hit.transform != transform && !hit.transform.IsChildOf(transform))
                        hasLOS = false;
                }
            }

            visibleTarget = closeEnough && inFront && hasLOS;

            // 위치는 "보여줄 때만" 갱신 (페이드아웃 시엔 마지막 위치를 유지)
            if (visibleTarget)
            {
                Vector3 worldPosForScreen = transform.position + worldOffset;
                Vector3 screenPos = cam.WorldToScreenPoint(worldPosForScreen);

                Vector2 clamped = new Vector2(
                    Mathf.Clamp(screenPos.x, screenClamp.x, Screen.width - screenClamp.x),
                    Mathf.Clamp(screenPos.y, screenClamp.y, Screen.height - screenClamp.y)
                );
                ui.position = clamped;

                CheckItemToolTipSelected();
                
            }
        }
        else
        {
            // 선택 해제 시에는 목표 가시성을 false로 만들어 페이드아웃
            visibleTarget = false;
        }


        // 공통: 페이드 인/아웃
        float targetAlpha;

        if (isSelectedColor)
        {
            targetAlpha = 1f;
        }
        else
        {
            targetAlpha = visibleTarget ? deselectedAlpha : 0f;
        }

        cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, appearSpeed * Time.deltaTime);

        // alpha가 아주 작아졌을 때만 비활성화 (즉시 끄지 않음)
        bool shouldShow = cg.alpha > 0.01f;
        if (ui.gameObject.activeSelf != shouldShow)
            ui.gameObject.SetActive(shouldShow);

    }

    void OnDestroy()
    {
        if (created && ui)
            Destroy(ui.gameObject);
    }


    public void ObjInteracted()
    {
        /*
        interactCount++;
        if(interactCount >= interactCountMax)
        {
            title = null;
            subTitle = null;
            description = null;
            toolTip_Object.SetText(this);
        }
        */
    }

    public void UpdateToolTipUI()
    {
        if (toolTip_Object != null)
        {
            toolTip_Object.SetText(this);
        }
        else if(!isComponentSelected)
        {
            ComponentSelect();
        }
    }

    public void CheckItemToolTipSelected()
    {
        if (toolTip_Object != null)
        {
            if(interacter.SelectedObj == gameObject)
            {
                if (!isSelectedColor)
                {
                    toolTip_Object.ThisItemSelected(true);
                    ui.SetAsLastSibling();
                    isSelectedColor = true;
                }
            }
            else
            {
                toolTip_Object.ThisItemSelected(false);
                isSelectedColor = false;
            }
        }
    }
}
