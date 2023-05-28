using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SideMenu))]
public class SideMenuEditor : Editor
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

    SerializedProperty animatedElements;
    SerializedProperty deactivateWhileInvisible;

    SerializedProperty overrideBack;
    SerializedProperty myBack;

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

        animatedElements = serializedObject.FindProperty("AnimatedElements");
        deactivateWhileInvisible = serializedObject.FindProperty("DeactivateWhileInvisible");

        overrideBack = serializedObject.FindProperty("OverrideBack");
        myBack = serializedObject.FindProperty("MyBack");
    }

    public override void OnInspectorGUI()
    {
        SideMenu mySideMenu = target as SideMenu;
        ZUIManager zM = FindObjectOfType<ZUIManager>();
        zM.GetAllHolders();

        #region Activate Button
        if (GUILayout.Button("Activate", GUILayout.Height(30)))
        {

            foreach (SideMenu sM in zM.AllSideMenus)
            {
                if (sM == null) continue;

                Undo.RecordObject(sM.gameObject, "Activate Side-menu");
                if (sM == mySideMenu)
                {
                    sM.gameObject.SetActive(true);
                }
                else
                {
                    sM.gameObject.SetActive(false);
                }
            }
        }
        #endregion

        #region User Interface
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Is Visible?", visible.boolValue.ToString());
        EditorGUILayout.PropertyField(deactivateWhileInvisible);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Menu Elements", EditorStyles.boldLabel);
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

        EditorGUILayout.PropertyField(overrideBack);
        if (overrideBack.boolValue)
            EditorGUILayout.PropertyField(myBack);

        EditorGUILayout.Space();
        #endregion

        #region Tools
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

        #region Update Animated Elements Button
        if (GUILayout.Button("Update Animated Elements", GUILayout.Height(30)))
        {
            //Save old elements list to make a check after updating.
            List<UIElement> oldElements = mySideMenu.AnimatedElements;
            added = 0;
            removed = 0;

            Undo.RecordObject(mySideMenu, "Update Animated Items");

            mySideMenu.AnimatedElements = GetAnimatedElements(mySideMenu.transform);
            UIElement popupUE = mySideMenu.GetComponent<UIElement>();
            if (popupUE)
                mySideMenu.AnimatedElements.Insert(0, popupUE);

            //Check which elements are added and which elements are removed.
            for (int i = 0; i < mySideMenu.AnimatedElements.Count; i++)
            {
                Undo.RecordObject(mySideMenu.AnimatedElements[i], "Update Animated Items");
                if (!oldElements.Contains(mySideMenu.AnimatedElements[i]))
                {
                    added++;
                }
            }
            removed = oldElements.Count - mySideMenu.AnimatedElements.Count + added;

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

        if (!zM && !AssetDatabase.Contains(target))
        {
            Debug.LogError("There's no ZUIManager script in the scene, you can have it by using the menu bar ZUI>Creation Window>Setup. Or by creating an empty GameObject and add ZUIManager script to it.");
            serializedObject.ApplyModifiedProperties();
            return;
        }

        #region Check Menu Independant Elements
        if (mySideMenu.AnimatedElements != null)
        {
            for (int i = 0; i < mySideMenu.AnimatedElements.Count; i++)
            {
                if (mySideMenu.AnimatedElements[i] == null) continue;

                if (!mySideMenu.AnimatedElements[i].MenuDependent)
                {
                    if (EditorUtility.DisplayDialog("Error", mySideMenu.AnimatedElements[i].gameObject.name + " is menu independant but is inside this Side-menu's elements list.", "Remove it from the list", "Switch it to menu dependant"))
                    {
                        Undo.RecordObject(mySideMenu, "Removing from list");
                        mySideMenu.AnimatedElements[i].ControlledBy = null;
                        mySideMenu.AnimatedElements.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else
                    {
                        Undo.RecordObject(mySideMenu, "Switch to menu dependant");
                        mySideMenu.AnimatedElements[i].MenuDependent = true;
                    }
                }
                if (mySideMenu.AnimatedElements[i].ControlledBy != mySideMenu)
                    mySideMenu.AnimatedElements[i].ControlledBy = mySideMenu;
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
            if (cUE && cUE.MenuDependent && (cUE.ControlledBy == null || cUE.ControlledBy == (SideMenu)target))
                ue.Add(cUE);

            ue.AddRange(GetAnimatedElements(c));
        }
        return ue;
    }
}
