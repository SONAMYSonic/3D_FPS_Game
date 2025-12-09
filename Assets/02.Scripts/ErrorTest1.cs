using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]     // Rigidbody2D 컴포넌트가 반드시 붙어있도록 강제
public class ErrorTest1 : MonoBehaviour
{
    // 오류: 프로그램이 비정상적으로 동작하게 하는 문제
    // 예외: 런타임중에 발생하는 오류 (참조, 나누기, 인덱스 범위 벗어나기 등등)

    // 크게 3가지
    // 1. 문법 오류: 문법에 맞지 않는 코드나 오타로 인해 발생 (컴파일 오류) => IDE에서 빨간줄로 표시
    // 2. 런타임 오류: 실행 중에 발생하는 오류 (에디터 콘솔창에 비교적 명확하게 출력) => 테스트를 진행하면서 잡아준다
    // 3. 알고리즘 or 휴먼 오류 or AI오류: 주어진 문제에 대해 잘못된 해석이나 구현으로 내가 원하지 않는 결과가 나오는 오류
    // => 가장 해결하기 어렵다. 공부/자료수집/분석 + 많은 경험을 통해 오류를 찾아내고 해결하는 능력 키우기

    // 유니티에서 런타임에 주로 나타나는 오류(예외)

    private void Start()
    {
        // MissingReferenceException
        // 사용하고자 하는 컴포넌트가 null일 때..

        // 초기화시에 null검사하는 방어코드
        // 방어코드 => null 검사
        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        if (rigidbody2D == null)
        {
            // 적절한 처리
            // - AddComponent
            // - 오류를 로깅
        }
        Debug.Log(rigidbody2D.linearVelocity);

        // NullReferenceException
        // 사용하고자 하는 각체가 null값일 때 그 객체의 필드나 메서드에 접근하려고 하면 발생
        Rigidbody2D rigidbody2D_2 = null;
        Debug.Log(rigidbody2D_2.linearVelocity);
    }
}
