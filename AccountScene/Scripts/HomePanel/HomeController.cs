using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeController : MonoBehaviour//홈패널에 붙어있음
{
    [SerializeField] private GameObject menuPanel;
    public void MenuBtnClick()
    {
        menuPanel.SetActive(true);
    }
}
