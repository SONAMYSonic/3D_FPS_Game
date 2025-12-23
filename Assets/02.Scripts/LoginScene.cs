using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScene : MonoBehaviour
{
    // 로그인씬 (로그인/회원가입) -> 게임씬

    private enum SceneMode
    {
        Login,
        Register
    }

    private SceneMode _mode = SceneMode.Login;

    [Header("로그인")]
    [SerializeField] private GameObject _loginObject;
    [SerializeField] private Button _gotoRegisterButton;
    [SerializeField] private Button _loginButton;
    [SerializeField] private TMP_InputField _loginIdInputField;
    [SerializeField] private TMP_InputField _loginPasswordInputField;

    [Header("회원가입")]
    [SerializeField] private GameObject _registerObject;
    [Tooltip("회원가입에서 로그인으로 돌아가기 버튼")]
    [SerializeField] private Button _gotoLoginButton;
    [SerializeField] private Button _registerButton;
    [SerializeField] private TMP_InputField _registerIdInputField;
    [SerializeField] private TMP_InputField _registerPasswordInputField;
    [SerializeField] private TMP_InputField _registerPasswordConfirmInputField;

    [Header("기타")]
    [SerializeField] private TextMeshProUGUI _alarmText;

    private void Start()
    {
        AddButtonEvents();
        Refresh();
    }

    private void AddButtonEvents()
    {
        _gotoRegisterButton.onClick.AddListener(GotoRegister);
        _loginButton.onClick.AddListener(Login);
        _gotoLoginButton.onClick.AddListener(GotoLogin);
        _registerButton.onClick.AddListener(Register);
    }

    private void Refresh()
    {
        // 회원가입 오브젝트는 회원가입 모드일때만 노출
        _registerObject.SetActive(_mode == SceneMode.Register);
        // 로그인 오브젝트는 로그인 모드일때만 노출
        _loginObject.SetActive(_mode == SceneMode.Login);
    }

    private void Login()
    {
        // 로그인
        // 1. 아이디 입력을 확인한다.
        string id = _loginIdInputField.text;
        if (string.IsNullOrEmpty(id))
        {
            Debug.Log("아이디를 입력해주세요.");
            _alarmText.text = "아이디를 입력해주세요.";
            return;
        }

        // 2. 비밀번호 입력을 확인한다.
        string password = _loginPasswordInputField.text;
        if (string.IsNullOrEmpty(password))
        {
            Debug.Log("비밀번호를 입력해주세요.");
            _alarmText.text = "비밀번호를 입력해주세요.";
            return;
        }

        // 3. 실제 저장된 아이디/비밀번호 계정이 있는지 확인한다.
        // 3-1. 아이디가 있는지 확인한다.
        if (!PlayerPrefs.HasKey(id))
        {
            _alarmText.text = "아이디/비밀번호를 확인해주세요.";
            return;
        }

        string myPassword = PlayerPrefs.GetString(id);
        // 3-2. 비밀번호가 일치하는지 확인한다.
        if (myPassword != password)
        {
            _alarmText.text = "아이디/비밀번호를 확인해주세요.";
            return;
        }

        // 4. 있다면 씬 이동
        // 동기(유저가 대기)
        SceneManager.LoadScene("LoadingScene");

    }

    private void Register()
    {
        // 1. 아이디 입력을 확인한다.
        string id = _registerIdInputField.text;
        if (string.IsNullOrEmpty(id))
        {
            _alarmText.text = "아이디를 입력해주세요.";
            return;
        }
        // 1-1. 아이디 형식 확인 (이메일)
        if (!Regex.IsMatch(id, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            _alarmText.text = "올바른 이메일 형식이 아닙니다.";
            return;
        }

        // 2. 비밀번호 입력을 확인한다.
        string password = _registerPasswordInputField.text;
        if (string.IsNullOrEmpty(password))
        {
            _alarmText.text = "비밀번호를 입력해주세요.";
            return;
        }
        // 2-1. 비밀번호 형식 확인 (영문, 숫자, 특수문자만, 7~20자, 특수문자 하나 이상, 대소문자 각 한 개 이상)
        if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+=-])[A-Za-z\d!@#$%^&*()_+=-]{7,20}$"))
        {
            _alarmText.text = "비밀번호는 7~20자 영문 대소문자, 숫자, 특수문자를 포함해야 합니다.";
            return;
        }

        // 2-2. 비밀번호 재입력 칸을 확인한다.
        string password2 = _registerPasswordConfirmInputField.text;
        if (string.IsNullOrEmpty(password2) || password != password2)
        {
            _alarmText.text = "비밀번호를 확인해주세요.";
            return;
        }

        // 3. 실제 저장된 아이디/비밀번호 계정이 있는지 확인한다.
        // 3-1. 아이디가 있는지 확인한다.
        if (PlayerPrefs.HasKey(id))
        {
            _alarmText.text = "중복된 아이디입니다.";
            return;
        }

        PlayerPrefs.SetString(id, password);
        _alarmText.text = "회원가입이 완료되었습니다.";
        GotoLogin();
    }

    private void GotoLogin()
    {
        _mode = SceneMode.Login;
        Refresh();
    }

    private void GotoRegister()
    {
        _mode = SceneMode.Register;
        Refresh();
    }

}