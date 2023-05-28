using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Menu))]
public class MenuEditor : Editor {

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

    SerializedProperty previousMenu;
    SerializedProperty nextMenu;
    SerializedProperty deactivateWhileInvisible;

    SerializedProperty animatedElements;
    SerializedProperty sharedAnimatedElements;
    SerializedProperty switchAfter;

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

        previousMenu = serializedObject.FindProperty("PreviousMenu");
        nextMenu = serializedObject.FindProperty("NextMenu");
        deactivateWhileInvisible = serializedObject.FindProperty("DeactivateWhileInvisible");

        animatedElements = serializedObject.FindProperty("AnimatedElements");
        sharedAnimatedElements = serializedObject.FindProperty("SharedAnimatedElements");
        switchAfter = serializedObject.FindProperty("SwitchAfter");

        overrideBack = serializedObject.FindProperty("OverrideBack");
        myBack = serializedObject.FindProperty("MyBack");
    }

    public override void OnInspectorGUI()
    {
        Menu myMenu = target as Menu;
        ZUIManager zuiManager = FindObjectOfType<ZUIManager>();
        zuiManager.GetAllHolders();

        #region Activate Button
        if (GUILayout.Button("Activate", GUILayout.Height(30)))
        {
            List<Menu> AllMenus = GetAllMenus();
            foreach (Menu m in AllMenus)
            {
                if (m == null) continue;

                Undo.RecordObject(m.gameObject, "Activate Menu");
                if (m == myMenu)
                {
                    m.gameObject.SetActive(true);
                }
                else
                {
                    m.gameObject.SetActive(false);
                    foreach (UIElement ue in m.SharedAnimatedElements)
                    {
                        if (ue != null)
                            ue.gameObject.SetActive(false);
                    }
                }
            }
            foreach (UIElement ue in myMenu.SharedAnimatedElements)
            {
                if (ue != null)
                    ue.gameObject.SetActive(true);
            }

        }
        #endregion

        #region User Interface
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Is Visible?", visible.boolValue.ToString());
        EditorGUILayout.PropertyField(previousMenu);
        EditorGUILayout.PropertyField(nextMenu);
        EditorGUILayout.PropertyField(deactivateWhileInvisible);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Menu Elements", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(animatedElements, true);
        EditorGUILayout.PropertyField(sharedAnimatedElements, true);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Switching", EditorStyles.boldLabel);
        if (useSimpleActivation.boolValue == true)
        {
            EditorGUILayout.HelpBox("No animations will be played, all the animated elements will be ignored because \"Use Simple Activation\" option is set to true.", MessageType.Info);
        }
        EditorGUILayout.PropertyField(useSimpleActivation);
        EditorGUILayout.PropertyField(switchAfter);

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
            List<UIElement> oldElements = myMenu.AnimatedElements;
            added = 0;
            removed = 0;

            Undo.RecordObject(myMenu, "Update Animated Items");

            myMenu.AnimatedElements = GetAnimatedElements(myMenu.transform);
            UIElement menuUE = myMenu.GetComponent<UIElement>();
            if (menuUE)
                myMenu.AnimatedElements.Insert(0, menuUE);

            //Check which elements are added and which elements are removed.
           for (int i = 0; i < myMenu.AnimatedElements.Count; i++)
            {
                Undo.RecordObject(myMenu.AnimatedElements[i], "Update Animated Items");
                if (!oldElements.Contains(myMenu.AnimatedElements[i]))
                {
                    added++;
                }
            }
            removed = oldElements.Count - myMenu.AnimatedElements.Count + added;

            updatedElements = true;
        }
        #endregion

        #region Elements Updated Info
        if (updatedElements)
        {
            string removedText = removed != 0 ? "Removed " + removed + " element" + (removed == 1? "." : "s.") : "";
            string addedText = added != 0 ? "Added " + added + " element" + (added == 1 ? ". " : "s. ") : "";
            string finalText = (added != 0 || removed != 0) ? addedText + removedText : "Nothing changed. Is the element you want this holder to control being controlled by another holder?";

            EditorGUILayout.HelpBox(finalText, (added != 0 || removed != 0) ? MessageType.Info : MessageType.Warning);
        }
        #endregion

        if (!zuiManager)
        {
            if (!AssetDatabase.Contains(target))
                Debug.LogError("There's no ZUIManager script in the scene, you can have it by using the menu bar Tools>ZUI>Creation Window>Setup. Or by creating an empty GameObject and add ZUIManager script to it.");
            serializedObject.ApplyModifiedProperties();
            return;
        }

        #region Set As Default Menu Button
        if (zuiManager.CurActiveMenu != myMenu)
        {
            if (GUILayout.Button("Set As Default Menu", GUILayout.Height(30)))
            {
                Undo.RecordObject(zuiManager, "Set Default Menu");

                zuiManager.CurActiveMenu = myMenu;
            }
        }
        else
        {
            GUILayout.Space(7.5f);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Default Menu", GUILayout.Height(15));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(7.5f);
        }
        #endregion


        #region Check Menu Independant Elements
        if (myMenu.AnimatedElements != null)
        {
            for (int i = 0; i < myMenu.AnimatedElements.Count; i++)
            {
                if (myMenu.AnimatedElements[i] == null) continue;

                if (!myMenu.AnimatedElements[i].MenuDependent)
                {
                    if (EditorUtility.DisplayDialog("Error", myMenu.AnimatedElements[i].gameObject.name + " is menu independant but is inside this menu's elements list.", "Remove it from the list", "Switch it to menu dependant"))
                    {
                        Undo.RecordObject(myMenu, "Removing from list");
                        myMenu.AnimatedElements[i].ControlledBy = null;
                        myMenu.AnimatedElements.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else
                    {
                        Undo.RecordObject(myMenu, "Switch to menu dependant");
                        myMenu.AnimatedElements[i].MenuDependent = true;
                    }
                }
                if (myMenu.AnimatedElements[i].ControlledBy != myMenu)
                    myMenu.AnimatedElements[i].ControlledBy = myMenu;
            }
        }
        if (myMenu.SharedAnimatedElements != null)
        {
            for (int i = 0; i < myMenu.SharedAnimatedElements.Count; i++)
            {
                if (myMenu.SharedAnimatedElements[i] == null) continue;

                if (!myMenu.SharedAnimatedElements[i].MenuDependent)
                {
                    if (EditorUtility.DisplayDialog("Error", myMenu.SharedAnimatedElements[i].gameObject.name + " is menu independant but is inside this Menu's elements list.", "Remove it from the list", "Switch it to menu dependant"))
                    {
                        Undo.RecordObject(myMenu, "Removing from list");
                        myMenu.SharedAnimatedElements.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else
                    {
                        Undo.RecordObject(myMenu, "Switch to menu dependant");
                        myMenu.SharedAnimatedElements[i].MenuDependent = true;
                    }
                }
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
            if (cUE && cUE.MenuDependent && (cUE.ControlledBy == null || cUE.ControlledBy == (Menu)target))
                ue.Add(cUE);

            ue.AddRange(GetAnimatedElements(c));
        }
        return ue;
    }

    List<Menu> GetAllMenus()
    {
        List<Menu> menus = new List<Menu>();

        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            menus.AddRange(GetMenusInCanvas(c.transform));
        }

        return menus;
    }
    List<Menu> GetMenusInCanvas(Transform canvas)
    {
        List<Menu> ts = new List<Menu>();

        foreach (Transform c in canvas)
        {
            Menu cMenu = c.GetComponent<Menu>();
            if (cMenu)
                ts.Add(cMenu);

            ts.AddRange(GetMenusInCanvas(c));
        }
        return ts;
    }

}
