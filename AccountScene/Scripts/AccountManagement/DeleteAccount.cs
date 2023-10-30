using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;
using Firebase.Firestore;
using Debug = UnityEngine.Debug;
using System.Linq;
using System;

public class DeleteAccount : MonoBehaviour
{
    [SerializeField] private GameObject deletePanel;
    [SerializeField] private Transform textPanelPrefab;
    [SerializeField] private Transform textPanelParentTransform;//프리팹 놓을 위치. 여기서는 캔버스

    public void YesBtn()
    {
        DeleteThisAccount();
        deletePanel.SetActive(false);
    }
    public void NoBtn()
    {
        deletePanel.SetActive(false);
    }
    private void DeleteThisAccount()
    {
        FirebaseUser user = CurrentAccount.Instance.currentAuth.CurrentUser;

        if (user != null)
        {
            List<string> providers = user.ProviderData.Select(provider => provider.ProviderId).ToList();//만약 두가지 방법으로 모두 로그인이 가능하다면 password, google.com 둘다 작동된다.
            List<string> container = new List<string>();
            if (providers.Contains("password")) // 일반 이메일 및 암호
            {
                Debug.Log("현재 로그인한 사용자는 일반 이메일 및 암호를 사용하여 가입했습니다.");
                container.Add("normal");
            }
            if (providers.Contains("google.com")) // Google 소셜 로그인
            {
                Debug.Log("현재 로그인한 사용자는 Google 소셜 계정을 통해 로그인했습니다.");
                container.Add("google");
            }
            AccountDeletionProcedureBySubject(container, user);
        }
    }
    private void AccountDeletionProcedureBySubject(List<string> container, FirebaseUser user)
    {
        if(container.Count == 1)
        {
            if (container[0] == "normal")
            {
                DeleteFirestoreAndRealtimeDatabase(user);
            }
            else if (container[0] == "google")
            {
                OauthDisconnect(user);
            }
        }
        else if(container.Count == 2)
        {
            OauthDisconnect(user);
        }
    }
    
    //구글 로그인 계정 및 패스워드, 구글 로그인 통합 계정을 위한 삭제 방법
    private void OauthDisconnect(FirebaseUser firebaseUser)
    {
        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                return;
            }
            else if (task.IsFaulted)
            {
                ElseGoogleLogin();
                return;
            }
            if (task.Result != null)
            {
                GoogleSignInUser googleUser = task.Result;
                if (googleUser != null)//연결되어 있던 구글 계정이 존재한다면 
                {
                    string googleEmail = googleUser.Email;
                    string firebaseEmail = firebaseUser.Email;
                    if (firebaseEmail == googleEmail)//연결되어 있는 구글 계정이 현재 로그인 되어 있는 계정과 같은 계정이라면
                    {
                        DeleteFirestoreAndRealtimeDatabase(firebaseUser);
                        GoogleSignIn.DefaultInstance.Disconnect();//구글 계정 데이터 앱에서 지우기
                    }
                    else//연결되어 있는 구글 계정은 존재하지만 현재 로그인되어 있는 계정과는 다르다면 직접 다시 로그인해야함.
                        ElseGoogleLogin();
                }
            }
            else
                NotifyTextEventController.NotifyText = "Task is null.";
        });
    }
    private void ElseGoogleLogin()//연결되어 있던 구글 계정과 로그인된 계정의 이메일이 다르다면
    {
        Instantiate(textPanelPrefab, textPanelParentTransform);
    }
    
    //토큰이 존재할 경우와 계정만 존재할경우 구별해야함
    private void DeleteFirestoreAndRealtimeDatabase(FirebaseUser user)//일반계정 구글 계정 둘다 실행됨
    {
        string uid = user.UserId;
        string token = "";
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        //realTime삭제전 토큰 값 가져온 후 제거. firestore는 문서의 notification데이터를 가져온 후 삭제해야함.
        reference.Child("CurrentTokens").Child(uid).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                return;
            }
            else if (task.IsCanceled)
            {
                return;
            }
            WaitingDeleteRealtimeDatabase();
            if (task.Result.Value != null)//해당 key가 존재한다면 삭제(토큰을 발급받은 상태)
            {
                token = task.Result.Value.ToString();
                reference.Child("CurrentTokens").Child(uid).RemoveValueAsync();
                DeleteFieldValue(token, uid, db, reference, user);
            }
            else//(토큰을 발급받지 않은 상태)
            {
                FirebaseAuthDelete(user);
                CurrentAccount.Instance.LogOut();
            }
        });
        
        //필드 별 작성되어있는 토큰 값들은 서버측에서 토큰 전송이 실패하면 자동 삭제 시킨다.
    }
    private void WaitingDeleteRealtimeDatabase()//계정이 삭제되기전에 쓰기작업해야함. 아니면 쓰기 규칙에 의해 작성이 안됨
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("DeleteAnAccount").Child(CurrentAccount.Instance.currentAuth.CurrentUser.UserId).SetValueAsync("Deletion complete").ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                return;
            }
            NotifyTextEventController.NotifyText = "This account will be deleted within 24 hours.";
        });
    }

    //토큰이 있을 경우, 알림 설정했을 경우와 안했을 경우 나눔. 삭제시킬 realtimedatabase의 필드명을 가져오기위해서 작동시켜야함. 그 후 문서 삭제
    private void DeleteFieldValue(string token, string uid, FirebaseFirestore db, DatabaseReference reference, FirebaseUser user)
    {
        DocumentReference sourceDocumentRef = db.Collection("Users").Document(uid).Collection("MyNotification").Document(token);
        sourceDocumentRef.GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("문서 가져오기 실패: " + task.Exception);
                return;
            }
            DocumentSnapshot sourceDocument = task.Result;

            if (sourceDocument.Exists)//토큰이 있고 알림설정했을 경우
            {
                Dictionary<string, object> data = sourceDocument.ToDictionary();
                //리얼타임데이터베이스 삭제하기 위해 null 할당
                
                //firestore삭제
                DocumentReference userData = db.Collection("Users").Document(uid);
                DocumentReference userNotification = db.Collection("Users").Document(uid).Collection("MyNotification").Document(token);
                userNotification.DeleteAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.Log("Deletion failed");
                        return;
                    }
                    else if(task.IsCanceled)
                    {
                        Debug.Log("Deletion failed");
                        return;
                    }
                    Debug.Log("Delete Firestore Success");
                    userData.DeleteAsync().ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.Log("Deletion UserData failed");
                            return;
                        }
                        else if (task.IsCanceled)
                        {
                            Debug.Log("Deletion UserData failed");
                            return;
                        }
                        Debug.Log("Delete UserData Success");
                        RealTimeUpdate(reference, data, null, uid, user);
                    });
                });
                
            }
            else//토큰은 있지만 알림 설정안했을 경우
            {
                DocumentReference userData = db.Collection("Users").Document(uid);
                userData.DeleteAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.Log("Deletion UserData failed");
                        return;
                    }
                    else if (task.IsCanceled)
                    {
                        Debug.Log("Deletion UserData failed");
                        return;
                    }
                    Debug.Log("Delete UserData Success");
                    FirebaseAuthDelete(user);//FirebaseAuthDelete(user)와 CurrentAccount.Instance.LogOut()는 세트
                    CurrentAccount.Instance.LogOut();
                });
            }
        });
    }
    //유저데이터가 존재하고 알림설정 데이터도 있을경우 실행.마지막에 실행될 작업으로 마지막에 계정생성과 로그아웃작업 필요
    private void RealTimeUpdate(DatabaseReference reference, Dictionary<string, object> dict, string _token, string uid, FirebaseUser user)
    {
        foreach (KeyValuePair<string, object> pair in dict)
        {
            if (pair.Value is List<object> list)
            {
                foreach (var time in list)
                {
                    reference.Child($"{pair.Key}_{time}").Child(uid).SetValueAsync(_token).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            return;
                        }
                        else if (task.IsCanceled)
                        {
                            return;
                        }
                        FirebaseAuthDelete(user);
                        CurrentAccount.Instance.LogOut();
                    });
                }
            }
        }

    }
    private void FirebaseAuthDelete(FirebaseUser user)
    {
        if (user != null)
        {
            user.DeleteAsync().ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    return;
                }
                if (task.IsFaulted)
                {
                    return;
                }
            });
        }
    }
}