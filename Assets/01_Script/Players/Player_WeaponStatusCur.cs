using UnityEngine;

public class Player_WeaponStatusCur : MonoBehaviour
{
    public WeaponParams weaponParamsEqupped { get; private set; }
    public void SetParamStat(WeaponParams _params)
    {
        weaponParamsEqupped = _params;
    }

}
