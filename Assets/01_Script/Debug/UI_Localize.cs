using UnityEngine;
using Michsky.UI.Heat; // Heat UI namespace

public class UI_Localize : MonoBehaviour
{
    [SerializeField] private LocalizationManager locManager;

    void Start()
    {
        // Set language
        locManager.SetLanguage("ko-KR");

        // Set language without notifying
        // No reference required for this call as it's static
        LocalizationManager.SetLanguageWithoutNotify("ko-KR");

    }
}
