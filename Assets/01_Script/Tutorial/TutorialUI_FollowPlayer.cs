using UnityEngine;
using UnityEngine.UI;

public class TutorialUI_FollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("UI")]
    [SerializeField] private RectTransform uiRect;
    [SerializeField] private Canvas canvas;

    [Header("Offset")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.5f, 0f);

    private Camera cam;

    private void Start()
    {
        if (uiRect == null)
            uiRect = GetComponent<RectTransform>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            cam = null;
        else
            cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
    }

    private void FixedUpdate()
    {
        if (target == null || uiRect == null || canvas == null)
        {
            target = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).transform;
            uiRect = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            return;
        }
            

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // 대상이 카메라 뒤에 있으면 UI 숨김
        if (screenPos.z < 0f)
        {
            uiRect.gameObject.SetActive(false);
            return;
        }
        else
        {
            if (!uiRect.gameObject.activeSelf)
                uiRect.gameObject.SetActive(true);
        }

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            uiRect.position = screenPos;
        }
        else
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                cam,
                out localPos
            );

            uiRect.localPosition = localPos;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}