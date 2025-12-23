using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private TextMeshProUGUI _progressText;

    private void Start()
    {
        StartCoroutine(LoadScene_Coroutine());
    }

    private IEnumerator LoadScene_Coroutine()
    {
        // 씬 로드상황에 대한 데이터를 가지고 있는 객체를 반환
        AsyncOperation ao = SceneManager.LoadSceneAsync("SampleScene");

        // 로드되는 씬의 모습이 화면에 안보이게...
        ao.allowSceneActivation = false;

        // 로드가 완료될때까지...
        while(!ao.isDone)
        {
            // ao는 진행률도 가지고 있다...
            _progressSlider.value = ao.progress;
            _progressText.text = $"{(ao.progress * 100f):F2}%";

            if(ao.progress >= 0.9f)
            {
                // 로드가 거의 완료되었다면...
                _progressSlider.value = 1f;
                _progressText.text = "100%";
                // 잠시 대기
                yield return new WaitForSeconds(0.5f);
                // 씬 전환 허용
                ao.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
