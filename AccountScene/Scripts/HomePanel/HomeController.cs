using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeController : MonoBehaviour//Ȩ�гο� �پ�����
{
    [SerializeField] private GameObject menuPanel;
    public void MenuBtnClick()
    {
        menuPanel.SetActive(true);
    }
}
