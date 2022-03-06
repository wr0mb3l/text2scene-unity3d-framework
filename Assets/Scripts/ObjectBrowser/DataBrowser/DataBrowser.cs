using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataBrowser : MonoBehaviour
{
    public ShapeNetInterface shapeNetInterface;
    public Material GoetheOn;
    public Material GoetheOff;

    public DataBrowserFilterController FilterPanel { get; private set; }
    public DataSpaceControl SpaceControl { get; private set; }
    public DataPanel DataPanel { get; private set; }
    public DataSearchPanel SearchPanel { get; private set; }

    /// <summary>
    /// This dictionary stores the last state of each different type of interface, that was opened with this instance of the DataBrowser.
    /// </summary>
    public Dictionary<string, object> LastBrowserStateMap { get; private set; }

    private Interface _selectedInterface;
    /// <summary>
    /// The property that references the actual opened interface.
    /// If setting this property, the BrowserSetupEvent-method of the setted interface will be called.
    /// </summary>
    public Interface SelectedInterface
    {
        get { return _selectedInterface; }
        set
        {
            _selectedInterface = value;
            StartCoroutine(_selectedInterface.OnSetupBrowser(this));
        }
    }

    public delegate void BrowserUpdateMethod();
    /// <summary>
    /// A delegate function that can be setted, to define how the browser should be updated, if changes was made.
    /// </summary>
    public BrowserUpdateMethod BrowserUpdater;

    /// <summary>
    /// The Init method initializes the DataBrowser and all of its components.
    /// </summary>
    public void Start()
    {
        LastBrowserStateMap = new Dictionary<string, object>();
        FilterPanel = GetComponentInChildren<DataBrowserFilterController>();
        FilterPanel.BaseInit();
        SpaceControl = GetComponentInChildren<DataSpaceControl>();
        SpaceControl.Init();
        DataPanel = GetComponentInChildren<DataPanel>();
        DataPanel.Init();
        SearchPanel = GetComponentInChildren<DataSearchPanel>();
        SearchPanel.Init();
    }

    /// <summary>
    /// Stores the state of the browser with the selected interface. If the selected interface has no entry, it will be added to the state map.
    /// </summary>
    /// <param name="space">The name of the interface</param>
    /// <param name="lastState">The actual state of the brwoser</param>
    public void SetActualState(string space, object lastState)
    {
        if (!LastBrowserStateMap.ContainsKey(space))
            LastBrowserStateMap.Add(space, lastState);
        else LastBrowserStateMap[space] = lastState;
    }
}
