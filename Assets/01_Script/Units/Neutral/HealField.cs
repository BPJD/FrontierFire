using UnityEngine;
using System.Collections;

public class HealField : MonoBehaviour
{

    [SerializeField] int healValue = 10;
    [SerializeField] float delay = 0.5f;
    WaitForSeconds _delay;

    UnitStatus healUnit;
    Transform unitTr;

    [SerializeField] GameObject particle;

    private void Start()
    {
        _delay = new WaitForSeconds(delay);

        StartCoroutine(Heal());
    }

    IEnumerator Heal()
    {
        while (true)
        {
            if(healUnit != null)
            {
                healUnit.UnitGetHeal(healValue);

                GameObject _particle = Instantiate(particle, unitTr.position + Vector3.up, particle.transform.rotation);
                Destroy(_particle, 3f);
            }
            yield return _delay;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            healUnit = other.gameObject.GetComponent<UnitStatus>();
            unitTr = other.gameObject.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            healUnit = null;
            unitTr = null;
        }
    }


}
