using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ItemTier를 어디서 가져오는지에 맞춰 수정하세요.
// 예) StatUpgradesSO 안에 enum이 있다면: using static StatUpgradesSO;
using static WeaponStatUpgradesSO;

[System.Serializable]
public struct WeaponRarityWeight
{
    public WeaponItemTier itemTier;
    [Min(0f)] public float weight; // 0이면 안 나옴
}

public class Data_WeaponStatUpgrades : MonoBehaviour
{
    [SerializeField]
    private List<WeaponStatUpgradeEntry> upgradeEntries = new List<WeaponStatUpgradeEntry>();

    private Dictionary<int, WeaponStatUpgradesSO> upgradeDict;

    // =========================
    // (추가) 등급 가중치 기본값
    // =========================
    [SerializeField]
    private WeaponRarityWeight[] defaultRarityWeights = new WeaponRarityWeight[]
    {
        new WeaponRarityWeight{ itemTier = WeaponItemTier.D,  weight = 0f },
        new WeaponRarityWeight{ itemTier = WeaponItemTier.C,  weight = 25f },
        new WeaponRarityWeight{ itemTier = WeaponItemTier.B,  weight = 12f },
        new WeaponRarityWeight{ itemTier = WeaponItemTier.A,  weight = 12f },
        new WeaponRarityWeight{ itemTier = WeaponItemTier.S,  weight = 6f  },
        new WeaponRarityWeight{ itemTier = WeaponItemTier.SS, weight = 2f  }
    };

    // (추가) 등급별 버킷 (id 리스트)
    private Dictionary<WeaponItemTier, HashSet<int>> rarityBuckets;
    private bool bucketsBuilt = false;

    void Awake()
    {
        BuildDict();
    }

    void OnValidate()
    {
        // 에디터에서 값 변경 시 중복/누락 빠르게 검출
        BuildDict();
        bucketsBuilt = false; // 데이터 바뀌면 버킷 재생성
    }

    private void BuildDict()
    {
        if (upgradeDict == null) upgradeDict = new Dictionary<int, WeaponStatUpgradesSO>();
        else upgradeDict.Clear();

        foreach (var entry in upgradeEntries)
        {
            if (entry == null) continue;

            if (entry.statUpID <= 0)
            {
                Debug.LogWarning($"[Data_WeaponStatUpgrades] 유효하지 않은 ID 값: {entry.statUpID}");
                continue;
            }

            if (entry.statUp == null)
            {
                Debug.LogWarning($"[Data_WeaponStatUpgrades] ID {entry.statUpID} 에 매핑된 SO가 비어있습니다.");
                continue;
            }

            if (!upgradeDict.ContainsKey(entry.statUpID))
                upgradeDict.Add(entry.statUpID, entry.statUp);
        }
    }

    /// <summary>
    /// ID로 무기 강화 SO를 가져옵니다. (단일)
    /// </summary>
    public WeaponStatUpgradesSO GetUpgrade(int id)
    {
        if (upgradeDict != null && upgradeDict.TryGetValue(id, out var so))
            return so;

        Debug.LogWarning($"[Data_WeaponStatUpgrades] ID {id} 에 해당하는 강화 SO가 없습니다.");
        return null;
    }

    /// <summary>
    /// 동일 ID에 해당하는 모든 강화 SO들을 리스트로 반환합니다. (패키지 효과 지원)
    /// </summary>
    public List<WeaponStatUpgradesSO> GetAllUpgrades(int id)
    {
        List<WeaponStatUpgradesSO> results = new List<WeaponStatUpgradesSO>();

        if (upgradeEntries == null) return results;

        foreach (var entry in upgradeEntries)
        {
            if (entry == null || entry.statUp == null) continue;
            if (entry.statUpID == id)
                results.Add(entry.statUp);
        }

        if (results.Count == 0)
            Debug.LogWarning($"[Data_WeaponStatUpgrades] ID {id} 에 해당하는 강화 SO가 없습니다.");

        return results;
    }

    /// <summary>
    /// 인스펙터에 등록된 강화 항목 수를 반환합니다.
    /// </summary>
    public int GetUpgradeCount()
    {
        return upgradeEntries?.Count ?? 0;
    }

    // ==========================================================
    // (추가) 등급 버킷 빌드
    // ==========================================================
    private void EnsureRarityBuckets()
    {
        if (bucketsBuilt) return;

        rarityBuckets = new Dictionary<WeaponItemTier, HashSet<int>>();
        foreach (WeaponItemTier r in System.Enum.GetValues(typeof(WeaponItemTier)))
            rarityBuckets[r] = new HashSet<int>();

        for (int i = 0; i < upgradeEntries.Count; i++)
        {
            var e = upgradeEntries[i];
            if (e == null || e.statUp == null) continue;

            var so = e.statUp;

            // WeaponStatUpgradesSO의 등급 필드명이 다르면 수정:
            // 예: so.up_tier, so.tier, so.w_tier 등
            var tier = (WeaponItemTier)so.up_tier;

            // 같은 statUpID가 여러 SO에 있어도 HashSet이 1번만 보관
            rarityBuckets[tier].Add(e.statUpID);
        }

        bucketsBuilt = true;
    }

    // ==========================================================
    // (추가) 등급 굴림 / 등급 내 랜덤 선택
    // ==========================================================
    public WeaponItemTier RollRarity(WeaponRarityWeight[] weights = null)
    {
        var ws = (weights == null || weights.Length == 0) ? defaultRarityWeights : weights;

        float total = 0f;
        foreach (var w in ws) total += Mathf.Max(0f, w.weight);
        if (total <= 0f) return WeaponItemTier.D;

        float roll = Random.value * total;
        foreach (var w in ws)
        {
            float ww = Mathf.Max(0f, w.weight);
            if (roll < ww) return w.itemTier;
            roll -= ww;
        }

        return ws[ws.Length - 1].itemTier; // 부동소수 안전장치
    }

    public int GetRandomIdByRarity(WeaponItemTier rarity, bool allowFallbackToNearest = true)
    {
        EnsureRarityBuckets();

        // 1) 목표 등급
        if (rarityBuckets.TryGetValue(rarity, out var set) && set != null && set.Count > 0)
            return RandomIdFromSet(set);

        if (!allowFallbackToNearest)
        {
            // 최후 폴백: 전체에서 랜덤 (가드 포함)
            return RandomIdFromSet(null);
        }

        // 2) 인접 등급으로 폴백
        WeaponItemTier[] ordered =
        {
            WeaponItemTier.D, WeaponItemTier.C, WeaponItemTier.B, WeaponItemTier.A, WeaponItemTier.S, WeaponItemTier.SS
        };

        int idx = System.Array.IndexOf(ordered, rarity);
        for (int step = 1; step < ordered.Length; step++)
        {
            int left = idx - step;
            int right = idx + step;

            if (left >= 0)
            {
                var L = rarityBuckets[ordered[left]];
                if (L != null && L.Count > 0) return RandomIdFromSet(L);
            }

            if (right < ordered.Length)
            {
                var R = rarityBuckets[ordered[right]];
                if (R != null && R.Count > 0) return RandomIdFromSet(R);
            }
        }

        // 3) 전부 비었으면 전체에서 랜덤 (가드 포함)
        return RandomIdFromSet(null);
    }

    /// <summary>
    /// 등급 굴려서 → 그 등급에서 하나까지 한 번에
    /// </summary>
    public int GetRandomIdByRolledRarity(WeaponRarityWeight[] weights = null, bool allowFallbackToNearest = true)
    {
        var r = RollRarity(weights);
        return GetRandomIdByRarity(r, allowFallbackToNearest);
    }

    // ==========================================================
    // (추가) 안전 랜덤 헬퍼
    // ==========================================================
    private int RandomIdFromSet(HashSet<int> set)
    {
        if (set == null || set.Count == 0)
        {
            if (upgradeEntries == null || upgradeEntries.Count == 0)
            {
                Debug.LogWarning("[Data_WeaponStatUpgrades] 아이템 데이터가 비었습니다.");
                return -1;
            }

            return upgradeEntries[Random.Range(0, upgradeEntries.Count)].statUpID;
        }

        int idx = Random.Range(0, set.Count);
        int i = 0;
        foreach (var id in set) // HashSet은 인덱스 접근 없음
        {
            if (i++ == idx) return id;
        }

        return -1; // 이론상 도달 X
    }

    // ==========================================================
    // 기존 랜덤(중복 없는 ID)도 남기고 싶으면 유지 가능
    // ==========================================================
    public int GetRandomUpgradeID()
    {
        if (upgradeEntries == null || upgradeEntries.Count == 0)
        {
            Debug.LogWarning("[Data_WeaponStatUpgrades] 등록된 항목이 없습니다.");
            return -1;
        }

        var uniqueIDs = upgradeEntries
            .Where(e => e != null && e.statUpID > 0)
            .Select(e => e.statUpID)
            .Distinct()
            .ToList();

        if (uniqueIDs.Count == 0)
        {
            Debug.LogWarning("[Data_WeaponStatUpgrades] 유효한 ID가 없습니다.");
            return -1;
        }

        int randomIndex = Random.Range(0, uniqueIDs.Count);
        return uniqueIDs[randomIndex];
    }
}

[System.Serializable]
public class WeaponStatUpgradeEntry
{
    public int statUpID;
    public WeaponStatUpgradesSO statUp;
}
