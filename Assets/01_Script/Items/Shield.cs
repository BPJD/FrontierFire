using Combat;
using DamageNumbersPro;
using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour
{

    public int shieldHP { get; set; } = 100;
    public int shieldHPMax { get; set; } = 100;
    public float shieldRevision { get; set; } = 1f;
    AudioSource soundPlayer;
    [SerializeField] AudioClip[] sound_defends;
    [SerializeField] AudioClip sound_broken;
    [SerializeField] GameObject eft_broken;

    bool isShieldHeal = false;


    Data_DamageNumbers data_DNum;
    ShieldManager manager;

    void Start()
    {
        soundPlayer = GetComponent<AudioSource>();
        manager = GetComponentInParent<ShieldManager>();
        data_DNum = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag).GetComponent<Data_DamageNumbers>();
    }

    private void OnEnable()
    {
        StartCoroutine(ShieldRepair());
    }

    public DamageResult TakeDamage(in DamagePayload p)
    {
        float raw = p.baseDamage * shieldRevision;

        // 반올림/클램프는 마지막에
        int final = Mathf.Clamp(Mathf.RoundToInt(raw), 1, p.baseDamage);

        // 3) 비주얼/사운드
        PrintDamageNumber(final, p.hitPoint);

        if (isShieldHeal)
        {
            shieldHP += final;
        }
        else
        {
            shieldHP -= final;
        }


        CheckBroken();

        return new DamageResult
        {
            finalDamage = final
        };

    }

    void PrintDamageNumber(int finalDamage, Vector3 pos)
    {
        DamageNumber _number;

        _number = data_DNum.GetDamageNumberPrefab((Data_DamageNumbers.NumberType)4);

        _number.Spawn(pos, finalDamage);

    }


    void CheckBroken()
    {
        AudioClip _clip;

        if(shieldHP <= 0)
        {
            _clip = sound_broken;
            manager.StartCoroutine(manager.ShieldRespawn());
            Destroy(Instantiate(eft_broken, transform.position, Quaternion.identity), 5f);
            this.gameObject.SetActive(false);
        }

        if (soundPlayer != null)
        {
            _clip = sound_defends[Random.Range(0, sound_defends.Length)];
            soundPlayer.PlayOneShot(_clip);
        }
    }

    IEnumerator ShieldRepair()
    {
        isShieldHeal = true;

        yield return new WaitForSeconds(0.5f);

        isShieldHeal = false;
    }
}
