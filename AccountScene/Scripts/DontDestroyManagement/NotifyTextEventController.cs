using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class NotifyTextEventController
{
    public static event Action<string> OnNotifyTextChanged;

    public static string NotifyText
    {
        set//set�� CurrentOBJTransform�� ���� �Ҵ�Ǿ��� �� ����ȴ�. get�� transform a = OBJScene_DataRepository.CurrentOBJTransform ó�� ���� ������ �� ����ȴ�.
        {
            OnNotifyTextChanged?.Invoke(value);//get�� ������ ���� ���� �� ����.
        }
    }
}
