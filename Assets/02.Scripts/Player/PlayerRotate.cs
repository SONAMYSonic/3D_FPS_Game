using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    public float RotateSpeed = 200f;    // 0 ~ 360

    private float _accumulationX = 0;

    private void Update()
    {
        // 게임 시작하면 y축이 0도에서 -> -1도

        if (!Input.GetMouseButton(1))    // 마우스 오른쪽 버튼
            return;

        // 1. 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X");

        // 2. 마우스 입력을 누적한다.
        _accumulationX += mouseX * RotateSpeed * Time.deltaTime;

        // 4. 누적한 회전 방향으로 카메라 회전하기
        transform.eulerAngles = new Vector3(0f, _accumulationX, 0f);

    }
}
