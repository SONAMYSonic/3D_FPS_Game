using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMove : MonoBehaviour
{
    [Serializable]
    public class MoveConfig
    {
        public float Gravity;
        public float RunStamina;
        public float JumpStamina;
    }

    public MoveConfig _config;

    private CharacterController _controller;
    private PlayerStats _stats;
    [SerializeField] private Animator _soliderAnimator;

    private float _yVelocity = 0f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (GameManager.Instance.State != EGameState.Playing)
        {
            return;
        }

        if (_controller.enabled == false)
        {
            return;
        }

        // 0. 중력을 누적한다.
        _yVelocity += _config.Gravity * Time.deltaTime;

        // 1. 키보드 입력 받기
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // 2. 입력에 따른 방향 구하기
        Vector3 direction = new Vector3(x, 0, y);
        _soliderAnimator.SetFloat("Speed", direction.magnitude * _stats.MoveSpeedValue);
        direction.Normalize();

        // - 점프! : 점프 키를 누르고 && 땅이라면
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            _yVelocity = _stats.JumpPowerValue;
            _soliderAnimator.SetTrigger("Jump");
        }

        // - 카메라가 쳐다보는 방향으로 변환한다. (월드 -> 로컬)
        direction = Camera.main.transform.TransformDirection(direction);
        direction.y = _yVelocity;

        float moveSpeed = _stats.MoveSpeedValue;
        if (Input.GetKey(KeyCode.LeftShift) && _stats.TryConsumeStamina(_config.RunStamina * Time.deltaTime))
        {
            moveSpeed = _stats.RunSpeedValue;
        }

        // 3. 방향으로 이동시키기  
        _controller.Move(direction * moveSpeed * Time.deltaTime);
    }

    public void SetVelocity(float yVelocity)
    {
        _yVelocity = yVelocity;
    }
}