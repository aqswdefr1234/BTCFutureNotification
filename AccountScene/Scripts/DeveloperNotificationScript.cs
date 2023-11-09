using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using TMPro;
public class DeveloperNotificationScript : MonoBehaviour
{
    [SerializeField] private Transform notificationPanel;
    [SerializeField] private Transform notificationPrefab;
    void Start()
    {
        StartCoroutine(IsInitializationFirebase());
    }
    IEnumerator IsInitializationFirebase()
    {
        WaitForSeconds delay = new WaitForSeconds(0.5f);
        int loop = 0;
        while (true)
        {   
            yield return delay;
            if (CurrentAccount.Instance.isInitialization == true)
            {
                DeveloperNotificationRead();
                break;
            }
            loop++;
            if (loop > 40)
            {
                NotifyTextEventController.NotifyText = "Initialization failed! Have you checked your internet connection?";
                break;
            }
        }
    }
    private void DeveloperNotificationRead()
    {
        List<string> list = new List<string>();
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("DeveloperNotification").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                NotifyTextEventController.NotifyText = "Initialization failed! Have you checked your internet connection?";
                return;
            }
            DataSnapshot snapshot = task.Result;
            if (snapshot != null)
            {
                foreach (var item in snapshot.Children)
                {
                    string notification = item.Value.ToString();
                    Transform textPrefab = Instantiate(notificationPrefab, notificationPanel);
                    textPrefab.GetComponent<TMP_Text>().text = notification;
                }
            }
        });
    }
}
