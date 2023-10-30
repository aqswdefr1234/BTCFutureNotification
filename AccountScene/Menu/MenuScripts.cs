using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;
using System.Collections;
using UnityEngine.UI;

public class MenuScripts : MonoBehaviour//MenuPanel�� �پ�����
{
    [SerializeField] private Sprite selectedImage;
    [SerializeField] private Sprite unSelectedImage;
    [SerializeField] private GameObject autoLoginButton;

    [SerializeField] private TMP_Text currentUserText;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject signUpPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject deletePanel;

    [SerializeField] private GameObject loginBtn;
    [SerializeField] private GameObject signUpBtn;
    [SerializeField] private GameObject logOutBtn;
    [SerializeField] private GameObject deleteBtn;

    private bool isAutoLoginBoxCheck = false;

    private void OnEnable()
    {
        string currentEmail = CurrentAccount.Instance.currentAccountEmail;
        if (PlayerPrefs.GetInt("isAutoLogin") == 1)
        {
            isAutoLoginBoxCheck = true;
            autoLoginButton.GetComponent<Image>().sprite = selectedImage;
        }
        else
        {
            isAutoLoginBoxCheck = false;
            autoLoginButton.GetComponent<Image>().sprite = unSelectedImage;
        }

        if (currentEmail == null || currentEmail == "")
        {
            loginBtn.SetActive(true);
            signUpBtn.SetActive(true);
            logOutBtn.SetActive(false);
            deleteBtn.SetActive(false);
            autoLoginButton.SetActive(false);
            currentUserText.text = "Please Log In";
        }
        else
        {
            loginBtn.SetActive(false);
            signUpBtn.SetActive(false);
            logOutBtn.SetActive(true);
            deleteBtn.SetActive(true);
            autoLoginButton.SetActive(true);
            currentUserText.text = currentEmail;
        }
    }
    public void LogInBtnClick()
    {
        loginPanel.SetActive(true);
        menuPanel.SetActive(false);
    }
    public void SignUpBtnClick()
    {
        signUpPanel.SetActive(true);
        menuPanel.SetActive(false);
    }
    public void LogOutBtnClick()
    {
        CurrentAccount.Instance.LogOut();
        loginBtn.SetActive(true);
        signUpBtn.SetActive(true);
        autoLoginButton.SetActive(false);
        logOutBtn.SetActive(false);
        deleteBtn.SetActive(false);
        currentUserText.text = "Please Log In";
    }
    public void CheckBoxBtnClick()
    {
        if(isAutoLoginBoxCheck == false)//üũ �������ִ� ���¿��� Ŭ��������
        {
            PlayerPrefs.SetInt("isAutoLogin", 1);
            autoLoginButton.GetComponent<Image>().sprite = selectedImage;//üũ�̹����� �ٲ���
            isAutoLoginBoxCheck = true;
        }
        else if(isAutoLoginBoxCheck == true)//üũ ���ִ� ���¿��� Ŭ��������
        {
            PlayerPrefs.SetInt("isAutoLogin", 0);
            autoLoginButton.GetComponent<Image>().sprite = unSelectedImage;//����� �̹����� �ٲ���
            isAutoLoginBoxCheck = false;
        }
    }
    public void DeleteAccountBtn()
    {
        deletePanel.SetActive(true);
        menuPanel.SetActive(false);
    }
}
