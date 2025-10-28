using UnityEngine;

public class WeaponSoundPlay : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] AudioClip[] soundFires;
    [SerializeField] AudioClip soundReloadStart;
    [SerializeField] AudioClip soundReloadEnd;

    PlayerWeaponController weaponCon;

    AudioSource soundPlayer;

    private void Start()
    {
        SoundPlayerConnect();
    }

    public void SoundPlayerConnect()
    {
        weaponCon = GetComponentInParent<PlayerWeaponController>();
        if (weaponCon != null)
        {
            soundPlayer = weaponCon.GetPlayerAudioSource();
        }
        else
        {
            soundPlayer = GetComponentInParent<AudioSource>();
        }
    }

    public void PlaySoundFire()
    {
        if (soundFires != null) 
        {
            int _value = Random.Range(0, soundFires.Length);
            AudioClip _clip = soundFires[_value];
            soundPlayer.PlayOneShot(_clip);
        }
        
    }

    public void PlaySoundReload(bool isStart)
    {
        AudioClip _clip = isStart ? soundReloadStart : soundReloadEnd;

        if (_clip != null)
        {
            soundPlayer.PlayOneShot(_clip);
        }
    }
}
