using UnityEngine;

public class GrapplingController : MonoBehaviour
{
    // ========================================================================
    // [Inspector 할당]
    // ========================================================================
    [Header("Base Settings")]
    [SerializeField] private LayerMask _grappleLayer;
    [SerializeField] private Transform _gunTip;
    [SerializeField] private Transform _cameraTransform;

    [Header("Rope Physics Settings")]
    [SerializeField] private float _maxDistance = 100f;
    [Tooltip("스프링 강도 (높을수록 강하게 당김)")]
    [SerializeField] private float _springForce = 10f;
    [Tooltip("스프링 저항 (높을수록 덜 출렁거림)")]
    [SerializeField] private float _damper = 7f;
    [SerializeField] private float _massScale = 4.5f;

    // [추가된 변수: 로프 길이 비율 설정]
    [Header("Rope Length Ratios")]
    [Tooltip("로프의 최소 길이 비율 (0.0 ~ 1.0)")]
    [Range(0f, 1f)][SerializeField] private float _minRopeRatio = 0.25f;

    [Tooltip("로프의 최대 길이 비율 (0.0 ~ 1.0). 이 값이 작을수록 로프가 짧아져 위로 당겨집니다.")]
    [Range(0f, 1f)][SerializeField] private float _maxRopeRatio = 0.8f;

    // ========================================================================
    // [내부 변수]
    // ========================================================================
    private LineRenderer _lineRenderer;
    private Vector3 _grapplePoint;
    private SpringJoint _playerJoint;
    private Rigidbody _playerRigidbody;
    private CharacterController _playerController;
    private PlayerMove _playerMove;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _playerRigidbody = GetComponentInParent<Rigidbody>();
        _playerController = GetComponentInParent<CharacterController>();
        _playerMove = GetComponentInParent<PlayerMove>();

        _lineRenderer.enabled = false;
        if (_playerRigidbody != null) _playerRigidbody.isKinematic = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1)) StartGrapple();
        else if (Input.GetMouseButtonUp(1)) StopGrapple();
    }

    private void LateUpdate()
    {
        if (_playerJoint != null)
        {
            _lineRenderer.SetPosition(0, _gunTip.position);
            _lineRenderer.SetPosition(1, _grapplePoint);
        }
    }

    private void StartGrapple()
    {
        if (_playerRigidbody == null || _playerController == null) return;

        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _maxDistance, _grappleLayer))
        {
            _grapplePoint = hit.point;

            _playerController.enabled = false;
            _playerRigidbody.isKinematic = false;

            _playerJoint = _playerRigidbody.gameObject.AddComponent<SpringJoint>();
            _playerJoint.autoConfigureConnectedAnchor = false;
            _playerJoint.connectedAnchor = _grapplePoint;

            float distanceFromPoint = Vector3.Distance(_playerRigidbody.position, _grapplePoint);

            // [수정된 부분] 변수로 설정한 비율 적용
            _playerJoint.maxDistance = distanceFromPoint * _maxRopeRatio;
            _playerJoint.minDistance = distanceFromPoint * _minRopeRatio;

            _playerJoint.spring = _springForce;
            _playerJoint.damper = _damper;
            _playerJoint.massScale = _massScale;

            _lineRenderer.positionCount = 2;
            _lineRenderer.enabled = true;
        }
    }

    private void StopGrapple()
    {
        if (_playerJoint != null)
        {
            Destroy(_playerJoint);
            _playerJoint = null;
        }

        if (_lineRenderer != null) _lineRenderer.enabled = false;

        if (_playerRigidbody != null && _playerController != null)
        {
            Vector3 momentum = _playerRigidbody.linearVelocity;

            _playerRigidbody.isKinematic = true;
            _playerController.enabled = true;

            // [핵심 수정] PlayerMove에게 솟구치는 힘(Y축 속도) 전달
            if (_playerMove != null)
            {
                _playerMove.SetVelocity(momentum.y);
            }
        }
    }
}