using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ViewportContentSize : MonoBehaviour
{
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private int spacing;
    void Start()
    {
        int childCount = contentRect.childCount;
        float childButtonHeight = contentRect.GetChild(0).GetComponent<RectTransform>().rect.height;//��ư���� ���̰�
        contentRect.GetComponent<VerticalLayoutGroup>().spacing = spacing;
        contentRect.sizeDelta = new Vector2(contentRect.rect.width, childCount * (childButtonHeight + spacing));
    }
}
