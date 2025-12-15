using UnityEngine;
using DG.Tweening;

public class PlayerScan : MonoBehaviour
{
    public GameObject ScannerObject;

    private void Awake()
    {
        ScannerObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 기존 트윈 제거 (중요!)
            ScannerObject.transform.DOKill();

            // 스케일 초기화 후 활성화
            ScannerObject.transform.localScale = Vector3.one;
            ScannerObject.SetActive(true);

            // 0.5초동안 scale 1씩 커지다가 사라지기
            ScannerObject.transform.DOScale(70f, 20f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    ScannerObject.SetActive(false);
                });
        }
    }

    private void OnDisable()
    {
        // 비활성화 시 트윈 정리
        ScannerObject.transform.DOKill();
        ScannerObject.transform.localScale = Vector3.one;
    }
}
