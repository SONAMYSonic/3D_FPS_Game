using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    public float RotationSpeed = 200f;
    private float _accumulationX = 0;
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

        // 탑뷰일 때는 플레이어 회전 비활성화
        if (_isTopView)
        {
            return;
        }

        // GetAxisRaw: 스무딩 없이 즉각적인 입력값 반환
        float mouseX = Input.GetAxisRaw("Mouse X");
        _accumulationX += mouseX * RotationSpeed * Time.deltaTime;

        transform.eulerAngles = new Vector3(0, _accumulationX);
    }
}