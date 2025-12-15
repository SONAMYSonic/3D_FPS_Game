using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _offsetY = 10f;

    // offsetY 값을 외부에서 접근할 수 있도록 프로퍼티로 만듭니다.
    public float OffsetY
    {
        get { return _offsetY; }
        set { _offsetY = value; }
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = _target.position;
        Vector3 finalPosition = targetPosition + new Vector3(0f, _offsetY, 0f);

        transform.position = finalPosition;

        Vector3 targetAngle = _target.eulerAngles;
        targetAngle.x = 90f; // Look straight down
        transform.eulerAngles = targetAngle;
    }
}
