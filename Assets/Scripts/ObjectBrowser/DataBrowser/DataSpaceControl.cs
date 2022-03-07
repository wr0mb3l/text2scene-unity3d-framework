using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataSpaceControl : MonoBehaviour
{
    private Text SiteIndicator;

    private List<Interface> Interfaces;
    private List<DataSpaceContainer> InterfaceButtons;
    private int MaxSites;
    private Button NextSite;
    private Button PreviousSite;
    private DataBrowser Browser;

    private Transform _parent;

    private int _spacePointer;
    public int SpacePointer
    {
        get { return _spacePointer; }
        set
        {
            if (InterfaceButtons.Count != 0)
                _spacePointer = Mathf.Max(0, Mathf.Min(value, (Interfaces.Count / InterfaceButtons.Count) * InterfaceButtons.Count));
            else
                _spacePointer = 0;
            ActualizeButtons();
            ActualizeSiteVariables();
        }
    }

    private bool _baseInit;
    private void BaseInit()
    {
        _parent = transform.parent;
        Browser = _parent.GetComponent<DataBrowser>();
        SiteIndicator = transform.Find("SiteIndicator").GetComponent<Text>();
        Button[] buttons = GetComponentsInChildren<Button>();
        InterfaceButtons = new List<DataSpaceContainer>();
        foreach (Button button in buttons)
        {
            if (button.name.Contains("Space"))
            {
                DataSpaceContainer buttonDataSpaceContainer = button.GetComponent<DataSpaceContainer>();
                InterfaceButtons.Add(buttonDataSpaceContainer);
                button.onClick.AddListener(() =>
                {
                    if (buttonDataSpaceContainer.ButtonOn) return;
                    foreach (DataSpaceContainer b in InterfaceButtons)
                        b.ButtonOn = false;
                    buttonDataSpaceContainer.ButtonOn = true;
                    Browser.SelectedInterface = (Interface)buttonDataSpaceContainer.ButtonValue;
                });
            }
        }

        PreviousSite = transform.Find("ButtonPrevious").GetComponent<Button>();
        PreviousSite.onClick.AddListener(PreviousClick);
        NextSite = transform.Find("ButtonNext").GetComponent<Button>();
        NextSite.onClick.AddListener(NextClick);

        Interfaces = new List<Interface>();

        Interfaces.Add(GameObject.Find("ShapeNetInterface").gameObject.GetComponent<ShapeNetInterface>());

        _baseInit = true;
    }

    public void Init()
    {
        if (!_baseInit) BaseInit();
        MaxSites = Mathf.CeilToInt(Interfaces.Count / (float)InterfaceButtons.Count);
        SpacePointer = 0;
    }

    private void ActualizeSiteVariables()
    {
        SiteIndicator.text = "Site " + Mathf.Max(SpacePointer / InterfaceButtons.Count, 1) + " of " + MaxSites;
        PreviousSite.interactable = SpacePointer > 0;
        NextSite.interactable = (SpacePointer + InterfaceButtons.Count) < Interfaces.Count;
    }

    private void ActualizeButtons()
    {
        for (int i = 0; i < InterfaceButtons.Count; i++)
        {
            InterfaceButtons[i].gameObject.SetActive((SpacePointer + i) < Interfaces.Count);
            if (InterfaceButtons[i].gameObject.activeInHierarchy)
            {
                InterfaceButtons[i].ButtonValue = Interfaces[SpacePointer + i];
                InterfaceButtons[i].ChangeText(Interfaces[SpacePointer + i].Name);
                InterfaceButtons[i].GetComponent<Button>().interactable = true;
                InterfaceButtons[i].ButtonOn = _baseInit && Interfaces[SpacePointer + i] == Browser.SelectedInterface;
            }
        }
    }

    private void NextClick()
    {
        SpacePointer += InterfaceButtons.Count;
    }
    private void PreviousClick()
    {
        SpacePointer -= InterfaceButtons.Count;
    }
}