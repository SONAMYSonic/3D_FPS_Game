using System;
using TMPro;
using UnityEngine;

public class UI_Click : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _leftClickCountTextUI;
    [SerializeField] private TextMeshProUGUI _rightClickCountTextUI;


    // 옵저버 패턴이란 : 객체(주체자(subject))의 데이터가 바뀔때마다 주체자를 감시하는 객체에게 그 상태의 변경을 알려주는 패턴
    // 유튜브 패턴이란 :         유튜버      의  영상이 올라올때마다   구독자에게                   알림을 주는 패턴

    private void Start()
    {
        Refresh();

        // 구독 시작 (데이터 변경되면 Refresh 호출해주세요라고 등록)
        // 구독자들이 등록한 함수를 '콜백 함수'라고 한다. (어떤 이벤트가 발생하면 실행되는 함수를 콜백 함수라고 부른다.)
        ClickManager.Instance.OnDataChanged += Refresh;
    }


    private void Update()
    {
        // 초당 60번이상 매니저에게 접근해서 데이터를 읽어옵니다.
        // 굉장히 비효율적이다.
        // 매니저의 데이터가 바뀔때만 매니저의 데이터에 접근하고 싶다.

        //Refresh();
    }

    private void Refresh()
    {
        _leftClickCountTextUI.text = $"왼쪽 클릭: {ClickManager.Instance.LeftClickCount}번";
        _rightClickCountTextUI.text = $"오른쪽 클릭: {ClickManager.Instance.RightClickCount}번";
    }
}