using UnityEngine;
using UnityEngine.UI;

public class UI_OptionPopup : MonoBehaviour
{
    // 별로 안 열리는 옵션 팝업은 프리팹화 한 후 Instantiate 한다

    // 자주 열리는 팝업은 미리 씬에 배치해두고 활성화/비활성화로 처리한다

    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;

    public void Show()
    {
        gameObject.SetActive(true);

        // todo
        // 1. 애니메이션 처리
        // 2. 사운드 처리
        // 3. 이펙트 처리
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        // 콜백함수: 어떤 이벤트가 일어나면 자동으로 호출되는 함수
        _restartButton.onClick.AddListener(GameRestart);
        _continueButton.onClick.AddListener(GameContinue);
        _exitButton.onClick.AddListener(GameExit);
    }

    // 함수란 한가지 기능만 해야 하고, 그 기능이 무엇을 하는지 (의도, 결과)가 나타나는 이름을 가져야 한다.
    // ~클릭했을때 라는 이름은 기능의 이름이 아니라 "언제 호출되는지"가 들어나 있다.

    private void GameContinue()
    {
        GameManager.Instance.Resume();
        Hide();
    }

    private void GameRestart()
    {

    }

    private void GameExit()
    {
        
    }

    private void Update()
    {

    }
}
