using System;
using System.Collections;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using UnityEngine;
using Google;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;//ContinueWithOnMainThread 사용시 필요

public class GoogleLoginScripts : MonoBehaviour
{
    private string webClientId = "1069658341853-s2r0s6f0bsj2ihfvo7097rfbi0fj2h5p.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    private void Awake()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestEmail = true,
            RequestIdToken = true
        };
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
    }
    
    public void OnGoogleSignInButtonClicked()
    {
        SignInWithGoogle(GoogleSignIn.DefaultInstance);
    }
    private void SignInWithGoogle(GoogleSignIn googleSignin)
    {
        googleSignin.SignIn().ContinueWithOnMainThread(OnAuthenticationFinished);
    }
    
    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    NotifyTextEventController.NotifyText = "Got Error: " + error.Status + " " + error.Message;
                }
                else
                {
                    NotifyTextEventController.NotifyText = "Got Unexpected Exception?!?" + task.Exception;
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Canceled");
        }
        else
        {
            SignInAndUpWithGoogleOnFirebase(task.Result.IdToken);
        }
    }

    private void SignInAndUpWithGoogleOnFirebase(string idToken)
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                NotifyTextEventController.NotifyText = "Sign-in canceled.";
                return;
            }
            if (task.IsFaulted)
            {
                NotifyTextEventController.NotifyText = "Sign-in failed.";
                return;
            }
            CurrentAccount.Instance.SetInstance(auth);
            NotifyTextEventController.NotifyText = auth.CurrentUser.Email;
        });
    }
}
