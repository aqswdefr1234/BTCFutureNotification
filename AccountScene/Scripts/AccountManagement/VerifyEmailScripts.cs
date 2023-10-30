using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using Debug = UnityEngine.Debug;

public class VerifyEmailScripts : MonoBehaviour
{
    public void SendVerificationEmail(FirebaseAuth _auth)//CurrentAcount, LoginAccount 스크립트에서 사용중
    {
        FirebaseAuth auth = _auth;
        if (auth.CurrentUser != null)
        {
            auth.CurrentUser.SendEmailVerificationAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    return;
                }
                if (task.IsFaulted)
                {
                    return;
                }
                NotifyTextEventController.NotifyText = "Send Success Email";
            });
        }
        else
        {
            NotifyTextEventController.NotifyText = "Failed Send Email";
        }
    }
}
