using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;

    private void LateUpdate()
    {
        transform.position = Target.position;
    }
}
