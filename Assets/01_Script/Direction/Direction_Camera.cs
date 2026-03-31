using UnityEngine;

public class Direction_Camera : MonoBehaviour
{
    [SerializeField] CameraMovingSystem m_CameraMovingSystem;


    public void Direction_Shake(float power, float duration)
    {
        m_CameraMovingSystem.PlayCameraShake(power, duration);
    }
}
