using System.Collections;
using System.Collections.Generic;
using Google;
using Firebase;//firebase app �ʱ�ȭ�� �ʿ�
using Firebase.Auth;
using Firebase.Extensions;//ContinueWithOnMainThread ���� �ʿ�
using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class CreateAccount : MonoBehaviour//next��ư �ι������� ���������Ǵ°� ���ľ���
{
    [SerializeField] private TMP_Text emailErrorText;
    [SerializeField] private TMP_Text passErrorText;
    [SerializeField] private TMP_InputField createAccount_EmailInput;
    [SerializeField] private TMP_InputField createAccount_PassInput;

    [SerializeField] private GameObject signUpPanel;

    private bool isExistEmail = true;//�ߺ����� Ȯ�� ���߰ų�, �Է��� �̸��� �ּҰ� �ٲ���ų�, �ߺ��Ǵ� �̸����� ��� true  ,  �ߺ�����Ȯ�ΰ�� �ߺ����� ������ false
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
        if(Regex.IsMatch(newValue, @"[^a-zA-Z0-9!@#$%^&*()_+{}:<>?~.,;'\[\]\\\-]") == true) //true��� ����,����,Ư�����ڸ� ������ �ٸ� ���ڰ� �� ��
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

        bool isValidEmail = CheckEmailValidity(currentEnterEmail);//�̸��� ���� �ǹٸ��� Ȯ��

        if (isValidEmail == true && IsPasswordUsable == true && isExistEmail == false)//�̸��� �� ��й�ȣ�� ���������� �ԷµǾ��ٸ� ��������
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
    private void CheckForDuplicatesFirebaseAuth(string email)//����Ϸ��� ������ �̹� ��ϵǾ����� Ȯ���ϱ�
    {
        Debug.Log("CheckForDuplicatesFirebaseAuth");
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.FetchProvidersForEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("�̸��� ��� Ȯ�� �� ���� �߻�: " + task.Exception);
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
