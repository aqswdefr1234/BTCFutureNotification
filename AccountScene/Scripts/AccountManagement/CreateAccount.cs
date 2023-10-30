using System.Collections;
using System.Collections.Generic;
using Google;
using Firebase;//firebase app 초기화에 필요
using Firebase.Auth;
using Firebase.Extensions;//ContinueWithOnMainThread 사용시 필요
using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class CreateAccount : MonoBehaviour//next버튼 두번눌러야 계정생성되는것 고쳐야함
{
    [SerializeField] private TMP_Text emailErrorText;
    [SerializeField] private TMP_Text passErrorText;
    [SerializeField] private TMP_InputField createAccount_EmailInput;
    [SerializeField] private TMP_InputField createAccount_PassInput;

    [SerializeField] private GameObject signUpPanel;

    private bool isExistEmail = true;//중복여부 확인 안했거나, 입력한 이메일 주소가 바뀌었거나, 중복되는 이메일일 경우 true  ,  중복여부확인결과 중복되지 않으면 false
    private bool IsPasswordUsable = false;
    
    private void Start()
    {
        createAccount_EmailInput.onValueChanged.AddListener(OnEmailInputFieldValueChanged);
        createAccount_PassInput.onValueChanged.AddListener(OnPasswordInputFieldValueChanged);
    }
    private void OnEmailInputFieldValueChanged(string newValue)
    {
        isExistEmail = true;
        emailErrorText.text = "";
        Debug.Log(newValue);
    }
    private void OnPasswordInputFieldValueChanged(string newValue)
    {
        if(Regex.IsMatch(newValue, @"[^a-zA-Z0-9!@#$%^&*()_+{}:<>?~.,;'\[\]\\\-]") == true) //true라면 영문,숫자,특수문자를 제외한 다른 문자가 들어간 것
        {
            createAccount_PassInput.text = "";
            passErrorText.text = "Passwords can only use English letters, numbers, and special characters.";
            IsPasswordUsable = false;
        }
        else
        {
            if (newValue.Length > 7)
            {
                IsPasswordUsable = true;
                passErrorText.text = "This password is available.";
            }
            else
            {
                IsPasswordUsable = false;
                passErrorText.text = "Password must be at least 8 characters long.";
            }
        }
    }
    public void CreateAccount_Next_Btn()
    {
        string currentEnterEmail = createAccount_EmailInput.text;
        string currentEnterPass = createAccount_PassInput.text;

        bool isValidEmail = CheckEmailValidity(currentEnterEmail);//이메일 형식 옳바른지 확인

        if (isValidEmail == true && IsPasswordUsable == true && isExistEmail == false)//이메일 및 비밀번호가 정상적으로 입력되었다면 계정생성
        {
            signUpPanel.SetActive(false);
            CreateAccountFirebase(currentEnterEmail, currentEnterPass);
        }
    }

    private bool CheckEmailValidity(string email)
    {
        if (email.Contains("@") && email.Contains(".") && email.Length > 4)
        {
            return true;
        }
        else
        {
            emailErrorText.text = "Invalid email format.";
            return false;
        }
    }
    public void CheckEmailAvailability()
    {
        string currentEnterEmail = createAccount_EmailInput.text;
        CheckForDuplicatesFirebaseAuth(currentEnterEmail);
    }
    private void CheckForDuplicatesFirebaseAuth(string email)//사용하려는 계정이 이미 등록되었는지 확인하기
    {
        Debug.Log("CheckForDuplicatesFirebaseAuth");
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.FetchProvidersForEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("이메일 등록 확인 중 오류 발생: " + task.Exception);
                NotifyTextEventController.NotifyText = "Task is failed";
                return;
            }
            List<string> providers = new List<string>(task.Result);
            if (providers.Count > 0)
            {
                isExistEmail = true;
                emailErrorText.text = "This email address already exists.";
            }
            else
            {
                isExistEmail = false;
                emailErrorText.text = "Email address is available.";
            }
        });
    }
    private void CreateAccountFirebase(string email, string password)
    {
        Debug.Log("CreateAccountFirebase");
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }
            NotifyTextEventController.NotifyText = "Please Log In!";
        });
    }
    public void GoogleMoblieLoginBtn()
    {
        transform.GetComponent<GoogleLoginScripts>().OnGoogleSignInButtonClicked();
        signUpPanel.SetActive(false);
    }
}
