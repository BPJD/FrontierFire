using System.Collections.Generic;
using UnityEngine;
using static StatUpgradesSO;

[System.Serializable]
public struct RarityWeight
{
    public StatUpgradesSO.StatItemTier itemTier;
    [Min(0f)] public float weight; // 0 이면 안 나옴
}

public class Data_StatUpgrades : MonoBehaviour
{
    [SerializeField]
    private List<StatUpSOEntry> statUpEntries = new List<StatUpSOEntry>();

    private Dictionary<int, StatUpgradesSO> statUpDict;




    public readonly string localize_statUp = "Stat_";
    public readonly string[] loczlize_Stats =
        { "hp", "atk", "def", "immune", "armorlv", "speed", "jump", "multijump", "acc", "crirate",
        "cridmg", "dmg", "ammoget", "ammomax", "droprate", "heal", "healrate" };

    [SerializeField]
    RarityWeight[] defaultRarityWeights = new RarityWeight[]
    {
        new RarityWeight{ itemTier = StatItemTier.D,  weight = 55f },
        new RarityWeight{ itemTier = StatItemTier.C,  weight = 25f },
        new RarityWeight{ itemTier = StatItemTier.B,  weight = 12f },
        new RarityWeight{ itemTier = StatItemTier.A,  weight = 12f },
        new RarityWeight{ itemTier = StatItemTier.S,  weight = 6f  },
        new RarityWeight{ itemTier = StatItemTier.SS, weight = 2f  }
    };

    // 등급별 버킷 (id 리스트)
    private Dictionary<StatItemTier, HashSet<int>> rarityBuckets;
    private bool bucketsBuilt = false;



    void EnsureRarityBuckets()
    {
        if (bucketsBuilt) return;
        rarityBuckets = new Dictionary<StatItemTier, HashSet<int>>();
        foreach (StatItemTier r in System.Enum.GetValues(typeof(StatItemTier)))
            rarityBuckets[r] = new HashSet<int>();

        for (int i = 0; i < statUpEntries.Count; i++)
        {
            var e = statUpEntries[i];
            if (e == null || e.statUp == null) continue;
            var so = e.statUp;

            // 같은 statUpID가 여러 SO에 있어도 HashSet이 1번만 보관
            rarityBuckets[(StatItemTier)so.up_tier].Add(e.statUpID);
        }
        bucketsBuilt = true;
    }

    // --- 새로 추가: 등급 굴림 / 등급 내 랜덤 선택 ---

    public StatItemTier RollRarity(RarityWeight[] weights = null)
    {
        var ws = (weights == null || weights.Length == 0) ? defaultRarityWeights : weights;

        float total = 0f;
        foreach (var w in ws) total += Mathf.Max(0f, w.weight);
        if (total <= 0f) return StatItemTier.D;

        float roll = Random.value * total;
        foreach (var w in ws)
        {
            float ww = Mathf.Max(0f, w.weight);
            if (roll < ww) return w.itemTier;
            roll -= ww;
        }
        return ws[ws.Length - 1].itemTier; // 부동소수 안전장치
    }

    public int GetRandomIdByRarity(StatItemTier rarity, bool allowFallbackToNearest = true)
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
        StatItemTier[] ordered =
        {
        StatItemTier.D, StatItemTier.C, StatItemTier.B, StatItemTier.A, StatItemTier.S, StatItemTier.SS
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

    // “등급 굴려서 → 그 등급에서 하나”까지 한 번에
    public int GetRandomIdByRolledRarity(RarityWeight[] weights = null, bool allowFallbackToNearest = true)
    {
        var r = RollRarity(weights);
        return GetRandomIdByRarity(r, allowFallbackToNearest);
    }

    void Awake()
    {
        BuildIdDict();
    }

    void BuildIdDict()
    {
        statUpDict = new Dictionary<int, StatUpgradesSO>();
        foreach (var entry in statUpEntries)
        {
            if (entry == null || entry.statUp == null) continue;
            if (!statUpDict.ContainsKey(entry.statUpID))
                statUpDict.Add(entry.statUpID, entry.statUp);
        }
    }

    public StatUpgradesSO GetStatUp(int id)
    {
        if (statUpDict.TryGetValue(id, out StatUpgradesSO prefab))
            return prefab;

        return null;
    }

    public int GetStatUpCount()
    {
        return statUpEntries.Count;
    }

    public int GetWeaponIDbyList(int array)
    {
        return statUpEntries[array].statUpID;
    }

    public List<StatUpgradesSO> GetAllStatUps(int id)
    {
        var results = new List<StatUpgradesSO>();
        if (statUpEntries == null) return results;

        foreach (var e in statUpEntries)
        {
            if (e == null || e.statUp == null) continue;
            if (e.statUpID == id) results.Add(e.statUp);
        }

        if (results.Count == 0)
            Debug.LogWarning($"[Data_StatUpgrades] ID {id} 패키지 SO 없음");
        return results;
    }

    // 3) 안전 랜덤 헬퍼
    int RandomIdFromSet(HashSet<int> set)
    {
        if (set == null || set.Count == 0)
        {
            if (statUpEntries == null || statUpEntries.Count == 0)
            {
                Debug.LogWarning("[Data_StatUpgrades] 아이템 데이터가 비었습니다.");
                return -1;
            }
            return statUpEntries[Random.Range(0, statUpEntries.Count)].statUpID;
        }

        int idx = Random.Range(0, set.Count);
        int i = 0;
        foreach (var id in set) // HashSet은 인덱스 접근 없음
        {
            if (i++ == idx) return id;
        }
        return -1; // 이론상 도달 X
    }
}

[System.Serializable]
public class StatUpSOEntry
{
    public int statUpID;
    public StatUpgradesSO statUp;
}
