using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game.Weapons
{
    /// <summary>
    /// 사미환(Whip Sword)의 늘어남과 휘어짐을 제어하는 클래스입니다.
    /// </summary>
    public class ZabimaruController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("마디와 마디 사이의 최대 거리입니다.")]
        [SerializeField] private float _maxGapDistance = 0.5f;

        [Tooltip("검이 늘어나는 속도입니다.")]
        [SerializeField] private float _extensionSpeed = 5f;

        [Header("References")]
        [Tooltip("칼날 마디들의 리스트입니다. 순서대로 할당해야 합니다.")]
        [SerializeField] private List<Transform> _segments = new List<Transform>();

        [Tooltip("Animation Rigging의 Chain IK Constraint입니다.")]
        [SerializeField] private ChainIKConstraint _chainIK;

        // 내부 상태 변수
        public float _currentExtensionFactor = 0f; // 0: 검(수축), 1: 채찍(이완)
        public bool _isWhipMode = false;

        /// <summary>
        /// 외부에서 검/채찍 모드를 토글할 때 호출합니다.
        /// </summary>
        public void ToggleMode()
        {
            _isWhipMode = !_isWhipMode;
        }

        /// <summary>
        /// 현재 모드에 따라 마디 간격을 조절하고 IK 가중치를 설정합니다.
        /// </summary>
        private void Update()
        {
            UpdateExtensionFactor();
            UpdateSegmentPositions();
            UpdateIKWeight();
        }

        /// <summary>
        /// 목표 확장 수치(0 또는 1)로 현재 수치를 보간합니다.
        /// </summary>
        private void UpdateExtensionFactor()
        {
            float targetFactor = _isWhipMode ? 1f : 0f;

            // 부드러운 전환을 위해 Lerp 사용
            _currentExtensionFactor = Mathf.Lerp(_currentExtensionFactor, targetFactor, Time.deltaTime * _extensionSpeed);
        }

        /// <summary>
        /// 확장 수치에 맞춰 마디 사이의 로컬 위치를 변경합니다.
        /// </summary>
        private void UpdateSegmentPositions()
        {
            // 첫 번째 마디는 고정이므로 인덱스 1부터 시작
            for (int i = 1; i < _segments.Count; i++)
            {
                Transform currentSeg = _segments[i];

                // 마디가 늘어날 방향(로컬 Z축 가정)으로 위치 이동
                // 기본 위치(0)에서 _maxGapDistance만큼 벌어짐
                Vector3 newLocalPos = currentSeg.localPosition;
                newLocalPos.z = _currentExtensionFactor * _maxGapDistance; // 축이 다르면 x, y 수정 필요

                currentSeg.localPosition = newLocalPos;
            }
        }

        /// <summary>
        /// 검 모드일 때는 IK를 끄고(직선 유지), 채찍 모드일 때는 IK를 켭니다.
        /// </summary>
        private void UpdateIKWeight()
        {
            if (_chainIK == null) return;

            // 채찍 모드일 때만 IK가 작동하여 휘어짐. 
            // 검 모드일 때는 0이 되어 일자로 펴진 원래 애니메이션/트랜스폼 유지.
            _chainIK.weight = _currentExtensionFactor;
        }
    }
}