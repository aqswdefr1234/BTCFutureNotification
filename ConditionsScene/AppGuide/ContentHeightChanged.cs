using UnityEngine;
using UnityEngine.UI;
public class ContentHeightChanged : MonoBehaviour
{
    void Start()
    {
        RectTransform contentRectTransform = transform.GetComponent<RectTransform>();
        VerticalLayoutGroup verticalLayoutGroup = transform.GetComponent<VerticalLayoutGroup>();
        float totalHeight = 0f;

        for (int i = 0; i < contentRectTransform.childCount; i++)
        {
            RectTransform child = contentRectTransform.GetChild(i).GetComponent<RectTransform>();
            totalHeight += child.rect.height;
        }

        totalHeight += verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom;
        totalHeight += verticalLayoutGroup.spacing * (contentRectTransform.childCount - 1);
        totalHeight += 100f; //°øÂ÷°ª
        contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
    }
}
