using UnityEngine;

public class ErrorTest2 : MonoBehaviour
{
    private void Start()
    {
        // MissingReferenceException: 보통 삭제한 게임 오브젝트를 참조/접근(필드, 메서드) 하려고 할 때 발생
        Destroy(gameObject);
        Debug.Log(gameObject.name); // 삭제된 게임 오브젝트에 접근하려고 시도

        try
        {
            // IndexOutOfRangeException: 배열(리스트)에서 유효하지 않은 인덱스에 접근하려고 할 때 발생
            int[] numbers = new int[10];
            Debug.Log(numbers[13]); // 유효하지 않은 인덱스에 접근
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Caught an exception: " + ex.Message);
        }

        // DevideByZeroException: 0으로 나누기를 시도할 때 발생
        int a = 10;
        int b = 0;
        Debug.Log(a / b); // 0으로 나누기 시도
    }
}
