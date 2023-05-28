using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineGragger : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public Color setting;
    public float speed = 1;

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log(transform.GetSiblingIndex());
        if (eventData.delta.x < 0)
        {
            setting.g += eventData.delta.x * speed;
        }
        else if (eventData.delta.x > 0)
        {
            setting.g += eventData.delta.x * speed;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ZUIManager.Instance.OpenMenu("Menu");
        MenuPresenter.instance.Present(setting);
    }
}
