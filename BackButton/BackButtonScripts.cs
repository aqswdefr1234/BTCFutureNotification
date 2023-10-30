using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButtonScripts : MonoBehaviour//�ڷΰ��� ��ư�� ���� �پ��ִ� ��ũ��Ʈ
{
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(BackBtnClick);
    }
    private void BackBtnClick()
    {
        Transform parentTransform = gameObject.transform;
        if(parentTransform != null)
        {
            for(int i = 0; i < 10; i++)
            {
                if (parentTransform.CompareTag("Top_Level_Panel") == true)
                {
                    parentTransform.gameObject.SetActive(false);
                    break;
                }
                else
                    parentTransform = parentTransform.parent;
            }
        }
    }
}
