using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[AddComponentMenu("UI/ZUI/UI Element", 0)]
public class UIElement : ZUIElementBase
{
    [System.Serializable]
    public class AnimationSection
    {
        [Tooltip("Should this section animation be enable?")]
        public bool UseSection;
        [Tooltip("Type of hiding animation.")]
        public MotionType HideType = MotionType.EaseOut;
        [Tooltip("Type of showing animation.")]
        public MotionType ShowType = MotionType.EaseOut;
        //To determine whether the showing type should be identical to the hiding type or not.
        public bool TypeLink = true;
        //Two types of wanted values (Vector3, Float) to make sure that this class fits all the animation sections' change in values...
        //(e.g. Movement: Change in positino (Vector3), Rotation: Change in euler (Vector3), Opacity: Change in alpha (Float)).
        //The desired vector3 value this object should use upon changing visiblity to "hidden". (optional)
        public Vector3 WantedVectorValue;
        //The desired float value this object should use upon changing visiblity to "hidden". (optional)
        public float WantedFloatValue;
        [Tooltip("Custom time to show after (seconds).")]
        public float ShowAfter = -1;
        [Tooltip("Custom time to hide after (seconds).")]
        public float HideAfter = -1;
        [Tooltip("Custom duration for the showing animation (seconds).")]
        public float ShowingDuration = -1;
        [Tooltip("Custom duration for the hiding animation (seconds).")]
        public float HidingDuration = -1;
        //The normal vector value this element is set to at the start. (used when changing visiblity to "visible")
        //this variable is initialized at Start, if you wish to change the start value, directly change this variable after initialization. 
        [HideInInspector]
        public Vector3 startVectorValue;
        [HideInInspector]
        //The normal float value this element is set to at the start. (used when changing visiblity to "visible")
        //this variable is initialized at Start, if you wish to change the start value, directly change this variable after initialization.
        public float startFloatValue;
        //EasingEquationsParameters contains each equation (EasIn,EaseOut, etc...) needed parameters. (e.g. EaseIn parameters: Easing Power, EaseInElastic: Elacticity Power).
        [SerializeField]
        internal EasingEquationsParameters hidingEasingParams;
        [SerializeField]
        internal EasingEquationsParameters showingEasingParams;
        //To keep a reference of the current running co-routine controling this animation section; in order to reset if requested to change while running.
        internal IEnumerator motionEnum;

        public AnimationSection(bool use, Vector3 wantedVec)
        {
            UseSection = use;
            WantedVectorValue = wantedVec;
            HideType = MotionType.EaseOut;
            ShowType = MotionType.EaseOut;
            TypeLink = true;
            ShowAfter = -1;
            HideAfter = -1;
            ShowingDuration = -1;
            HidingDuration = -1;
    }
        public AnimationSection()
        {
            HideType = MotionType.EaseOut;
            ShowType = MotionType.EaseOut;
            TypeLink = true;
            ShowAfter = -1;
            HideAfter = -1;
            ShowingDuration = -1;
            HidingDuration = -1;
        }
    }

    #region Variables

    #region Settings
    [Tooltip("Should this element be controlled by a menu?")]
    public bool MenuDependent = true;
    [Tooltip("Should this element prewarm the animation at start?")]
    public bool Prewarm = true;
    [Tooltip("Should this element deactivate while its not visible on the screen.")]
    public bool DeactivateWhileInvisible;
    [Tooltip("The duration in seconds it takes before the object starts animating in to the screen.")]
    public float ShowAfter = 0.0f;
    [Tooltip("The duration in seconds it takes before the object starts animating out of the screen.")]
    public float HideAfter = 0.0f;
    [Tooltip("Duration of the showing animation in seconds (controls the speed).")]
    public float HidingDuration = 0.5f;
    [Tooltip("Duration of the hiding animation in seconds (controls the speed).")]
    public float ShowingDuration = 0.5f;
    [Tooltip("Should this element animate with the same speed despite what the timescale is? (enable this if you want to animate the element during pause screens)")]
    public bool UseUnscaledTime = false;

    //To determine whether or not the showing duration should be identical to the hiding duration.
    [SerializeField]
    private bool durationLink = true;

    //A reference to the holder (Menu, Pop-up, Side-menu, UIElementsGroup) currently controling this element.
    //Note: if this element is only used as "Multi-Animated Element" for menus then this reference will be empty.
    public ZUIElementBase ControlledBy;
    public enum ScreenSides { Top, Bottom, Left, Right, TopLeftCorner, TopRightCorner, BotLeftCorner, BotRightCorner, Custom }
    public enum RotationDirection { ClockWise, AntiClockWise}
    #endregion

    #region Movement
    public AnimationSection MovementSection = new AnimationSection(true, new Vector2(9999,9999));
    [Tooltip("The position the element will move to when its hiding.")]
    public ScreenSides HidingPosition;
    [Tooltip("The gap between the element and the edge of the screen while it's hiding (in percentage of element's width or height).")]
    public float EdgeGap = 0.25f;
    [Tooltip("Mark this as \"true\" if you want custom hiding position be parent related, \"false\" if you want it to be canvas related.")]
    public bool LocalCustomPosition = true;

    //The position calculated based on the chosen "HidingPosition" enum value.
    private Vector3 outOfScreenPos;

    //Made for the custom editor.
    public AnimationSection _MovementSection { get { return MovementSection; } }
    #endregion

    #region Rotation
    public AnimationSection RotationSection;
    [Tooltip("The direction of the rotation when this element is turning from invisible to visible.")]
    public RotationDirection ShowingDirection;
    [Tooltip("The direction of the rotation when this element is turning from invisible to visible.")]
    public RotationDirection HidingDirection;

    //Made for the custom editor.
    public AnimationSection _RotationSection { get { return RotationSection; } }
    #endregion

    #region Scale
    public AnimationSection ScaleSection;

    //Made for the custom editor
    public AnimationSection _ScaleSection { get { return ScaleSection; } }
    #endregion

    #region Opacity
    public AnimationSection OpacitySection;
    [Tooltip("The component you want to change it's opacity (Graphic or CanvasGroup components).")]
    public Component TargetFader;

    //Made for the custom editor
    public AnimationSection _OpacitySection { get { return OpacitySection; } }
    #endregion

    #region Slice
    public AnimationSection SliceSection;
    //A reference to the image component to slice. (will be assigned at initialization only if this section is used)
    [HideInInspector]
    public Image SliceImage;

    //Made for the custom editor
    public AnimationSection _SliceSection { get { return SliceSection; } }
    #endregion

    #region Actiate/Deactivate
    private IEnumerator activationEnum;
    #endregion

    #region Private Variables
    private RectTransform myRT;
    private CanvasScaler parentCanvasScaler;
    private RectTransform parentCanvasRT;
    private RectTransform directParentRT;
    private float canvasHalfWidth;
    private float canvasHalfHeight;
    private float dParentHalfWidth;
    private float dParentHalfHeight;
    private float myRTWidth;
    private float myRTHeight;
    //Used to indicate if there was a ChangeVisibility call before changing it by default at Start()
    //to avoid making unintended changes.
    private bool forceVisibilityCall;
    private float hidingTime;
    private float showingTime;
    private IEnumerator soundEnum;
    private IEnumerator startEventEnum;
    private IEnumerator completeEventEnum;
    #endregion

    #endregion

    void Start()
    {
        //If no holder has already initialized this element, then don't initialize again.
        if (!Initialized)
            Initialize();

        //This element will switch to the desired visiblity at start only if it's not being controlled by a holder. (in this case the holder will change the visibility)
        if (MenuDependent)
        {
            //Enable this if it doesn't bother you.
            //if (!ControlledBy)
            //    Debug.Log("Element \"" + gameObject.name + "\" is Menu Dependent but there's no holder controlling it, are you sure it was meant to be Menu Dependent?", gameObject);
            return;
        }

        //If a visibility call were made before Start function is called, then don't change visibility. (holders make visibility calls to it's elements at initialization)
        if (!forceVisibilityCall)
        {
            if (Prewarm)
                ChangeVisibilityImmediate(Visible);
            else
            {
                ChangeVisibility(Visible);
            }
        }
    }

    public void Initialize()
    {
        if (Initialized) return;

        //Get the total time taken for all of this element's animation sections to finish hiding/showing.
        hidingTime = GetAllHidingTime();
        showingTime = GetAllShowingTime();

        myRT = GetComponent<RectTransform>();
        parentCanvasScaler = GetComponentInParent<CanvasScaler>();
        parentCanvasRT = parentCanvasScaler.GetComponent<RectTransform>();
        if (transform.parent)
            directParentRT = transform.parent.GetComponent<RectTransform>();

        Vector2 canvasLossyScale = parentCanvasRT.lossyScale;

        canvasHalfWidth = canvasLossyScale.x * parentCanvasRT.rect.width / 2;
        canvasHalfHeight = canvasLossyScale.y * parentCanvasRT.rect.height / 2;

        if (directParentRT)
        {
            dParentHalfWidth = canvasLossyScale.x * directParentRT.rect.width / 2;
            dParentHalfHeight = canvasLossyScale.y * directParentRT.rect.height / 2;
        }

        myRTWidth = canvasLossyScale.x * myRT.rect.width;
        myRTHeight = canvasLossyScale.y * myRT.rect.height;

        MovementSection.startVectorValue = myRT.position;
        outOfScreenPos = GetHidingPosition(HidingPosition, EdgeGap, MovementSection.WantedVectorValue, LocalCustomPosition);

        RotationSection.startVectorValue = myRT.eulerAngles;

        ScaleSection.startVectorValue = myRT.localScale;

        FindTargetFader();
        FindSliceImage();

        Initialized = true;
    }

    /// <summary>
    /// The duration it takes to finish the hiding animation.
    /// </summary>
    /// <returns></returns>
    public float GetAllHidingTime()
    {
        if (hidingTime != 0)
            return hidingTime;

        if (HideAfter + HidingDuration > hidingTime)
            hidingTime = HideAfter + HidingDuration;

        float movementDuration = (MovementSection.HidingDuration > 0 ? MovementSection.HidingDuration : HidingDuration);
        if (MovementSection.HideAfter + movementDuration > hidingTime)
            hidingTime = MovementSection.HideAfter + movementDuration;

        float rotationDuration = (RotationSection.HidingDuration > 0 ? RotationSection.HidingDuration : HidingDuration);
        if (RotationSection.HideAfter + rotationDuration > hidingTime)
            hidingTime = RotationSection.HideAfter + rotationDuration;

        float scaleDuration = (ScaleSection.HidingDuration > 0 ? ScaleSection.HidingDuration : HidingDuration);
        if (ScaleSection.HideAfter + scaleDuration > hidingTime)
            hidingTime = ScaleSection.HideAfter + scaleDuration;

        float opacityDuration = (OpacitySection.HidingDuration > 0 ? OpacitySection.HidingDuration : HidingDuration);
        if (OpacitySection.HideAfter + opacityDuration > hidingTime)
            hidingTime = OpacitySection.HideAfter + opacityDuration;
        return hidingTime;
    }
    /// <summary>
    /// The duration it takes to finish the showing animation.
    /// </summary>
    /// <returns></returns>
    public float GetAllShowingTime()
    {
        if (showingTime != 0)
            return showingTime;

        if (ShowAfter + ShowingDuration > showingTime)
            showingTime = ShowAfter + ShowingDuration;

        float movementDuration = (MovementSection.ShowingDuration > 0 ? MovementSection.ShowingDuration : ShowingDuration);
        if (MovementSection.ShowAfter + movementDuration > showingTime)
            showingTime = MovementSection.ShowAfter + movementDuration;

        float rotationDuration = (RotationSection.ShowingDuration > 0 ? RotationSection.ShowingDuration : ShowingDuration);
        if (RotationSection.ShowAfter + rotationDuration > showingTime)
            showingTime = RotationSection.ShowAfter + rotationDuration;

        float scaleDuration = (ScaleSection.ShowingDuration > 0 ? ScaleSection.ShowingDuration : ShowingDuration);
        if (ScaleSection.ShowAfter + scaleDuration > showingTime)
            showingTime = ScaleSection.ShowAfter + scaleDuration;

        float opacityDuration = (OpacitySection.ShowingDuration > 0 ? OpacitySection.ShowingDuration : ShowingDuration);
        if (OpacitySection.ShowAfter + opacityDuration > showingTime)
            showingTime = OpacitySection.ShowAfter + opacityDuration;
        return showingTime;
    }


    /// <summary>
    /// Change the visibilty of the object by playing the desired animation.
    /// </summary>
    /// <param name="visible">Should this element be visible?</param>
    /// <param name="trivial">If true, sounds won't play and events won't fire</param>
    public override void ChangeVisibility(bool visible, bool trivial = false)
    {
        forceVisibilityCall = true;

        if (!Initialized)
            Initialize();

        //If this GameObject is not active, then change to the desired visibility immediatly and enable it.
        if (!gameObject.activeSelf)
        {
            ChangeVisibilityImmediate(Visible);
            gameObject.SetActive(true);
        }

        //If there's a change in visibility, then play the sound clips. (to avoid playing them if the element is already at the desired visibility when a new call is made)
        if (Visible != visible && !trivial)
        {
            if (SFXManager.Instance)
            {
                if (soundEnum != null)
                    StopCoroutine(soundEnum);

                if (visible)
                    soundEnum = PlaySoundAfter(ShowingClip, ShowAfter);
                else
                    soundEnum = PlaySoundAfter(HidingClip, HideAfter);

                //Since coroutines can only start on active objects, then don't try starting them if the object isn't active.
                if (gameObject.activeInHierarchy)
                    StartCoroutine(soundEnum);
            }
            else if (ShowingClip || HidingClip)
                Debug.LogError("You're trying to play sounds with no SFXManager in the scene. Please add one via Tools>ZUI>Creation Window...>Setup", gameObject);
        }

        Visible = visible;

        #region Events Handling
        if (!trivial)
        {
            if (startEventEnum != null)
                StopCoroutine(startEventEnum);
            if (completeEventEnum != null)
                StopCoroutine(completeEventEnum);

            if (visible)
            {
                if (OnShow != null)
                    startEventEnum = FireEventAfter(OnShow, ShowAfter);
                if (OnShowComplete != null)
                    completeEventEnum = FireEventAfter(OnShowComplete, showingTime);
            }
            else if (!visible)
            {
                if (OnHide != null)
                    startEventEnum = FireEventAfter(OnHide, HideAfter);
                //Only start waiting to fire the hide complete event if the element will not deactivate (because if it's deactivated before firing then the event won't fire)..
                //...and fire it in DeactivateMe function instead
                if (OnHideComplete != null && !DeactivateWhileInvisible)
                {
                    completeEventEnum = FireEventAfter(OnHideComplete, hidingTime);
                }
            }

            //Since coroutines can only start on active objects, then don't try starting them if the object isn't active.
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(startEventEnum);
                StartCoroutine(completeEventEnum);
            }
        }
        #endregion

        //If this element is set to use simple activation (enabling and disabling the gameObject) then do it and get out of the function.
        if (UseSimpleActivation)
        {
            ControlActivation(visible);
            if (visible && OnShow != null)
                OnShow.Invoke();
            else if (!visible && OnHide != null)
                OnHide.Invoke();
            return;
        }

        if (MovementSection.UseSection)
        {
            #region Movement Control
            MotionType type = GetSectionType(MovementSection);
            float duration = GetSectionDuration(MovementSection);
            EasingEquationsParameters easingParams = GetEasingParams(MovementSection);

            ControlMovement(visible, type, HidingPosition, duration, easingParams, EdgeGap, false, MovementSection.WantedVectorValue, LocalCustomPosition);
            #endregion
        }
        if (RotationSection.UseSection)
        {
            #region Rotation Control
            MotionType type = GetSectionType(RotationSection);
            float duration = GetSectionDuration(RotationSection);
            EasingEquationsParameters easingParams = GetEasingParams(RotationSection);

            ControlRotation(visible, type, visible ? ShowingDirection : HidingDirection, RotationSection.WantedVectorValue, duration, easingParams, false);
            #endregion
        }
        if (ScaleSection.UseSection)
        {
            #region Scale Control
            MotionType type = GetSectionType(ScaleSection);
            float duration = GetSectionDuration(ScaleSection);
            EasingEquationsParameters easingParams = GetEasingParams(ScaleSection);

            ControlScale(visible, type, ScaleSection.WantedVectorValue, duration, easingParams, false);
            #endregion
        }
        if (OpacitySection.UseSection)
        {
            #region Opacity Control
            MotionType type = GetSectionType(OpacitySection);
            float duration = GetSectionDuration(OpacitySection);
            EasingEquationsParameters easingParams = GetEasingParams(OpacitySection);

            ControlOpacity(visible, type, OpacitySection.WantedFloatValue, duration, easingParams, false);
            #endregion
        }
        if (SliceSection.UseSection)
        {
            #region Slice Control
            MotionType type = GetSectionType(SliceSection);
            float duration = GetSectionDuration(SliceSection);
            EasingEquationsParameters easingParams = GetEasingParams(SliceSection);

            ControlSlice(visible, type, SliceSection.WantedFloatValue, duration, easingParams, false);
            #endregion
        }

        //If set to deactivate while invisible, then wait until the total hiding time passes and deactivate.
        if (DeactivateWhileInvisible)
        {
            if (!visible)
                Invoke("DeactivateMe", hidingTime);
            else
                CancelInvoke("DeactivateMe");
        }
    }
    /// <summary>
    /// Change the visibility of this element instantly without playing animation.
    /// </summary>
    /// <param name="visible">Should this element be visible or not?</param>
    /// <param name="trivial">If true, sounds won't play and events won't fire</param>
    public override void ChangeVisibilityImmediate(bool visible, bool trivial = false)
    {
        forceVisibilityCall = true;

        if (!Initialized)
            Initialize();
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);


        //Only play sound effects if visibility has changed.
        if (Visible != visible && !trivial)
        {
            if (SFXManager.Instance)
                SFXManager.Instance.PlayClip(visible ? ShowingClip : HidingClip);
            else if (ShowingClip || HidingClip)
                Debug.LogError("You're trying to play sounds with no SFXManager in the scene. Please add one via Tools>ZUI>Creation Window...>Setup", gameObject);
        }

        Visible = visible;

        if (!trivial)
        {
            if (visible)
            {
                if (OnShow != null)
                    OnShow.Invoke();
                if (OnShowComplete != null)
                    OnShowComplete.Invoke();
            }
            else if (!visible)
            {
                if (OnHide != null)
                    OnHide.Invoke();
                if (OnHideComplete != null)
                    OnHideComplete.Invoke();
            }
        }

        //If this element is set to use simple activation (enabling and disabling the gameObject) then do it and get out of the function.
        if (UseSimpleActivation)
        {
            gameObject.SetActive(visible);
            return;
        }

        //Switch visiblity states in all the used animation sections.
        if (MovementSection.UseSection)
        {
            Vector3 ePos = visible ? MovementSection.startVectorValue : outOfScreenPos;
            myRT.position = ePos;
        }
        if (RotationSection.UseSection)
        {
            Vector3 eEuler = visible ? RotationSection.startVectorValue : RotationSection.WantedVectorValue;
            myRT.eulerAngles = eEuler;
        }
        if (ScaleSection.UseSection)
        {
            Vector3 eScale = visible ? ScaleSection.startVectorValue : ScaleSection.WantedVectorValue;
            myRT.localScale = eScale;
        }
        if (OpacitySection.UseSection)
        {
            float eOpacity = visible ? OpacitySection.startFloatValue : OpacitySection.WantedFloatValue;
            if (TargetFader)
            {
                if (TargetFader is Graphic)
                {
                    Graphic tf = TargetFader as Graphic;
                    Color col = tf.color;
                    col.a = eOpacity;
                    tf.color = col;
                }
                else if (TargetFader is CanvasGroup)
                {
                    CanvasGroup tf = TargetFader as CanvasGroup;
                    tf.alpha = eOpacity;
                }
            }
        }
        if (SliceSection.UseSection)
        {
            float eFill = visible ? SliceSection.startFloatValue : SliceSection.WantedFloatValue;
            if (SliceImage)
                SliceImage.fillAmount = eFill;
        }

        if (DeactivateWhileInvisible && !visible)
            DeactivateMe(false);
    }

    #region Animations Control

    #region Movement Control
    /// <summary>
    /// Play movement animation.
    /// </summary>
    /// <param name="visible"></param>
    /// <param name="motionType"></param>
    /// <param name="side"></param>
    /// <param name="duration"></param>
    /// <param name="bouncesCount"></param>
    /// <param name="bouncePower"></param>
    public void ControlMovement(bool visible, MotionType motionType, ScreenSides side, float duration, EasingEquationsParameters easingParams, float edgeGap = 0.25f, bool instantStart = false, Vector3 customPosition = new Vector3(), bool customLocal = false)
    {
        Vector3 outPos = outOfScreenPos;

        if (side != HidingPosition || edgeGap != EdgeGap || (side == ScreenSides.Custom && customLocal != LocalCustomPosition))
        {
            outPos = GetHidingPosition(side, edgeGap, customPosition, customLocal);
        }

        Vector3 ePos = visible ? MovementSection.startVectorValue : outPos;
        Vector3 sPos = myRT.position;

        //If the GameObject isn't active then we can't play co-routines so change it instantly.
        if (!gameObject.activeInHierarchy)
        {
            myRT.position = ePos;
            return;
        }

        if (MovementSection.motionEnum != null)
            StopCoroutine(MovementSection.motionEnum);

        MovementSection.motionEnum = VectorMotion((v)=> {
            myRT.position = v;
        }, sPos, ePos, MovementSection.HideAfter, MovementSection.ShowAfter,
                                            duration, easingParams, motionType, instantStart);
        StartCoroutine(MovementSection.motionEnum);
    }

    Vector3 GetHidingPosition(ScreenSides hidingPos, float edgeGap, Vector2 customPosition, bool customLocal, bool instantStart = false)
    {
        Vector3 pos = new Vector3();
        float y = 0;
        float x = 0;

        Vector2 distanceToEdge = new Vector2(myRT.pivot.x, myRT.pivot.y);

        Vector3 originalPosition = MovementSection.startVectorValue;

        switch (hidingPos)
        {
            case ScreenSides.Top:
                y = parentCanvasRT.position.y + canvasHalfHeight + myRTHeight * (distanceToEdge.y + edgeGap);
                pos = new Vector3(originalPosition.x, y, originalPosition.z);
                break;
            case ScreenSides.Bottom:
                y = parentCanvasRT.position.y - canvasHalfHeight - myRTHeight * (1 - distanceToEdge.y + edgeGap);
                pos = new Vector3(originalPosition.x, y, originalPosition.z);
                break;
            case ScreenSides.Left:
                x = parentCanvasRT.position.x - canvasHalfWidth - myRTWidth * (1 - distanceToEdge.x + edgeGap);
                pos = new Vector3(x, originalPosition.y, originalPosition.z);
                break;
            case ScreenSides.Right:
                x = parentCanvasRT.position.x + canvasHalfWidth + myRTWidth * (distanceToEdge.x + edgeGap);
                pos = new Vector3(x, originalPosition.y, originalPosition.z);
                break;
            case ScreenSides.TopLeftCorner:
                y = parentCanvasRT.position.y + canvasHalfHeight + myRTHeight * (distanceToEdge.y + edgeGap);
                x = parentCanvasRT.position.x - canvasHalfWidth - myRTWidth * (1 - distanceToEdge.x + edgeGap);
                pos = new Vector3(x, y, originalPosition.z);
                break;
            case ScreenSides.TopRightCorner:
                y = parentCanvasRT.position.y + canvasHalfHeight + myRTHeight * (distanceToEdge.y + edgeGap);
                x = parentCanvasRT.position.x + canvasHalfWidth + myRTWidth * (distanceToEdge.x + edgeGap);
                pos = new Vector3(x, y, originalPosition.z);
                break;
            case ScreenSides.BotLeftCorner:
                y = parentCanvasRT.position.y - canvasHalfHeight - myRTHeight * (1 - distanceToEdge.y + edgeGap);
                x = parentCanvasRT.position.x - canvasHalfWidth - myRTWidth * (1 - distanceToEdge.x + edgeGap);
                pos = new Vector3(x, y, originalPosition.z);
                break;
            case ScreenSides.BotRightCorner:
                y = parentCanvasRT.position.y - canvasHalfHeight - myRTHeight * (1 - distanceToEdge.y + edgeGap);
                x = parentCanvasRT.position.x + canvasHalfWidth + myRTWidth * (distanceToEdge.x + edgeGap);
                pos = new Vector3(x, y, originalPosition.z);
                break;
            case ScreenSides.Custom:
                Vector3 holderPos = new Vector3();
                float holderHalfWidth = 0;
                float holderHalfHeight = 0;

                if (customLocal && directParentRT)
                {
                    holderPos = directParentRT.position;
                    holderHalfWidth = dParentHalfWidth;
                    holderHalfHeight = dParentHalfHeight;
                }
                else
                {
                    holderPos = parentCanvasRT.position;
                    holderHalfWidth = canvasHalfWidth;
                    holderHalfHeight = canvasHalfHeight;
                }

                pos = new Vector3(
                    holderPos.x + (customPosition.x - 0.5f) * holderHalfWidth * 2,
                    holderPos.y + (customPosition.y - 0.5f) * holderHalfHeight * 2, originalPosition.z);
                break;
        }
        return pos;
    }
    #endregion

    #region Rotation Control
    /// <summary>
    /// Play rotation animation.
    /// </summary>
    /// <param name="visible"></param>
    /// <param name="motionType"></param>
    /// <param name="euler"></param>
    /// <param name="duration"></param>
    public void ControlRotation(bool visible, MotionType motionType, RotationDirection direction, Vector3 euler, float duration, EasingEquationsParameters easingParams, bool instantStart = false)
    {
        euler = ClampAngleVector(euler);

        Vector3 eEuler = visible ? RotationSection.startVectorValue : euler;
        Vector3 sEuler = myRT.eulerAngles;


        //Control rotating direction
        if (eEuler.z > sEuler.z)
        {
            if (direction == RotationDirection.ClockWise)
            {
                eEuler -= new Vector3(eEuler.x > 0 ? 360 : 0, eEuler.y > 0 ? 360 : 0, eEuler.z > 0 ? 360 : 0);
            }
        }
        else
        {
            if (direction == RotationDirection.AntiClockWise)
            {
                sEuler -= new Vector3(sEuler.x > 0 ? 360 : 0, sEuler.y > 0 ? 360 : 0, sEuler.z > 0 ? 360 : 0);
            }
        }

        //If the GameObject isn't active then we can't play co-routines so change it instantly.
        if (!gameObject.activeInHierarchy)
        {
            myRT.eulerAngles = eEuler;
            return;
        }

        if (RotationSection.motionEnum != null)
            StopCoroutine(RotationSection.motionEnum);

        RotationSection.motionEnum = VectorMotion((v)=> { myRT.eulerAngles = v; }, sEuler, eEuler, RotationSection.HideAfter, RotationSection.ShowAfter, duration, easingParams, motionType, instantStart);
        StartCoroutine(RotationSection.motionEnum);

    }

    Vector3 ClampAngleVector(Vector3 vec)
    {
        if (vec.x < 0) vec.x += 360;
        if (vec.y < 0) vec.y += 360;
        if (vec.z < 0) vec.z += 360;

        if (vec.x > 360) vec.x -= 360;
        if (vec.y > 360) vec.y -= 360;
        if (vec.z > 360) vec.z -= 360;

        if (vec.x < 0 || vec.x > 360 || vec.y < 0 || vec.y > 360 || vec.z < 0 || vec.z > 360)
            vec = ClampAngleVector(vec);

        return vec;

    }
    #endregion

    #region Scale Control
    /// <summary>
    /// Play scale animation
    /// </summary>
    /// <param name="visible"></param>
    /// <param name="motionType"></param>
    /// <param name="scale"></param>
    /// <param name="duration"></param>
    public void ControlScale(bool visible, MotionType motionType, Vector3 scale, float duration, EasingEquationsParameters easingParams, bool instantStart = false)
    {
        Vector3 eScale = visible ? ScaleSection.startVectorValue : scale;
        Vector3 sScale = myRT.localScale;

        //If the GameObject isn't active then we can't play co-routines so change it instantly.
        if (!gameObject.activeInHierarchy)
        {
            myRT.localScale = eScale;
            return;
        }

        if (ScaleSection.motionEnum != null)
            StopCoroutine(ScaleSection.motionEnum);

        ScaleSection.motionEnum = VectorMotion((v)=> { myRT.localScale = v; }, sScale, eScale, ScaleSection.HideAfter, ScaleSection.ShowAfter, duration, easingParams, motionType, instantStart);
        StartCoroutine(ScaleSection.motionEnum);

    }
    #endregion

    #region Opacity Control
    /// <summary>
    /// Play opacity animation
    /// </summary>
    /// <param name="visible"></param>
    /// <param name="motionType"></param>
    /// <param name="opac"></param>
    /// <param name="duration"></param>
    public void ControlOpacity(bool visible, MotionType motionType, float opac, float duration, EasingEquationsParameters easingParams, bool instantStart = false)
    {
        if (!TargetFader)
            FindTargetFader();

        if (!TargetFader)
            return;

        float eOpacity = visible ? OpacitySection.startFloatValue : opac;
        float sOpacity = 0;

        //If the GameObject isn't active then we can't play co-routines so change it instantly.
        if (!gameObject.activeInHierarchy)
        {
            if (TargetFader is Graphic)
            {
                Graphic tf = TargetFader as Graphic;
                Color col = tf.color;
                col.a = eOpacity;
                tf.color = col;
            }
            else if (TargetFader is CanvasGroup)
            {
                CanvasGroup tf = TargetFader as CanvasGroup;
                tf.alpha = eOpacity;
            }
            return;
        }

        if (TargetFader is Graphic)
        {
            Graphic tf = TargetFader as Graphic;
            sOpacity = tf.color.a;
        }
        else if (TargetFader is CanvasGroup)
        {
            CanvasGroup tf = TargetFader as CanvasGroup;
            sOpacity = tf.alpha;
        }

        if (OpacitySection.motionEnum != null)
            StopCoroutine(OpacitySection.motionEnum);

        OpacitySection.motionEnum = FloatMotion((f) => {
            if (TargetFader is Graphic)
            {
                Graphic tf = TargetFader as Graphic;
                Color col = tf.color;
                col.a = f;
                tf.color = col;
            }
            else if (TargetFader is CanvasGroup)
            {
                CanvasGroup tf = TargetFader as CanvasGroup;
                tf.alpha = f;
            }
        }, sOpacity, eOpacity, OpacitySection.HideAfter, OpacitySection.ShowAfter, duration, easingParams, motionType, instantStart);

        StartCoroutine(OpacitySection.motionEnum);
    }

    public void FindTargetFader()
    {
        if (TargetFader)
        {
            if (TargetFader is Graphic)
            {
                Graphic tf = TargetFader as Graphic;
                OpacitySection.startFloatValue = tf.color.a;
            }
            else if (TargetFader is CanvasGroup)
            {
                CanvasGroup tf = TargetFader as CanvasGroup;
                OpacitySection.startFloatValue = tf.alpha;
            }
        }
        else
        {
            TargetFader = GetComponent<Graphic>();
            if (TargetFader)
                OpacitySection.startFloatValue = ((Graphic)TargetFader).color.a;
            else
            {
                TargetFader = GetComponent<CanvasGroup>();
                if (TargetFader)
                    OpacitySection.startFloatValue = ((CanvasGroup)TargetFader).alpha;
                else if (OpacitySection.UseSection)
                    Debug.LogError("Theres no Image, Text, RawImage nor CanvasGroup component on " + gameObject.name + ", must have one of them to use the Opacity transition.", gameObject);
            }
        }
    }
    #endregion

    #region Slice Control
    /// <summary>
    /// Play slicing animation
    /// </summary>
    /// <param name="visible"></param>
    /// <param name="motionType"></param>
    /// <param name="fill"></param>
    /// <param name="duration"></param>
    public void ControlSlice(bool visible, MotionType motionType, float fill, float duration, EasingEquationsParameters easingParams, bool instantStart = false)
    {
        if (!SliceImage)
            FindSliceImage();

        if (!SliceImage)
            return;

        float eFill = visible ? SliceSection.startFloatValue : fill;
        float sFill = 0;

        //If the GameObject isn't active then we can't play co-routines so change it instantly.
        if (!gameObject.activeInHierarchy)
        {
            SliceImage.fillAmount = eFill;
            return;
        }
        sFill = SliceImage.fillAmount;

        if (SliceSection.motionEnum != null)
            StopCoroutine(SliceSection.motionEnum);

        SliceSection.motionEnum = FloatMotion((f) => { SliceImage.fillAmount = f; }, sFill, eFill, SliceSection.HideAfter, 
                                                    SliceSection.ShowAfter, duration, easingParams, motionType, instantStart);

        StartCoroutine(SliceSection.motionEnum);
    }

    public void FindSliceImage()
    {
        if (SliceImage)
        {
            SliceSection.startFloatValue = SliceImage.fillAmount;
        }
        else
        {
            SliceImage = GetComponent<Image>();
            if (SliceImage)
                SliceSection.startFloatValue = SliceImage.fillAmount;
            else if (SliceSection.UseSection)
                Debug.LogError("Theres no Image component on " + gameObject.name + ", must have one to use the Slice transition.", gameObject);
        }
    }
    #endregion

    IEnumerator VectorMotion(UnityAction<Vector3> output, Vector3 start, Vector3 end, float sectionHideAfter, float sectionShowAfter, float duration, EasingEquationsParameters easingParams, MotionType motionType, bool instantStart = false)
    {
		if (!instantStart && Time.timeScale != 0)
        {
            float startAfter = Visible ? (sectionShowAfter < 0 ? ShowAfter : sectionShowAfter) :
            (sectionHideAfter < 0 ? HideAfter : sectionHideAfter);
            yield return new WaitForSeconds(startAfter);
        }

        float curTime = UseUnscaledTime ? Time.unscaledTime : Time.time;
        float startTime = curTime;

        while (curTime < startTime + duration)
        {
            float t = (curTime - startTime) / duration;
            float ease = ZUIEquations.GetEaseFloat(t, motionType, easingParams);
            output(UnClampedLerp(start, end, ease));
            yield return null;
            curTime = UseUnscaledTime ? Time.unscaledTime : Time.time;
        }
        output(end);
    }
    IEnumerator FloatMotion(UnityAction<float> output, float start, float end, float sectionHideAfter, float sectionShowAfter, float duration, EasingEquationsParameters easingParams, MotionType motionType, bool instantStart = false)
    {
        if (!instantStart && Time.timeScale != 0)
        {
            float startAfter = Visible ? (sectionShowAfter < 0 ? ShowAfter : sectionShowAfter) :
            (sectionHideAfter < 0 ? HideAfter : sectionHideAfter);
            yield return new WaitForSeconds(startAfter);
        }

        float curTime = UseUnscaledTime ? Time.unscaledTime : Time.time;
        float startTime = curTime;

        while (curTime < startTime + duration)
        {
            float t = (curTime - startTime) / duration;
            float ease = ZUIEquations.GetEaseFloat(t, motionType, easingParams);
            output(UnClampedLerp(start, end, ease));
            yield return null;
            curTime = UseUnscaledTime ? Time.unscaledTime : Time.time;
        }
        output(end);
    }
    #endregion

    #region Activation Control
    /// <summary>
    /// Activate/deactivate element.
    /// </summary>
    /// <param name="visible"></param>
    public void ControlActivation(bool visible)
    {
        gameObject.SetActive(visible);
    }
    #endregion

    #region Sounds Control
    IEnumerator PlaySoundAfter(AudioClip c, float s)
    {
        yield return new WaitForSeconds(s);

        SFXManager.Instance.PlayClip(c);
    }
    #endregion

    #region Events Control
    IEnumerator FireEventAfter(UnityEvent e, float s)
    {
        yield return new WaitForSeconds(s);

        e.Invoke();
    }
    #endregion

    public Vector3 UnClampedLerp(Vector3 a, Vector3 b, float t)
    {

        return new Vector3(UnClampedLerp(a.x, b.x, t), UnClampedLerp(a.y, b.y, t), UnClampedLerp(a.z, b.z, t));
    }
    public float UnClampedLerp(float a, float b, float t)
    {
        return t * b + (1 - t) * a;
    }

    MotionType GetSectionType(AnimationSection s)
    {
        return !Visible ? s.HideType : (s.TypeLink ? s.HideType : s.ShowType);
    }
    float GetSectionDuration(AnimationSection s)
    {
        float hidingDuration = s.HidingDuration < 0 ? HidingDuration : s.HidingDuration;
        float showingDuration = s.ShowingDuration < 0 ? (durationLink ? HidingDuration : ShowingDuration) : s.ShowingDuration;

        return !Visible ? hidingDuration : showingDuration;
    }
    EasingEquationsParameters GetEasingParams(AnimationSection s)
    {
        return !Visible ? s.hidingEasingParams : (s.TypeLink ? s.hidingEasingParams : s.showingEasingParams);
    }
}
