using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Messaging;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Database;
using Debug = UnityEngine.Debug;
using TMPro;
using System;
using System.Threading.Tasks;

public class FCM_Initialization : MonoBehaviour
{
    private FirebaseFirestore db;
    private DatabaseReference reference;
    private string uid = null;

    public static string currentToken = null;
    public static event Action<string> OnTokenNewValue;
    public static string CurrentToken
    {
        get
        {
            if (currentToken != null)
            {
                return currentToken;
            }
            else
                return null;
        }
        set
        {
            if(value == null || value == "")
                currentToken = null;
            else if(value != null)
            {
                currentToken = value;
                OnTokenNewValue?.Invoke(value);
            }
                
        }
    }
    void Awake()
    {
        OnTokenNewValue += TokenHandler;
        FirebaseInitialization();
    }
    private void TokenHandler(string newToken)
    {
        if(newToken != null && newToken != "" && newToken != "StubToken")
            transform.GetComponent<LoadConditions>().LoadCurrentSelectedNotification(newToken);
    }
    
    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }
    private void FirebaseInitialization()
    {
        if(CurrentAccount.Instance.currentAuth != null)
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    db = FirebaseFirestore.DefaultInstance;
                    reference = FirebaseDatabase.DefaultInstance.RootReference;
                    uid = CurrentAccount.Instance.currentAuth.CurrentUser.UserId;
                    Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
                    RealtimeDatabaseGetToken();
                }
                else
                {
                    NotifyTextEventController.NotifyText = "Not available now. Are you connected to the internet?";
                }
            });
        }
        else
            NotifyTextEventController.NotifyText = "Please Log In";
    }
    private void SaveUser(string uid, string email, string token, string oldToken)
    {
        DocumentReference docRef = db.Collection("Users").Document(uid);

        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "UID", uid },
            { "Email", email },
            { "Token", token },
            {"oldToken", oldToken},
            { "Timestamp", Timestamp.FromDateTime(DateTime.UtcNow) }
        };

        docRef.SetAsync(user).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                NotifyTextEventController.NotifyText = "User data successfully written to Firestore.";
            }
            else
            {
                NotifyTextEventController.NotifyText = task.Exception.ToString();
            }
        });
    }
    private void InformationUpdate(string uid, string email, string token)
    {
        DocumentReference docRef = db.Collection("Users").Document(uid);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                return;
            }
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)//이전에 사용한 유저일 경우
                {
                    string oldToken = snapshot.GetValue<string>("Token");
                    NotifyTextEventController.NotifyText = "Token is Changed";
                    SaveUser(uid, email, token, oldToken);
                    CloudUpdate(db.Collection("Users").Document(uid).Collection("MyNotification"), token, oldToken);
                }
                else//사용한적 없는 새로운 유저
                {
                    SaveUser(uid, email, token, "");
                }
            }
        });
    }
    private void CloudUpdate(CollectionReference collectionRef, string newToken,string oldToken)//이전에 사용한 유저일 경우 작동
    {
        DocumentReference sourceDocumentRef = collectionRef.Document(oldToken);
        sourceDocumentRef.GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("문서 가져오기 실패: " + task.Exception);
                return;
            }
            DocumentSnapshot sourceDocument = task.Result;

            if (sourceDocument.Exists)
            {
                // 새로운 문서 id로 새로운 문서를 생성합니다.
                Dictionary<string, object> data = sourceDocument.ToDictionary();
                DocumentReference newDocumentRef = collectionRef.Document(newToken);
                newDocumentRef.SetAsync(data);

                //리얼타임데이터베이스 변경
                RealTimeUpdate(data, newToken);

                // 기존 문서를 삭제합니다.
                sourceDocumentRef.DeleteAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        return;
                    }
                    else if (task.IsCanceled)
                    {
                        return;
                    }
                    //transform.GetComponent<LoadConditions>().LoadCurrentSelectedNotification(newToken);//스크린 갱신
                });
            }
        });
    }
    private void RealTimeUpdate(Dictionary<string, object>  dict, string _token)
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
                        transform.GetComponent<LoadConditions>().LoadCurrentSelectedNotification(_token);//스크린 갱신
                    });
                }
            }
        }

    }
    private void RealtimeDatabaseGetToken()
    {
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

            if (task.Result.Value != null)//해당 key가 존재한다면 이미 사용한 적이 있는 유저
            {
                string realtimeDatabaseToken = task.Result.Value.ToString();
                FCMGetToken(realtimeDatabaseToken);
            }
            else//존재하지 않는다면(아마 처음 사용하는 유저)
            {
                FirstUseFCMTokenGet();
            }

        });
    }
    private void FCMGetToken(string realtimeDatabaseToken)
    {
        FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to retrieve token with {task.Exception}");
                CurrentToken = null;
                return;
            }

            string fcmToken = task.Result;
            if (fcmToken != realtimeDatabaseToken)
            {
                CurrentToken = fcmToken;
                InformationUpdate(uid, CurrentAccount.Instance.currentAuth.CurrentUser.Email, fcmToken);
                SetCurrentTokensField(fcmToken);
            }
            else
            {
                CurrentToken = fcmToken;
            }
                
        });
    }
    private void FirstUseFCMTokenGet()
    {
        FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to retrieve token with {task.Exception}");
                CurrentToken = null;
                return;
            }
            string fcmToken = task.Result;
            CurrentToken = fcmToken;
            InformationUpdate(uid, CurrentAccount.Instance.currentAuth.CurrentUser.Email, fcmToken);
            SetCurrentTokensField(fcmToken);
        });
    }
    private void SetCurrentTokensField(string token)
    {
        reference.Child("CurrentTokens").Child(uid).SetValueAsync(token).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                return;
            }
            else if (task.IsCanceled)
            {
                return;
            }

        });
    }
}
