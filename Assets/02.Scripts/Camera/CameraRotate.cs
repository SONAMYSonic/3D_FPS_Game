using UnityEngine;

// 카메라 회전 기능
// 마우스를 조작하면 카메라를 그 방향으로 회전시키고 싶다.
public class CameraRotate : MonoBehaviour
{
    public float rotateSpeed = 200f;    // 0 ~ 360

    // 유니티는 0~360각도 체계이므로 우리가 따로 저장할 -360 ~ 360 체계로 누적할 변수
    private float _accumulationX = 0;
    private float _accumulationY = 0;
    // 변수명은 길게 하고, 데이터(JSON 등)로 저장될 때는 자동으로 짧게 해주는 유니티 기능이 있다

    private void Update()
    {
        // 게임 시작하면 y축이 0도에서 -> -1도

        if (!Input.GetMouseButton(1))    // 마우스 오른쪽 버튼
            return;

        // 1. 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Debug.Log($"Mouse X: {mouseX}, Mouse Y: {mouseY}");

        // 2. 마우스 입력을 누적한다.
        _accumulationX += mouseX * rotateSpeed * Time.deltaTime;
        _accumulationY += mouseY * rotateSpeed * Time.deltaTime;

        // 3. 사람처럼 -90 ~ 90도 사이로 제한한다.
        _accumulationY = Mathf.Clamp(_accumulationY, -90f, 90f);

        // 4. 누적한 회전 방향으로 카메라 회전하기
        transform.eulerAngles = new Vector3(-_accumulationY, _accumulationX, 0f);


        // 새로운 위치 = 이전 위치 + (속도 * 방향 * 시간)
        // 새로운 회전 = 이전 회전 + (속도 * 방향 * 시간)

        // 쿼터니언: 사원수: 쓰는 이유는 짐벌락 현상 방지
        // 공부: 짐벌락, 쿼터니언을 왜 쓰나 (게임 수학/물리)

        // 문제: 잘 되긴하는데 한번씩 세상이 뒤집어진다

    }
}
