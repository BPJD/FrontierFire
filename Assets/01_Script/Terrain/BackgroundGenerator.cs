using UnityEngine;
using System.Collections;

public class BackgroundGenerator : MonoBehaviour
{
    Transform camTr;
    [SerializeField] Transform planeObj;
    [SerializeField] Transform mountainObj;

    [SerializeField] GameObject[] nearPlanes;
    [SerializeField] GameObject[] farPlanes;
    [SerializeField] GameObject[] endObjs;

    [SerializeField] float endObjRate = 0.45f;
    int MaxX = 0;
    int MinX = 0;

    int planeCount = 7;

    float interval = 141.42f;
    int camCountMax = 2;
    int camCountMin = 2;

    [SerializeField] float endObjPosRangeMax = 510f;
    [SerializeField] float endObjPosRangeMin = 300f;

    [SerializeField] bool dontGenerate = false;



    WaitForSeconds delay = new WaitForSeconds(0.1f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        camTr = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        MaxX = camCountMax * 100;
        MinX = -(camCountMin * 100);
    }
    void Start()
    {
        if (dontGenerate) return;
        GenerateStartDisplay();
        StartCoroutine(GenerateTerrain());
    }

    IEnumerator GenerateTerrain()
    {
        while (true)
        {
            if(camTr.position.x >= interval * (camCountMax - 2))
            {
                AncorX();
            }

            if (camTr.position.x <= -(interval * (camCountMin - 2)))
            {
                AncorZ();
            }

            yield return delay;
        }
    }

    void GenerateStartDisplay()
    {
        int _startX = 100;
        int _startZ = -100;

        for(int i = -1; i < planeCount + 1; i++)
        {
            for (int j = 0; j < (planeCount - i) + 1; j++)
            {
                if (planeCount - i > 3) //멀리있으면 가파른 언덕이 만들어지게 하고싶음.
                {
                    GameObject obj = Instantiate(GenerateNearPlane(), planeObj);
                    obj.transform.localPosition = new Vector3(_startX - (100 * j), 0f, _startZ);
                }
                else
                {
                    GameObject obj = Instantiate(GenerateFarPlane(), planeObj);
                    obj.transform.localPosition = new Vector3(_startX - (100 * j), 0f, _startZ);

                    GameObject endObj = GenerateEndObjs();
                    if (endObj != null)
                    {
                        float randX = Random.Range(-600f, 600f);
                        float randZ = Random.Range(endObjPosRangeMin, endObjPosRangeMax);
                        float randY = Random.Range(-5f, -15f);
                        Vector3 pos = transform.position + new Vector3(randX, randY, randZ);
                        float randRotY = Random.Range(0f, 360f);
                        Instantiate(endObj, pos, Quaternion.Euler(Vector3.up * randRotY));
                    }
                }
            }
            _startZ += 100;
        }
    }

    GameObject GenerateNearPlane()
    {
        int rand = Random.Range(0, nearPlanes.Length);
        return nearPlanes[rand];
    }

    GameObject GenerateFarPlane()
    {
        float farPlaneRate = 0.65f;
        if(Random.Range(0f, 1f) <= farPlaneRate)
        {
            int rand = Random.Range(0, farPlanes.Length);
            return farPlanes[rand];
        }
        else
        {
            return GenerateNearPlane();
        }
    }

    GameObject GenerateEndObjs()
    {
        if (Random.Range(0f, 1f) <= endObjRate)
        {
            int rand = Random.Range(0, endObjs.Length);
            return endObjs[rand];
        }
        else
        {
            return null;
        }
    }

    void AncorX()
    {
        for(int i = -2; i < planeCount; i++)
        {
            if(i < 5)
            {
                GameObject obj = Instantiate(GenerateNearPlane(), planeObj);
                obj.transform.localPosition = new Vector3(MaxX, 0f, MaxX + (100f * i));
            }
            else
            {
                GameObject obj = Instantiate(GenerateFarPlane(), planeObj);
                obj.transform.localPosition = new Vector3(MaxX, 0f, MaxX + (100f * i));

                GameObject endObj = GenerateEndObjs();
                if(endObj != null)
                {
                    float randX = Random.Range(camTr.position.x + 600f, camTr.position.x + 900f);
                    float randZ = Random.Range(endObjPosRangeMin, endObjPosRangeMax);
                    Vector3 pos = new Vector3(randX, -5f, randZ);
                    float randRotY = Random.Range(0f, 360f);
                    Instantiate(endObj, pos, Quaternion.Euler(Vector3.up * randRotY), mountainObj);
                }
            }
        }
        MaxX += 100;
        camCountMax++;
    }

    void AncorZ()
    {
        for (int i = -2; i < planeCount; i++)
        {
            if (i < 5)
            {
                GameObject obj = Instantiate(GenerateNearPlane(), planeObj);
                obj.transform.localPosition = new Vector3((MinX + -(100f * i)), 0f, MinX);
            }
            else
            {
                GameObject obj = Instantiate(GenerateFarPlane(), planeObj);
                obj.transform.localPosition = new Vector3((MinX + -(100f * i)), 0f, MinX);

                GameObject endObj = GenerateEndObjs();
                if (endObj != null)
                {
                    float randX = Random.Range(camTr.position.x - 600f, camTr.position.x - 900f);
                    float randZ = Random.Range(endObjPosRangeMin, endObjPosRangeMax);
                    float randY = Random.Range(-5f, -15f);
                    Vector3 pos = new Vector3(randX, randY, randZ);
                    float randRotY = Random.Range(0f, 360f);
                    Instantiate(endObj, pos, Quaternion.Euler(Vector3.up * randRotY));
                }
            }
        }
        MinX -= 100;
        camCountMin++;
    }
    
}
