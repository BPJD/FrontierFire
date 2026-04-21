using UnityEngine;
using TMPro; // TextMeshPro 쓴다면
using UnityEngine.UI;
using System.Collections.Generic;

public class Item_ToolTip : MonoBehaviour
{
    [Header("Tooltip Content")]
    public Image toolTip_icon;
    public string title = "Item Name";
    public string subTitle = "Item Name";
    [TextArea] public string description = "Item description...";
    public Color titleColor = Color.white;

    [Header("Weapon ToolTip Content")]
    public List<string> weaponStat = new List<string>();
    public List<int> weaponStatIds = new List<int>();
    //public string[] weaponStat = new string[10];

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
    public UI_NormalToolTipTextSet.SettedAction normalToolTip_action;
    public UI_NormalToolTipTextSet.SettedKey normalToolTip_key;


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

    public WeaponParams thisItemWeaponParams { get; set; }

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
            var p = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
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
        if (!created || !player || !cam || interacter == null) return;

        Vector3 worldPos = transform.position + worldOffset;

        // 1) 거리 체크
        float sqrDist = (player.position - worldPos).sqrMagnitude;
        bool closeEnough = sqrDist <= interacter.uiShowDistance * interacter.uiShowDistance;

        // 2) 카메라 전면 체크
        Vector3 camToTarget = worldPos - cam.transform.position;
        bool inFront = Vector3.Dot(cam.transform.forward, camToTarget) > 0f;

        // 3) LOS 체크
        bool hasLOS = true;
        if (requireLineOfSight && inFront && closeEnough)
        {
            if (Physics.Raycast(
                cam.transform.position,
                camToTarget.normalized,
                out RaycastHit hit,
                camToTarget.magnitude,
                losMask))
            {
                if (hit.transform != transform && !hit.transform.IsChildOf(transform))
                    hasLOS = false;
            }
        }

        visibleTarget = closeEnough && inFront && hasLOS;

        // 실제 선택 여부는 항상 별도로 계산
        bool isActuallySelected = interacter.SelectedObj == gameObject;

        // 안 보이면 선택 상태 자체를 끊어준다
        if (!visibleTarget)
        {
            if (isActuallySelected)
            {
                interacter.SelectedObj = null;
                isActuallySelected = false;
            }

            if (toolTip_Object != null && isSelectedColor)
            {
                toolTip_Object.ThisItemSelected(false);
            }

            isSelectedColor = false;
        }
        else
        {
            // 보일 때만 위치 갱신
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            Vector2 clamped = new Vector2(
                Mathf.Clamp(screenPos.x, screenClamp.x, Screen.width - screenClamp.x),
                Mathf.Clamp(screenPos.y, screenClamp.y, Screen.height - screenClamp.y)
            );

            ui.position = clamped;

            // 보이는 상태에서 선택 강조 갱신
            if (toolTip_Object != null)
            {
                if (isActuallySelected)
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
                    if (isSelectedColor)
                    {
                        toolTip_Object.ThisItemSelected(false);
                        isSelectedColor = false;
                    }
                }
            }
        }

        // 알파는 visibleTarget 우선
        float targetAlpha = 0f;
        if (visibleTarget)
        {
            targetAlpha = isSelectedColor ? 1f : deselectedAlpha;
        }

        cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, appearSpeed * Time.deltaTime);

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
