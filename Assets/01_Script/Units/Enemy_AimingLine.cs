using UnityEngine;
using System.Collections;

public class Enemy_AimingLine : MonoBehaviour
{
    LineRenderer line;

    public Transform targetTr { get; private set; }
    public Transform fireTr { get; private set; }

    Vector3 pinPoint;

    public bool isLineDraw = false;
    bool isLineBlink = false;

    bool isTargetSet = false;

    WaitForSeconds blinkDelay = new WaitForSeconds(0.05f);

    Vector3 targetPosRevision = new Vector3(0f, 1.5f, 0f);

    public void SetTransforms(Transform target, Transform fire)
    {
        targetTr = target;
        fireTr = fire;
        if(targetTr != null && fireTr != null)
        {
            isTargetSet = true;
        }
        else
        {
            isTargetSet = false;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isLineDraw && isTargetSet && !isLineBlink)
        {
            line.enabled = true;
            line.SetPosition(0, fireTr.position);
            line.SetPosition(1, targetTr.position + targetPosRevision);
        }
        else if (isLineBlink)
        {
            line.SetPosition(0, fireTr.position);
            line.SetPosition(1, pinPoint + targetPosRevision);
        }
    }

    public Vector3 PinPoint()
    {
        return new Vector3(pinPoint.x, pinPoint.y, 0f);
    }

    IEnumerator LineBlink()
    {
        pinPoint = targetTr.position;
        isLineBlink = true;
        while (isLineBlink)
        {
            line.enabled = false;
            yield return blinkDelay;
            line.enabled = true;
            yield return blinkDelay;
        }
    }

    public void Blink(bool isBlink, bool isDead)
    {
        if (isBlink && !isDead)
        {
            StartCoroutine(LineBlink());
        }
        else
        {
            isLineBlink = false;
        }
    }



    
}
