using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ZUIManager))]
public class ZUIManagerEditor : Editor
{
    SerializedProperty autoFindHolders;

    SerializedProperty allMenus;
    SerializedProperty animateFirstMenuAtStart;

    SerializedProperty allSideMenus;

    SerializedProperty allPopups;

    SerializedProperty escIsBack;

    void OnEnable()
    {
        autoFindHolders = serializedObject.FindProperty("AutoFindHolders");

        allMenus = serializedObject.FindProperty("AllMenus");
        animateFirstMenuAtStart = serializedObject.FindProperty("AnimateFirstMenuAtStart");

        allSideMenus = serializedObject.FindProperty("AllSideMenus");


        allPopups = serializedObject.FindProperty("AllPopups");

        escIsBack = serializedObject.FindProperty("EscIsBack");

    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        ZUIManager myZUIManager = target as ZUIManager;

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(autoFindHolders, new GUIContent("Auto Find Holders"));

        if (!autoFindHolders.boolValue)
        {
            #region Menus
            EditorGUILayout.LabelField("Menus", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(allMenus, true);
            EditorGUILayout.PropertyField(animateFirstMenuAtStart);

            //EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Update All Menus", GUILayout.Height(30)))
            {
                Undo.RecordObject(myZUIManager, "Update All Menus");
                myZUIManager.AllMenus = GetAllMenus();
            }
            if (GUILayout.Button("Activate All Menus", GUILayout.Height(30)))
            {
                foreach (Menu m in myZUIManager.AllMenus)
                {
                    Undo.RecordObject(m.gameObject, "Activate All Menus");
                    m.gameObject.SetActive(true);
                }
            }
            #endregion

            EditorGUILayout.Space();

            #region Pop-ups
            EditorGUILayout.LabelField("Pop-ups", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(allPopups, true);

            if (GUILayout.Button("Update All Pop-ups", GUILayout.Height(30)))
            {
                Undo.RecordObject(myZUIManager, "Update All Pop-ups");
                myZUIManager.AllPopups = GetAllPopups();
            }
            if (GUILayout.Button("Activate All Pop-ups", GUILayout.Height(30)))
            {
                foreach (Popup sM in myZUIManager.AllPopups)
                {
                    Undo.RecordObject(sM.gameObject, "Activate All Pop-ups");
                    sM.gameObject.SetActive(true);
                }
            }
            #endregion

            EditorGUILayout.Space();

            #region Side-menus
            EditorGUILayout.LabelField("Side-menus", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(allSideMenus, true);

            if (GUILayout.Button("Update All Side-menus", GUILayout.Height(30)))
            {
                Undo.RecordObject(myZUIManager, "Update All Side-menus");
                myZUIManager.AllSideMenus = GetAllSideMenus();
            }
            if (GUILayout.Button("Activate All Side-menus", GUILayout.Height(30)))
            {
                foreach (SideMenu sM in myZUIManager.AllSideMenus)
                {
                    Undo.RecordObject(sM.gameObject, "Activate All Side-menus");
                    sM.gameObject.SetActive(true);
                }
            }
            #endregion
        }
        EditorGUILayout.Space();

        #region General Settings
        EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
        if (autoFindHolders.boolValue)
            EditorGUILayout.PropertyField(animateFirstMenuAtStart);
        EditorGUILayout.PropertyField(escIsBack);

        if (!autoFindHolders.boolValue && GUILayout.Button("Update All", GUILayout.Height(30)))
        {
            Undo.RecordObject(myZUIManager, "Update All");
            myZUIManager.AllMenus = GetAllMenus();
            myZUIManager.AllPopups = GetAllPopups();
            myZUIManager.AllSideMenus = GetAllSideMenus();
        }
        #endregion

        serializedObject.ApplyModifiedProperties();
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

    List<SideMenu> GetAllSideMenus()
    {
        List<SideMenu> sideMenus = new List<SideMenu>();

        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            sideMenus.AddRange(GetSideMenusInCanvas(c.transform));
        }

        return sideMenus;
    }
    List<SideMenu> GetSideMenusInCanvas(Transform canvas)
    {
        List<SideMenu> allSideMenus = new List<SideMenu>();

        foreach (Transform c in canvas)
        {
            SideMenu cSideMenu = c.GetComponent<SideMenu>();
            if (cSideMenu)
                allSideMenus.Add(cSideMenu);

            allSideMenus.AddRange(GetSideMenusInCanvas(c));
        }
        return allSideMenus;
    }

    List<Popup> GetAllPopups()
    {
        List<Popup> popups = new List<Popup>();

        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            popups.AddRange(GetPopupsInCanvas(c.transform));
        }

        return popups;
    }
    List<Popup> GetPopupsInCanvas(Transform canvas)
    {
        List<Popup> allPopups = new List<Popup>();

        foreach (Transform c in canvas)
        {
            Popup cPopup = c.GetComponent<Popup>();
            if (cPopup)
                allPopups.Add(cPopup);

            allPopups.AddRange(GetPopupsInCanvas(c));
        }
        return allPopups;
    }

}
