using UnityEngine;

/// <summary>
/// 커서 상태를 관리하는 클래스
/// CameraFollow의 OnTopViewChanged 이벤트를 구독하여 커서 상태 제어
/// </summary>
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 초기 커서 잠금
        LockCursor();
    }

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
        if (isTopView)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}