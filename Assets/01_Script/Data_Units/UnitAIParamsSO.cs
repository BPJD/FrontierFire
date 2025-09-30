using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitAIParams", menuName = "Data/UnitAIParams")]
public class UnitAIParamsSO : ScriptableObject
{
    public int ai_atkCount;
    public float ai_atkSpeed;
    public float ai_atkDelay;
    public float ai_atkRange;
    public float ai_sightRange;
    public float ai_dropRate;
}
