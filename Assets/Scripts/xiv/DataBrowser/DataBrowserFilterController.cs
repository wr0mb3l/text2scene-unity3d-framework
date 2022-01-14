using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataBrowserFilterController : MonoBehaviour
{
    private Text Title;
    private Text SiteIndicator;

    public Sprite Checked;
    public Sprite Mixed;
    public Sprite Unchecked;

    public Dictionary<string, ShapeNetInterface.CheckboxStatus> Types { get; private set; }
    public bool ShowingSubTypes;
    public List<string> TypeList { get; private set; }
    public DataFilter[] DataFilters { get; private set; }
    public Button[] Checkboxes { get; private set; }
    public Button[] Openers { get; private set; }
    private int MaxSites;
    private Button NextSite;
    private Button PreviousSite;
    public Button Back { get; private set; }
    private Button SelectAll;
    private Button DeselectAll;
    private DataBrowser Browser;

    private Transform _parent;

    private int _typePointer;
    public int TypePointer
    {
        get { return _typePointer; }
        set
        {
            _typePointer = Mathf.Max(0, Mathf.Min(value, (Types.Count / Checkboxes.Length) * Checkboxes.Length));
            FilterUpdater?.Invoke();
            ActualizeSiteVariables();
        }
    }

    public delegate void FilterUpdateEvent();
    public FilterUpdateEvent FilterUpdater;

    public delegate void CheckboxUpdateEvent(string category, ShapeNetInterface.CheckboxStatus status);
    public CheckboxUpdateEvent CheckboxUpdater;

    private bool _baseInit;
    public void BaseInit()
    {
        _parent = transform.parent;
        Browser = _parent.GetComponent<DataBrowser>();
        Title = transform.Find("Title").GetComponent<Text>();
        SiteIndicator = transform.Find("SiteIndicator").GetComponent<Text>();
        DataFilters = GetComponentsInChildren<DataFilter>();
        Checkboxes = new Button[DataFilters.Length];
        Openers = new Button[DataFilters.Length];
        PreviousSite = transform.Find("ButtonPrevious").GetComponent<Button>();
        PreviousSite.onClick.AddListener(PreviousClick);
        NextSite = transform.Find("ButtonNext").GetComponent<Button>();
        NextSite.onClick.AddListener(NextClick);
        Back = transform.Find("ButtonBack").GetComponent<Button>();

        int i = 0;
        foreach (DataFilter dataFilter in DataFilters)
        {
            Button cb = dataFilter.transform.Find("Checkbox").GetComponent<Button>();
            cb.onClick.AddListener(() =>
            {
                if (cb.GetComponent<Image>().sprite == Unchecked)
                {
                    dataFilter.Status = ShapeNetInterface.CheckboxStatus.AllChecked;
                    // cb.GetComponent<Image>().sprite = Checked;
                }
                else
                {
                    dataFilter.Status = ShapeNetInterface.CheckboxStatus.NoneChecked;
                    // cb.GetComponent<Image>().sprite = Unchecked;
                }
                CheckboxUpdater((string)dataFilter.ButtonValue, dataFilter.Status);
                Browser.BrowserUpdater?.Invoke();
            });
            Checkboxes[i] = dataFilter.transform.Find("Checkbox").GetComponent<Button>();
            Openers[i++] = dataFilter.transform.Find("SubcategoryOpener").GetComponent<Button>();
        }

        SelectAll = transform.Find("ButtonSelectAll").GetComponent<Button>();
        SelectAll.onClick.AddListener(delegate { SetCategoryStates(ShapeNetInterface.CheckboxStatus.AllChecked); });
        DeselectAll = transform.Find("ButtonDeselectAll").GetComponent<Button>();
        DeselectAll.onClick.AddListener(delegate { SetCategoryStates(ShapeNetInterface.CheckboxStatus.NoneChecked); });

        _baseInit = true;
    }

    public void Init(string title, Dictionary<string, ShapeNetInterface.CheckboxStatus> types)
    {
        if (!_baseInit) BaseInit();
        Types = types;

        TypeList = new List<string>(Types.Keys);
        TypeList.Sort();

        Back.gameObject.SetActive(ShowingSubTypes);
        MaxSites = Mathf.CeilToInt(types.Count / (float)Checkboxes.Length);
        Title.text = title;
        TypePointer = 0;
    }

    private void ActualizeSiteVariables()
    {
        SiteIndicator.text = "Site " + ((TypePointer / Checkboxes.Length) + 1) + " of " + Mathf.Max(1, MaxSites);
        PreviousSite.interactable = TypePointer > 0;
        NextSite.interactable = (TypePointer + Checkboxes.Length) < Types.Count;
    }

    private void SetCategoryStates(ShapeNetInterface.CheckboxStatus status)
    {
        foreach (string type in TypeList)
            CheckboxUpdater(type, status);
        foreach (Button cb in Checkboxes)
        {
            if (status == ShapeNetInterface.CheckboxStatus.AllChecked)
            {
                // cb.GetComponent<Image>().sprite = Checked;
                cb.transform.parent.GetComponent<DataFilter>().Status = ShapeNetInterface.CheckboxStatus.AllChecked;
            }
            if (status == ShapeNetInterface.CheckboxStatus.NoneChecked)
            {
                // cb.GetComponent<Image>().sprite = Unchecked;
                cb.transform.parent.GetComponent<DataFilter>().Status = ShapeNetInterface.CheckboxStatus.NoneChecked;
            }
            if (status == ShapeNetInterface.CheckboxStatus.PartsChecked)
            {
                // cb.GetComponent<Image>().sprite = Mixed;
                cb.transform.parent.GetComponent<DataFilter>().Status = ShapeNetInterface.CheckboxStatus.PartsChecked;
            }
        }
        ActualizeCheckBoxes();
        Browser.BrowserUpdater?.Invoke();
    }

    private void ActualizeCheckBoxes()
    {
        for (int i = 0; i < Checkboxes.Length; i++)
            Checkboxes[i].gameObject.SetActive((_typePointer + i) < TypeList.Count);

    }

    private void NextClick()
    {
        TypePointer += Checkboxes.Length;
    }
    private void PreviousClick()
    {
        TypePointer -= Checkboxes.Length;
    }
}
