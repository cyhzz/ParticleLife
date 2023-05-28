using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class ZUIManager : MonoBehaviour {
    public static ZUIManager Instance;

    [Tooltip("Automatically find all the holders at the start of the scene. (may cause lag spike at the first frame, it's advised to disable it before releasing a build)")]
    public bool AutoFindHolders = true;

    #region Menus
    [Tooltip("Click on \"Update All Menus\" to get all the animated elements under this object.")]
    public List<Menu> AllMenus = new List<Menu>();
    public Menu CurActiveMenu;
    [Tooltip("Should the first menu be animated at the start?")]
    public bool AnimateFirstMenuAtStart;
    #endregion

    #region Pop-ups
    [Tooltip("All the side menus in the scene.")]
    public List<Popup> AllPopups = new List<Popup>();
    public Popup CurActivePopup;
    #endregion

    #region Side-menus
    [Tooltip("All the side menus in the scene.")]
    public List<SideMenu> AllSideMenus = new List<SideMenu>();
    public SideMenu CurActiveSideMenu;
    #endregion

    #region General Settings
    [Tooltip("Should \"Cancel\" button act as back button?")]
    public bool EscIsBack = true;
    public bool DisableBack;        //Should back functionality be disabled?
    #endregion

    private bool waitingToSwitch;
    private IEnumerator switchingEnum;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (AutoFindHolders)
        {
            GetAllHolders();
        }

        //If there's no default menu, find one
        if (!CurActiveMenu && AllMenus.Count > 0)
        {
            //Find the first menu that is not null
            for (int i = 0; i < AllMenus.Count; i++)
            {
                if (AllMenus[i] != null)
                {
                    CurActiveMenu = AllMenus[i];
                    break;
                }
            }
        }

        foreach (Menu m in AllMenus)
        {
            if (m == null) continue;

            m.InitializeElements();

            if (m == CurActiveMenu)
            {
                if (AnimateFirstMenuAtStart)
                {
                    m.ChangeVisibilityImmediate(false, true);
                    m.gameObject.SetActive(true);
                }

                if (!m.gameObject.activeSelf) m.gameObject.SetActive(true);

                m.ChangeVisibility(true, m.IgnoreEventsOnInitialization);
            }
            else
            {
                m.ChangeVisibilityImmediate(false, true);
            }
        }
    }

    void Update()
    {
        if (EscIsBack && Input.GetButtonDown("Cancel"))
        {
            Back();
        }
    }

    #region Menu Functions
    /// <summary>
    /// Open menu by name, and close the current active one.
    /// </summary>
    /// <param name="menuName"></param>
    public void OpenMenu(string menuName)
    {
        if (waitingToSwitch) return;    //Do nothing if there's a menu switching already

        Menu lastM = CurActiveMenu;
        bool foundIt = false;

        foreach (Menu m in AllMenus)
        {
            if (m == null)
            {
                Debug.LogError("Found an empty element in the menu's list, please make sure there's no empty elements", gameObject);
                continue;
            }
            if (m.name == menuName)
            {
                if (lastM.SwitchAfter == 0)
                {
                    if (!m.gameObject.activeSelf) m.gameObject.SetActive(true);

                    m.ChangeVisibility(true);
                }
                else
                {
                    if (switchingEnum != null)
                        StopCoroutine(switchingEnum);
                    switchingEnum = SwitchAfterHiding(lastM, m);
                    StartCoroutine(switchingEnum);
                }
                CurActiveMenu = m;
                foundIt = true;
                break;
            }
        }

        if (lastM != CurActiveMenu)
            lastM.ChangeVisibility(false);

        if (!foundIt)
            Debug.LogError("There's no menu named \"" + menuName + "\" inside Menu Manager's \"All Menus\"  list.", gameObject);

    }
    /// <summary>
    /// Open menu by reference, and close the current active one.
    /// </summary>
    /// <param name="menu"></param>
    public void OpenMenu(Menu menu)
    {
        if (waitingToSwitch) return;    //Do nothing if there's a menu switching already

        Menu lastM = CurActiveMenu;
        bool foundIt = false;

        foreach (Menu m in AllMenus)
        {
            if (m == null)
            {
                Debug.LogError("Found an empty element in the menu's list, please make sure there's no empty elements", gameObject);
                continue;
            }
            if (m == menu)
            {
                if (lastM.SwitchAfter == 0)
                {
                    if (!m.gameObject.activeSelf) m.gameObject.SetActive(true);

                    m.ChangeVisibility(true);
                }
                else
                {
                    if (switchingEnum != null)
                        StopCoroutine(switchingEnum);
                    switchingEnum = SwitchAfterHiding(lastM, m);
                    StartCoroutine(switchingEnum);
                }
                CurActiveMenu = m;
                foundIt = true;
                break;
            }
        }

        if (lastM != CurActiveMenu)
            lastM.ChangeVisibility(false);

        if (!foundIt)
            Debug.LogError("There's no menu named \"" + menu.name + "\" inside Menu Manager's \"All Menus\"  list.", gameObject);

    }
    /// <summary>
    /// Open current menu's NextMenu.
    /// </summary>
    public void NextMenu()
    {
        if (CurActiveMenu.NextMenu)
            OpenMenu(CurActiveMenu.NextMenu);
    }
    IEnumerator SwitchAfterHiding(Menu lastMenu, Menu newMenu)
    {
        waitingToSwitch = true;
        yield return new WaitForSeconds(lastMenu.GetAllHidingTime() * lastMenu.SwitchAfter);
        if (!newMenu.gameObject.activeSelf) newMenu.gameObject.SetActive(true);

        newMenu.ChangeVisibility(true);

        waitingToSwitch = false;
    }
    #endregion

    #region Side-menu Functions
    /// <summary>
    /// Open side menu by name, and close the current active one.
    /// </summary>
    /// <param name="sideMenuName"></param>
    public void OpenSideMenu(string sideMenuName)
    {
        SideMenu lastSM = CurActiveSideMenu;
        bool foundIt = false;

        foreach (SideMenu sM in AllSideMenus)
        {
            if (sM == null) continue;

            if (sM.name == sideMenuName)
            {
                sM.ChangeVisibility(true);
                CurActiveSideMenu = sM;
                foundIt = true;
            }
        }

        if (lastSM && lastSM != CurActiveSideMenu)
            lastSM.ChangeVisibility(false);

        if (!foundIt)
            Debug.LogError("There's no side menu named \"" + sideMenuName + "\" inside Menu Manager's  \"All Side Menus\" list.", gameObject);

    }
    /// <summary>
    /// Open side menu by reference, and close the current active one.
    /// </summary>
    /// <param name="sideMenuName"></param>
    public void OpenSideMenu(SideMenu sideMenu)
    {
        SideMenu lastSM = CurActiveSideMenu;
        bool foundIt = false;

        foreach (SideMenu sM in AllSideMenus)
        {
            if (sM == sideMenu)
            {
                sM.ChangeVisibility(true);
                CurActiveSideMenu = sM;
                foundIt = true;
            }
        }

        if (lastSM && lastSM != CurActiveSideMenu)
            lastSM.ChangeVisibility(false);

        if (!foundIt)
            Debug.LogError("There's no side menu named \"" + sideMenu.name + "\" inside Menu Manager's  \"All Side Menus\" list.", gameObject);

    }
    /// <summary>
    /// Close the current active side menu if found.
    /// </summary>
    public void CloseSideMenu()
    {
        if (CurActiveSideMenu)
        {
            CurActiveSideMenu.ChangeVisibility(false);
            CurActiveSideMenu = null;
        }
    }
    #endregion

    #region Pop-up Functions
    /// <summary>
    /// Open a popup by name.
    /// </summary>
    /// <param name="popupName">The name of the popup gameObject.</param>
    public void OpenPopup(string popupName)
    {
        foreach (Popup p in AllPopups)
        {
            if (p == null) continue;

            if (p.name == popupName)
            {
                OpenPopup(p);
                return;
            }
        }

        Debug.LogError("Opening Pop-up faild. Couldn't find a pop-up with the name " + popupName + ". Please make sure its listed in the Pop-ups list.", gameObject);
    }
    /// <summary>
    /// Open a popup by name. Can include information.
    /// </summary>
    /// <param name="popupName">The name of the popup gameObject.</param>
    /// <param name="info">The information to put inside the popup.</param>
    /// <param name="title">The title to put on the popup.</param>
    public void OpenPopup(string popupName, string info, string title)
    {
        foreach (Popup p in AllPopups)
        {
            if (p == null) continue;

            if (p.name == popupName)
            {
                OpenPopup(p, info, title);
                return;
            }
        }

        Debug.LogError("Opening Pop-up faild. Couldn't find a pop-up with the name " + popupName + ". Please make sure its listed in the Pop-ups list.", gameObject);
    }
    /// <summary>
    /// Open a popup by reference.
    /// </summary>
    /// <param name="popup"></param>
    public void OpenPopup(Popup popup)
    {
        if (CurActivePopup)
        {
            CurActivePopup.ChangeVisibility(false);
        }
        
        popup.ChangeVisibility(true);
        CurActivePopup = popup;
    }
    /// <summary>
    /// Open a popup by reference, and include information.
    /// </summary>
    /// <param name="popup"></param>
    /// <param name="info">The information to put inside the popup.</param>
    /// <param name="title">The title to put on the popup.</param>
    public void OpenPopup(Popup popup, string info, string title)
    {
        if (CurActivePopup)
        {
            CurActivePopup.ChangeVisibility(false);
        }
        popup.ChangeVisibility(true);
        popup.UpdateInformation(info, title);
        CurActivePopup = popup;
    }

    /// <summary>
    /// Close the current opened popup
    /// </summary>
    public void ClosePopup()
    {
        if (CurActivePopup)
            CurActivePopup.ChangeVisibility(false);
        CurActivePopup = null;
    }
    /// <summary>
    /// Close popup by name, only if its opened.
    /// </summary>
    public void ClosePopup(string popupName)
    {
        foreach (Popup p in AllPopups)
        {
            if (p.name == popupName)
            {
                ClosePopup(p);
                return;
            }
        }

        Debug.LogError("Closing Pop-up faild. Couldn't find a pop-up with the name " + popupName + ". Please make sure its listed in the Pop-ups list.", gameObject);
    }
    /// <summary>
    /// Close popup by reference, only if its opened.
    /// </summary>
    public void ClosePopup(Popup popup)
    {
        popup.ChangeVisibility(false);
        CurActivePopup = null;
    }
    #endregion

    /// <summary>
    /// Open current menu's "PreviousMenu" or closes the opened popup or sidemenu.
    /// </summary>
    public void Back()
    {
        if (DisableBack)
            return;

        if (CurActivePopup)
        {
            if (CurActivePopup.OverrideBack)
            {
                CurActivePopup.MyBack.Invoke();
                return;
            }
            else
                ClosePopup();
        }
        else if (CurActiveSideMenu)
        {
            if (CurActiveSideMenu.OverrideBack)
            {
                CurActiveSideMenu.MyBack.Invoke();
                return;
            }
            else
                CloseSideMenu();
        }
        else
        {
            if (CurActiveMenu.OverrideBack)
            {
                CurActiveMenu.MyBack.Invoke();
                return;
            }
            else if (CurActiveMenu.PreviousMenu)
            {
                if (waitingToSwitch)
                {
                    waitingToSwitch = false;
                    StopCoroutine(switchingEnum);
                }
                OpenMenu(CurActiveMenu.PreviousMenu);
            }
        }
    }
    /// <summary>
    /// Fire the default back functionality and ignore the current active holder's Override Back functionality.
    /// </summary>
    public void ForceBack()
    {
        if (DisableBack)
            return;

        if (CurActivePopup)
            ClosePopup();
        else if (CurActiveSideMenu)
            CloseSideMenu();
        else if (CurActiveMenu.PreviousMenu)
        {
            if (waitingToSwitch)
            {
                waitingToSwitch = false;
                StopCoroutine(switchingEnum);
            }
            OpenMenu(CurActiveMenu.PreviousMenu);
        }
    }

    /// <summary>
    /// Turning back functionality on/off.
    /// </summary>
    /// <param name="on"></param>
    public void BackEnabled(bool on)
    {
        DisableBack = !on;
    }

    public void GetAllHolders()
    {
        AllMenus = GetAllMenus();
        AllPopups = GetAllPopups();
        AllSideMenus = GetAllSideMenus();
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
