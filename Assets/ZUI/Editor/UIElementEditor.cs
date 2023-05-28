using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CanEditMultipleObjects()]
[CustomEditor(typeof(UIElement))]
public class UIElementEditor : Editor
{
    #region Settings Properties
    private SerializedProperty menuDependant;
    private SerializedProperty visible;
    private SerializedProperty controlledBy;
    private SerializedProperty prewarm;
    private SerializedProperty deactivateWhileInvisible;
    private SerializedProperty showAfter;
    private SerializedProperty hideAfter;
    private SerializedProperty showingDuration;
    private SerializedProperty hidingDuration;
    private SerializedProperty useUnscaledTime;
    private SerializedProperty durationLink;
    #endregion

    #region Movement Properties
    private SerializedProperty movementSection;

    private SerializedProperty hidingPosition;
    private SerializedProperty edgeGap;
    private SerializedProperty localCustomPosition;
    private bool recordingPosition;
    private bool lastRecordingState;
    AnimBool showMovementProps;
    private static bool movementFoldout = true;
    #endregion

    #region Rotation Properties
    private SerializedProperty rotationSection;

    private SerializedProperty showingDirection;
    private SerializedProperty hidingDirection;
    AnimBool showRotationProps;
    private static bool rotationFoldout;
    #endregion

    #region Scale Properties
    private SerializedProperty scaleSection;

    AnimBool showScaleProps;
    private static bool scaleFoldout;
    #endregion

    #region Opacity Properties
    private SerializedProperty opacitySection;

    private SerializedProperty targetFader;
    AnimBool showOpacityProps;
    private static bool opacityFoldout;
    #endregion

    #region Slice Properties
    private SerializedProperty sliceSection;
    private SerializedProperty sliceImage;

    AnimBool showSliceProps;
    private static bool sliceFoldout;
    #endregion

    #region Activation Properties
    private SerializedProperty useSimpleActivation;
    #endregion

    #region Sounds Properties
    private SerializedProperty showingClip;
    private SerializedProperty hidingClip;
    #endregion

    #region Events Properties
    private SerializedProperty onShow;
    private SerializedProperty onHide;
    private SerializedProperty onShowComplete;
    private SerializedProperty onHideComplete;
    private SerializedProperty ignoreEventsOnInitialization;
    #endregion

    #region Editor Variables
    private Component lastTargetFader;
    private static int selectedAnimTab = 0;
    private Texture linkIcon;
    private int lastAnimSection;
    private bool lastDurationLink;
    #endregion

    void OnEnable()
    {
        #region Settings Properties
        menuDependant = serializedObject.FindProperty("MenuDependent");
        visible = serializedObject.FindProperty("Visible");
        controlledBy = serializedObject.FindProperty("ControlledBy");
        prewarm = serializedObject.FindProperty("Prewarm");
        deactivateWhileInvisible = serializedObject.FindProperty("DeactivateWhileInvisible");
        showAfter = serializedObject.FindProperty("ShowAfter");
        hideAfter = serializedObject.FindProperty("HideAfter");
        showingDuration = serializedObject.FindProperty("ShowingDuration");
        hidingDuration = serializedObject.FindProperty("HidingDuration");
        useUnscaledTime = serializedObject.FindProperty("UseUnscaledTime");
        durationLink = serializedObject.FindProperty("durationLink");
        #endregion

        #region Movement Properties
        movementSection = serializedObject.FindProperty("MovementSection");
        hidingPosition = serializedObject.FindProperty("HidingPosition");
        edgeGap = serializedObject.FindProperty("EdgeGap");
        localCustomPosition = serializedObject.FindProperty("LocalCustomPosition");

        bool showMovementPropsBool = true;
        for (int i = 0; i < targets.Length; i++)
        {
            if (!movementSection.FindPropertyRelative("UseSection").boolValue)
                showMovementPropsBool = false;
        }
        showMovementProps = new AnimBool(showMovementPropsBool);
        showMovementProps.valueChanged.AddListener(Repaint);
        #endregion

        #region Rotation Properties
        rotationSection = serializedObject.FindProperty("RotationSection");
        showingDirection = serializedObject.FindProperty("ShowingDirection");
        hidingDirection = serializedObject.FindProperty("HidingDirection");

        bool showRotationPropsBool = true;
        for (int i = 0; i < targets.Length; i++)
        {
            if (!rotationSection.FindPropertyRelative("UseSection").boolValue)
                showRotationPropsBool = false;
        }
        showRotationProps = new AnimBool(showRotationPropsBool);
        showRotationProps.valueChanged.AddListener(Repaint);
        #endregion

        #region Scale Properties
        scaleSection = serializedObject.FindProperty("ScaleSection");

        bool showScalePropsBool = true;
        for (int i = 0; i < targets.Length; i++)
        {
            if (!scaleSection.FindPropertyRelative("UseSection").boolValue)
                showScalePropsBool = false;
        }
        showScaleProps = new AnimBool(showScalePropsBool);
        showScaleProps.valueChanged.AddListener(Repaint);
        #endregion

        #region Opacity Properties
        opacitySection = serializedObject.FindProperty("OpacitySection");
        targetFader = serializedObject.FindProperty("TargetFader");
        lastTargetFader = targetFader.objectReferenceValue as Component;

        bool showOpacityPropsBool = true;
        for (int i = 0; i < targets.Length; i++)
        {
            if (!opacitySection.FindPropertyRelative("UseSection").boolValue)
                showOpacityPropsBool = false;
        }
        showOpacityProps = new AnimBool(showOpacityPropsBool);
        showOpacityProps.valueChanged.AddListener(Repaint);
        #endregion

        #region Slice Properties
        sliceSection = serializedObject.FindProperty("SliceSection");
        sliceImage = serializedObject.FindProperty("SliceImage");

        bool showSlicePropsBool = true;
        for (int i = 0; i < targets.Length; i++)
        {
            if (!sliceSection.FindPropertyRelative("UseSection").boolValue)
                showSlicePropsBool = false;
        }
        showSliceProps = new AnimBool(showSlicePropsBool);
        showSliceProps.valueChanged.AddListener(Repaint);
        #endregion

        #region Activation Properties
        useSimpleActivation = serializedObject.FindProperty("UseSimpleActivation");
        #endregion

        #region Souds
        showingClip = serializedObject.FindProperty("ShowingClip");
        hidingClip = serializedObject.FindProperty("HidingClip");
        #endregion

        #region Events
        onShow = serializedObject.FindProperty("OnShow");
        onHide = serializedObject.FindProperty("OnHide");
        onShowComplete = serializedObject.FindProperty("OnShowComplete");
        onHideComplete = serializedObject.FindProperty("OnHideComplete");
        ignoreEventsOnInitialization = serializedObject.FindProperty("IgnoreEventsOnInitialization");
        #endregion

        #region Get Icons
        linkIcon = (Texture)EditorGUIUtility.Load("ZUI/LinkIcon.png");
        #endregion

        #region Editor Variables
        lastAnimSection = selectedAnimTab;
        movementFoldout = EditorPrefs.GetBool("movementFoldout", true);
        rotationFoldout = EditorPrefs.GetBool("rotationFoldout");
        scaleFoldout = EditorPrefs.GetBool("scaleFoldout");
        opacityFoldout = EditorPrefs.GetBool("opacityFoldout");
        lastDurationLink = durationLink.boolValue;
        #endregion
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        bool usedActivationControl = false;
        for (int i = 0; i < targets.Length; i++)
        {
            UIElement element = targets[i] as UIElement;
            if (element.UseSimpleActivation)
                usedActivationControl = true;
        }

        EditorGUILayout.Space();


        #region Settings
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        bool someoneControlled = false;
        for (int i = 0; i < targets.Length; i++)
        {
            UIElement e = (UIElement)targets[i];
            if (e.ControlledBy != null)
                someoneControlled = true;
        }
        string controllerName = "NONE";

        //if we are only selecting one object and there's someone controlled in the selection array, then it is this object.
        if (targets.Length == 1 && someoneControlled)
            controllerName = controlledBy.objectReferenceValue.name;
        else if (targets.Length > 1 && someoneControlled)
            controllerName = "-";

        #region Controlled By
        EditorGUILayout.LabelField("Controlled By: ", controllerName);
        if (someoneControlled && GUILayout.Button("Remove Control"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                UIElement e = (UIElement)targets[i];

                Undo.RecordObject(e, "Remove Control");
                Menu m = e.ControlledBy as Menu;
                if (m != null)
                {
                    Undo.RecordObject(m, "Remove Control");
                    m.AnimatedElements.Remove((UIElement)targets[i]);
                }
                else
                {
                    UIElementsGroup eg = e.ControlledBy as UIElementsGroup;

                    if (eg != null)
                    {
                        Undo.RecordObject(eg, "Remove Control");
                        eg.AnimatedElements.Remove((UIElement)targets[i]);
                    }
                    else
                    {
                        SideMenu sm = e.ControlledBy as SideMenu;

                        if (sm != null)
                        {
                            Undo.RecordObject(sm, "Remove Control");
                            sm.AnimatedElements.Remove((UIElement)targets[i]);
                        }
                        else
                        {
                            Popup p = e.ControlledBy as Popup;

                            if (p != null)
                            {
                                Undo.RecordObject(p, "Remove Control");
                                p.AnimatedElements.Remove((UIElement)targets[i]);
                            }
                        }
                    }
                }
                e.ControlledBy = null;
            }
        }
        #endregion

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Is Visible?", visible.boolValue.ToString());
        EditorGUILayout.PropertyField(menuDependant);
        if (!menuDependant.boolValue)
        {
            EditorGUILayout.PropertyField(visible);
            EditorGUILayout.PropertyField(prewarm);
        }
        EditorGUILayout.PropertyField(useUnscaledTime);
        EditorGUILayout.PropertyField(deactivateWhileInvisible);
        #endregion

        if (!usedActivationControl)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
            selectedAnimTab = GUILayout.Toolbar(selectedAnimTab, new string[] { "Hiding", "Showing" });

            if (selectedAnimTab != lastAnimSection)
            {
                GUI.FocusControl("");
                lastAnimSection = selectedAnimTab;
            }

            bool hidingTab = selectedAnimTab == 0;

            #region Animation Sections
            if (hidingTab)
            {
                EditorGUILayout.PropertyField(hideAfter);
                EditorGUILayout.PropertyField(hidingDuration);
            }
            else
            {
                EditorGUILayout.PropertyField(showAfter);

                #region Showing Duration
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(durationLink.boolValue);
                if (durationLink.boolValue)
                    showingDuration.floatValue = hidingDuration.floatValue;
                EditorGUILayout.PropertyField(showingDuration);
                EditorGUI.EndDisabledGroup();
                durationLink.boolValue = GUILayout.Toggle(durationLink.boolValue, new GUIContent(linkIcon, "Toggle hiding duration link."), EditorStyles.miniButton, GUILayout.Width(25));
                if (lastDurationLink != durationLink.boolValue)
                {
                    GUI.FocusControl("");
                    lastDurationLink = durationLink.boolValue;
                }
                EditorGUILayout.EndHorizontal();
                #endregion
            }

            EditorGUILayout.Space();

            //Movement
            DrawAnimationSection(movementSection, hidingTab, ref movementFoldout, ref showMovementProps);

            EditorGUILayout.Space();

            //Rotation
            DrawAnimationSection(rotationSection, hidingTab, ref rotationFoldout, ref showRotationProps);

            EditorGUILayout.Space();

            //Scale
            DrawAnimationSection(scaleSection, hidingTab, ref scaleFoldout, ref showScaleProps);

            EditorGUILayout.Space();

            //Opacity
            DrawAnimationSection(opacitySection, hidingTab, ref opacityFoldout, ref showOpacityProps);

            EditorGUILayout.Space();

            //Slice
            DrawAnimationSection(sliceSection, hidingTab, ref sliceFoldout, ref showSliceProps);
            #endregion

            GUILayout.Space(10);
            EditorGUILayout.LabelField("* Shared settings between both tabs.", EditorStyles.miniLabel);

            #region Separator
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("__________________");
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);
            #endregion
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("No animation controls available. This GameObject is being controlled by \"Simple Activate/Deactivate\" option.", MessageType.Info);
        }

        #region Activation
        EditorGUILayout.LabelField("Simple Activate/Deactivate", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useSimpleActivation, new GUIContent("Use Activation Control"));
        #endregion

        EditorGUILayout.Space();

        #region Sounds and Events
        EditorGUILayout.LabelField("Sounds & Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(showingClip, new GUIContent("Showing Clip"));
        EditorGUILayout.PropertyField(hidingClip, new GUIContent("Hiding Clip"));

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onShow, new GUIContent("On Show"));
        EditorGUILayout.PropertyField(onHide, new GUIContent("On Hide"));

        EditorGUILayout.PropertyField(onShowComplete, new GUIContent("On Show Complete"));
        EditorGUILayout.PropertyField(onHideComplete, new GUIContent("On Hide Complete"));

        EditorGUILayout.PropertyField(ignoreEventsOnInitialization, new GUIContent("Ignore On Initialization"));
        #endregion

        EditorGUILayout.Space();

        EditorPrefs.SetBool("movementFoldout", movementFoldout);
        EditorPrefs.SetBool("rotationFoldout", rotationFoldout);
        EditorPrefs.SetBool("scaleFoldout", scaleFoldout);
        EditorPrefs.SetBool("opacityFoldout", opacityFoldout);

        serializedObject.ApplyModifiedProperties();
    }

    void DrawAnimationSection(SerializedProperty section, bool hiding, ref bool foldout, ref AnimBool animBool)
    {
        #region Properties Initialization
        SerializedProperty useSection = section.FindPropertyRelative("UseSection");
        SerializedProperty type = section.FindPropertyRelative((hiding ? "Hide" : "Show") + "Type");
        SerializedProperty hideType = section.FindPropertyRelative("HideType"); //We still need a reference of the hide type to assign it's value to show type if there's a link.
        SerializedProperty typeLink = section.FindPropertyRelative("TypeLink");
        SerializedProperty wantedVectorValue = section.FindPropertyRelative("WantedVectorValue");
        SerializedProperty wantedFloatValue = section.FindPropertyRelative("WantedFloatValue");
        SerializedProperty startAfter = section.FindPropertyRelative((hiding ? "Hide" : "Show") + "After");
        SerializedProperty duration = section.FindPropertyRelative((hiding ? "Hiding" : "Showing") + "Duration");
        SerializedProperty easingParams = section.FindPropertyRelative((hiding ? "hiding" : "showing") + "EasingParams");
        #endregion

        string sectionName = section.displayName.Substring(0, section.displayName.Length - 8);

        #region Header
        EditorStyles.foldout.fontStyle = FontStyle.Bold;
        string chosenType = useSection.boolValue ? type.enumDisplayNames[type.enumValueIndex] : "None";
        foldout = EditorGUILayout.Foldout(foldout, sectionName + " (" + chosenType + ")", true);
        EditorStyles.foldout.fontStyle = FontStyle.Normal;
        #endregion

        if (foldout)
        {
            if (sectionName == "Opacity")
            {
                EditorGUILayout.PropertyField(targetFader);
                if ((useSection.boolValue && targetFader.objectReferenceValue == null) || targetFader.objectReferenceValue != lastTargetFader)
                {
                    //In case the dragged object isn't one of the accepted types, then look for an accepted type in that gameobject's components.
                    if (targetFader.objectReferenceValue == null || (targetFader.objectReferenceValue.GetType() != typeof(Graphic) && targetFader.objectReferenceValue.GetType() != typeof(CanvasGroup)))
                    {
                        Component fader = null;
                        UIElement element = (UIElement)target;

                        fader = element.gameObject.GetComponent<Graphic>();
                        if (!fader)
                            fader = element.gameObject.GetComponent<CanvasGroup>();
                        if (fader)
                        {
                            Undo.RecordObject(element, "Add Fader");
                            targetFader.objectReferenceValue = fader;
                        }

                    }
                    lastTargetFader = targetFader.objectReferenceValue as Component;
                }
            }

            EditorGUILayout.PropertyField(useSection, new GUIContent("Use " + sectionName + "*"));

            
            #region Group Checks
            bool showPropsBool = true;
            bool allStartAfterShow = true;
            bool allDurationShow = true;
            bool allHidingPositionsCustom = true;

            for (int i = 0; i < targets.Length; i++)
            {
                UIElement element = targets[i] as UIElement;
                System.Reflection.PropertyInfo sectionProp = element.GetType().GetProperty("_" + section.name);
                UIElement.AnimationSection s = sectionProp.GetValue(element, null) as UIElement.AnimationSection;

                if (hiding && s.HideAfter < 0)
                    allStartAfterShow = false;
                else if (!hiding && s.ShowAfter < 0)
                    allStartAfterShow = false;

                if (hiding && s.HidingDuration < 0)
                    allDurationShow = false;
                else if (!hiding && s.ShowingDuration < 0)
                    allDurationShow = false;

                if (!s.UseSection)
                    showPropsBool = false;

                if (element.HidingPosition != UIElement.ScreenSides.Custom)
                    allHidingPositionsCustom = false;
            }
            #endregion

            animBool.target = showPropsBool;

            if (!showPropsBool && useSection.boolValue && animBool.faded < 0.1f)
                EditorGUILayout.PropertyField(type);

            if (EditorGUILayout.BeginFadeGroup(animBool.faded))
            {
                #region Opacity & Slice target references
                if (sectionName == "Opacity")
                {
                    if (targetFader.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("There's no Graphics (Image, Text, etc...) nor Canvas Group components on this element, opacity animation won't play.", MessageType.Warning);
                    }
                }
                else if (sectionName == "Slice")
                {
                    GetTargetSliceImage();

                    if (sliceImage.objectReferenceValue == null)
                        EditorGUILayout.HelpBox("There's no Image components on this element, slicing won't work.", MessageType.Warning);
                    else if(((Image)sliceImage.objectReferenceValue).type != Image.Type.Filled)
                        EditorGUILayout.HelpBox("Image type is not \"Filled\", please make sure you change the type of the Image to \"Filled\" otherwise slicing animation won't play.", MessageType.Warning);
                }
                #endregion

                if (hiding)
                    EditorGUILayout.PropertyField(type);
                else
                {
                    #region Showing Type
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(typeLink.boolValue);
                    if (typeLink.boolValue)
                        type.enumValueIndex = hideType.enumValueIndex;
                    EditorGUILayout.PropertyField(type);
                    EditorGUI.EndDisabledGroup();
                    typeLink.boolValue = GUILayout.Toggle(typeLink.boolValue, new GUIContent(linkIcon, "Toggle hiding type link."), EditorStyles.miniButton, GUILayout.Width(25));
                    EditorGUILayout.EndHorizontal();
                    #endregion
                }

                #region Wanted Values
                if (sectionName == "Movement" && hiding)
                {
                    EditorGUILayout.PropertyField(hidingPosition, new GUIContent("Hiding Position"));
                    if (allHidingPositionsCustom)
                    {
                        #region Discard Button
                        if (recordingPosition)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("X", GUILayout.Width(50), GUILayout.Height(15)))
                                DiscardRecording();
                            EditorGUILayout.EndHorizontal();
                        }
                        #endregion

                        #region Record Button
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (recordingPosition)
                            GUI.color = Color.red;
                        recordingPosition = GUILayout.Toggle(recordingPosition, !recordingPosition ? "Record Position" : "Finish Recording", EditorStyles.miniButton);
                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();

                        if (lastRecordingState != recordingPosition)
                        {
                            //If recording start
                            if (recordingPosition)
                            {
                                StartRecording();
                            }
                            //If recording end
                            if (!recordingPosition)
                            {
                                EndRecording();
                            }
                        }
                        lastRecordingState = recordingPosition;
                        #endregion

                        Vector2 customVec = wantedVectorValue.vector3Value;
                        wantedVectorValue.vector3Value =  EditorGUILayout.Vector2Field(new GUIContent("Hiding Position", "Custom hiding position as percentage of the screen."), customVec);
                        EditorGUILayout.PropertyField(localCustomPosition, new GUIContent("Local?"));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(edgeGap);
                    }

                }
                if (sectionName == "Rotation")
                {
                    if (hiding)
                    {
                        EditorGUILayout.PropertyField(wantedVectorValue, new GUIContent("Hiding Rotation", "The rotation this element should change to when it's invisible."));
                        EditorGUILayout.PropertyField(hidingDirection);
                    }
                    else
                        EditorGUILayout.PropertyField(showingDirection);
                }
                if (sectionName == "Scale" && hiding)
                    EditorGUILayout.PropertyField(wantedVectorValue, new GUIContent("Hiding Scale", "The scale this element should change to when it's invisible."));
                if (sectionName == "Opacity" && hiding)
                    EditorGUILayout.Slider(wantedFloatValue, 0, 1, new GUIContent("Hiding Opacity", "The opacity this element should fade to when it's invisible."));
                if (sectionName == "Slice" && hiding)
                    EditorGUILayout.Slider(wantedFloatValue, 0, 1, new GUIContent("Hiding Fill Amount", "The fill amount this element's image should change to when it's invisible."));
                #endregion

                //Ease Function Parameters Drawing
                if (hiding || !typeLink.boolValue)
                    DrawEasingParams(type.enumNames[type.enumValueIndex], easingParams);

                #region Custom Properties

                #region Custom Properties Drawing
                if (allStartAfterShow)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(startAfter);
                    if (GUILayout.Button("Delete", EditorStyles.miniButtonRight))
                    {
                        Undo.RecordObjects(targets, "Delete Custom " + startAfter.displayName);
                        for (int i = 0; i < targets.Length; i++)
                        {
                            UIElement element = targets[i] as UIElement;
                            System.Reflection.PropertyInfo sectionProp = element.GetType().GetProperty("_" + section.name);
                            UIElement.AnimationSection s = sectionProp.GetValue(element, null) as UIElement.AnimationSection;

                            if (hiding)
                                s.HideAfter = -1;
                            else
                                s.ShowAfter = -1;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (allDurationShow)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(duration);
                    if (GUILayout.Button("Delete", EditorStyles.miniButtonRight))
                    {
                        Undo.RecordObjects(targets, "Delete Custom Duration");
                        for (int i = 0; i < targets.Length; i++)
                        {
                            UIElement element = targets[i] as UIElement;
                            System.Reflection.PropertyInfo sectionProp = element.GetType().GetProperty("_" + section.name);
                            UIElement.AnimationSection s = sectionProp.GetValue(element, null) as UIElement.AnimationSection;

                            if (hiding)
                                s.HidingDuration = -1;
                            else
                                s.ShowingDuration = -1;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                #endregion

                //Seperate in case there an option to add custom property
                if (!allStartAfterShow || !allDurationShow)
                    EditorGUILayout.Space();

                #region Custom Properties Adding
                if (!allStartAfterShow)
                {
                    string txt = targets.Length == 1 ? "Add custom \"" + startAfter.displayName + "\"" : "Add custom \"" + startAfter.displayName + "\" to all";
                    string tooltip = "Add a custom \"" + startAfter.displayName + "\" for this animation section, meaning this section will ignore general \"" + startAfter.displayName + "\".";
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent(txt, tooltip), EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                    {
                        Undo.RecordObjects(targets, "Add Custom " + startAfter.displayName);
                        for (int i = 0; i < targets.Length; i++)
                        {
                            UIElement element = targets[i] as UIElement;
                            System.Reflection.PropertyInfo sectionProp = element.GetType().GetProperty("_" + section.name);
                            UIElement.AnimationSection s = sectionProp.GetValue(element, null) as UIElement.AnimationSection;

                            if (hiding && s.HideAfter < 0)
                                s.HideAfter = element.HideAfter;
                            else if (!hiding && s.ShowAfter < 0)
                                s.ShowAfter = element.ShowAfter;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (!allDurationShow)
                {
                    string txt = targets.Length == 1 ? "Add custom \"" + duration.displayName + "\"" : "Add custom \"" + duration.displayName + "\" to all";
                    string tooltip = "Add a custom \"" + duration.displayName + "\" for this animation section, meaning this section will ignore general \"" + duration.displayName + "\".";
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent(txt, tooltip), EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.currentViewWidth/2)))
                    {
                        Undo.RecordObjects(targets, "Add Custom Duration");
                        for (int i = 0; i < targets.Length; i++)
                        {
                            UIElement element = targets[i] as UIElement;
                            System.Reflection.PropertyInfo sectionProp = element.GetType().GetProperty("_" + section.name);
                            UIElement.AnimationSection s = sectionProp.GetValue(element, null) as UIElement.AnimationSection;

                            if (hiding && s.HidingDuration < 0)
                                s.HidingDuration = element.HidingDuration;
                            else if (!hiding && s.ShowingDuration < 0)
                                s.ShowingDuration = element.ShowingDuration;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
                #endregion
            }
            EditorGUILayout.EndFadeGroup();
        }
    }
    void DrawEasingParams(string equationName, SerializedProperty easingParams)
    {
        SerializedProperty property = easingParams.FindPropertyRelative(equationName);

        if (property != null)
        {
            SerializedProperty endProp = property.Copy();
            endProp.Next(false);

            SerializedProperty c = property.Copy();
            c.Next(true);

            while (true)
            {
                EditorGUILayout.PropertyField(c, new GUIContent(c.displayName), true);
                c.Next(false);

                if (SerializedProperty.EqualContents(c, endProp))
                    break;
            }
        }
    }

    void StartRecording()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            UIElement element = targets[i] as UIElement;
            RectTransform elementRT = element.GetComponent<RectTransform>();
            RectTransform holderRT = localCustomPosition.boolValue? element.transform.parent.GetComponent<RectTransform>() : element.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

            float canvasWidth = holderRT.lossyScale.x * holderRT.rect.width;
            float canvasHeight = holderRT.lossyScale.y * holderRT.rect.height;

            if (element.MovementSection.WantedVectorValue == new Vector3(9999, 9999))
            {
                float canvasStartX = holderRT.position.x - canvasWidth / 2;
                float canvasStartY = holderRT.position.y - canvasHeight / 2;

                Undo.RecordObject(element, "Position");
                element.MovementSection.WantedVectorValue = new Vector3((elementRT.position.x - canvasStartX) / canvasWidth,
                    (elementRT.position.y - canvasStartY) / canvasHeight, elementRT.position.z);
            }

            element.MovementSection.startVectorValue = elementRT.position;

            elementRT.position = new Vector3(
                holderRT.position.x + (element.MovementSection.WantedVectorValue.x - 0.5f) * canvasWidth,
                holderRT.position.y + (element.MovementSection.WantedVectorValue.y - 0.5f) * canvasHeight, elementRT.position.z);
        }
    }
    void EndRecording()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            UIElement element = targets[i] as UIElement;
            RectTransform elementRT = element.GetComponent<RectTransform>();
            RectTransform holderRT = localCustomPosition.boolValue ? element.transform.parent.GetComponent<RectTransform>() : element.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

            Undo.RecordObject(element, "Record Hiding Position");

            float canvasWidth = holderRT.lossyScale.x * holderRT.rect.width;
            float canvasHeight = holderRT.lossyScale.y * holderRT.rect.height;
            float canvasStartX = holderRT.position.x - canvasWidth / 2;
            float canvasStartY = holderRT.position.y - canvasHeight / 2;

            element.MovementSection.WantedVectorValue = new Vector2((elementRT.position.x - canvasStartX) / canvasWidth,
                (elementRT.position.y - canvasStartY) / canvasHeight);

            elementRT.position = element.MovementSection.startVectorValue;
        }
    }
    void DiscardRecording()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            UIElement element = targets[i] as UIElement;
            RectTransform elementRT = element.GetComponent<RectTransform>();

            elementRT.position = element.MovementSection.startVectorValue;
        }
        lastRecordingState = recordingPosition = false;
    }

    void OnDisable()
    {
        if (recordingPosition)
        {
            if (EditorUtility.DisplayDialog("Recording", "You are recording a position, would you like to apply it?", "Apply", "No"))
                EndRecording();
            else
                DiscardRecording();
        }
    }

    void GetTargetFader()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            Component fader = null;
            UIElement element = (UIElement)targets[i];
            
            fader = element.gameObject.GetComponent<Graphic>();
            if (!fader)
                fader = element.gameObject.GetComponent<CanvasGroup>();
            if (fader)
            {
                Undo.RecordObject(element, "Add Fader");
                element.TargetFader = fader;
                Debug.Log(fader);
            }
        }
    }
    void GetTargetSliceImage()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            Image img = null;
            UIElement element = (UIElement)targets[i];
            img = element.gameObject.GetComponent<Image>();

            if (img)
                element.SliceImage = img;
        }
    }
}