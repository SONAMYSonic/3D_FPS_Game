using UnityEngine;

public interface IDroneState
{
    // 상태 진입 시 1회 호출
    void Enter();

    // 매 프레임 호출 (Update 로직)
    void Execute();

    // 상태 종료 시 1회 호출
    void Exit();
}