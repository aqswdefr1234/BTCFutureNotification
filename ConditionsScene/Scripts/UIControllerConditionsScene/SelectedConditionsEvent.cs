using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedConditionsEvent
{
    public static event Action<string> OnSelectedConditionsChanged;
    public static string SelectedConditionStirng
    {
        set
        {
            OnSelectedConditionsChanged?.Invoke(value);
        }
    }
}
