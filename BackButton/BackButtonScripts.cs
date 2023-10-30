using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButtonScripts : MonoBehaviour//뒤로가기 버튼에 직접 붙어있는 스크립트
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
