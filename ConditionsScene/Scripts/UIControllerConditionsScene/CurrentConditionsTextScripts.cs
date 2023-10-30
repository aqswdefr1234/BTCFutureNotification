using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CurrentConditionsTextScripts : MonoBehaviour
{
    [SerializeField] private TMP_Text currentSelectedConditionsText;

    void Start()
    {
        SelectedConditionsEvent.OnSelectedConditionsChanged += ConditionsHandler;
    }
    private void ConditionsHandler(string newText)
    {
        currentSelectedConditionsText.text = newText;
    }
}
