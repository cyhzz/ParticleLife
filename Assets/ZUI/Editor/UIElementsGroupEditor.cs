using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIElementsGroup))]
public class UIElementsGroupEditor : Editor
{
    #region Inherited Properties
    SerializedProperty visible;
    SerializedProperty useSimpleActivation;

    SerializedProperty showingClip;
    SerializedProperty hidingClip;

    SerializedProperty onShow;
    SerializedProperty onHide;
    SerializedProperty onShowComplete;
    SerializedProperty onHideComplete;
    SerializedProperty ignoreEventsOnInitialization;
    #endregion

    SerializedProperty prewarm;
    SerializedProperty animatedElements;
    SerializedProperty deactivateWhileInvisible;

    private bool updatedElements;
    private int added = 0;
    private int removed = 0;

    void OnEnable()
    {
        #region Inherited Properties
        visible = serializedObject.FindProperty("Visible");
        useSimpleActivation = serializedObject.FindProperty("UseSimpleActivation");

        showingClip = serializedObject.FindProperty("ShowingClip");
        hidingClip = serializedObject.FindProperty("HidingClip");

        onShow = serializedObject.FindProperty("OnShow");
        onHide = serializedObject.FindProperty("OnHide");
        onShowComplete = serializedObject.FindProperty("OnShowComplete");
        onHideComplete = serializedObject.FindProperty("OnHideComplete");
        ignoreEventsOnInitialization = serializedObject.FindProperty("IgnoreEventsOnInitialization");
        #endregion

        prewarm = serializedObject.FindProperty("Prewarm");
        animatedElements = serializedObject.FindProperty("AnimatedElements");
        deactivateWhileInvisible = serializedObject.FindProperty("DeactivateWhileInvisible");

    }

    public override void OnInspectorGUI()
    {
        UIElementsGroup myElementsGroup = target as UIElementsGroup;

        #region User Interface
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(visible);
        EditorGUILayout.PropertyField(prewarm);
        EditorGUILayout.PropertyField(deactivateWhileInvisible);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Elements", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(animatedElements, true);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Switching", EditorStyles.boldLabel);
        if (useSimpleActivation.boolValue == true)
        {
            EditorGUILayout.HelpBox("No animations will be played, all the animated elements will be ignored because \"Use Simple Activation\" option is set to true.", MessageType.Info);
        }
        EditorGUILayout.PropertyField(useSimpleActivation);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Sounds & Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(showingClip);
        EditorGUILayout.PropertyField(hidingClip);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onShow);
        EditorGUILayout.PropertyField(onHide);

        EditorGUILayout.PropertyField(onShowComplete);
        EditorGUILayout.PropertyField(onHideComplete);

        EditorGUILayout.PropertyField(ignoreEventsOnInitialization, new GUIContent("Ignore On Initialization"));

        EditorGUILayout.Space();
        #endregion

        #region Tools
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

        #region Update Animated Elements Button
        if (GUILayout.Button("Update Animated Elements", GUILayout.Height(30)))
        {
            //Save old elements list to make a check after updating.
            List<UIElement> oldElements = myElementsGroup.AnimatedElements;
            added = 0;
            removed = 0;

            Undo.RecordObject(myElementsGroup, "Update Animated Items");

            myElementsGroup.AnimatedElements = GetAnimatedElements(myElementsGroup.transform);
            UIElement freeMenuUE = myElementsGroup.GetComponent<UIElement>();
            if (freeMenuUE)
                myElementsGroup.AnimatedElements.Insert(0, freeMenuUE);

            //Check which elements are added and which elements are removed.
            for (int i = 0; i < myElementsGroup.AnimatedElements.Count; i++)
            {
                Undo.RecordObject(myElementsGroup.AnimatedElements[i], "Update Animated Items");
                if (!oldElements.Contains(myElementsGroup.AnimatedElements[i]))
                {
                    added++;
                }
            }
            removed = oldElements.Count - myElementsGroup.AnimatedElements.Count + added;

            updatedElements = true;
        }
        #endregion

        #region Elements Updated Info
        if (updatedElements)
        {
            string removedText = removed != 0 ? "Removed " + removed + " element" + (removed == 1 ? "." : "s.") : "";
            string addedText = added != 0 ? "Added " + added + " element" + (added == 1 ? ". " : "s. ") : "";
            string finalText = (added != 0 || removed != 0) ? addedText + removedText : "Nothing changed. Is the element you want this holder to control being controlled by another holder?";

            EditorGUILayout.HelpBox(finalText, (added != 0 || removed != 0) ? MessageType.Info : MessageType.Warning);
        }
        #endregion

        #region Check Menu Independant Elements
        if (myElementsGroup.AnimatedElements != null)
        {
            for (int i = 0; i < myElementsGroup.AnimatedElements.Count; i++)
            {
                if (myElementsGroup.AnimatedElements[i] == null) continue;

                if (!myElementsGroup.AnimatedElements[i].MenuDependent)
                {
                    if (EditorUtility.DisplayDialog("Error", myElementsGroup.AnimatedElements[i].gameObject.name + " is menu independant but is inside this Element Group's elements list.", "Remove it from the list", "Switch it to menu dependant"))
                    {
                        Undo.RecordObject(myElementsGroup, "Removing from list");
                        myElementsGroup.AnimatedElements[i].ControlledBy = null;
                        myElementsGroup.AnimatedElements.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else
                    {
                        Undo.RecordObject(myElementsGroup, "Switch to menu dependant");
                        myElementsGroup.AnimatedElements[i].MenuDependent = true;
                    }
                }
                if (myElementsGroup.AnimatedElements[i].ControlledBy != myElementsGroup)
                    myElementsGroup.AnimatedElements[i].ControlledBy = myElementsGroup;
            }
        }
        #endregion
        #endregion

        serializedObject.ApplyModifiedProperties();
    }

    List<UIElement> GetAnimatedElements(Transform holder)
    {
        List<UIElement> ue = new List<UIElement>();

        foreach (Transform c in holder)
        {
            UIElement cUE = c.GetComponent<UIElement>();
            if (cUE && cUE.MenuDependent && (cUE.ControlledBy == null || cUE.ControlledBy == (UIElementsGroup)target))
                ue.Add(cUE);

            ue.AddRange(GetAnimatedElements(c));
        }
        return ue;
    }
}
