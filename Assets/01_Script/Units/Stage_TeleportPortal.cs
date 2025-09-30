using UnityEngine;

public class Stage_TeleportPortal : MonoBehaviour, IInteractable
{
    [SerializeField] Transform destination;
    Transform target;

    // Update is called once per frame

    private void Start()
    {
        if(destination == null)
        {
            destination = transform;
        }

        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Interact()
    {
        target.position = new Vector3(destination.position.x, destination.position.y, 0f);
    }

}
