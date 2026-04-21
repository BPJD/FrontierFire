using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadingSceneController
{
    public static string nextSceneName = "Scene_Lobby";

    private static bool isLoading = false;

    public static void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("LoadScene called with null or empty sceneName.");
            return;
        }

        isLoading = true;
        nextSceneName = sceneName;

        SceneManager.LoadScene("Scene_Loading");
    }

    public static void Reset()
    {
        isLoading = false;
    }
}