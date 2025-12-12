using UnityEngine;
using UnityEngine.UI;

public class Item_WeaponCompare : MonoBehaviour
{

    public WeaponParams weaponParamsItem { get; private set; }

    [SerializeField] Image[] img_CompareStats = new Image[8];

    [SerializeField] Sprite img_Up, img_Equal, img_Down;
    [SerializeField] Color up, equal, down;
    

    public void SetParamStat(bool isPlayerEquip, WeaponParams _params)
    {
        if (isPlayerEquip)
        {
            
        }
        else
        {
            weaponParamsItem = _params;
        }

    }

    public void IconSet(int _stat, int _compare)
    {
        Color _up = up;
        Color _equal = equal;
        Color _down = down;

        if (_stat == 6)
        {
            _up = down;
            _down = up;
        }


        switch (_compare)
        {
            case > 0:
                img_CompareStats[_stat].sprite = img_Up;
                img_CompareStats[_stat].color = _up;
                break;
            case 0:
                img_CompareStats[_stat].sprite = img_Equal;
                img_CompareStats[_stat].color = _equal;
                break;
            case < 0:
                img_CompareStats[_stat].sprite = img_Down;
                img_CompareStats[_stat].color = _down;
                break;
        }
    }
}
