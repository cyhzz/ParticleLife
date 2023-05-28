using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public  class ZUIElementBase : MonoBehaviour {

    public bool Visible;
    [Tooltip("If enabled this GameObject will be activated and deactivated instead of animating its elements.")]
    public bool UseSimpleActivation = false;

    [Tooltip("The clip that will be played once this menu is opened.")]
    public AudioClip ShowingClip;
    [Tooltip("The clip that will be played once this menu is closed.")]
    public AudioClip HidingClip;

    public UnityEvent OnShow;
    public UnityEvent OnHide;
    public UnityEvent OnShowComplete;
    public UnityEvent OnHideComplete;

    [Tooltip("Don't play a sound or fire an events at the initialization frame of this element's life.")]
    public bool IgnoreEventsOnInitialization;

    protected bool Initialized;

    /// <summary>
    /// Change the visibilty of the object by playing the desired animation.
    /// </summary>
    /// <param name="visible">Should this element be visible?</param>
    /// <param name="trivial">If true, sounds won't play and events won't fire</param>
    public virtual void ChangeVisibility(bool visible, bool trivial = false)
    {
        throw new System.NotImplementedException();
    }
    /// <summary>
    /// Change the visibilty of the object instantly without playing animation.
    /// </summary>
    /// <param name="visible">Should this element be visible?</param>
    /// <param name="trivial">If true, sounds won't play and events won't fire</param>
    public virtual void ChangeVisibilityImmediate(bool visible, bool trivial = false)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Change the visibilty of the object by playing the desired animation.
    /// </summary>
    /// <param name="visible">Should this element be visible?</param>
    public void ChangeVisibility(bool visible)
    {
        ChangeVisibility(visible, false);
    }
    /// <summary>
    /// Change the visibilty of the object instantly without playing animation.
    /// </summary>
    /// <param name="visible">Should this element be visible?</param>
    public void ChangeVisibilityImmediate(bool visible)
    {
        ChangeVisibilityImmediate(visible, false);
    }

    /// <summary>
    /// Switch the visibility of the object by playing the desired animation.
    /// </summary>
    public virtual void SwitchVisibility()
    {
        ChangeVisibility(!Visible);
    }
    /// <summary>
    /// Switch the visibility of the object instantly without playing animation.
    /// </summary>
    public virtual void SwitchVisibilityImmediate()
    {
        ChangeVisibilityImmediate(!Visible);
    }

    /// <summary>
    /// Deactivate this element's Game Object.
    /// </summary>
    /// <param name="forceInvisibility">Make sure the element is completly invisible by changing the visiblity immediately.</param>
    protected void DeactivateMe(bool forceInvisibility)
    {
        if (forceInvisibility)
            ChangeVisibilityImmediate(false, true);

        gameObject.SetActive(false);
    }
    /// <summary>
    /// Deactivate this element's Game Object.
    /// </summary>
    protected void DeactivateMe()
    {
        if (OnHideComplete != null)
            OnHideComplete.Invoke();

        gameObject.SetActive(false);
    }

}
