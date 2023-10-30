using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class DetailedConditionsPanelScripts : MonoBehaviour//DetailedConditionsPanel�� �پ�����.
{
    [SerializeField] private Sprite selectedImage;
    [SerializeField] private Sprite unSelectedImage;

    private Button[] buttons;
    private List<string> selectedDetail = new List<string>();
    private string modifiedString;
    private Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
    void Start()
    {
        Transform panel = transform;
        buttons = panel.GetComponentsInChildren<Button>();
        modifiedString = panel.parent.name.Replace("Panel", "");
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => OnButtonClick(button));
        }
    }
    void OnButtonClick(Button button)
    {
        if (button.GetComponent<Image>().sprite == unSelectedImage)
        {
            button.GetComponent<Image>().sprite = selectedImage;
            selectedDetail.Add(button.name);//���õ� ��ư �̸� �ֱ�
        }
        else
        {
            button.GetComponent<Image>().sprite = unSelectedImage;
            selectedDetail.Remove(button.name);
        }
        SaveConditions.SaveDict(modifiedString, selectedDetail);
        CurrentSelectedConditionsTextChange();
    }
    private void CurrentSelectedConditionsTextChange()//detail ��ũ��Ʈ ���� �۵��ȴ�.
    {
        dict = SaveConditions.conditionsDictionary;
        string oneString = "";
        foreach (var kvp in dict)
        {
            oneString += kvp.Key + " : ";
            foreach (string val in kvp.Value)
                oneString += val + " ";
            oneString += "\n";
        }
        SelectedConditionsEvent.SelectedConditionStirng = oneString;
    }
}
