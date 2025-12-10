using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game.Weapons
{
    public class ZabimaruController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("마디와 마디 사이의 최대 거리입니다.")]
        [SerializeField] private float _maxGapDistance = 0.5f;

        [Tooltip("검이 늘어나는 속도입니다.")]
        [SerializeField] private float _extensionSpeed = 5f;

        [Tooltip("칼이 늘어나는 방향입니다. (님 설정대로라면 Y축이므로 0, 1, 0)")]
        [SerializeField] private Vector3 _extensionDirection = Vector3.up;

        [Header("References")]
        [SerializeField] private List<Transform> _segments = new List<Transform>();
        [SerializeField] private ChainIKConstraint _chainIK;

        [Tooltip("Damped Transform의 Source가 되는 오브젝트 (Goal Source)")]
        [SerializeField] private Transform _targetSource;

        // 내부 상태 변수
        private float _currentExtensionFactor = 0f;
        public bool IsWhipMode = false;

        // [핵심] 각 마디의 '원래 위치'를 기억할 리스트
        private List<Vector3> _initialSegmentPositions = new List<Vector3>();
        private Vector3 _initialTargetSourcePosition; // 타겟 소스의 원래 위치

        private void Start()
        {
            // 1. 게임 시작 시, 님이 배치해둔 마디들의 위치를 저장합니다.
            foreach (var seg in _segments)
            {
                _initialSegmentPositions.Add(seg.localPosition);
            }

            // 2. 타겟 소스의 원래 위치도 저장합니다.
            if (_targetSource != null)
            {
                _initialTargetSourcePosition = _targetSource.localPosition;
            }
        }

        private void Update()
        {
            UpdateExtensionFactor();
            UpdateSegmentPositions();
            UpdateIKWeight();
            UpdateTargetPosition();
        }

        private void UpdateExtensionFactor()
        {
            float targetFactor = IsWhipMode ? 1f : 0f;
            _currentExtensionFactor = Mathf.Lerp(_currentExtensionFactor, targetFactor, Time.deltaTime * _extensionSpeed);
        }

        private void UpdateSegmentPositions()
        {
            // 인덱스 0번(손잡이/시작점)은 안 움직이므로 1번부터 시작
            for (int i = 1; i < _segments.Count; i++)
            {
                // 공식: 원래 위치 + (방향 * (늘어남계수 * 거리 * 순서))
                // 순서(i)를 곱하는 이유: 뒤에 있는 뼈일수록 앞 뼈들이 늘어난 만큼 더 많이 이동해야 하니까요.

                // 간단하게 '이전 마디로부터 벌어지는 방식'이 아니라 '원래 위치에서 추가로 이동'하는 방식 적용
                Vector3 initialPos = _initialSegmentPositions[i];

                // 내가 이동해야 할 추가 거리 = (최대 간격 * 0~1계수 * 내 순서)
                // 만약 계층구조(부모-자식)라면 '* i'를 뺍니다. (부모가 움직이면 자식도 가니까)
                // ★ 중요: 마디들이 부모-자식 관계라면 아래 코드를 쓰세요.
                Vector3 offset = _extensionDirection * (_maxGapDistance * _currentExtensionFactor);

                // 만약 마디들이 부모-자식이 아니라 평행한 관계라면 offset에 * i 를 해야 합니다.
                // 일단 부모-자식 관계라고 가정하고 작성합니다.

                _segments[i].localPosition = initialPos + offset;
            }
        }

        private void UpdateIKWeight()
        {
            if (_chainIK != null) _chainIK.weight = _currentExtensionFactor;
        }

        private void UpdateTargetPosition()
        {
            if (_targetSource == null) return;

            // 전체 늘어난 길이 계산
            // (마디 개수 - 1) * 간격 * 늘어남 계수
            float totalExtraLength = (_segments.Count - 1) * _maxGapDistance * _currentExtensionFactor;

            // 타겟도 원래 위치에서 방향대로 쭉 밀어줍니다.
            _targetSource.localPosition = _initialTargetSourcePosition + (_extensionDirection * totalExtraLength);
        }
    }
}