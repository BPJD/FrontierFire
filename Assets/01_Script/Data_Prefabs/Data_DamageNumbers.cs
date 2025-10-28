using DamageNumbersPro;
using UnityEngine;

public class Data_DamageNumbers : MonoBehaviour
{
    public enum NumberType { Default, Normal, SemiDefend, FullDefend, Blocked, Critical, WeakPoint, Heal }

    [SerializeField] DamageNumber[] numberPrefabs;

    /*

    0 : 기본(거의 안씀, 테스트용)
    1 : 일반피해
    2 : 일부 반감
    3 : 대부분 반감
    4 : 막힘
    5 : 치명타
    6 : 약점
    7 : 힐

    */

    public DamageNumber GetDamageNumberPrefab(NumberType type)
    {
        return numberPrefabs[(int)type];
    }
}
