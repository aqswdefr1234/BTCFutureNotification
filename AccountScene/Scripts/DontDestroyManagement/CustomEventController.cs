using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CustomEventController : MonoBehaviour
{
    [SerializeField] private TMP_Text notifyText;
    void Start()
    {
        NotifyTextEventController.OnNotifyTextChanged += HandleNotifyTextChanged;
    }
    private void HandleNotifyTextChanged(string newText)
    {
        notifyText.text = newText;
        StartCoroutine(DeleteText());
    }
    IEnumerator DeleteText()
    {
        yield return new WaitForSeconds(5f);
        notifyText.text = "";
    }
}
