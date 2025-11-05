using UnityEngine;

public class EndBlockGenerator : MonoBehaviour
{
    [SerializeField] GameObject lEnd, rEnd;
    Transform tr;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
        tr.localScale += Vector3.forward * 7f;
        tr.position += Vector3.forward * 2f;

        Vector3 pos = new Vector3(tr.position.x, tr.position.y - (tr.localScale.y * 0.5f), 0f);
        GameObject obj = lEnd;

        if (tr.localPosition.x > 0f)
        {
            obj = rEnd;
        }

        Transform propObj = GameObject.FindGameObjectWithTag("Prop").transform;
        Instantiate(obj, pos, Quaternion.identity, propObj);
        
    }

}
