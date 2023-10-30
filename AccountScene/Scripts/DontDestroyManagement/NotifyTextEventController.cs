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
        set//set은 CurrentOBJTransform에 값이 할당되었을 때 실행된다. get은 transform a = OBJScene_DataRepository.CurrentOBJTransform 처럼 값을 가져올 때 실행된다.
        {
            OnNotifyTextChanged?.Invoke(value);//get이 없으면 값을 읽을 수 없다.
        }
    }
}
