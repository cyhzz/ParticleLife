using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(UIAnimation))]
public class UIAnimationEditor : Editor
{

    SerializedProperty loop;
    SerializedProperty restartOnVisible;
    SerializedProperty startAnimationAfter;

    SerializedProperty animationFrames;

    AnimBool showMovementProps;
    AnimBool showRotationProps;
    AnimBool showScaleProps;
    AnimBool showOpacityProps;
    AnimBool showSliceProps;
    private static bool movementFoldout = true;
    private static bool rotationFoldout;
    private static bool scaleFoldout;
    private static bool opacityFoldout;
    private static bool sliceFoldout;

    private bool curLocalCustomPosition = true;
    static int chosenFrame = -1;
    int lastAddedFrame = -1;

    private bool recordingPosition;
    private bool lastRecordingState;
    public Vector2 originalPosition;

    void OnEnable()
    {
        loop = serializedObject.FindProperty("Loop");
        restartOnVisible = serializedObject.FindProperty("RestartOnVisible");
        startAnimationAfter = serializedObject.FindProperty("StartAnimationAfter");
        animationFrames = serializedObject.FindProperty("AnimationFrames");

        showMovementProps = new AnimBool();
        showRotationProps = new AnimBool();
        showScaleProps = new AnimBool();
        showOpacityProps = new AnimBool();
        showSliceProps = new AnimBool();

        showMovementProps.valueChanged.AddListener(Repaint);
        showRotationProps.valueChanged.AddListener(Repaint);
        showScaleProps.valueChanged.AddListener(Repaint);
        showOpacityProps.valueChanged.AddListener(Repaint);
        showSliceProps.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startAnimationAfter);
        EditorGUILayout.PropertyField(loop);
        EditorGUILayout.PropertyField(restartOnVisible);

        EditorGUILayout.LabelField("Frames", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.color = Color.green;
        if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            InsertFrame(0);
        }
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        for (int i = 0; i < animationFrames.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();

            GUIStyle s = new GUIStyle(GUI.skin.button);
            s.normal = chosenFrame == i ? GUI.skin.button.onNormal : GUI.skin.button.normal;
            if (GUILayout.Button("Frame " + (i + 1) + (lastAddedFrame == i ? " (Newest)" : ""), s, GUILayout.Height(25)))
            {
                chosenFrame = chosenFrame != i ? i : -1;

                GUI.FocusControl("");
            }

            #region Delete Button
            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(25)))
            {
                animationFrames.DeleteArrayElementAtIndex(i);
                if (i == lastAddedFrame)
                    lastAddedFrame = -1;
                i--;
                continue;
            }
            GUI.color = Color.white;
            #endregion

            EditorGUILayout.EndHorizontal();

            #region Frame Details
            //If this frame is selected
            if (chosenFrame == i)
            {
                SerializedProperty curFrame = animationFrames.GetArrayElementAtIndex(i);
              
                #region Properties
                SerializedProperty startAfter = curFrame.FindPropertyRelative("StartAfter");
                SerializedProperty duration = curFrame.FindPropertyRelative("Duration");

                SerializedProperty movementSection = curFrame.FindPropertyRelative("MovementSection");
                SerializedProperty rotationSection = curFrame.FindPropertyRelative("RotationSection");
                SerializedProperty scaleSection = curFrame.FindPropertyRelative("ScaleSection");
                SerializedProperty opacitySection = curFrame.FindPropertyRelative("OpacitySection");
                SerializedProperty sliceSection = curFrame.FindPropertyRelative("SliceSection");

                SerializedProperty audioClip = curFrame.FindPropertyRelative("AudioClip");
                SerializedProperty onPlay = curFrame.FindPropertyRelative("OnPlay");
                #endregion

                EditorGUILayout.PropertyField(startAfter);
                EditorGUILayout.PropertyField(duration);

                EditorGUILayout.Space();

                //Movement
                DrawAnimationSection(movementSection, ref movementFoldout, ref showMovementProps);

                EditorGUILayout.Space();

                //Rotation
                DrawAnimationSection(rotationSection, ref rotationFoldout, ref showRotationProps);

                EditorGUILayout.Space();

                //Scale
                DrawAnimationSection(scaleSection, ref scaleFoldout, ref showScaleProps);

                EditorGUILayout.Space();

                //Opacity
                DrawAnimationSection(opacitySection, ref opacityFoldout, ref showOpacityProps);

                EditorGUILayout.Space();

                //Slice
                DrawAnimationSection(sliceSection, ref sliceFoldout, ref showSliceProps);

                EditorGUILayout.Space();

                #region Sounds and Events
                EditorGUILayout.LabelField("Sounds & Events", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(audioClip, new GUIContent("Clip"));

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(onPlay, new GUIContent("On Play"));
                #endregion

                EditorGUILayout.Space();
            }
            #endregion

            #region Add Button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.color = Color.green;
            if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                InsertFrame(i + 1);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            #endregion

            GUI.color = Color.white;
        }

        serializedObject.ApplyModifiedProperties();
    }

    void InsertFrame(int index)
    {
        UIAnimation anim = ((UIAnimation)target);
        anim.AnimationFrames.Insert(index, new UIAnimation.AnimationFrame(0.3f, 0.5f, -1, -1, -1, -1, -1));

        lastAddedFrame = index;
    }

    void StartRecording(Vector3 wantedVectorValue)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            UIAnimation uiAnimation = target as UIAnimation;
            UIElement element = uiAnimation.GetComponent<UIElement>();
            RectTransform elementRT = element.GetComponent<RectTransform>();
            RectTransform holderRT = curLocalCustomPosition ? element.transform.parent.GetComponent<RectTransform>() : element.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

            float canvasWidth = holderRT.lossyScale.x * holderRT.rect.width;
            float canvasHeight = holderRT.lossyScale.y * holderRT.rect.height;

            if (wantedVectorValue == new Vector3(9999, 9999))
            {
                float canvasStartX = holderRT.position.x - canvasWidth / 2;
                float canvasStartY = holderRT.position.y - canvasHeight / 2;

                Undo.RecordObject(uiAnimation, "Position");
                wantedVectorValue = new Vector3((elementRT.position.x - canvasStartX) / canvasWidth,
                    (elementRT.position.y - canvasStartY) / canvasHeight, elementRT.position.z);
            }

            element.MovementSection.startVectorValue = elementRT.position;

            Vector3 chp = wantedVectorValue;

            elementRT.position = new Vector3(
                holderRT.position.x + (chp.x - 0.5f) * canvasWidth,
                holderRT.position.y + (chp.y - 0.5f) * canvasHeight, elementRT.position.z);
        }
    }
    void EndRecording()
    {
        UIAnimation uiAnimation = target as UIAnimation;
        UIElement element = uiAnimation.GetComponent<UIElement>();
        RectTransform elementRT = element.GetComponent<RectTransform>();
        RectTransform holderRT = curLocalCustomPosition ? element.transform.parent.GetComponent<RectTransform>() : element.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        Undo.RecordObject(element, "Record Hiding Position");

        float canvasWidth = holderRT.lossyScale.x * holderRT.rect.width;
        float canvasHeight = holderRT.lossyScale.y * holderRT.rect.height;
        float canvasStartX = holderRT.position.x - canvasWidth / 2;
        float canvasStartY = holderRT.position.y - canvasHeight / 2;

        uiAnimation.AnimationFrames[chosenFrame].MovementSection.WantedVectorValue = new Vector3((elementRT.position.x - canvasStartX) / canvasWidth,
                (elementRT.position.y - canvasStartY) / canvasHeight, elementRT.position.z);

        elementRT.position = element.MovementSection.startVectorValue;
    }
    void DiscardRecording()
    {
        UIAnimation uiAnimation = target as UIAnimation;
        UIElement element = uiAnimation.GetComponent<UIElement>();
        RectTransform elementRT = element.GetComponent<RectTransform>();

        elementRT.position = element.MovementSection.startVectorValue;
        lastRecordingState = recordingPosition = false;
    }

    void DrawAnimationSection(SerializedProperty section, ref bool foldout, ref AnimBool animBool)
    {
        #region Properties Initialization
        SerializedProperty useSection = section.FindPropertyRelative("UseSection");
        SerializedProperty type = section.FindPropertyRelative("Type");
        SerializedProperty wantedVectorValue = section.FindPropertyRelative("WantedVectorValue");
        SerializedProperty wantedFloatValue = section.FindPropertyRelative("WantedFloatValue");
        SerializedProperty duration = section.FindPropertyRelative("Duration");
        SerializedProperty easingParams = section.FindPropertyRelative("easingParams");

        SerializedProperty curFrame = animationFrames.GetArrayElementAtIndex(chosenFrame);

        SerializedProperty goToStartValue = curFrame.FindPropertyRelative("Start" + section.name.Substring(0, section.name.Length - 7));

        SerializedProperty movementHidingPosition = curFrame.FindPropertyRelative("MovementHidingPosition");
        SerializedProperty edgeGap = curFrame.FindPropertyRelative("EdgeGap");
        SerializedProperty localCustomPosition = curFrame.FindPropertyRelative("LocalCustomPosition");

        SerializedProperty direction = curFrame.FindPropertyRelative("Direction");
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
            EditorGUILayout.PropertyField(useSection, new GUIContent("Use " + sectionName + "*"));

            animBool.target = useSection.boolValue;

            if (EditorGUILayout.BeginFadeGroup(animBool.faded))
            {
                EditorGUILayout.PropertyField(type);
                EditorGUILayout.PropertyField(goToStartValue, new GUIContent("Go To Normal?"));

                #region Wanted Values
                if (sectionName == "Movement" && !goToStartValue.boolValue)
                {
                    EditorGUILayout.PropertyField(movementHidingPosition, new GUIContent("Hiding Position"));

                    if (movementHidingPosition.enumValueIndex == 8)   //Custom position
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

                        curLocalCustomPosition = localCustomPosition.boolValue;
                        if (lastRecordingState != recordingPosition)
                        {
                            //If recording start
                            if (recordingPosition)
                            {
                                StartRecording(wantedVectorValue.vector3Value);
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
                        wantedVectorValue.vector3Value = EditorGUILayout.Vector2Field(new GUIContent("Wanted Position", "Custom position as percentage of the screen."), customVec);
                        EditorGUILayout.PropertyField(localCustomPosition, new GUIContent("Local?"));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(edgeGap);
                    }

                }
                else if (sectionName == "Rotation")
                {
                    if (!goToStartValue.boolValue)
                        EditorGUILayout.PropertyField(wantedVectorValue, new GUIContent("Wanted Rotation", "The rotation this element should change to."));
                    EditorGUILayout.PropertyField(direction);
                }
                else if (sectionName == "Scale" && !goToStartValue.boolValue)
                    EditorGUILayout.PropertyField(wantedVectorValue, new GUIContent("Wanted Scale", "The scale this element should change to."));
                else if (sectionName == "Opacity" && !goToStartValue.boolValue)
                    EditorGUILayout.Slider(wantedFloatValue, 0, 1, new GUIContent("Wanted Opacity", "The opacity this element should fade to."));
                else if (sectionName == "Slice" && !goToStartValue.boolValue)
                    EditorGUILayout.Slider(wantedFloatValue, 0, 1, new GUIContent("Wanted Fill Amount", "The fill amount this element's image should change to."));
                #endregion

                //Ease Function Parameters Drawing
                DrawEasingParams(type.enumNames[type.enumValueIndex], easingParams);
                #region Custom Properties

                #region Custom Properties Drawing
                if (duration.floatValue >= 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(duration);
                    if (GUILayout.Button("Delete", EditorStyles.miniButtonRight))
                    {
                        Undo.RecordObject(target, "Delete Custom Duration");

                        UIAnimation.AnimationSection s = GetFrameAnimationSection(section);
                        s.Duration = -1;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                #endregion

                //Seperate in case there an option to add custom property
                if (duration.floatValue < 0)
                    EditorGUILayout.Space();

                #region Custom Properties Adding
                if (duration.floatValue < 0)
                {
                    string txt = targets.Length == 1 ? "Add custom \"" + duration.displayName + "\"" : "Add custom \"" + duration.displayName + "\" to all";
                    string tooltip = "Add a custom \"" + duration.displayName + "\" for this animation section, meaning this section will ignore general \"" + duration.displayName + "\".";
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent(txt, tooltip), EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                    {
                        Undo.RecordObject(target, "Add Custom Duration");

                        UIAnimation anim = target as UIAnimation;
                        UIAnimation.AnimationSection s = GetFrameAnimationSection(section);
                        s.Duration = anim.AnimationFrames[chosenFrame].Duration;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                #endregion

                #endregion

                EditorGUILayout.EndFadeGroup();
            }
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

    UIAnimation.AnimationSection GetFrameAnimationSection(SerializedProperty section)
    {
        UIAnimation anim = target as UIAnimation;
        System.Reflection.PropertyInfo frameListPropInfo = anim.GetType().GetProperty("_AnimationFrames");
        IList frameListI = frameListPropInfo.GetValue(anim, null) as IList;
        System.Reflection.PropertyInfo sectionProp = frameListPropInfo.PropertyType.GetGenericArguments()[0].GetProperty("_" + section.name);
        foreach (var listItem in frameListI)
        {
            if (frameListI.IndexOf(listItem) == chosenFrame)
            {
                UIAnimation.AnimationSection s = sectionProp.GetValue(listItem, null) as UIAnimation.AnimationSection;
                return s;
            }
        }

        return null;
    }
}
