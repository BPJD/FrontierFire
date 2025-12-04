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
        float _randValue = Random.Range(0f, 1f);

        switch (nextMapType)
        {
            case PortalType.Normal:
                if(_randValue < 0.4f)
                {
                    return RewardType.Stat;
                }
                else
                {
                    return RewardType.Weapon;
                }

            case PortalType.Elite:
                if (_randValue < 0.4f)
                {
                    return RewardType.Upgrade;
                }
                else
                {
                    return RewardType.Stat;
                }

            case PortalType.Boss:
                return RewardType.Boss;

            default:
                return RewardType.Stat;

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
