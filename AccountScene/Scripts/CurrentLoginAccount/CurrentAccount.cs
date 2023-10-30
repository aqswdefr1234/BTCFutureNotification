using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using System.Collections;
using Debug = UnityEngine.Debug;
using Google;
using System.Collections.Generic;
using System.Linq;
using System;

internal class CurrentAccount : MonoBehaviour
{
    internal FirebaseAuth currentAuth;
    internal string currentAccountEmail;
    internal string currentAccountNick;
    internal bool isAutoLogin = false;//playerprefs int값을 가져와 넣는다. 0이면 false를 넣고, 1이면 true를 넣는다.
    private static CurrentAccount instance;

    private void Awake()
    {
        
        if (instance != null)
        {
            Destroy(gameObject); // 중복된 객체 파괴
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
                return;
            }
            if(PlayerPrefs.GetInt("isAutoLogin") == 1)
                isAutoLogin = true;
            else
                isAutoLogin = false;

            
            if (isAutoLogin == true)
            {
                currentAuth = FirebaseAuth.DefaultInstance;
                currentAuth.CurrentUser.ReloadAsync().ContinueWithOnMainThread(task => 
                {
                    if (task.IsCanceled)
                    {
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        return;
                    }
                    if (currentAuth.CurrentUser != null)
                        SetInstance(currentAuth);
                });
            }
            else
            {
                if (FirebaseAuth.DefaultInstance.CurrentUser != null)
                {
                    Debug.Log(FirebaseAuth.DefaultInstance.CurrentUser.Email);
                    FirebaseAuth.DefaultInstance.SignOut();
                    SignOutIfUserIsSignedIn();
                }
            }
        });
    }
    internal static CurrentAccount Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }
    internal void SetInstance(FirebaseAuth auth)
    {
        currentAuth = auth;
        currentAccountEmail = auth.CurrentUser.Email;
        currentAccountNick = auth.CurrentUser.DisplayName;
        AuthStateChanged();
    }
    internal void LogOut()
    {
        if (currentAuth != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
            currentAuth.SignOut();
            currentAuth = null;
            currentAccountEmail = null;
            currentAccountNick = null;
            SignOutIfUserIsSignedIn();
            PlayerPrefs.SetInt("isAutoLogin", 0);
        }
    }
    private void AuthStateChanged()
    {
        
        if (currentAuth.CurrentUser != null)
        {
            if (currentAuth.CurrentUser.IsEmailVerified)
            {
                NotifyTextEventController.NotifyText = "Authentication successful!";
            }
            else
                NotifyTextEventController.NotifyText = "Authentication fail";
        }
        else
        {
            NotifyTextEventController.NotifyText = "Please log in";
        }
    }
    private void SignOutIfUserIsSignedIn()
    {
        try
        {
            GoogleSignIn.DefaultInstance.SignInSilently().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("Sign-in canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("Sign-in failed: " + task.Exception);
                    return;
                }

                GoogleSignInUser user = task.Result;
                if (user != null)
                {
                    Debug.Log("User is signed in, signing out...");
                    GoogleSignIn.DefaultInstance.SignOut();
                }
                else
                {
                    Debug.Log("No user is signed in.");
                }
            });
        }
        catch(Exception ex)
        {
            NotifyTextEventController.NotifyText = "SignOutIfUserIsSignedIn " + ex.ToString();
        }

    }
}
