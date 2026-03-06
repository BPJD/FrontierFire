using UnityEngine;

public class Direction_BGMChange : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip; // BGM 클립을 인스펙터에서 할당할 수 있도록 SerializeField 사용
    Direction_BGMPlay bgmPlayer; // BGM 플레이어 스크립트 참조

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bgmPlayer = GameObject.FindGameObjectWithTag(Data_Strings.soundTag).GetComponent<Direction_BGMPlay>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(bgmPlayer == null)
        {
            Debug.LogError("[Direction_BGMChange] BGM 플레이어를 찾을 수 없습니다.");
            return;
        }
        bgmPlayer.PlayBGM(bgmClip, 2f);
    }
}
