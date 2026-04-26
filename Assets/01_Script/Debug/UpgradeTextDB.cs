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
    [Tooltip("에디터에서 읽을 Assets 상대 경로")]
    [SerializeField] private string editorAssetsFolder = "Assets/02_Datas/0_CSV";

    [Tooltip("파일명")]
    [SerializeField] private string csvFileName = "upgrade.csv";

    [Tooltip("빌드에서는 StreamingAssets에서 읽기")]
    [SerializeField] private bool useStreamingAssetsInBuild = true;

    public bool IsReady { get; private set; } = false;

    private readonly Dictionary<int, UpgradeTextRow> _map = new Dictionary<int, UpgradeTextRow>();

    public event Action OnReady;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        yield return LoadCsvCoroutine();

        IsReady = true;
        OnReady?.Invoke();
    }

    public bool TryGet(int id, out UpgradeTextRow row)
    {
        return _map.TryGetValue(id, out row);
    }

    private IEnumerator LoadCsvCoroutine()
    {
        _map.Clear();

        string text = null;

#if UNITY_EDITOR
        string editorPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            editorAssetsFolder,
            csvFileName
        );

        editorPath = editorPath.Replace("\\", "/");

        if (!File.Exists(editorPath))
        {
            Debug.LogError($"[UpgradeTextDB] CSV not found (Editor): {editorPath}");
            yield break;
        }

        text = File.ReadAllText(editorPath);
#else
        if (useStreamingAssetsInBuild)
        {
            string path = Path.Combine(Application.streamingAssetsPath, csvFileName);
            path = path.Replace("\\", "/");

            if (path.Contains("://") || path.Contains(":///"))
            {
                UnityWebRequest req = UnityWebRequest.Get(path);
                yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                bool hasError = req.result != UnityWebRequest.Result.Success;
#else
                bool hasError = req.isNetworkError || req.isHttpError;
#endif

                if (hasError)
                {
                    Debug.LogError($"[UpgradeTextDB] CSV load failed: {path}\n{req.error}");
                    req.Dispose();
                    yield break;
                }

                text = req.downloadHandler.text;
                req.Dispose();
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
            Debug.LogError("[UpgradeTextDB] Build에서는 Assets 경로를 직접 읽을 수 없습니다. StreamingAssets를 사용하세요.");
            yield break;
        }
#endif

        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogError("[UpgradeTextDB] CSV is empty.");
            yield break;
        }

        var rows = CsvUtil.Parse(text);

        if (rows == null || rows.Count == 0)
            yield break;

        int startIndex = 0;

        if (rows[0].Length > 0 &&
            rows[0][0].Trim().Equals("ID", StringComparison.OrdinalIgnoreCase))
        {
            startIndex = 1;
        }

        for (int i = startIndex; i < rows.Count; i++)
        {
            string[] r = rows[i];

            if (r == null || r.Length < 4)
                continue;

            int id = SafeInt(r, 0);

            if (id == 0)
                continue;

            UpgradeTextRow row = new UpgradeTextRow
            {
                id = id,
                name = SafeStr(r, 1),
                desc = SafeStr(r, 2).Replace("\\n", "\n"),
                effect = SafeStr(r, 3).Replace("\\n", "\n")
            };

            _map[id] = row;
        }
    }

    private static string SafeStr(string[] r, int idx)
    {
        if (r == null || idx < 0 || idx >= r.Length)
            return "";

        return (r[idx] ?? "").Trim();
    }

    private static int SafeInt(string[] r, int idx)
    {
        string s = SafeStr(r, idx);
        return int.TryParse(s, out int v) ? v : 0;
    }
}