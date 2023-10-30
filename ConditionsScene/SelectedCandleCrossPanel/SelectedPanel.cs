using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class SelectedPanel : MonoBehaviour
{
    [SerializeField] private Transform buttonPanel;
    private Button[] buttons;

    void Start()
    {
        buttons = buttonPanel.GetComponentsInChildren<Button>();
        Debug.Log(buttons.Length);
        foreach (Button button in buttons)
        {
            string buttonName = button.transform.name;
            button.onClick.AddListener(() => OnButtonClick(buttonName.Substring(0, buttonName.Length - 6)));//"Button" 문자열을 잘라냄
            Debug.Log($"buttonName : {buttonName} indicator : {buttonName.Substring(0, buttonName.Length - 6)}");
        }
    }
    private void OnButtonClick(string indicator)
    {
        Debug.Log(indicator);
        transform.Find(indicator + "Panel").gameObject.SetActive(true);
    }
}
