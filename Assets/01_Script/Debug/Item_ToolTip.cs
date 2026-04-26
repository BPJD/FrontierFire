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
        EnsureCreated();
        UpdateToolTipUI();
    }

    void ComponentSelect()
    {
        if (isComponentSelected)
            return;

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
            if (p) player = p.transform;
        }

        cam = Camera.main;

        if (player != null)
            interacter = player.gameObject.GetComponentInChildren<PlayerInteract>();

        if (rootCanvas == null)
        {
            var canvasObj = GameObject.FindGameObjectWithTag("ItemUICanvas");
            if (canvasObj != null)
                rootCanvas = canvasObj.GetComponent<Canvas>();
        }

        if (rootCanvas == null)
            return;

        var dataToolTips = rootCanvas.GetComponent<DataToolTips>();
        if (dataToolTips == null)
            return;

        tooltipPrefab = dataToolTips.GetToolTipData(tooltipType);

        if (tooltipPrefab == null)
            return;

        ui = Instantiate(tooltipPrefab, rootCanvas.transform);
        created = true;

        cg = ui.GetComponent<CanvasGroup>();
        if (!cg)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        toolTip_Object = ui.gameObject.GetComponent<UI_ToolTip_Object>();

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        ui.gameObject.SetActive(true);

        isComponentSelected = true;

    }



    void LateUpdate()
    {
        if (!created || !player || !cam || interacter == null || ui == null || cg == null)
            return;

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

        bool prevVisibleTarget = visibleTarget;
        visibleTarget = closeEnough && inFront && hasLOS;

        // UI 오브젝트는 비활성화하지 않고 항상 활성 유지
        if (!ui.gameObject.activeSelf)
            ui.gameObject.SetActive(true);

        // 실제 선택 여부
        bool isActuallySelected = interacter.SelectedObj == gameObject;

        // 안 보이면 선택 상태 해제
        if (!visibleTarget)
        {
            if (isActuallySelected)
            {
                interacter.SelectedObj = null;
                isActuallySelected = false;
            }

            if (toolTip_Object != null && isSelectedColor)
                toolTip_Object.ThisItemSelected(false);

            isSelectedColor = false;
        }
        else
        {
            // 보이기 시작하는 순간 툴팁 텍스트 강제 갱신
            if (!prevVisibleTarget)
                UpdateToolTipUI();

            // 위치 갱신
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            Vector2 clamped = new Vector2(
                Mathf.Clamp(screenPos.x, screenClamp.x, Screen.width - screenClamp.x),
                Mathf.Clamp(screenPos.y, screenClamp.y, Screen.height - screenClamp.y)
            );

            ui.position = clamped;

            // 선택 강조 갱신
            if (toolTip_Object != null)
            {
                if (isActuallySelected)
                {
                    if (!isSelectedColor)
                    {
                        toolTip_Object.ThisItemSelected(true);
                        ui.SetAsLastSibling();
                        isSelectedColor = true;

                        // 선택 상태가 되는 순간에도 한 번 더 갱신
                        UpdateToolTipUI();
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

        // 알파만 조절
        float targetAlpha = 0f;

        if (visibleTarget)
            targetAlpha = isSelectedColor ? 1f : deselectedAlpha;

        cg.alpha = Mathf.MoveTowards(
            cg.alpha,
            targetAlpha,
            appearSpeed * Time.deltaTime
        );

        // 클릭/레이캐스트 방지
        cg.blocksRaycasts = false;
        cg.interactable = false;
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
        EnsureCreated();

        if (toolTip_Object == null)
            return;

        toolTip_Object.SetText(this);
    }

    private void EnsureCreated()
    {
        if (!isComponentSelected || toolTip_Object == null || ui == null)
            ComponentSelect();
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
