using System.Net.Cache;
using UnityEngine;
using System;

public class TryCatchFinallyTest : MonoBehaviour
{
    public int Age;     // 사람 나이
    // 예외: 런타임중에 발생하는 오류 (참조, 나누기, 인덱스 범위 벗어나기 등등)

    // try-catch 문법은: 예외를 처리하는 기본 문법

    private void Start()
    {
        if (Age < 0)
        {
            Debug.LogError("나이는 0살 이상이어야 합니다!");
            throw new Exception("사람나이는 0살보다 적을 수 없습니다");    // DDD
        }

        // 아래 문법은 인덱스 범위를 벗어나므로 오류가 일어난다
        // => 다른 컴포넌트나 게임 오브젝트에도 영향을 줌으로써 프로그램이 정상적으로 동작 안할수도 있다
        int[] numbers = new int[32];
        

        // 베스트: 알고리즘을 잘 처리하는 것

        try
        {
            // 에러가 일어날만한 코드 작성

            int index = 75;     // 실제로는 내가 문제를 해결하기 위해 반복문이나 수식을 통해 얻은 인덱스
            numbers[index] = 1;
        }
        catch(Exception e)
        {
            Debug.LogError(e);

            // 예외가 발생했을 때 처리하는 코드 작성

            int index = numbers.Length - 1;
            numbers[index] = 1;
            Debug.Log("IndexOutOfRangeException 발생! 재만이를 찾아주세요");
        }
        finally
        {
            // 옵션: 정상이든 오류이든 실행할 코드 작성
        }


        // Try - Catch 구문은 되도록이면 안쓰는게 좋다
        // - 성능 저하 (제일 큼)
        // - 잘못된 알고리즘

        // 써야 하는 경우: 내가 제어할 수 없을 때..
        // 1. 네트워크 접근
        // - 로그인, 로그아웃 / 서버 / DB 아이템 저장-불러오기
        // 2. 파일 접근
        // - 용량 충분, 파일명 정확, 경로 정확, 권한 충분 등등
        // 3. DB 접근
        // - 
    }
}
