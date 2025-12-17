using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    public float RotationSpeed = 200f;

    private float _accumulationY = 0;
    private bool _isTopView = false;

    private void OnEnable()
    {
        CameraFollow.OnTopViewChanged += HandleTopViewChanged;
    }

    private void OnDisable()
    {
        CameraFollow.OnTopViewChanged -= HandleTopViewChanged;
    }

    private void HandleTopViewChanged(bool isTopView)
    {
        _isTopView = isTopView;
    }

    private void Update()
    {
        if (GameManager.Instance.State != EGameState.Playing)
        {
            return;
        }

        // 탑뷰일 때는 카메라 회전 비활성화
        if (_isTopView)
        {
            return;
        }

        // 카메라는 위/아래(X축)만 담당 - 로컬 좌표계 사용
        float mouseY = Input.GetAxisRaw("Mouse Y");

        _accumulationY += -mouseY * RotationSpeed * Time.deltaTime;
        _accumulationY = Mathf.Clamp(_accumulationY, -90f, 90f);

        // 로컬 좌표계에서 X축만 회전 (Y축은 부모인 플레이어가 담당)
        transform.localEulerAngles = new Vector3(_accumulationY, 0, 0);
    }
}