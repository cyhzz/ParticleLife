using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;

public class ZUICreationWindow : EditorWindow {

    private bool tryCreatingAgain;

    private Vector2 scrollPos;

    [MenuItem("Tools/ZUI/Creation Window...", false, 0)]
    public static void OpenWindow()
    {
        GetWindowWithRect(typeof(ZUICreationWindow), new Rect(0,0, 120, 235));
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Setup", GUILayout.Height(40), GUILayout.Width(110)))
            Setup();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        #region Separator
        GUILayout.Space(3);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("", GUILayout.Height(3), GUILayout.Width(90));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(3);
        #endregion

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Menu", GUILayout.Height(40), GUILayout.Width(110)))
            CreateMenu();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Pop-up", GUILayout.Height(40), GUILayout.Width(110)))
            CreatePopup();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Side-menu", GUILayout.Height(40), GUILayout.Width(110)))
            CreateSidemenu();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Elements Group", GUILayout.Height(40), GUILayout.Width(110)))
            CreateElementsGroup();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    void Setup()
    {
        if (!FindObjectOfType<ZUIManager>())
        {
            GameObject mm = new GameObject("ZUIManager", typeof(ZUIManager));
            mm.transform.SetAsFirstSibling();
            Undo.RegisterCreatedObjectUndo(mm, "ZUI Setup");
        }
        if (!FindObjectOfType<SFXManager>())
        {
            GameObject sfxm = new GameObject("SFXManager", typeof(SFXManager));
            Undo.RegisterCreatedObjectUndo(sfxm, "ZUI Setup");
            sfxm.transform.SetSiblingIndex(1);

            SFXManager sfx = sfxm.GetComponent<SFXManager>();
            sfx.Sources = new List<AudioSource>();

            for (int i = 0; i < 10; i++)
            {
                GameObject s = new GameObject("Source (" + (i + 1) + ")", typeof(AudioSource));
                Undo.RegisterCreatedObjectUndo(s, "ZUI Setup");
                AudioSource audioSource = s.GetComponent<AudioSource>();
                audioSource.playOnAwake = false;
                sfx.Sources.Add(audioSource);
                s.transform.SetParent(sfxm.transform);
            }
        }
    }

    void CreateMenu()
    {
        Canvas c = CheckManagers(true);
        if (c != null)
        {
            ZUIManager manager = FindObjectOfType<ZUIManager>();
            Menu[] allMenus = FindObjectsOfType<Menu>();

            GameObject menu = Instantiate((GameObject)EditorGUIUtility.Load("ZUI/Templates/Menu.prefab"), c.transform);
            Undo.RegisterCreatedObjectUndo(menu, "Create Menu");
            menu.name = "Menu (" + (allMenus.Length + 1) + ")";
            RectTransform menuRT = menu.GetComponent<RectTransform>();
            menuRT.offsetMin = menuRT.offsetMax = Vector2.zero;
            menuRT.localScale = Vector3.one;

            Selection.activeGameObject = menu;

            Undo.RecordObject(manager, "Create Menu");
            manager.AllMenus.Add(menu.GetComponent<Menu>());
        }
        else
        {
            if (tryCreatingAgain)
                CreateMenu();
        }
    }
    void CreatePopup()
    {
        Canvas c = CheckManagers(true);
        if (c != null)
        {
            ZUIManager manager = FindObjectOfType<ZUIManager>();
            Popup[] allPopups = FindObjectsOfType<Popup>();

            GameObject popup = Instantiate((GameObject)EditorGUIUtility.Load("ZUI/Templates/Popup.prefab"), c.transform);
            Undo.RegisterCreatedObjectUndo(popup, "Create Pop-up");
            popup.name = "Popup (" + (allPopups.Length + 1) + ")";
            RectTransform popRT = popup.GetComponent<RectTransform>();
            popRT.offsetMin = popRT.offsetMax = Vector2.zero;
            popRT.localScale = Vector3.one;

            Selection.activeGameObject = popup;

            Undo.RecordObject(manager, "Create Pop-up");
            manager.AllPopups.Add(popup.GetComponent<Popup>());

            Button b = popup.GetComponentInChildren<Button>();
            if (b)
            {
                UnityAction closeMethod = new UnityAction(FindObjectOfType<ZUIManager>().ClosePopup);
                UnityEventTools.AddPersistentListener(b.onClick, closeMethod);
            }
        }
        else
        {
            if (tryCreatingAgain)
                CreatePopup();
        }
    }
    void CreateSidemenu()
    {
        Canvas c = CheckManagers(true);
        if (c != null)
        {
            ZUIManager manager = FindObjectOfType<ZUIManager>();
            SideMenu[] allSideMenus = FindObjectsOfType<SideMenu>();

            GameObject menu = Instantiate((GameObject)EditorGUIUtility.Load("ZUI/Templates/SideMenu.prefab"), c.transform);
            Undo.RegisterCreatedObjectUndo(menu, "Create Side-menu");
            menu.name = "SideMenu (" + (allSideMenus.Length + 1) + ")";
            RectTransform smRT = menu.GetComponent<RectTransform>();
            smRT.offsetMin = smRT.offsetMax = Vector2.zero;
            smRT.localScale = Vector3.one;

            Selection.activeGameObject = menu;

            Undo.RecordObject(manager, "Create Side-menu");
            manager.AllSideMenus.Add(menu.GetComponent<SideMenu>());

            Button b = menu.GetComponentInChildren<Button>();
            if (b)
            {
                UnityAction closeMethod = new UnityAction(FindObjectOfType<ZUIManager>().CloseSideMenu);
                UnityEventTools.AddPersistentListener(b.onClick, closeMethod);
            }
        }
        else
        {
            if (tryCreatingAgain)
                CreateSidemenu();
        }
    }
    void CreateElementsGroup()
    {
        Canvas c = CheckManagers(false);
        if (c != null)
        {
            UIElementsGroup[] allElementGroups = FindObjectsOfType<UIElementsGroup>();

            GameObject group = Instantiate((GameObject)EditorGUIUtility.Load("ZUI/Templates/UIElementsGroup.prefab"), c.transform);
            Undo.RegisterCreatedObjectUndo(group, "Create Elements Group");
            group.name = "ElementsGroup (" + (allElementGroups.Length + 1) + ")";
            RectTransform egRT = group.GetComponent<RectTransform>();
            egRT.offsetMin = egRT.offsetMax = Vector2.zero;
            egRT.localScale = Vector3.one;

            Selection.activeGameObject = group;
        }
        else
        {
            if (tryCreatingAgain)
                CreateElementsGroup();
        }
    }

    Canvas CheckManagers(bool checkZUImanager)
    {
        if (checkZUImanager)
        {
            ZUIManager[] managers = FindObjectsOfType<ZUIManager>();

            if (managers.Length == 0)
            {
                Setup();
                tryCreatingAgain = true;
                Debug.Log("Couldn't find ZUIManager, had to setup the scene.");
                return null;
            }
            if (managers.Length > 1)
            {
                EditorUtility.DisplayDialog("Error", "More than 1 \"ZUIManager\" found, please make sure there's only one.", "OK");
                return null;
            }
        }

        Canvas c = null;

        if (Selection.activeGameObject)
            c = Selection.activeGameObject.GetComponentInParent<Canvas>();
        if (c == null)
            c = FindObjectOfType<Canvas>();
        if (c == null)
        {
            GameObject canv = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canv, "Create Canvas");
            canv.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            if (!FindObjectOfType<EventSystem>())
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
            tryCreatingAgain = true;
        }

        return c;
    }
}
