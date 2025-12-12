using UnityEngine;

public class Stage_NextStagePortal : MonoBehaviour, IInteractable
{
    public enum PortalType { Normal, Elite, Boss}
    [SerializeField] PortalType nextMapType = PortalType.Normal;

    AudioSource soundPlayer;
    [SerializeField] AudioClip sound_Interact;


    public enum RewardType { Weapon, Stat, Upgrade, Boss }
    [SerializeField] GameObject[] rewardIcons;
    [SerializeField] ParticleSystem rewardEft;
    RewardType stageReward = RewardType.Stat;

    [SerializeField] int[] rewardTypeWeights = { 1, 1, 1 }; // 무기, 스탯, 업그레이드 순서

    public bool TryInteract()
    {
        if (nextMapType != PortalType.Boss)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<Control_Stage>().StagePlay((int)nextMapType, stageReward);
        }
        else
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<Control_Stage>().BossStagePlay();
        }

        soundPlayer = GetComponent<AudioSource>();
        soundPlayer.PlayOneShot(sound_Interact);
        return true;
    }

    void Start()
    {
        if(nextMapType == PortalType.Boss)
        {
            return;
        }

        stageReward = SetRewardType();

        ActivateIcon(stageReward);
    }

    RewardType SetRewardType()
    {
        if(nextMapType == PortalType.Boss)
        {
            return RewardType.Boss;
        }

        int _randMax = rewardTypeWeights[0] + rewardTypeWeights[1] + rewardTypeWeights[2];
        int _randValue = Random.Range(0, _randMax);

        if (_randValue < rewardTypeWeights[0])
        {
            return RewardType.Weapon;
        }
        else if (_randValue < rewardTypeWeights[0] + rewardTypeWeights[1])
        {
            return RewardType.Stat;
        }
        else
        {
            return RewardType.Upgrade;
        }

    }

    void ActivateIcon(RewardType type)
    {
        rewardEft.Play();

        switch (type)
        {
            case RewardType.Weapon:
                rewardIcons[0].SetActive(true);
                break;
            case RewardType.Stat:
                rewardIcons[1].SetActive(true);
                break;
            case RewardType.Upgrade:
                rewardIcons[2].SetActive(true);
                break;
            case RewardType.Boss:

                break;

        }
    }

}
