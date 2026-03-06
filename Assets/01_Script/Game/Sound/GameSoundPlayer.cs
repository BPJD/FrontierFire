using UnityEngine;

public class GameSoundPlayer : MonoBehaviour
{
    public enum SoundType { Music, SFX, UI, Ambient }

    [SerializeField] AudioSource[] soundPlayersByType;


    public void GameSoundPlayByType(AudioClip clip, SoundType type)
    {
        int _soundType = (int)type;

        soundPlayersByType[_soundType].PlayOneShot(clip);
    }

    public AudioSource GetAudioSource(SoundType type)
    {
        return soundPlayersByType[(int)type];
    }
}
