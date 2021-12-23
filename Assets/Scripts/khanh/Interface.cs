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
    /// Indicates whether the initialization is complete
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>    
    /// A delegate function, that will be executed after the login happened.
    /// </summary>
    /// <param name="success">Stores if the login was successful.</param>
    /// <param name="message">Stores the message of the login.</param>
    public delegate void AfterLoginEvent(bool success, string message);

    /// <summary>
    /// A delegate-method that can define how the login should be executed.
    /// </summary>
    /// <param name="loginData"></param>
    /// <param name="afterLogin"></param>
    public delegate void LoginEvent(LoginData loginData, AfterLoginEvent afterLogin);

    /// <summary>
    /// Can add login functionality to the interface.
    /// If defined, the interface automatically becomes available in the login window.
    /// </summary>
    public LoginEvent OnLogin;

    /// <summary>
    /// A delegate-method that can define how the browser should be set.
    /// </summary>
    /// <param name="dataBrowser">The instance of the browser, that should be set.</param>
    //public delegate IEnumerator BrowserSetupEvent(DataBrowser dataBrowser);

    /// <summary>
    /// Makes the interface browsable.
    /// If defined, the interface automatically becomes available in the DataBrowser.
    /// </summary>
    //public BrowserSetupEvent OnSetupBrowser;

    /// <summary>
    /// Initializes the interface. It is not necessary to call this explicitly. 
    /// It will be automatically executed, when the scene-controller
    /// </summary>
    /// <returns></returns>
    public IEnumerator Initialize()
    {
        if (IsInitialized)
            throw new InvalidOperationException($"Can't initialize interface '{this}': It's already initialized!");

        yield return InitializeInternal();
        IsInitialized = true;
    }

    /// <summary>
    /// This method must be overwritten and should define how the interface works.
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerator InitializeInternal();

}