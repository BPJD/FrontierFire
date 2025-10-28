using UnityEngine;

namespace Combat
{
    [System.Serializable]
    public struct DamagePayload
    {
        public int baseDamage;                         // 호출자가 계산 전 “기본 공격력”
        public WeaponParamsSO.Ammos ammo;
        public WeaponParamsSO.AtkTypes atkType;
        public bool isCritical;
        public bool isWeakPoint;
        public bool isBlocked;
        public Vector3 hitPoint;

        // 선택: 외부 보정 (버프/디버프)
        public int addFlat;        // +고정
        public float mul;          // ×배수 (기본 1)

        public static DamagePayload Create(
            int baseDamage,
            WeaponParamsSO.Ammos ammo,
            WeaponParamsSO.AtkTypes atkType,
            bool isCritical = false,
            bool isWeakPoint = false,
            bool isBlocked = false,
            Vector3? hitPoint = null,
            int addFlat = 0,
            float mul = 1f)
        {
            return new DamagePayload
            {
                baseDamage = baseDamage,
                ammo = ammo,
                atkType = atkType,
                isCritical = isCritical,
                isWeakPoint = isWeakPoint,
                hitPoint = hitPoint ?? Vector3.zero,
                addFlat = addFlat,
                mul = mul
            };
        }
    }

    public struct DamageResult
    {
        public int finalDamage;
        public int damageTier;      // 사운드/이펙트 선택용
        public bool isCritical;
        public bool killed;
    }
}