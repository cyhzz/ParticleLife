using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonInstance : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent OnRightClick;
    public UnityEvent OnLeftClick;
    public UnityEvent OnEnter;
    public UnityEvent OnExit;
    public UnityEvent OnUp;
    public UnityEvent OnHover;
    public UnityEvent OnPressed;

    public float pressed_wait = 0.5f;
    public float pressed_gap = 0.02f;
    float pressed_counter = 0.00f;
    float init_counter = 0.00f;

    bool is_down;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick?.Invoke();
        }
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick?.Invoke();
        }
    }
    void Update()
    {
        if (is_in)
            OnHover?.Invoke();
        if (is_down)
        {
            init_counter += Time.deltaTime;
            if (init_counter > pressed_wait)
            {
                pressed_counter += Time.deltaTime;
                if (pressed_counter >= pressed_gap)
                {
                    pressed_counter = 0;
                    OnPressed?.Invoke();
                }
            }
        }
    }
    public bool is_in;
    public void OnPointerEnter(PointerEventData eventData)
    {
        is_in = true;
        OnEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        is_in = false;
        OnExit?.Invoke();
        is_down = false;
    }
    void OnDisable()
    {
        is_down = false;
        is_in = false;
        init_counter = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        is_down = false;
        OnUp?.Invoke();
        init_counter = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        is_down = true;
    }
}
