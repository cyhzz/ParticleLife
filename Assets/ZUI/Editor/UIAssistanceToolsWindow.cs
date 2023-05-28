using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class UIAssistanceToolsWindow : EditorWindow {

    private Texture anchorsToRectIcon;
    private Texture rectToAnchorsIcon;
    private Texture anchorsToParentIcon;
    private Texture rectToParentIcon;
    private Texture rAndAToParentIcon;
    private Texture centerAnchorstIcon;

    private Vector2 scrollPos;

    [MenuItem("Tools/ZUI/UI Assistance/Open window...", false, 26)]
    static void OpenWindow()
    {
        GetWindow(typeof(UIAssistanceToolsWindow));
    }

    void Awake()
    {
        anchorsToRectIcon = (Texture)EditorGUIUtility.Load("ZUI/UIAssistanceIcons/AnchorsToRect.png");
        rectToAnchorsIcon = (Texture)EditorGUIUtility.Load("ZUI/UIAssistanceIcons/RectToAnchors.png");
        anchorsToParentIcon = (Texture)EditorGUIUtility.Load("ZUI/UIAssistanceIcons/AnchorsToParent.png");
        rectToParentIcon = (Texture)EditorGUIUtility.Load("ZUI/UIAssistanceIcons/RectToParent.png");
        rAndAToParentIcon = (Texture)EditorGUIUtility.Load("ZUI/UIAssistanceIcons/RectAndAnchorsToParent.png");
        centerAnchorstIcon = (Texture)EditorGUIUtility.Load("ZUI/UIAssistanceIcons/CenterAnchors.png");
    }


    void OnGUI()
    {
        bool horizontal = position.width > position.height;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        if (horizontal)
            EditorGUILayout.BeginHorizontal();
        else
            EditorGUILayout.BeginVertical();

        if (GUILayout.Button(anchorsToRectIcon, GUILayout.Height(80), GUILayout.Width(80)))
            FitAnchorsToRect();

        if (GUILayout.Button(rectToAnchorsIcon, GUILayout.Height(80), GUILayout.Width(80)))
            FitRectToAnchors();

        GUILayout.Space(20);
        GUILayout.Box("", GUILayout.Height(horizontal? 80 : 5), GUILayout.Width(horizontal? 5 : 80));
        GUILayout.Space(20);

        if (GUILayout.Button(anchorsToParentIcon, GUILayout.Height(80), GUILayout.Width(80)))
            FitAnchorsToParent();

        if (GUILayout.Button(rectToParentIcon, GUILayout.Height(80), GUILayout.Width(80)))
            FitRectToParent();

        if (GUILayout.Button(rAndAToParentIcon, GUILayout.Height(80), GUILayout.Width(80)))
            FitAnchorAndRectToParent();

        GUILayout.Space(20);
        GUILayout.Box("", GUILayout.Height(horizontal ? 80 : 5), GUILayout.Width(horizontal ? 5 : 80));
        GUILayout.Space(20);

        if (GUILayout.Button(centerAnchorstIcon, GUILayout.Height(80), GUILayout.Width(80)))
            CenterAnchors();

        if (horizontal)
            EditorGUILayout.EndHorizontal();
        else
            EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    void OnDestroy()
    {
        anchorsToRectIcon = null;

        EditorUtility.UnloadUnusedAssetsImmediate();
    }

    //[MenuItem("ZUI/UI Assistance/", false, 20)]

    [MenuItem("Tools/ZUI/UI Assistance/Fit Anchors to Rect %#x", false, 0)]
    static void FitAnchorsToRect()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform selectedRT = go.GetComponent<RectTransform>();
            RectTransform parentRT = null;
            if (go.transform.parent)
                parentRT = go.transform.parent.GetComponent<RectTransform>();
            if (!selectedRT || !parentRT) continue;
            AspectRatioFitter arf = selectedRT.GetComponent<AspectRatioFitter>();
            bool arfDisabled = false;
            if (arf && arf.enabled)
            {
                arf.enabled = false;
                arfDisabled = true;
            }
            Undo.RecordObject(selectedRT, "Change Anchor");

            selectedRT.anchorMin = new Vector2(selectedRT.anchorMin.x + selectedRT.offsetMin.x / parentRT.rect.width,
                selectedRT.anchorMin.y + selectedRT.offsetMin.y / parentRT.rect.height);
            selectedRT.anchorMax = new Vector2(selectedRT.anchorMax.x + selectedRT.offsetMax.x / parentRT.rect.width,
                selectedRT.anchorMax.y + selectedRT.offsetMax.y / parentRT.rect.height);

            selectedRT.offsetMin = selectedRT.offsetMax = Vector2.zero;
            if (arfDisabled)
                arf.enabled = true;
        }
    }
    [MenuItem("Tools/ZUI/UI Assistance/Fit Rect to Anchors %#c", false, 1)]
    static void FitRectToAnchors()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform selectedRT = go.GetComponent<RectTransform>();
            if (!selectedRT) continue;

            Undo.RecordObject(selectedRT, "Change Rect");

            selectedRT.offsetMin = selectedRT.offsetMax = Vector2.zero;
        }
    }
    [MenuItem("Tools/ZUI/UI Assistance/Fit Anchors to Parent", false, 2)]
    static void FitAnchorsToParent()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform selectedRT = go.GetComponent<RectTransform>();
            RectTransform parentRT = null;
            if (go.transform.parent)
                parentRT = go.transform.parent.GetComponent<RectTransform>();
            if (!selectedRT || !parentRT) continue;

            Undo.RecordObject(selectedRT, "Fit anchors to parent");

            Rect parentRect = parentRT.rect;

            Vector2 lastOffMin = selectedRT.offsetMin;
            Vector2 lastOffMax = selectedRT.offsetMax;

            Vector2 lastAnchMin = selectedRT.anchorMin;
            Vector2 lastAnchMax = selectedRT.anchorMax;

            selectedRT.anchorMin = Vector2.zero;
            selectedRT.anchorMax = Vector2.one;

            selectedRT.offsetMin = new Vector2(lastOffMin.x + (lastAnchMin.x - selectedRT.anchorMin.x) * parentRect.width, lastOffMin.y + (lastAnchMin.y - selectedRT.anchorMin.y) * parentRect.height);
            selectedRT.offsetMax = new Vector2(lastOffMax.x + (lastAnchMax.x - selectedRT.anchorMax.x) * parentRect.width, lastOffMax.y + (lastAnchMax.y - selectedRT.anchorMax.y) * parentRect.height);
        }
    }
    [MenuItem("Tools/ZUI/UI Assistance/Fit Rect to Parent", false, 3)]
    static void FitRectToParent()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform selectedRT = go.GetComponent<RectTransform>();
            RectTransform parentRT = null;
            if (go.transform.parent)
                parentRT = go.transform.parent.GetComponent<RectTransform>();
            if (!selectedRT || !parentRT) continue;

            Undo.RecordObject(selectedRT, "Fit anchors to parent");

            Rect parentRect = parentRT.rect;

            selectedRT.offsetMin = Vector2.zero;
            selectedRT.offsetMax = new Vector2(parentRect.width - (selectedRT.anchorMax.x * parentRect.width) + (selectedRT.anchorMin.x * parentRect.width), parentRect.height - (selectedRT.anchorMax.y * parentRect.height) + (selectedRT.anchorMin.y * parentRect.height));

            Vector2 lastPivot = selectedRT.pivot;

            selectedRT.pivot = parentRT.pivot;
            selectedRT.localPosition = Vector2.zero;

            Vector2 changeInPosition = new Vector3((lastPivot.x - parentRT.pivot.x) * parentRect.width, (lastPivot.y - parentRT.pivot.y) * parentRect.height);

            selectedRT.pivot = lastPivot;
            selectedRT.localPosition = changeInPosition;
        }
    }
    [MenuItem("Tools/ZUI/UI Assistance/Fit Anchors and Rect to Parent", false, 4)]
    static void FitAnchorAndRectToParent()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform selectedRT = go.GetComponent<RectTransform>();
            RectTransform parentRT = null;
            if (go.transform.parent)
                parentRT = go.transform.parent.GetComponent<RectTransform>();
            if (!selectedRT || !parentRT) continue;

            Undo.RecordObject(selectedRT, "Fit to parent");
            selectedRT.anchorMin = Vector2.zero;
            selectedRT.anchorMax = Vector2.one;

            selectedRT.offsetMin = selectedRT.offsetMax = Vector2.zero;
        }
    }

    [MenuItem("Tools/ZUI/UI Assistance/Center Anchors", false, 15)]
    static void CenterAnchors()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform selectedRT = go.GetComponent<RectTransform>();
            RectTransform parentRT = null;
            if (go.transform.parent)
                parentRT = go.transform.parent.GetComponent<RectTransform>();
            if (!selectedRT || !parentRT) continue;

            Undo.RecordObject(selectedRT, "Fit anchors to parent");

            Rect parentRect = parentRT.rect;

            Vector2 lastSize = new Vector2(selectedRT.rect.width, selectedRT.rect.height);

            Vector2 lastAnchMin = selectedRT.anchorMin;
            Vector2 lastAnchMax = selectedRT.anchorMax;

            float minX = ((parentRect.width * selectedRT.anchorMin.x) + selectedRT.offsetMin.x + (selectedRT.rect.width * selectedRT.pivot.x)) / parentRect.width;
            float minY = ((parentRect.height * selectedRT.anchorMin.y) + selectedRT.offsetMin.y + (selectedRT.rect.height * selectedRT.pivot.y)) / parentRect.height;

            selectedRT.anchorMin = new Vector2(minX, minY);
            selectedRT.anchorMax = selectedRT.anchorMin;

            selectedRT.sizeDelta = lastSize;
            selectedRT.anchoredPosition = Vector2.zero;
        }
    }

}
