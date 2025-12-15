using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game.Weapons
{
    public class ZabimaruController : MonoBehaviour
    {
        [Header("===== Settings =====")]
        [Tooltip("마디와 마디 사이의 최대 거리입니다.")]
        [SerializeField] private float _maxGapDistance = 0.5f;

        [Tooltip("검이 늘어나는/줄어드는 속도입니다.")]
        [SerializeField] private float _extensionSpeed = 5f;

        [Tooltip("칼이 늘어나는 축 방향입니다. (기존 설정 유지: Y축)")]
        [SerializeField] private Vector3 _extensionDirection = Vector3.up;

        [Header("===== Attack Settings =====")]
        [Tooltip("공격 시 칼이 뻗어나갈 최대 거리 (요청하신 30)")]
        [SerializeField] private float _attackMaxReach = 30f;

        [Tooltip("공격 속도 (값이 클수록 빠르게 휘두름)")]
        [SerializeField] private float _attackSpeed = 2f;

        [Tooltip("공격 시 좌우로 흔들리는 폭 (뱀처럼 휘어짐)")]
        [SerializeField] private float _waveAmplitude = 5f;

        [Tooltip("공격 시 꼬불거리는 횟수 (주파수)")]
        [SerializeField] private float _waveFrequency = 10f;

        [Header("===== References =====")]
        [SerializeField] private List<Transform> _segments = new List<Transform>();
        [SerializeField] private ChainIKConstraint _chainIK;
        [Tooltip("Goal Source 오브젝트를 연결하세요.")]
        [SerializeField] private Transform _targetSource;

        // ===== 내부 상태 변수 =====
        private float _currentExtensionFactor = 0f;
        private bool _isWhipMode = false;
        private bool _isAttacking = false; // 현재 공격 중인지 확인

        // 초기 위치 저장용
        private List<Vector3> _initialSegmentPositions = new List<Vector3>();
        private Vector3 _initialTargetSourcePosition;

        private void Start()
        {
            // 1. 마디들의 초기 위치 저장
            foreach (var seg in _segments)
            {
                _initialSegmentPositions.Add(seg.localPosition);
            }

            // 2. 타겟 소스의 초기 위치 저장
            if (_targetSource != null)
            {
                _initialTargetSourcePosition = _targetSource.localPosition;
            }
        }

        private void Update()
        {
            HandleInput();          // 1. 키보드 입력 처리
            UpdateState();          // 2. 상태에 따른 수치 변화 (늘어남 정도 등)
            UpdateSegmentPositions(); // 3. 마디 벌리기
            UpdateIKWeight();       // 4. IK 가중치 적용

            // 5. 타겟 위치 제어 (공격 중이 아닐 때만 일반 로직 수행)
            if (!_isAttacking)
            {
                UpdateTargetPositionIdle();
            }
        }

        /// <summary>
        /// 사용자 입력을 처리합니다.
        /// </summary>
        private void HandleInput()
        {
            // Q키: 모드 전환 (검 <-> 채찍)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _isWhipMode = !_isWhipMode;
                Debug.Log($"[Zabimaru] Mode Switched: {(_isWhipMode ? "Whip" : "Sword")}");
            }

            // E키: 공격 (채찍 모드일 때만 가능하도록 설정, 원하시면 조건 제거 가능)
            if (Input.GetKeyDown(KeyCode.E) && _isWhipMode && !_isAttacking)
            {
                StartCoroutine(Co_PerformWhipAttack());
            }
        }

        private void UpdateState()
        {
            // 공격 중이 아닐 때는 모드에 따라 0 또는 1로 수렴
            if (!_isAttacking)
            {
                float targetFactor = _isWhipMode ? 1f : 0f;
                _currentExtensionFactor = Mathf.Lerp(_currentExtensionFactor, targetFactor, Time.deltaTime * _extensionSpeed);
            }
        }

        private void UpdateSegmentPositions()
        {
            // 마디들을 쭈욱 벌려주는 로직 (기존과 동일)
            // 인덱스 1부터 시작 (손잡이는 고정)
            for (int i = 1; i < _segments.Count; i++)
            {
                Vector3 initialPos = _initialSegmentPositions[i];

                // 마디가 늘어나는 거리 계산
                // 부모-자식 계층구조라고 가정 시 '* i' 제거. 평행 구조면 '* i' 필요.
                // 기존에 잘 작동했다면 그대로 둡니다.
                Vector3 offset = _extensionDirection * (_maxGapDistance * _currentExtensionFactor);

                _segments[i].localPosition = initialPos + offset;
            }
        }

        private void UpdateIKWeight()
        {
            if (_chainIK != null)
                _chainIK.weight = _currentExtensionFactor;
        }

        /// <summary>
        /// 공격 중이 아닐 때 타겟의 기본 위치 (단순히 늘어났다 줄어들었다 함)
        /// </summary>
        private void UpdateTargetPositionIdle()
        {
            if (_targetSource == null) return;

            // 전체 늘어난 길이
            float totalExtraLength = (_segments.Count - 1) * _maxGapDistance * _currentExtensionFactor;

            // 늘어나는 방향(Y)으로 위치 잡기
            _targetSource.localPosition = _initialTargetSourcePosition + (_extensionDirection * totalExtraLength);
        }

        /// <summary>
        /// [핵심] 뱀처럼 휘리릭거리는 공격 코루틴
        /// </summary>
        private IEnumerator Co_PerformWhipAttack()
        {
            _isAttacking = true;
            float timer = 0f;

            // 공격 애니메이션 지속 시간 (짧을수록 빠름)
            float duration = 1.5f / _attackSpeed;

            Debug.Log("[Zabimaru] Attack Start!");

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = timer / duration; // 0 ~ 1 사이 값

                // 1. Extension Factor를 강제로 1(최대)로 유지하거나, 공격 중에 쫙 펴지게 조절
                // 여기서는 공격 중에 100%로 펴지도록 설정
                _currentExtensionFactor = Mathf.Lerp(_currentExtensionFactor, 1f, Time.deltaTime * 10f);

                // 2. 타겟의 위치를 수학적(Sine Wave)으로 계산
                // Y축: 쭉 뻗어나감 (0 -> 30 -> 0)
                // X축: 좌우로 흔들림 (Sin 파동)

                // PingPong을 써서 0 -> 1 -> 0 으로 갔다가 돌아오게 만듦 (찌르고 회수)
                float reachProgress = Mathf.Sin(progress * Mathf.PI); // 0에서 시작해 1찍고 0으로
                float currentReach = reachProgress * _attackMaxReach;

                // 횡이동 (좌우 흔들림): 사인파를 이용해 뱀처럼 움직임
                // X축으로 흔들리기 위해 Vector3.right 사용 (축이 다르면 수정 필요)
                float wave = Mathf.Sin(progress * Mathf.PI * _waveFrequency) * _waveAmplitude * reachProgress;
                Vector3 lateralMove = Vector3.right * wave;

                // 최종 위치 계산:
                // 초기위치 + (뻗어나가는 방향 * 거리) + (옆으로 흔들리는 방향 * 웨이브)
                Vector3 attackPos = _initialTargetSourcePosition
                                  + (_extensionDirection * currentReach)
                                  + lateralMove;

                if (_targetSource != null)
                    _targetSource.localPosition = attackPos;

                yield return null;
            }

            // 공격 종료 후 정리
            _isAttacking = false;
            Debug.Log("[Zabimaru] Attack End");
        }
    }
}