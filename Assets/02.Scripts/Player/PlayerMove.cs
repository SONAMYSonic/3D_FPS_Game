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

        Debug.Log(_characterController.collisionFlags);

        // - 점프 처리
        if (Input.GetButtonDown("Jump") && _characterController.isGrounded)
        {
            _yVelocity = JumpForce;
        }

        // - 카메라가 쳐다보는 방향으로 변환한다
        direction = Camera.main.transform.TransformDirection(direction);
        direction.y = _yVelocity;   // 중력에 의한 y값 적용

        // 3. 방향으로 이동시키기
        _characterController.Move(direction * MoveSpeed * Time.deltaTime);
    }
}
