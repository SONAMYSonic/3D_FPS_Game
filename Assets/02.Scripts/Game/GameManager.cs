using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private EGameState _state = EGameState.Ready;
    public EGameState State => _state;

    [SerializeField] private TextMeshProUGUI _stateText;

    // 게임 상태 변경 이벤트
    public static event Action<EGameState> OnGameStateChanged;

    // 디미터 법칙 준수: 상태 확인 메서드
    public bool IsPlaying => _state == EGameState.Playing;
    public bool IsGameOver => _state == EGameState.GameOver;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _stateText.gameObject.SetActive(true);
        ChangeState(EGameState.Ready);
        _stateText.text = "준비중...";
        StartCoroutine(StartToPlay_Coroutine());
    }

    private void ChangeState(EGameState newState)
    {
        if (_state == newState) return;

        _state = newState;
        OnGameStateChanged?.Invoke(_state);
    }

    private IEnumerator StartToPlay_Coroutine()
    {
        yield return new WaitForSeconds(2f);

        _stateText.text = "시작!";

        yield return new WaitForSeconds(0.5f);

        ChangeState(EGameState.Playing);
        _stateText.gameObject.SetActive(false);
    }

    public void GameOver()
    {
        ChangeState(EGameState.GameOver);
        _stateText.gameObject.SetActive(true);
        _stateText.text = "게임 오버";
    }

    public void Victory()
    {
        ChangeState(EGameState.Victory);
        _stateText.gameObject.SetActive(true);
        _stateText.text = "승리!";
    }
}
