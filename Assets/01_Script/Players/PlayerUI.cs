using UnityEngine;
using Michsky.UI.Heat; // Heat UI namespace

public class PlayerUI : MonoBehaviour
{
    [SerializeField] UnitStatus playerStatus;
    [SerializeField] private ProgressBar myBar;

    void Start()
    {
        playerStatus.OnHpChanged += UpdateHpBar;
        UpdateHpBar(playerStatus.hpCur, playerStatus.hpCur);
    }

    void UpdateHpBar(int currentHp, int maxHp)
    {
        myBar.minValue = 0;
        myBar.maxValue = maxHp;

        myBar.SetValue(currentHp);
        myBar.UpdateUI();

    }

    void OnDestroy()
    {
        playerStatus.OnHpChanged -= UpdateHpBar;
    }
}
