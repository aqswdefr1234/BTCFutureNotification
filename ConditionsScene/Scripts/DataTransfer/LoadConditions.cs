using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using Debug = UnityEngine.Debug;
using Firebase.Messaging;

public class LoadConditions : MonoBehaviour
{
    [SerializeField] private TMP_Text myNotificationText;
    private FirebaseFirestore db;
    public List<string> inputIndicators = new List<string>();
    //public Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
    public List<string> listKey = new List<string>();
    public List<string> listValue = new List<string>();
    public void LoadCurrentSelectedNotification(string token)
    {
        inputIndicators.Clear();

        string uid = CurrentAccount.Instance.currentAuth.CurrentUser.UserId;
        db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("Users").Document(uid).Collection("MyNotification").Document(token);
        string oneString = "";

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                NotifyTextEventController.NotifyText = "Failed to retrieve document";
                return;
            }
            if (task.IsCompletedSuccessfully)
            {
                NotifyTextEventController.NotifyText = "Read Data Success!";
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Dictionary<string, object> dict = snapshot.ToDictionary();
                    foreach (KeyValuePair<string, object> pair in dict)
                    {
                        oneString += pair.Key + " : ";
                        
                        if (pair.Value is List<object> list)
                        {
                            foreach (var time in list)
                            {
                                inputIndicators.Add($"{pair.Key}_{time}");
                                oneString += " "+ time.ToString();
                            }
                        }
                        oneString += "\n";
                    }
                    myNotificationText.text = oneString;
                    
                }
                else
                {
                    Debug.Log(String.Format("Document {0} does not exist!", snapshot.Id));
                }
            }
        });
    }
}
