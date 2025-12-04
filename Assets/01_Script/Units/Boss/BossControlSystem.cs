using UnityEngine;

public class BossControlSystem : MonoBehaviour
{
    StageModule stageModule;

    public bool isBossLive { get; private set; } = true;

    [SerializeField] ParticleSystem deathEft;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stageModule = GetComponentInParent<StageModule>();
        if(stageModule != null)
        {
            stageModule.isBossStage = true;
        }
        
    }

    public void BossDead()
    {
        stageModule.BossStageClear();
        isBossLive = false;
        deathEft.Play();
        Animator _anicon = GetComponent<Animator>();
        
        if(_anicon != null)
        {
            _anicon.SetTrigger("Death");
        }
    }


}
