public static class Data_Strings
{
    public const string playerTag = "Player";
    public const string DeadUnitTag = "Dead";
    public const string DataObjTag = "Data";
    public const string UnitTag = "Unit";
    public const string WeakPointTag = "WeakPoint";
    public const string terrainTag = "Terrain";
    public const string shieldTag = "Shield";
    public const string soundTag = "Sound";

    public const string gameDifficultyKey = "GameDifficulty";
    public static float[] hpRevisionByDifficultyBase = { -0.25f, 0f, 0.2f, 0.4f }; // 난이도에 따른 HP 증가 비율 기본값 (예: 0.1f는 10% 증가)
    public static float[] damageIncreaseByDifficultyBase = { 0f, 0f, 0.1f, 0.2f }; // 난이도에 따른 데미지 증가 비율 기본값 (예: 0.2f는 20% 증가)
    public static float[] playerImmuneRevisionByDifficultyBase =
    {
    -0.25f, // Easy
     0f,   // Normal
     0.05f,// Hard
     0.3f  // Hard+
};

}
