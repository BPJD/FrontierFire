using UnityEngine;

public class Gun_Parent_Data : MonoBehaviour
{
    public Transform gunPos { get; private set; }
    public Transform gunPar { get; private set; }
    public Transform shoulder { get; private set; }

    [SerializeField] Transform _gunPos;
    [SerializeField] Transform _gunPar;
    [SerializeField] Transform _shoulder;

    private void Awake()
    {
        gunPos = _gunPos;
        gunPar = _gunPar;
        shoulder = _shoulder;
    }
}
