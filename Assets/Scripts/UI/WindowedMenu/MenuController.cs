using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEditor;
using System;
using System.Xml.Linq;
using TMPro;
using MathHelper;

public enum MenuType
{
    LoginMenu,
    DocumentBrowserMenu,
    AnnotationMenu
}

public class MenuController : MonoBehaviour
{
    static List<Menu> menuList;
    static Menu lastActiveMenu;

    static TextAnnotatorInterface textAnnotatorInterface;
    static ResourceManagerInterface resourceManagerInterface;

    private void InitializeInterfaces()
    { 
        Debug.Log("Init Interfaces");
        textAnnotatorInterface = gameObject.AddComponent<TextAnnotatorInterface>();
        StartCoroutine(textAnnotatorInterface.Initialize());
        resourceManagerInterface = gameObject.AddComponent<ResourceManagerInterface>();
        textAnnotatorInterface.ResourceManager = resourceManagerInterface;
    }

    public static TextAnnotatorInterface GetTextAnnotatorInterface()
    {
        return textAnnotatorInterface;
    }

    public static void SwitchMenu(MenuType type)
    {
        Debug.Log(Enum.GetName(typeof(MenuType), type));
        if (lastActiveMenu != null)
        {
            lastActiveMenu.gameObject.SetActive(false);
        }
        
        Menu desiredMenu = menuList.FirstOrDefault(menu => menu.Type == type);
        if (desiredMenu != null)
        {
            desiredMenu.gameObject.SetActive(true);
            lastActiveMenu = desiredMenu;
        }
        else
        {
            Debug.LogWarning("The desired menu is not found");
        }
    }    


    // Start is called before the first frame update
    void Awake()
    {
        InitializeInterfaces();
        DontDestroyOnLoad(gameObject);
        Init_UI();
    }                                                                                                                                                         

    private void Init_UI()
    {
        menuList = GetComponentsInChildren<Menu>().ToList();
        menuList.ForEach(menu => menu.gameObject.SetActive(false));
        SwitchMenu(MenuType.LoginMenu);
    }
}
