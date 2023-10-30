using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScripts : MonoBehaviour
{
    [SerializeField] private GameObject setConditionsPanel;
    [SerializeField] private GameObject bollingerBandPanel;
    [SerializeField] private GameObject rsiPanel;
    [SerializeField] private GameObject goldenCrossPanel;
    [SerializeField] private GameObject deadCrossPanel;
    [SerializeField] private GameObject appGuidePanel;
    [SerializeField] private GameObject selectedDeadPanel;
    [SerializeField] private GameObject selectedGoldenPanel;

    public void SetConditionsBtnClick()
    {
        setConditionsPanel.SetActive(true);
    }
    public void BollingerBandBtnClick()
    {
        bollingerBandPanel.SetActive(true);
    }
    public void RSIBtnClick()
    {
        rsiPanel.SetActive(true);
    }
    public void SelectedGoldenCrossBtnClick()
    {
        selectedGoldenPanel.SetActive(true);
    }
    public void SelectedDeadCrossBtnClick()
    {
        selectedDeadPanel.SetActive(true);
    }
    public void AppGuideBtnClick()
    {
        appGuidePanel.SetActive(true);
    }
}
