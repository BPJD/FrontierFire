using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class UpgradeTextRow
{
    public int id;
    public string name;
    public string desc;
    public string effect;
}

public class UpgradeTextDB : MonoBehaviour
{
    public static UpgradeTextDB I { get; private set; }

    [Header("CSV Source")]
    [Tooltip("에디터에서 읽을 Assets 상대 경로 (기본: Assets/02_Datas/0_CSV)")]
    [SerializeField] private string editorAssetsFolder = "Assets/02_Datas/0_CSV";

    [Tooltip("파일명 (예: upgrade.csv)")]
    [SerializeField] private string csvFileName = "upgrade.csv";

    [Tooltip("빌드에서는 StreamingAssets에서 읽기 (권장). true면 StreamingAssetsPath 사용")]
    [SerializeField] private bool useStreamingAssetsInBuild = true;

    public bool IsReady { get; private set; } = false;

    private readonly Dictionary<int, UpgradeTextRow> _map = new();
    public event Action OnReady;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        yield return LoadCsvCoroutine();
        IsReady = true;
        OnReady?.Invoke();
    }

    public bool TryGet(int id, out UpgradeTextRow row) => _map.TryGetValue(id, out row);

    IEnumerator LoadCsvCoroutine()
    {
        _map.Clear();

        string text = null;

#if UNITY_EDITOR
        // 에디터: Assets 폴더에서 직접 읽기
        string editorPath = Path.Combine(Directory.GetCurrentDirectory(), editorAssetsFolder, csvFileName);
        editorPath = editorPath.Replace("\\", "/");

        if (!File.Exists(editorPath))
        {
            //Debug.LogError($"[UpgradeTextDB] CSV not found (Editor): {editorPath}\n" +
            //               $"경로 확인: {editorAssetsFolder}/{csvFileName}");
            yield break;
        }

        text = File.ReadAllText(editorPath);
#else
        // 빌드: StreamingAssets 권장
        if (useStreamingAssetsInBuild)
        {
            string path = Path.Combine(Application.streamingAssetsPath, csvFileName);

            // Android/웹은 UnityWebRequest 필요할 수 있음
            if (path.Contains("://") || path.Contains(":///"))
            {
                using var req = UnityWebRequest.Get(path);
                yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    Debug.LogError($"[UpgradeTextDB] CSV load failed: {path}\n{req.error}");
                    yield break;
                }
                text = req.downloadHandler.text;
            }
            else
            {
                if (!File.Exists(path))
                {
                    Debug.LogError($"[UpgradeTextDB] CSV not found (Build): {path}");
                    yield break;
                }
                text = File.ReadAllText(path);
            }
        }
        else
        {
            Debug.LogError("[UpgradeTextDB] Build에서 Assets 경로 읽기는 불가합니다. StreamingAssets로 옮기세요.");
            yield break;
        }
#endif

        if (string.IsNullOrWhiteSpace(text))
        {
            //Debug.LogError("[UpgradeTextDB] CSV is empty.");
            yield break;
        }

        var rows = CsvUtil.Parse(text);
        if (rows.Count == 0) yield break;

        int startIndex = 0;
        if (rows[0].Length > 0 && rows[0][0].Trim().Equals("ID", StringComparison.OrdinalIgnoreCase))
            startIndex = 1;

        for (int i = startIndex; i < rows.Count; i++)
        {
            var r = rows[i];
            if (r.Length < 4) continue;

            int id = SafeInt(r, 0);
            if (id == 0) continue;

            var row = new UpgradeTextRow
            {
                id = id,
                name = SafeStr(r, 1),
                desc = SafeStr(r, 2).Replace("\\n", "\n"),
                effect = SafeStr(r, 3).Replace("\\n", "\n"),
            };

            _map[id] = row;
        }

        //Debug.Log($"[UpgradeTextDB] Loaded: {_map.Count} rows");
    }

    static string SafeStr(string[] r, int idx)
        => (idx >= 0 && idx < r.Length) ? (r[idx] ?? "").Trim() : "";

    static int SafeInt(string[] r, int idx)
    {
        var s = SafeStr(r, idx);
        return int.TryParse(s, out var v) ? v : 0;
    }
}
