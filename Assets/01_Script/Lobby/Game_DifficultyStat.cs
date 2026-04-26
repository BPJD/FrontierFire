using TMPro;
using UnityEngine;
using Michsky.UI.Heat;

public class Game_DifficultyStat : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI enemyHPText, enemydamageText, playerImmuneText;
    [SerializeField] int difficultyLevel = 1;   // 0: Easy, 1: Normal, 2: Hard, 3: Hard+

    [SerializeField] Color minus, normal, plus;

    [SerializeField] LocalizedObject enemyHPLocalized, enemyDamageLocalized, playerImmuneLocalized;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float _hpRevision = Data_Strings.hpRevisionByDifficultyBase[difficultyLevel];
        float _damageIncrease = Data_Strings.damageIncreaseByDifficultyBase[difficultyLevel];
        float _playerImmuneRevision = Data_Strings.playerImmuneRevisionByDifficultyBase[difficultyLevel];

        enemyHPText.color = GetColor(_hpRevision, false);
        enemydamageText.color = GetColor(_damageIncrease, false);
        playerImmuneText.color = GetColor(_playerImmuneRevision, true);

        enemyHPText.text = _hpRevision.ToString("+0%;-0%;+0%");
        enemydamageText.text = _damageIncrease.ToString("+0%;-0%;+0%");
        playerImmuneText.text = _playerImmuneRevision.ToString("+0%;-0%;+0%");
    }

    Color GetColor(float value, bool isColorReversed)
    {
        if (value < 0f)
            return isColorReversed ? plus : minus;
        else if (value > 0f)
            return isColorReversed ? minus : plus;
        else
            return normal;
    }

}
