using UnityEngine;

public class Data_WeaponUpgradeModels : MonoBehaviour
{
    [SerializeField] GameObject[] statUpModels;
    [SerializeField] GameObject[] classEfts;
    [SerializeField] GameObject[] classAppearEfts;
    [SerializeField] GameObject[] classGetEfts;
    [SerializeField] GameObject[] weaponEfts;

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

    public GameObject GetWeaponEft(int upgradeCount)
    {
        switch (upgradeCount)
        {
            case 0:
                return null;
            case 1:
            case 2:
                return weaponEfts[0];

            case 3:
            case 4:
                return weaponEfts[1];

            case >= 5:
                return weaponEfts[2];

            default:
                return null;

        }
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
