using UnityEngine;

public class DefaultUnit_Chest : MonoBehaviour, IInteractable
{
    [SerializeField] ParticleSystem openEft;
    [SerializeField] ParticleSystem idleEft;

    [SerializeField] GameObject dropObj;

    bool isOpened = false;

    public bool TryInteract()
    {
        Vector3 dropPos = new Vector3(transform.position.x, transform.position.y + 0.25f, 0f);
        if (!isOpened)
        {
            GetComponentInChildren<Animator>().SetTrigger("Open");

            Instantiate(dropObj, dropPos, Quaternion.identity);

            enabled = false;

            openEft.Play(true);
            idleEft.Stop(true);
            isOpened = true;

            GetComponent<BoxCollider>().enabled = false;
            GetComponent<AudioSource>().Play();
            return true;
        }
        else
        {
            return false;
        }
    }
}
