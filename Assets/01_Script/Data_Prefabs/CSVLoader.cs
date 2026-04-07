using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class UpgradeRow
{
    public int id;
    public string name;
    public string desc;
    public string effect;

    // 디버그/임시용: 값 A~D (없어도 됨)
    public float a;
    public float b;
    public float c;
    public float d;
}

public class CSVLoader : MonoBehaviour
{
    [Header("CSV Location")]
    [Tooltip("StreamingAssets 안의 파일명. 예: upgrade.csv")]
    public string fileName = "upgrade.csv";

    [Header("Loaded Data (Debug)")]
    public List<UpgradeRow> upgrades = new List<UpgradeRow>();

    async void Start()
    {
        // 시작하자마자 로드 (원하면 호출 방식 바꿔도 됨)
        upgrades = await LoadFromStreamingAssetsAsync(fileName);

        //Debug.Log($"[UpgradeCsvLoader] Loaded rows: {upgrades.Count}");
        if (upgrades.Count > 0)
        {
            var u = upgrades[0];
            //Debug.Log($"First: id={u.id}, name={u.name}, desc={u.desc}, effect={u.effect}");
        }
    }

    public async Task<List<UpgradeRow>> LoadFromStreamingAssetsAsync(string csvFileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, csvFileName);

        string text = await ReadAllTextAsync(path);
        if (string.IsNullOrEmpty(text))
        {
            //Debug.LogError($"[UpgradeCsvLoader] CSV is empty or missing: {path}");
            return new List<UpgradeRow>();
        }

        return ParseUpgradeCsv(text);
    }

    private List<UpgradeRow> ParseUpgradeCsv(string csvText)
    {
        var rows = CsvUtil.Parse(csvText);
        var result = new List<UpgradeRow>();
        if (rows.Count == 0) return result;

        // 헤더 처리: "ID,이름,설명,효과,값 A,값 B,값 C,값 D" 같은 형태 가정
        int startIndex = 0;

        // 첫 줄이 헤더인지 체크 (ID 라는 문자열이 있으면 헤더로 간주)
        if (rows[0].Length > 0 && rows[0][0].Trim().Equals("ID", StringComparison.OrdinalIgnoreCase))
            startIndex = 1;

        for (int i = startIndex; i < rows.Count; i++)
        {
            var r = rows[i];
            if (r.Length == 0) continue;

            // 빈 줄(전체 공백) 스킵
            bool allEmpty = true;
            for (int k = 0; k < r.Length; k++)
            {
                if (!string.IsNullOrWhiteSpace(r[k])) { allEmpty = false; break; }
            }
            if (allEmpty) continue;

            // 최소 4열(ID/이름/설명/효과) 필요
            if (r.Length < 4)
            {
                //Debug.LogWarning($"[UpgradeCsvLoader] Row {i} has too few columns: {r.Length}");
                continue;
            }

            var data = new UpgradeRow
            {
                id = ToInt(r, 0),
                name = Get(r, 1),
                desc = Get(r, 2),
                effect = Get(r, 3),

                // 값 A~D는 없을 수도 있어서 안전하게
                a = ToFloat(r, 4),
                b = ToFloat(r, 5),
                c = ToFloat(r, 6),
                d = ToFloat(r, 7),
            };

            // 효과에서 "\n" 문자열을 실제 줄바꿈으로 치환하고 싶으면:
            // data.effect = data.effect.Replace("\\n", "\n");

            result.Add(data);
        }

        return result;
    }

    private static string Get(string[] r, int idx)
        => (idx >= 0 && idx < r.Length) ? (r[idx] ?? "").Trim() : "";

    private static int ToInt(string[] r, int idx)
    {
        string s = Get(r, idx);
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) return v;
        if (int.TryParse(s, out v)) return v;
        return 0;
    }

    private static float ToFloat(string[] r, int idx)
    {
        string s = Get(r, idx);
        if (string.IsNullOrEmpty(s)) return 0f;

        // "1,5" 같은 로케일 방지: 우선 Invariant로 시도, 실패 시 일반 파싱
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return v;
        if (float.TryParse(s, out v)) return v;
        return 0f;
    }

    private static async Task<string> ReadAllTextAsync(string path)
    {
        // Android StreamingAssets는 파일 IO가 아니라 UnityWebRequest가 필요할 수 있음.
        // 하지만 "임시 디버그" 용도라면 PC/에디터 기준으로 아래로도 충분함.
        // (Android까지 필요하면 말해줘. UnityWebRequest 버전도 바로 줌.)

        try
        {
            return await Task.Run(() => File.ReadAllText(path));
        }
        catch (Exception e)
        {
            //Debug.LogError($"[UpgradeCsvLoader] Read failed: {path}\n{e}");
            return "";
        }
    }
}
