using System.Collections;
using System.Collections.Generic;
using Google;
using Firebase.Auth;
using Firebase.Extensions;//ContinueWithOnMainThread 사용시 필요
using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;
using System;
using System.Text.RegularExpressions;

public class LoginAccount : MonoBehaviour
{
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_InputField signIn_EmailInput;
    [SerializeField] private TMP_InputField signIn_PassInput;
    [SerializeField] private GameObject signUpPanel;
    [SerializeField] private GameObject loginPanel;

    [SerializeField] private GameObject resetPanel;
    [SerializeField] private TMP_InputField reset_EmailInput;

    private void Start()
    {
        signIn_PassInput.onValueChanged.AddListener((string newValue) => OnPasswordInputFieldValueChanged(newValue, signIn_PassInput));
    }
    private void OnPasswordInputFieldValueChanged(string newValue, TMP_InputField targetInput)
    {
        if (Regex.IsMatch(newValue, @"[^a-zA-Z0-9!@#$%^&*()_+{}:<>?~.,;'\[\]\\\-]") == true) //true라면 영문,숫자,특수문자를 제외한 다른 문자가 들어간 것
        {
            targetInput.text = "";
            NotifyTextEventController.NotifyText = "Passwords can only use English letters, numbers, and special characters.";
        }
    }
    public void LoginNextBtnClick()
    {
        LoginAccountMethod(signIn_EmailInput.text, signIn_PassInput.text);
    }
    private void LoginAccountMethod(string email, string password)
    {
        try
        {
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    NotifyTextEventController.NotifyText = "Login was canceled.";
                    return;
                }
                else if (task.IsFaulted)
                {
                    NotifyTextEventController.NotifyText = "Invalid email or password...";
                    return;
                }

                if (auth.CurrentUser.IsEmailVerified)
                {
                    NotifyTextEventController.NotifyText = "Welcome!";
                    CurrentAccount.Instance.SetInstance(auth);
                    loginPanel.SetActive(false);
                }
                else
                {
                    NotifyTextEventController.NotifyText = "Please check your email. Please log in again later.";
                    transform.GetComponent<VerifyEmailScripts>().SendVerificationEmail(auth);
                }
            });
        }
        catch(Exception e)
        {
            NotifyTextEventController.NotifyText = e.ToString();
        }
    }
    public void signUpBtnClick()
    {
        signUpPanel.SetActive(true);
        loginPanel.SetActive(false);
    }
    public void GoogleMoblieLoginBtn()
    {
        transform.GetComponent<GoogleLoginScripts>().OnGoogleSignInButtonClicked();
        loginPanel.SetActive(false);
    }

    //ResetPanel
    public void ResetForgotPasswordBtn()
    {
        loginPanel.SetActive(false);
        resetPanel.SetActive(true);
    }
    public void ResetNextBtn()
    {
        FirebaseAuth.DefaultInstance.SendPasswordResetEmailAsync(reset_EmailInput.text).ContinueWithOnMainThread(task => 
        {
            if (task.IsCanceled)
            {
                NotifyTextEventController.NotifyText = "Password reset email delivery has been cancelled. Please check your network";
                return;
            }
            if (task.IsFaulted)
            {
                NotifyTextEventController.NotifyText = "Sending email failed. This email may not be registered.";
                return;
            }

            NotifyTextEventController.NotifyText = "A password reset email has been sent.";
            resetPanel.SetActive(false);
        });
    }
}
