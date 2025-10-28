using UnityEngine;

public class Data_WeaponUpgradeModels : MonoBehaviour
{
    [SerializeField] GameObject[] statUpModels;
    [SerializeField] GameObject[] classEfts;
    [SerializeField] GameObject[] classAppearEfts;
    [SerializeField] GameObject[] classGetEfts;

    public GameObject GetStatUpObj(int category)
    {
        return statUpModels[category];
    }

    public GameObject GetClassEft(int statClass)
    {
        return classEfts[statClass];
    }

    public GameObject GetClassAppearEft(int statClass)
    {
        return classAppearEfts[statClass];
    }

    public void InstanceStatUpObj(Transform tr, int category, int statClass)
    {
        Instantiate(statUpModels[category], tr.position, tr.rotation, tr);

        if (classEfts[statClass] != null)
        {
            Instantiate(classEfts[statClass], tr.position, tr.rotation, tr);
        }

        if (classAppearEfts[statClass] != null)
        {
            Instantiate(classAppearEfts[statClass], tr.position, tr.rotation, tr);
        }
    }

    public GameObject GetClassGetEft(int statClass)
    {
        return classGetEfts[statClass];
    }
}
