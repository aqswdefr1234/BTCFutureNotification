using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartButtonScripts : MonoBehaviour
{
    [SerializeField] private GameObject verifyEmailBtn;
    public void SceneLoadBtcScene()
    {
        if (CurrentAccount.Instance.currentAuth != null)
        {
            SceneManager.LoadScene("ConditionsScene");
            Debug.Log(CurrentAccount.Instance.currentAuth.CurrentUser.Email);
        }
        else
            NotifyTextEventController.NotifyText = "Please log in";
    }
}
