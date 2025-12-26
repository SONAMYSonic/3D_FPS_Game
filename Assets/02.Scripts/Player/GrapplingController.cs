using UnityEngine;

/// <summary>
/// 그래플링 훅 시스템을 관리하는 컴포넌트입니다.
/// </summary>
public class GrapplingController : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════
    // Inspector 설정
    // ═══════════════════════════════════════════════════════════
    
    [Header("기본 설정")]
    [SerializeField] private LayerMask _grappleLayer;
    [SerializeField] private Transform _gunTip;
    
    [Header("조준 설정")]
    [SerializeField, Tooltip("플레이어 Transform (Y축 회전 담당)")]
    private Transform _playerTransform;
    
    [SerializeField, Tooltip("카메라 또는 카메라 피벗 Transform (X축 회전 담당)")]
    private Transform _cameraPivot;

    [Header("입력 설정")]
    [SerializeField, Tooltip("그래플링 발사 키")]
    private KeyCode _grappleKey = KeyCode.Q;

    [Header("그래플링 물리 설정")]
    [SerializeField, Tooltip("최대 그래플 거리")]
    private float _maxDistance = 50f;
    
    [SerializeField, Tooltip("그래플 포인트로 당겨지는 속도")]
    private float _pullSpeed = 15f;
    
    [SerializeField, Tooltip("그래플 중 중력 배율 (0 = 무중력, 1 = 정상)")]
    [Range(0f, 1f)]
    private float _gravityMultiplier = 0.3f;
    
    [SerializeField, Tooltip("로프 최대 길이 비율")]
    [Range(0.5f, 1f)]
    private float _maxRopeRatio = 0.95f;

    [Header("모멘텀 설정")]
    [SerializeField, Tooltip("그래플 해제 시 유지되는 속도 배율")]
    [Range(0f, 2f)]
    private float _momentumMultiplier = 1.2f;
    
    [Header("도착 판별")]
    [SerializeField, Tooltip("그래플 포인트 도착 판별 거리")]
    private float _arrivalDistance = 2f;
    
    [SerializeField, Tooltip("로프 과신장 시 보정 강도")]
    private float _overstretchCorrection = 10f;

    // ═══════════════════════════════════════════════════════════
    // 내부 변수
    // ═══════════════════════════════════════════════════════════
    
    private LineRenderer _lineRenderer;
    private CharacterController _playerController;
    private PlayerMove _playerMove;
    
    private bool _isGrappling = false;
    private Vector3 _grapplePoint;
    private float _ropeLength;
    private Vector3 _grappleVelocity;

    public bool IsGrappling => _isGrappling;
    public Vector3 GrappleVelocity => _grappleVelocity;

    // ═══════════════════════════════════════════════════════════
    // Unity 생명주기
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _playerController = GetComponentInParent<CharacterController>();
        _playerMove = GetComponentInParent<PlayerMove>();

        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
        
        // 자동 할당 시도
        if (_playerTransform == null)
        {
            _playerTransform = GetComponentInParent<CharacterController>()?.transform;
        }
    }

    private void Start()
    {
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (_playerTransform == null)
        {
            Debug.LogError("[GrapplingController] Player Transform이 할당되지 않았습니다!");
        }
        
        if (_cameraPivot == null)
        {
            Debug.LogError("[GrapplingController] Camera Pivot이 할당되지 않았습니다! (카메라 또는 CameraParent를 할당하세요)");
        }
        else
        {
            Debug.Log($"[GrapplingController] Camera Pivot: {_cameraPivot.name}");
        }
    }

    private void Update()
    {
        HandleInput();
        
        if (_isGrappling)
        {
            UpdateGrapple();
        }
    }

    private void LateUpdate()
    {
        if (_isGrappling && _lineRenderer != null)
        {
            _lineRenderer.SetPosition(0, _gunTip.position);
            _lineRenderer.SetPosition(1, _grapplePoint);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // 입력 처리
    // ═══════════════════════════════════════════════════════════

    private void HandleInput()
    {
        if (Input.GetKeyDown(_grappleKey))
        {
            TryStartGrapple();
        }
        else if (Input.GetKeyUp(_grappleKey))
        {
            StopGrapple();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // 조준 방향 계산
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 플레이어의 Y축 회전 + 카메라의 X축 회전을 결합하여 조준 방향을 계산합니다.
    /// </summary>
    private Vector3 GetAimDirection()
    {
        if (_playerTransform == null || _cameraPivot == null)
        {
            // 폴백: Camera.main 사용
            Camera cam = Camera.main;
            if (cam != null)
            {
                return cam.transform.forward;
            }
            return Vector3.forward;
        }

        // 플레이어의 Y축 회전 (좌우)
        float playerYRotation = _playerTransform.eulerAngles.y;
        
        // 카메라 피벗의 로컬 X축 회전 (상하)
        float cameraXRotation = _cameraPivot.localEulerAngles.x;
        
        // X축 회전 정규화 (Unity는 0~360도를 사용하므로 -180~180으로 변환)
        if (cameraXRotation > 180f)
        {
            cameraXRotation -= 360f;
        }

        // 결합된 회전으로 방향 벡터 계산
        Quaternion combinedRotation = Quaternion.Euler(cameraXRotation, playerYRotation, 0f);
        Vector3 aimDirection = combinedRotation * Vector3.forward;

        return aimDirection;
    }

    // ═══════════════════════════════════════════════════════════
    // 그래플링 시작
    // ═══════════════════════════════════════════════════════════

    private void TryStartGrapple()
    {
        if (_playerController == null)
        {
            Debug.LogWarning("[GrapplingController] CharacterController가 없습니다!");
            return;
        }

        // 조준 방향 계산
        Vector3 aimDirection = GetAimDirection();
        Vector3 rayOrigin = _cameraPivot != null ? _cameraPivot.position : _playerController.transform.position + Vector3.up;

        // 디버그
        Debug.Log($"[GrapplingController] 발사! 방향: {aimDirection}");
        Debug.DrawRay(rayOrigin, aimDirection * _maxDistance, Color.red, 2f);

        // 레이캐스트 발사
        Ray ray = new Ray(rayOrigin, aimDirection);

        if (!Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _grappleLayer))
        {
            Debug.Log("[GrapplingController] 그래플 대상을 찾지 못했습니다.");
            return;
        }

        // 그래플링 시작
        _grapplePoint = hit.point;
        _ropeLength = Vector3.Distance(_playerController.transform.position, _grapplePoint);
        _isGrappling = true;
        _grappleVelocity = Vector3.zero;

        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.enabled = true;
        }

        Debug.Log($"[GrapplingController] 그래플링 시작! 대상: {hit.collider.name}, 거리: {_ropeLength:F1}m");
    }

    // ═══════════════════════════════════════════════════════════
    // 그래플링 업데이트
    // ═══════════════════════════════════════════════════════════

    private void UpdateGrapple()
    {
        if (_playerController == null || !_playerController.enabled)
        {
            return;
        }

        Vector3 playerPos = _playerController.transform.position;
        Vector3 toGrapplePoint = _grapplePoint - playerPos;
        float currentDistance = toGrapplePoint.magnitude;
        Vector3 grappleDirection = toGrapplePoint.normalized;

        // 1. 그래플 포인트 방향으로 당기는 힘
        _grappleVelocity += grappleDirection * _pullSpeed * Time.deltaTime;

        // 2. 약한 중력 적용
        _grappleVelocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;

        // 3. 로프 길이 제한
        float maxAllowedDistance = _ropeLength * _maxRopeRatio;
        
        if (currentDistance > maxAllowedDistance)
        {
            float overstretch = currentDistance - maxAllowedDistance;
            _grappleVelocity += grappleDirection * overstretch * _overstretchCorrection * Time.deltaTime;
        }

        // 4. 이동 적용
        _playerController.Move(_grappleVelocity * Time.deltaTime);

        // 5. 도착 체크
        if (currentDistance < _arrivalDistance)
        {
            Debug.Log("[GrapplingController] 그래플 포인트 도착!");
            StopGrapple();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // 그래플링 종료
    // ═══════════════════════════════════════════════════════════

    private void StopGrapple()
    {
        if (!_isGrappling)
        {
            return;
        }

        _isGrappling = false;

        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }

        if (_playerMove != null)
        {
            float yMomentum = _grappleVelocity.y * _momentumMultiplier;
            _playerMove.SetVelocity(yMomentum);
            Debug.Log($"[GrapplingController] 그래플링 종료! Y 모멘텀: {yMomentum:F1}");
        }

        _grappleVelocity = Vector3.zero;
    }

    // ═══════════════════════════════════════════════════════════
    // 외부 인터페이스
    // ═══════════════════════════════════════════════════════════

    public void AddSwingInput(Vector3 inputDirection)
    {
        if (!_isGrappling)
        {
            return;
        }

        _grappleVelocity += inputDirection * 5f * Time.deltaTime;
    }

    private void OnDisable()
    {
        if (_isGrappling)
        {
            StopGrapple();
        }
    }
}
