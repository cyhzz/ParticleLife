using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(UIElement))]
[AddComponentMenu("UI/ZUI/UI Animation", 5)]
public class UIAnimation : MonoBehaviour {

    //The UIElement that will be animated.
    public UIElement myUIElement;
    [Tooltip("The duration in seconds before running the animation for the first time.")]
    public float StartAnimationAfter;

    [System.Serializable]
    public class AnimationSection
    {
        [Tooltip("Should this section animation be enable?")]
        public bool UseSection;
        [Tooltip("Type of animation.")]
        public MotionType Type = MotionType.EaseOut;
        public Vector3 WantedVectorValue;
        public float WantedFloatValue;
        [Tooltip("Custom duration for this section's animation (seconds).")]
        public float Duration = -1;
        [HideInInspector]
        public Vector3 startVectorValue;
        [HideInInspector]
        public float startFloatValue;
        [SerializeField]
        internal EasingEquationsParameters easingParams;

        public AnimationSection(bool use, Vector3 wantedVec)
        {
            UseSection = use;
            WantedVectorValue = wantedVec;
            Type = MotionType.EaseOut;
            Duration = -1;
        }
        public AnimationSection()
        {
            UseSection = false;
            Type = MotionType.EaseOut;
            Duration = -1;
        }
    }

    [System.Serializable]
    public class AnimationFrame
    {
        [Tooltip("The duration in seconds it would take to start this animation after the last one has finished.")]
        public float StartAfter = 0.0f;
        [Tooltip("The duration of the animation.")]
        public float Duration = 0.5f;

        #region Movement
        public AnimationSection MovementSection;
        [Tooltip("The position the element will move to when its hiding.")]
        public UIElement.ScreenSides MovementHidingPosition;
        [Tooltip("The gap between the element and the edge of the screen while it's hiding (in percentage of element's width or height).")]
        public float EdgeGap = 0.25f;
        [Tooltip("Mark this as \"true\" if you want custom hiding position be parent related, \"false\" if you want it to be canvas related.")]
        public bool LocalCustomPosition = true;
        [Tooltip("Should the element move to its start position?")]
        public bool StartMovement;
      
        //Made for the custom editor
        public AnimationSection _MovementSection { get { return MovementSection; } }
        #endregion

        #region Rotation
        public AnimationSection RotationSection;
        [Tooltip("The direction of the rotation when it's changing to the desired angle.")]
        public UIElement.RotationDirection Direction;
        [Tooltip("Should the element move to its start rotation?")]
        public bool StartRotation;

        //Made for the custom editor
        public AnimationSection _RotationSection { get { return RotationSection; } }
        #endregion

        #region Scale
        public AnimationSection ScaleSection;
        [Tooltip("Should the element move to its start scale?")]
        public bool StartScale;

        //Made for the custom editor
        public AnimationSection _ScaleSection { get { return ScaleSection; } }
        #endregion

        #region Opacity
        public AnimationSection OpacitySection;
        [Tooltip("Should the element move to its start opacity?")]
        public bool StartOpacity;

        //Made for the custom editor
        public AnimationSection _OpacitySection { get { return OpacitySection; } }
        #endregion

        #region Slice
        public AnimationSection SliceSection;
        [Tooltip("Should the element move to its start fill amount?")]
        public bool StartSlice;

        //Made for the custom editor
        public AnimationSection _SliceSection { get { return SliceSection; } }
        #endregion

        [Tooltip("The clip that will be played once this frame starts.")]
        public AudioClip AudioClip;

        public UnityEvent OnPlay;


        public AnimationFrame(float sAfter, float duration, float movementDuration, float rotDuration, float scalDuration, float opacDuration, float sliceDuration)
        {
            StartAfter = sAfter;
            Duration = duration;

            MovementSection = new AnimationSection(true, new Vector3(9999, 9999));
            MovementSection.Duration = movementDuration;

            RotationSection = new AnimationSection();
            RotationSection.Duration = rotDuration;

            ScaleSection = new AnimationSection();
            ScaleSection.Duration = scalDuration;

            OpacitySection = new AnimationSection();
            OpacitySection.Duration = opacDuration;

            SliceSection = new AnimationSection();
            SliceSection.Duration = sliceDuration;
        }
    }
    public List<AnimationFrame> AnimationFrames = new List<AnimationFrame>(1) { new AnimationFrame(0.3f, 0.5f, -1, -1, -1, -1, -1) };

    //Made for the custom editor
    public List<AnimationFrame> _AnimationFrames { get { return AnimationFrames; } }

    [Tooltip("Should the animation loop?")]
    public bool Loop = false;
    [Tooltip("Should the animation restart every time the element is visible?")]
    public bool RestartOnVisible = true;

    private RectTransform elementRT;
    private List<float> startingTimes = new List<float>();
    private IEnumerator cycleEnum;
    private bool positionControlled;
    private Vector3 startPosition;
    private bool eulerControlled;
    private Vector3 startEuler;
    private bool scaleControlled;
    private Vector3 startScale;
    private bool opacityControlled;
    private float startOpacity;
    private bool sliceControlled;
    private float startFillAmount;

    private bool initialized;

    void Awake()
    {
        myUIElement = GetComponent<UIElement>();
        myUIElement.OnShow.AddListener(() => { Play(); });
        myUIElement.OnHide.AddListener(() => { Stop(); });
    }
    void Start()
    {
        if (!initialized)
            Initialize();
    }

    void Initialize()
    {
        elementRT = myUIElement.GetComponent<RectTransform>();

        for (int i = 0; i < AnimationFrames.Count; i++)
        {
            AnimationFrame frame = AnimationFrames[i];

            if (frame.MovementSection.UseSection)
                positionControlled = true;
            if (frame.RotationSection.UseSection)
                eulerControlled = true;
            if (frame.ScaleSection.UseSection)
                scaleControlled = true;
            if (frame.OpacitySection.UseSection)
                opacityControlled = true;
            if (frame.SliceSection.UseSection)
                sliceControlled = true;
        }

        if (positionControlled)
            startPosition = elementRT.position;
        if (eulerControlled)
            startEuler = elementRT.eulerAngles;
        if (scaleControlled)
            startScale = elementRT.localScale;
        if (opacityControlled)
        {
            if (!myUIElement.TargetFader)
                myUIElement.FindTargetFader();

            if (myUIElement.TargetFader is Graphic)
            {
                Graphic tf = myUIElement.TargetFader as Graphic;
                startOpacity = tf.color.a;
            }
            else if (myUIElement.TargetFader is CanvasGroup)
            {
                CanvasGroup tf = myUIElement.TargetFader as CanvasGroup;
                startOpacity = tf.alpha;
            }
        }
        if (sliceControlled)
        {
            if (!myUIElement.SliceImage)
                myUIElement.FindSliceImage();

            if (myUIElement.SliceImage)
                startFillAmount = myUIElement.SliceImage.fillAmount;
        }

        initialized = true;
    }

    /// <summary>
    /// Start playing the animation.
    /// </summary>
    public void Play()
    {
        if (RestartOnVisible)
            ResetElement();

        cycleEnum = AnimationCycle();
        StartCoroutine(cycleEnum);

        CalculateTimeLine();
    }
    /// <summary>
    /// Stop playing the animation.
    /// </summary>
    public void Stop()
    {
        if (cycleEnum != null)
            StopCoroutine(cycleEnum);
    }

    void CalculateTimeLine()
    {
        float t = 0;
        for (int i = 0; i < AnimationFrames.Count; i++)
        {
            t += AnimationFrames[i].StartAfter;
            startingTimes.Add(t);
            t += AnimationFrames[i].Duration;
        }
    }

    void ResetElement()
    {
        if (!initialized)
            Initialize();

        if (positionControlled)
            elementRT.position = startPosition;
        if (eulerControlled)
            elementRT.eulerAngles = startEuler;
        if (scaleControlled)
            elementRT.localScale = startScale;
        if (opacityControlled)
        {
            if (myUIElement.TargetFader is Graphic)
            {
                Graphic tf = myUIElement.TargetFader as Graphic;
                tf.color = new Color(tf.color.r, tf.color.g, tf.color.b, startOpacity);
            }
            else if (myUIElement.TargetFader is CanvasGroup)
            {
                CanvasGroup tf = myUIElement.TargetFader as CanvasGroup;
                tf.alpha = startOpacity;
            }
        }
        if (sliceControlled)
        {
            if (myUIElement.SliceImage)
                myUIElement.SliceImage.fillAmount = startFillAmount;
        }
    }

    IEnumerator AnimationCycle()
    {
        yield return new WaitForSeconds(StartAnimationAfter);

        for (int i = 0; i < AnimationFrames.Count; i++)
        {
            AnimationFrame frame = AnimationFrames[i];
            yield return new WaitForSeconds(frame.StartAfter);

            //Play the animation
            if (frame.MovementSection.UseSection)
                myUIElement.ControlMovement(frame.StartMovement, frame.MovementSection.Type, frame.MovementHidingPosition, frame.MovementSection.Duration >= 0 ? frame.MovementSection.Duration : frame.Duration, frame.MovementSection.easingParams, frame.EdgeGap, true, frame.MovementSection.WantedVectorValue, frame.LocalCustomPosition);
            if (frame.RotationSection.UseSection)
                myUIElement.ControlRotation(frame.StartRotation, frame.RotationSection.Type, frame.Direction, frame.RotationSection.WantedVectorValue, frame.RotationSection.Duration >= 0 ? frame.RotationSection.Duration : frame.Duration, frame.RotationSection.easingParams, true);
            if (frame.ScaleSection.UseSection)
                myUIElement.ControlScale(frame.StartScale, frame.ScaleSection.Type, frame.ScaleSection.WantedVectorValue, frame.ScaleSection.Duration >= 0 ? frame.ScaleSection.Duration : frame.Duration, frame.ScaleSection.easingParams, true);
            if (frame.OpacitySection.UseSection)
                myUIElement.ControlOpacity(frame.StartOpacity, frame.OpacitySection.Type, frame.OpacitySection.WantedFloatValue, frame.OpacitySection.Duration >= 0 ? frame.OpacitySection.Duration : frame.Duration, frame.OpacitySection.easingParams, true);
            if (frame.SliceSection.UseSection)
                myUIElement.ControlSlice(frame.StartSlice, frame.SliceSection.Type, frame.SliceSection.WantedFloatValue, frame.SliceSection.Duration >= 0 ? frame.SliceSection.Duration : frame.Duration, frame.SliceSection.easingParams, true);

            if (i == AnimationFrames.Count - 1 && Loop)
                i = -1;

            //Play the sound clip
            if (SFXManager.Instance)
                SFXManager.Instance.PlayClip(frame.AudioClip);

            //Fire the event
            if (frame.OnPlay != null)
                frame.OnPlay.Invoke();

            yield return new WaitForSeconds(frame.Duration);
        }

        yield break;
    }
}
