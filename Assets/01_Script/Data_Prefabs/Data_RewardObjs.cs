using UnityEngine;

public class Data_RewardObjs : MonoBehaviour
{
    [SerializeField] GameObject[] rewardSelects;
    [SerializeField] GameObject[] rewardOnes;




    public GameObject GetRewardObj(Stage_NextStagePortal.RewardType rewardType, StageType stageType)
    {
        int _value = (int)rewardType;

        if (stageType == StageType.Normal)
        {
            return rewardOnes[_value];
        }
        else
        {
            return rewardSelects[_value];
        }
    }
}
