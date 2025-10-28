using UnityEngine;

public class Stage_TeleportPortal : MonoBehaviour, IInteractable
{
    [SerializeField] Transform destination;
    Transform target;

    AudioSource soundPlayer;

    // Update is called once per frame

    private void Start()
    {
        if(destination == null)
        {
            destination = transform;
        }

        soundPlayer = GetComponent<AudioSource>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public bool TryInteract()
    {
        target.position = new Vector3(destination.position.x, destination.position.y, 0f);
        soundPlayer.Play();
        return true;
    }

}
