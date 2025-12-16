using UnityEngine;

public class BoxDropItem : MonoBehaviour
{
    [SerializeField] GameObject dropItem;
    [SerializeField] ParticleSystem destroyEft;
    Transform tr;
    [SerializeField] float randPower = 1f;

    [SerializeField] float dropRate = 0.5f; // 아이템 드롭 확률

    public void NeutralUnitDead()
    {
        tr = transform;
        PlayerShootingStat playerDropStat = GameObject.FindWithTag(Data_Strings.playerTag).GetComponent<PlayerShootingStat>();
        dropRate += playerDropStat.playerItemDropRate;

        if(Random.Range(0f, 1f) <= dropRate)
        {
            ItemDrop();
        }

        destroyEft.Play(true);
        destroyEft.transform.SetParent(null);

        this.gameObject.SetActive(false);

    }

    void ItemDrop()
    {
        float _rand = Random.Range(-1f, 1f);
        Vector3 _randVelo = new Vector3(_rand, 0.8f, 0f);

        GameObject _dropItem = Instantiate(dropItem, tr.position, Quaternion.identity);
        _dropItem.GetComponent<Rigidbody>().linearVelocity = _randVelo * randPower;
    }
}
