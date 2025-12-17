using UnityEngine;
using UnityEngine.AI;

public class PointerAgent : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _playerAgent;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private PlayerMove _playerMove;

    private RaycastHit _rayHit;
    private bool _isTopViewMode = false;

    private void Awake()
    {
        if (_playerAgent == null)
        {
            _playerAgent = GetComponent<NavMeshAgent>();
        }
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
        }
        if (_playerMove == null)
        {
            _playerMove = GetComponent<PlayerMove>();
        }

        // 초기 상태: NavMeshAgent 비활성화
        _playerAgent.enabled = false;
    }

    private void Update()
    {
        bool isTopView = CameraFollow.Instance != null && CameraFollow.Instance.IsTopView;

        // 모드가 변경되었을 때만 전환 (매 프레임 호출 방지)
        if (_isTopViewMode != isTopView)
        {
            _isTopViewMode = isTopView;
            SetNavMeshAgentActive(isTopView);
        }

        // 탑뷰가 아니면 우클릭 처리 안 함
        if (!_isTopViewMode) return;

        // 우클릭 시 이동
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out _rayHit))
            {
                _playerAgent.SetDestination(_rayHit.point);
            }
        }
    }

    private void SetNavMeshAgentActive(bool active)
    {
        if (active)
        {
            // NavMeshAgent 활성화 전에 CharacterController 비활성화
            if (_playerMove != null) _playerMove.enabled = false;
            _controller.enabled = false;
            _playerAgent.enabled = true;
        }
        else
        {
            // CharacterController 활성화 전에 NavMeshAgent 비활성화
            _playerAgent.ResetPath();
            _playerAgent.enabled = false;
            _controller.enabled = true;
            if (_playerMove != null) _playerMove.enabled = true;
        }
    }
}
