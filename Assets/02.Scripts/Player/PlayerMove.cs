using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    // 필요 속성
    // - 이동속도
    public float MoveSpeed = 7f;

    public float Gravity = -9.81f;
    private float _yVelocity = 0f;      // 중력에 의해 누적될 y값 변수
    public float JumpForce = 5f;
    public float DashMultiplier = 2f;   // 대시 시 배수
    [SerializeField]private float _playerStamina = 100f; // 플레이어 스태미나
    public bool isDoubleJumping = false; // 더블 점프 중인지 여부

    public UI_PlayerStats UI_PlayerStats; // 플레이어 스탯 UI 참조

    private CharacterController _characterController;


    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 0. 중력을 누적한다
        _yVelocity += Gravity * Time.deltaTime;

        // 1. 키보드 입력 받기
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 2. 입력에 따라 방향 구하기
        // 현재는 유니티 세상의 절대적인 방향이 기준 (글로벌/월드 좌표계)
        // 내가 원하는 것은 카메라가 쳐다보는 방향이 기준

        // - 글로벌 좌표 방향을 구한다
        Vector3 direction = new Vector3(h, 0, v).normalized;

        //Debug.Log(_characterController.collisionFlags);

        // - 점프 처리
        // 스태미너가 10 이상일 때 더블 점프 가능, 더블 점프 시 스태미나 10 감소, 기본 점프 시에는 스태미나 감소 없음
        if (_characterController.isGrounded)
        {
            // 땅에 닿아있을 때
            _yVelocity = 0f; // y속도 초기화
            isDoubleJumping = false; // 더블 점프 상태 초기화
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _yVelocity = JumpForce;
            }
        }
        else
        {
            // 공중에 떠 있을 때
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!isDoubleJumping && _playerStamina >= 10f)
                {
                    _yVelocity = JumpForce;
                    isDoubleJumping = true;
                    _playerStamina -= 10f; // 스태미나 10 감소
                }
            }
        }

        // - 카메라가 쳐다보는 방향으로 변환한다
        direction = Camera.main.transform.TransformDirection(direction);
        direction.y = _yVelocity;   // 중력에 의한 y값 적용

        // 3. 방향으로 이동시키기
        _characterController.Move(direction * MoveSpeed * Time.deltaTime);

        // 대시 처리
        // 대시 키를 누르고 있으면서 이동 중일 때, 스태미나는 1초에 1씩 감소
        if (Input.GetKey(KeyCode.LeftShift) && direction.magnitude > 0.1f)
        {
            if (_playerStamina > 0f)
            {
                _characterController.Move(direction * MoveSpeed * DashMultiplier * Time.deltaTime);
                _playerStamina -= 1f * Time.deltaTime;

            }
        }
        else
        {
            // 대시 키를 떼었거나 이동하지 않을 때, 스태미나는 1초에 0.5씩 회복
            if (_playerStamina < 100f)
            {
                _playerStamina += 0.5f * Time.deltaTime;
            }
        }
    }
}
