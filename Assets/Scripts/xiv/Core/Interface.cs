using System;
using System.Collections;
using UnityEngine;

public abstract class Interface : MonoBehaviour
{
    /// <summary>
    /// The name of the interface as it should be displayed
    /// </summary>
    public string Name { get; protected set; }
    
    /// <summary>
    /// A delegate-method that can define how the browser should be set.
    /// </summary>
    /// <param name="dataBrowser">The instance of the browser, that should be set.</param>
    public delegate IEnumerator BrowserSetupEvent(DataBrowser dataBrowser);

    /// <summary>
    /// Makes the interface browsable.
    /// If defined, the interface automatically becomes available in the DataBrowser.
    /// </summary>
    public BrowserSetupEvent OnSetupBrowser;
}