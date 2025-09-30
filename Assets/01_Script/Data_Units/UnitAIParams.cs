[System.Serializable]
public class UnitAIParams
{
    public int ai_atkCount;
    public float ai_atkSpeed;
    public float ai_atkDelay;
    public float ai_atkRange;
    public float ai_sightRange;
    public float ai_dropRate;

    public UnitAIParams() { }

    // 복사 생성자
    public UnitAIParams(UnitAIParamsSO src)
    {
        ai_atkCount = src.ai_atkCount;
        ai_atkSpeed = src.ai_atkSpeed;
        ai_atkDelay = src.ai_atkDelay;
        ai_atkRange = src.ai_atkRange;
        ai_sightRange = src.ai_sightRange;
        ai_dropRate = src.ai_dropRate;
    }

    // 깊은 복사 생성자 (기본값 백업용)
    public UnitAIParams(UnitAIParams other)
    {
        ai_atkCount = other.ai_atkCount;
        ai_atkSpeed = other.ai_atkSpeed;
        ai_atkDelay = other.ai_atkDelay;
        ai_atkRange = other.ai_atkRange;
        ai_sightRange = other.ai_sightRange;
        ai_dropRate = other.ai_dropRate;
    }
}
