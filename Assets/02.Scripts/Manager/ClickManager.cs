using System;
using UnityEngine;

// 클릭 매니저의 역할: 게임에서 마우스 왼쪽 클릭과 오른쪽 클릭을 각각 몇 번했는지 추적하는 클래스
public class ClickManager : MonoBehaviour
{
    private UI_Click ui_click;

    public static ClickManager Instance;

    private int _leftClickCount = 0;
    private int _rightClickCount = 0;

    public int LeftClickCount => _leftClickCount;
    public int RightClickCount => _rightClickCount;


    // 우주하마는 유튜버로써 구독자 목록을 가지고 있고, 영상을 올릴때마다 구독자들의 알람 함수를 호출해준다.
    // 클릭 매니저는 구독 함수 목록을 가지고 있고, 데이터가 변경될때마다 그 함수들을 모두 호출해준다.
    public event Action OnDataChanged;

    // 세분화를 얼마나 할것인가? 
    // 너무 세분화 할수록 읽어야할 코드와 상황이 많아져서 머리가 힘들다.
    // public event Action OnLeftClickCountChanged;
    // public event Action OnRightClickCountChanged;
    // if - else if
    // switch case
    // 전략 패턴


    // 레이어드 아키텍처란 시스템을 (데이터, 매니저, UI) 계층으로 나누고  의존성을 한방향으로 강제함으로써
    // 좀 더 독립적인 구조를 지키는것.. 이를 위해서 옵저버 패턴을 쓴다.


    public event Action<int> OnLeftClickCountChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _leftClickCount += 1;

            OnDataChanged?.Invoke();
        }

        if (Input.GetMouseButtonDown(1))
        {
            _rightClickCount += 1;

            if (OnDataChanged != null)
            {
                OnDataChanged();
            }
        }
    }


}