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
    [SerializeField] private Transform textPanelParentTransform;//������ ���� ��ġ. ���⼭�� ĵ����

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
            List<string> providers = user.ProviderData.Select(provider => provider.ProviderId).ToList();//���� �ΰ��� ������� ��� �α����� �����ϴٸ� password, google.com �Ѵ� �۵��ȴ�.
            List<string> container = new List<string>();
            if (providers.Contains("password")) // �Ϲ� �̸��� �� ��ȣ
            {
                Debug.Log("���� �α����� ����ڴ� �Ϲ� �̸��� �� ��ȣ�� ����Ͽ� �����߽��ϴ�.");
                container.Add("normal");
            }
            if (providers.Contains("google.com")) // Google �Ҽ� �α���
            {
                Debug.Log("���� �α����� ����ڴ� Google �Ҽ� ������ ���� �α����߽��ϴ�.");
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
    
    //���� �α��� ���� �� �н�����, ���� �α��� ���� ������ ���� ���� ���
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
                if (googleUser != null)//����Ǿ� �ִ� ���� ������ �����Ѵٸ� 
                {
                    string googleEmail = googleUser.Email;
                    string firebaseEmail = firebaseUser.Email;
                    if (firebaseEmail == googleEmail)//����Ǿ� �ִ� ���� ������ ���� �α��� �Ǿ� �ִ� ������ ���� �����̶��
                    {
                        DeleteFirestoreAndRealtimeDatabase(firebaseUser);
                        GoogleSignIn.DefaultInstance.Disconnect();//���� ���� ������ �ۿ��� �����
                    }
                    else//����Ǿ� �ִ� ���� ������ ���������� ���� �α��εǾ� �ִ� �������� �ٸ��ٸ� ���� �ٽ� �α����ؾ���.
                        ElseGoogleLogin();
                }
            }
            else
                NotifyTextEventController.NotifyText = "Task is null.";
        });
    }
    private void ElseGoogleLogin()//����Ǿ� �ִ� ���� ������ �α��ε� ������ �̸����� �ٸ��ٸ�
    {
        Instantiate(textPanelPrefab, textPanelParentTransform);
    }
    
    //��ū�� ������ ���� ������ �����Ұ�� �����ؾ���
    private void DeleteFirestoreAndRealtimeDatabase(FirebaseUser user)//�Ϲݰ��� ���� ���� �Ѵ� �����
    {
        string uid = user.UserId;
        string token = "";
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        //realTime������ ��ū �� ������ �� ����. firestore�� ������ notification�����͸� ������ �� �����ؾ���.
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
            if (task.Result.Value != null)//�ش� key�� �����Ѵٸ� ����(��ū�� �߱޹��� ����)
            {
                token = task.Result.Value.ToString();
                reference.Child("CurrentTokens").Child(uid).RemoveValueAsync();
                DeleteFieldValue(token, uid, db, reference, user);
            }
            else//(��ū�� �߱޹��� ���� ����)
            {
                FirebaseAuthDelete(user);
                CurrentAccount.Instance.LogOut();
            }
        });
        
        //�ʵ� �� �ۼ��Ǿ��ִ� ��ū ������ ���������� ��ū ������ �����ϸ� �ڵ� ���� ��Ų��.
    }
    private void WaitingDeleteRealtimeDatabase()//������ �����Ǳ����� �����۾��ؾ���. �ƴϸ� ���� ��Ģ�� ���� �ۼ��� �ȵ�
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

    //��ū�� ���� ���, �˸� �������� ���� ������ ��� ����. ������ų realtimedatabase�� �ʵ���� �����������ؼ� �۵����Ѿ���. �� �� ���� ����
    private void DeleteFieldValue(string token, string uid, FirebaseFirestore db, DatabaseReference reference, FirebaseUser user)
    {
        DocumentReference sourceDocumentRef = db.Collection("Users").Document(uid).Collection("MyNotification").Document(token);
        sourceDocumentRef.GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("���� �������� ����: " + task.Exception);
                return;
            }
            DocumentSnapshot sourceDocument = task.Result;

            if (sourceDocument.Exists)//��ū�� �ְ� �˸��������� ���
            {
                Dictionary<string, object> data = sourceDocument.ToDictionary();
                //����Ÿ�ӵ����ͺ��̽� �����ϱ� ���� null �Ҵ�
                
                //firestore����
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
            else//��ū�� ������ �˸� ���������� ���
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
                    FirebaseAuthDelete(user);//FirebaseAuthDelete(user)�� CurrentAccount.Instance.LogOut()�� ��Ʈ
                    CurrentAccount.Instance.LogOut();
                });
            }
        });
    }
    //���������Ͱ� �����ϰ� �˸����� �����͵� ������� ����.�������� ����� �۾����� �������� ���������� �α׾ƿ��۾� �ʿ�
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