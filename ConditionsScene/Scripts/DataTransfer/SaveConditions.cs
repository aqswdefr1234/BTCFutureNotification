using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using Debug = UnityEngine.Debug;
using Firebase.Database;

public class SaveConditions : MonoBehaviour
{
    public static Dictionary<string, List<string>> conditionsDictionary = new Dictionary<string, List<string>>();//나중에 시리얼라이즈필드빼야함.
    private FirebaseFirestore db;
    private DatabaseReference reference;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }
    
    public static void SaveDict(string condition, List<string> list)//같은 키값이 있다면 안의 데이터만 업데이트 된다.
    {
        conditionsDictionary[condition] = list;
        if(list.Count == 0)
        {
            conditionsDictionary.Remove(condition);
        }
    }
    public void StartNotifications()
    {
        string uid = CurrentAccount.Instance.currentAuth.CurrentUser.UserId;
        string token = FCM_Initialization.CurrentToken;
        SaveConditionsFirestore(uid, token);
    }
    private void SaveConditionsFirestore(string uid, string token)
    {
        DocumentReference docRef = db.Collection("Users").Document(uid).Collection("MyNotification").Document(token);
        if (token != null)
        {
            docRef.SetAsync(conditionsDictionary).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    NotifyTextEventController.NotifyText = "Your notification has been saved.";
                    
                    SaveNotificationRealTime(token);
                }
                else
                {
                    NotifyTextEventController.NotifyText = task.Exception.ToString();
                }
                transform.GetComponent<LoadConditions>().LoadCurrentSelectedNotification(token);
            });
        }
    }
    private void SaveNotificationRealTime(string _token)
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        string uid = CurrentAccount.Instance.currentAuth.CurrentUser.UserId;

        RemoveRealtimeField(reference, uid, _token);

        foreach (KeyValuePair<string, List<string>> pair in conditionsDictionary)//선택된 데이터 딕셔너리에서 데이터 저장
        {
            if (pair.Value is List<string> list)
            {
                foreach (string time in list)
                {
                    Debug.Log($"pair.Key : {pair.Key}, time : {time}, _token : {_token}, uid : {uid}");
                    SaveRealTime(pair.Key, time, _token, uid);
                }
            }
        }
    }
    private void SaveRealTime(string indicator, string timeFrame, string _token, string uid)//realtime 저장
    {
        if (_token == "" || _token == null || _token == "StubToken")
            _token = "TestToken";

        reference.Child($"{indicator}_{timeFrame}").Child(uid).SetValueAsync(_token).ContinueWith(task =>
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
    private void RemoveRealtimeField(DatabaseReference _reference, string uid, string _token)//리얼타임데이터베이스에 저장하기 전 이전 지표들의 값을 제거
    {
        List<string> removedList = new List<string>();
        removedList = transform.GetComponent<LoadConditions>().inputIndicators;
        if (removedList.Count != 0)
        {
            foreach (string indicator in removedList)
            {
                _reference.Child(indicator).Child(uid).RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        NotifyTextEventController.NotifyText = "Failed to delete previous notifications";
                        return;
                    }
                });
            }
        }
    }
}
