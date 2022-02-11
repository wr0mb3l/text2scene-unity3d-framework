using LitJson;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class ResourceManagerInterface : Interface
{

    public const string AUTHORIZATION = "https://authority.hucompute.org/";
    public const string WS = "https://resources.hucompute.org/";
    public const string ROOT = "https://resources.hucompute.org/repository/1";

    public const string LOGIN = AUTHORIZATION + "login";
    public const string CHECK_SESSION = AUTHORIZATION + "checklogin";
    public const string REPOSITORIES = WS + "repositories?";
    public const string USERS = AUTHORIZATION + "users?";
    public const string ANNOVIEWS = WS + "annoviews?";
    public const string DOCUMENTS = WS + "documents?";
    public const string RESOURCE = WS + "resource?";
    public const string COPY = WS + "copy?";
    public const string MOVE = WS + "move?";
    public const string PASTE = WS + "paste?";
    public const string DOWNLOAD = WS + "download?";
    public const string UPLOAD = WS + "upload?";

    public const string JSONPARAM_SUCCESS = "success";
    public const string JSONPARAM_MSG = "message";
    public const string JSONPARAM_RESULT = "result";
    public const string JSONPARAM_SESSION = "session";
    public const string JSON_PARAM_USER = "user";
    public const string JSONPARAM_NAME = "name";
    public const string JSONPARAM_NODE = "node";
    public const string JSONPARAM_TOTAL = "total";
    public const string JSONPARAM_DATA = "data";
    public const string JSONPARAM_TEXT = "text";
    public const string JSONPARAM_URI = "uri";
    public const string JSONPARAM_PARENT = "parent";
    public const string JSONPARAM_ID = "id";
    public const string JSONPARAM_TYPE = "type";
    public const string JSONPARAM_MIMETYPE = "mimetype";
    public const string JSONPARAM_SIZE = "size";
    public const string JSONPARAM_CR_TIME = "created";
    public const string JSONPARAM_LC_TIME = "modified";
    public const string JSONPARAM_LEAF = "leaf";

    public const string LOGIN_MSG = "Dazu m�ssen Sie\neingeloggt sein.";
    public const string LOGIN_FAILED_MSG = "Anmeldung fehlgeschlagen.";
    public const string LOGIN_SUCCESS_MSG = "Anmeldung erfolgreich.";
    public const string REQUEST_FAILED_MSG = "Abfrage fehlgeschlagen.";
    public const string SESSION_EXPIRED_MSG = "Sitzung abgelaufen.\nBitte erneut anmelden.";
    public const string OP_SUCCESS_MSG = " wurde erfolgreich ausgef�hrt.";
    public const string OP_FAILED_MSG = " ist fehlgeschlagen.";
    public const string START_DOWNLOAD_MSG = "Download wird gestartet.";
    public const string START_UPLOAD_MSG = "Upload wird gestartet.";
    public const string DOWNLOAD_SUCCESS_MSG = "Download erfolgreich.";
    public const string DOWNLOAD_FAILED_MSG = "Download fehlgeschlagen.";
    public const string UPLOAD_SUCCESS_MSG = "Upload erfolgreich.";
    public const string UPLOAD_FAILED_MSG = "Upload fehlgeschlagen.";

    /// <summary>
    /// Speichert die ID des Users.
    /// </summary>
    public string UserID { get; private set; }

    private bool _sessionRequestStarted;
    private string _sessionID;
    /// <summary>
    /// Speichert die ID des aktuellen Sessions.
    /// </summary>
    public string SessionID
    {
        get { return _sessionID; }
        set
        {
            _sessionID = value;
            SessionExpired = false;
        }
    }

    /// <summary>
    /// Stores the data of the last login
    /// </summary>
    public LoginData Login;

    /// <summary>
    /// Will be used to show the state of the login on UI-elements
    /// </summary>
    public string LoginMessage { get; private set; }

    public bool SessionExpired { get; private set; }

    /// <summary>
    /// A delegate function, that can describe, what should happen, after the session id was gained.
    /// </summary>
    /// <param name="sessionID">The ID of the opened session</param>
    public delegate void OnSessionID(string sessionID);

    /// <summary>
    /// A delegate function, that can describe, what should happen, after the server was made a response.
    /// </summary>
    /// <param name="response">The response of the server as a JSON</param>
    public delegate void OnServerResponse(JsonData response);

    /// <summary>
    /// Initializes the ResourceManagerInterface-class on the first start.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator InitializeInternal()
    {
        Name = "ResourceManager";
        OnLogin = (loginData, afterLogin) =>
        {
            if (_sessionRequestStarted) Login = loginData;
            else StartCoroutine(GetSession(loginData, (sessionID) => { afterLogin(sessionID != null, LoginMessage); }));
        };
        //OnSetupBrowser = SetupBrowser;
        yield break;
    }

    private float _sessionTimer = 300; private float _timeDelta;
    public void Update()
    {
        if (_sessionID != null && !SessionExpired)
        {
            _timeDelta += Time.deltaTime;
            SessionExpired = _timeDelta >= _sessionTimer;
        }
    }

    public IEnumerator AutoLogin()
    {
        //yield return StartCoroutine(GetSession(new LoginData("kett", StolperwegeHelper.Md5Sum("Tig3rm4n"))));
        yield return StartCoroutine(GetSession(new LoginData("henlein", StolperwegeHelper.Md5Sum("messe6874"))));
    }

    public IEnumerator LoginWithCredential(string username, string password)
    {
        yield return StartCoroutine(GetSession(new LoginData(username, StolperwegeHelper.Md5Sum(password))));
    }

    /// <summary>
    /// Checks if there is a valid session and returns its id through the delegate method. 
    /// If there is no active session, the login will be started with the passed data.
    /// If there is neither a valid session nor login data was passed, the login window will be activated.
    /// This method will be waiting until the user has entered his login data.
    /// </summary>
    /// <param name="loginData">Contains the username and password of the user (optional).</param>
    /// <param name="onSessionID">Can be optionally defined, to determine what shoudl happen, after the server has opened a session for the user.</param>
    /// <returns></returns>
    public IEnumerator GetSession(LoginData loginData = null, OnSessionID onSessionID = null)
    {
        _sessionRequestStarted = true;
        WWWForm form = new WWWForm();
        UnityWebRequest webRequest;

        if (_sessionID == null)
        {
            Login = loginData;
            //if (Login == null && StolperwegeHelper.User != null) StolperwegeHelper.User.CallLogin(this);

            while (Login == null)
                yield return null;

            form.AddField("username", Login.Username);
            form.AddField("password", Login.Password);

            using (webRequest = UnityWebRequest.Post(LOGIN, form))
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                    Debug.Log(webRequest.error);
                else
                {
                    Debug.Log(webRequest.downloadHandler.text);

                    JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

                    bool loginSuccess = bool.Parse(data[JSONPARAM_SUCCESS].ToString());

                    if (!data.Keys.Contains(JSONPARAM_RESULT) || !loginSuccess)
                    {
                        Debug.Log(LOGIN_FAILED_MSG);
                        if (data.Keys.Contains(JSONPARAM_MSG))
                            LoginMessage = data[JSONPARAM_MSG].ToString();
                        else LoginMessage = "Login failed!";
                        onSessionID?.Invoke(null);
                        /*if (StolperwegeHelper.User != null && StolperwegeHelper.User.Login.Active)
                            StolperwegeHelper.User.Login.OnLoginResult(false, LoginMessage);*/
                        _sessionRequestStarted = false;
                        yield break;
                    }

                    data = data[JSONPARAM_RESULT];

                    if (!data.Keys.Contains(JSONPARAM_SESSION))
                    {
                        Debug.Log(LOGIN_FAILED_MSG);
                        if (data.Keys.Contains(JSONPARAM_MSG))
                            LoginMessage = data[JSONPARAM_MSG].ToString();
                        else LoginMessage = "Login failed: session not permitted!";
                        onSessionID?.Invoke(null);
                        /*if (StolperwegeHelper.User != null && StolperwegeHelper.User.Login.Active)
                            StolperwegeHelper.User.Login.OnLoginResult(false, LoginMessage);*/
                        _sessionRequestStarted = false;
                        yield break;
                    }

                    Debug.Log(LOGIN_SUCCESS_MSG);
                    SessionID = data[JSONPARAM_SESSION].ToString();
                    UserID = data[JSON_PARAM_USER].ToString();
                    LoginMessage = "Login succesful!";
                    /*if (StolperwegeHelper.User != null && StolperwegeHelper.User.Login.Active)
                        StolperwegeHelper.User.Login.OnLoginResult(true, LoginMessage);*/
                    Login = null;
                }
            }

        }
        else if (SessionExpired)
        {

            form.AddField(JSONPARAM_SESSION, _sessionID);

            using (webRequest = UnityWebRequest.Post(CHECK_SESSION, form))
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();
                if (webRequest.isNetworkError || webRequest.isNetworkError)
                    Debug.Log(webRequest.error);
                else
                {
                    JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

                    bool requestSucces = bool.Parse(data[JSONPARAM_SUCCESS].ToString());

                    if (!data.Keys.Contains(JSONPARAM_RESULT) || !requestSucces)
                    {
                        Debug.Log(REQUEST_FAILED_MSG);
                        LoginMessage = "Session could not be renewed!";
                        onSessionID?.Invoke(null);
                        _sessionRequestStarted = false;
                        yield break;
                    }

                    data = data[JSONPARAM_RESULT];

                    if (!data.Keys.Contains(JSONPARAM_SESSION))
                    {
                        Debug.Log(REQUEST_FAILED_MSG);
                        LoginMessage = "Session could not be renewed!";
                        onSessionID?.Invoke(null);
                        _sessionRequestStarted = false;
                        yield break;
                    }

                    SessionID = data[JSONPARAM_SESSION].ToString();
                    UserID = data[JSON_PARAM_USER].ToString();
                    LoginMessage = "Session reopened.";
                }
            }
        }
        else
            LoginMessage = "Session was already open.";

        onSessionID?.Invoke(_sessionID);
        _sessionRequestStarted = false;
    }

    /// <summary>
    /// A function to query the list of all users who have access rights to the ResourceManager.
    /// </summary>
    /// <param name="response">If setted, this delegate method will be called after the server response.</param>
    /// <returns></returns>
    public IEnumerator GetUserList(OnServerResponse response)
    {

        yield return StartCoroutine(GetSession());
        string requestURL = USERS + "session=" + _sessionID;
        StartCoroutine(SendRequest(requestURL, response));

    }

    /// <summary>
    /// A function to request each view of a certain TextAnnotator-Document.
    /// </summary>
    /// <param name="url">The URL of the Document</param>
    /// <param name="response">If setted, this delegate method will be called after the server response.</param>
    /// <returns></returns>
    public IEnumerator GetAnnoview(string url, OnServerResponse response)
    {
        yield return StartCoroutine(GetSession());
        string requestURL = url + "?session=" + _sessionID;
        StartCoroutine(SendRequest(requestURL, response));
    }

    public delegate void OnDataLoaded();

    /// <summary>
    /// A function to get informations about the content of a certain folder of the ResourceManager.
    /// After the folder content is loaded the informations of files will be automatically requested.
    /// </summary>
    /// <param name="actualFolder">The folder which content should be loaded.</param>
    /// <param name="onLoaded">Defines what should happen after loading the content.</param>
    /// <returns></returns>
    public IEnumerator GetRepositoryInformations(VRResourceData actualFolder, OnDataLoaded onLoaded)
    {
        actualFolder.Clear();
        yield return StartCoroutine(GetSession());

        string requestURL = REPOSITORIES + "session=" + _sessionID + "&node=" + actualFolder.ID + "&documents=true";

        UnityWebRequest webRequest = new UnityWebRequest(requestURL);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return webRequest.SendWebRequest();

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

        Debug.Log(data.ToJson());

        if (!bool.Parse(data[JSONPARAM_SUCCESS].ToString()))
        {
            Debug.Log("Request failed");
            yield break;
        }

        string parentPath = "";

        foreach (JsonData child in data[JSONPARAM_DATA])
        {
            string name, format;
            string path = child[JSONPARAM_URI].ToString();
            if (child.Keys.Contains(JSONPARAM_PARENT))
                parentPath = child[JSONPARAM_PARENT].ToString();
            if (actualFolder.Path == "root")
                actualFolder.Path = parentPath;

            string id = child[JSONPARAM_ID].ToString();

            if (child[JSONPARAM_TYPE].ToString().Equals("file"))
            {
                requestURL = DOCUMENTS + "node=" + id + "&session=" + _sessionID;
                webRequest = new UnityWebRequest(requestURL);
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                yield return webRequest.SendWebRequest();

                data = JsonMapper.ToObject(webRequest.downloadHandler.text);

                if (!bool.Parse(data[JSONPARAM_SUCCESS].ToString()))
                {
                    Debug.Log("File download failed");
                    continue;
                }

                if (data[JSONPARAM_DATA].Count > 0)
                {
                    JsonData fileInfo = data[JSONPARAM_DATA][0];

                    name = fileInfo[JSONPARAM_NAME].ToString();
                    format = (fileInfo.ContainsKey(JSONPARAM_MIMETYPE)) ? fileInfo[JSONPARAM_MIMETYPE].ToString() : "";
                    long size = long.Parse(fileInfo[JSONPARAM_SIZE].ToString());
                    DateTime created = VRResourceData.StringToDateTime(fileInfo[JSONPARAM_CR_TIME].ToString(), '-', ':', '.');
                    DateTime modified = VRResourceData.StringToDateTime(fileInfo[JSONPARAM_LC_TIME].ToString(), '-', ':', '.');
                    VRResourceData file = new VRResourceData(name, path, actualFolder, format, size, id, created, modified, VRData.SourceType.Remote);
                    actualFolder.AddFileToFormatMap(file, true);
                }
            }
            else
            {
                name = child[JSONPARAM_TEXT].ToString();
                VRResourceData folder = new VRResourceData(name, path, actualFolder, id, DateTime.Now, DateTime.Now, VRData.SourceType.Remote);
                if (bool.Parse(child[JSONPARAM_LEAF].ToString()))
                    actualFolder.EmptyFolders.Add(folder);
                else
                    actualFolder.NonEmptyFolders.Add(folder);

            }
        }

        onLoaded?.Invoke();
    }

    /// <summary>
    /// A function to get informations about the content of a certain folder of the ResourceManager.
    /// This method puts the focus on folders and documents that can be loaded by the TextAnnotator.
    /// </summary>
    /// <param name="actualFolder">The folder which content should be loaded.</param>
    /// <param name="onLoaded">Defines what should happen after loading the content.</param>
    /// <returns></returns>
    public IEnumerator GetRepositoryTAInformations(VRResourceData actualFolder, OnDataLoaded onLoaded)
    {
        actualFolder.Clear();
        yield return StartCoroutine(GetSession());

        string requestURL = REPOSITORIES + "session=" + _sessionID + "&node=" + actualFolder.ID + "&documents=true";
        Debug.Log(requestURL);

        UnityWebRequest webRequest = new UnityWebRequest(requestURL);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return webRequest.SendWebRequest();

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

        Debug.Log(data.ToJson());

        if (!bool.Parse(data[JSONPARAM_SUCCESS].ToString()))
        {
            Debug.Log("Request failed");
            yield break;
        }

        string parentPath = "";

        foreach (JsonData child in data[JSONPARAM_DATA])
        {
            string name;
            string path = child[JSONPARAM_URI].ToString();
            if (child.Keys.Contains(JSONPARAM_PARENT))
                parentPath = child[JSONPARAM_PARENT].ToString();
            if (actualFolder.Path == "root")
                actualFolder.Path = parentPath;

            string id = child[JSONPARAM_ID].ToString();

            if (child[JSONPARAM_TYPE].ToString().Equals("file"))
            {
                if (child.ContainsKey(JSONPARAM_MIMETYPE) && child[JSONPARAM_MIMETYPE].ToString().Equals("application/bson"))
                {
                    name = child.ContainsKey("qtip") ? child["qtip"].ToString() : id + " " + child["text"];
                    if (name.Contains("<br>"))
                    {
                        name = name.Substring(name.LastIndexOf("<br>"));
                    }
                    name = name.Replace("<br>", "").Replace("\n", "");
                    VRResourceData file = new VRResourceData(name, path, actualFolder, child[JSONPARAM_MIMETYPE].ToString(), 0, id, new DateTime(), new DateTime(), VRData.SourceType.Remote);
                    actualFolder.AddFileToFormatMap(file, true);
                }
            }
            else
            {
                name = child[JSONPARAM_TEXT].ToString();
                VRResourceData folder = new VRResourceData(name, path, actualFolder, id, DateTime.Now, DateTime.Now, VRData.SourceType.Remote);
                if (bool.Parse(child[JSONPARAM_LEAF].ToString()))
                    actualFolder.EmptyFolders.Add(folder);
                else
                    actualFolder.NonEmptyFolders.Add(folder);

            }
        }

        onLoaded?.Invoke();
    }

    /// <summary>
    /// A delegate function that can define the actions after a request has been answered by the server.
    /// </summary>
    /// <param name="succes">True if the request was successful, otherwise false.</param>
    public delegate void OnRequestAnswer(bool succes);

    /// <summary>
    /// A function to delete a certain element in the ResourceManager.
    /// </summary>
    /// <param name="element">The element to be deleted.</param>
    /// <param name="onAnswer"></param>
    /// <returns></returns>
    /*public IEnumerator DeleteResource(VRResourceData element, OnRequestAnswer onAnswer)
    {
        yield return StartCoroutine(GetSession());

        string requestURL = RESOURCE + "target=" + element.Path + "&parent=" + element.ParentPath + "&session=" + _sessionID;

        yield return StartCoroutine(SendRequest(requestURL, (JsonData data) =>
        {
            string msg = "Das L�schen von " + element.ID;

            bool success = bool.Parse(data[JSONPARAM_SUCCESS].ToString());

            if (success) msg += OP_SUCCESS_MSG;
            else msg += OP_FAILED_MSG;

            //StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(msg, 3, MessageBoxScript.PriorityType.High));
            Debug.Log(msg);

            onAnswer(success);
        }));

    }*/

    /// <summary>
    /// A function for downloading data from the ResourceManager and storing it on the local filesystem.
    /// </summary>
    /// <param name="file">The file, that should be downloaded.</param>
    /// <param name="downloadFolder">The folder, where the data should be stored locally.</param>
    /// <param name="onAnswer">The actions to be performed after the download is done.</param>
    /// <returns></returns>
    public IEnumerator DownloadResource(VRResourceData file, string downloadFolder, OnRequestAnswer onAnswer)
    {

        bool success = false;

        yield return StartCoroutine(GetSession());

        UnityWebRequest webRequest = new UnityWebRequest(DOWNLOAD + "target=" + file.ID + "&session=" + _sessionID);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        Debug.Log(START_DOWNLOAD_MSG);

        yield return webRequest.SendWebRequest();

        string msg;

        try
        {
            File.WriteAllBytes(downloadFolder + "\\" + file.NameWithFormat, webRequest.downloadHandler.data);
            msg = DOWNLOAD_SUCCESS_MSG;
            success = true;
        }
        catch (Exception)
        {
            msg = DOWNLOAD_FAILED_MSG;
        }
        Debug.Log(msg);

        onAnswer(success);
    }

    /// <summary>
    /// A function for downloading data from the ResourceManager and storing it on the local filesystem.
    /// </summary>
    /// <param name="id">The ID of the file, that should be downloaded.</param>
    /// <param name="downloadFolder">The folder, where the data should be stored locally.</param>
    /// <param name="filename">The name under which the file should be saved.</param>
    /// <param name="sessionID">The ID of the opened session.</param>
    /// <param name="onAnswer">The actions to be performed after the download is done.</param>
    /// <returns></returns>
    public static IEnumerator DownloadResource(string id, string downloadFolder, string filename, string sessionID, OnRequestAnswer onAnswer)
    {

        bool success = false;

        Debug.Log(DOWNLOAD + "target=" + id + "&session=" + sessionID);

        UnityWebRequest webRequest = new UnityWebRequest(DOWNLOAD + "target=" + id + "&session=" + sessionID);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        Debug.Log(START_DOWNLOAD_MSG);

        yield return webRequest.SendWebRequest();

        string msg;
        Debug.Log(downloadFolder + "\\" + filename);
        try
        {
            File.WriteAllBytes(downloadFolder + "\\" + filename, webRequest.downloadHandler.data);
            msg = DOWNLOAD_SUCCESS_MSG;
            success = true;
        }
        catch (Exception)
        {
            msg = DOWNLOAD_FAILED_MSG;
        }
        Debug.Log(msg);

        onAnswer(success);
    }

    /// <summary>
    /// A function for uploading data from the local filesystem to the ResourceManager.
    /// </summary>
    /// <param name="file">The file, that should be uploaded.</param>
    /// <param name="targetURL">The target directory on the ResourceManager.</param>
    /// <param name="onAnswer">The actions to be performed after the upload is done.</param>
    /// <returns></returns>
/*    public IEnumerator UploadResource(VRResourceData file, string targetURL, OnRequestAnswer onAnswer)
    {
        yield return StartCoroutine(GetSession());

        byte[] fileInBytes = File.ReadAllBytes(file.Path);
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("file", fileInBytes, file.NameWithFormat, ""));

        byte[] boundary = UnityWebRequest.GenerateBoundary();
        byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
        byte[] terminate = Encoding.UTF8.GetBytes(String.Concat("\r\n--", Encoding.UTF8.GetString(boundary), "--"));

        byte[] body = new byte[formSections.Length + terminate.Length];
        Buffer.BlockCopy(formSections, 0, body, 0, formSections.Length);
        Buffer.BlockCopy(terminate, 0, body, formSections.Length, terminate.Length);

        UnityWebRequest request = new UnityWebRequest(UPLOAD + "session=" + _sessionID + "&target=" + targetURL);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.uploadHandler.contentType = string.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));
        request.method = "POST";
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log(request.error);
            onAnswer(false);
            yield break;
        }

        onAnswer(true);
    }*/

    /// <summary>
    /// A function for moving data on the ResourceManager from its actual url to an another one.
    /// </summary>
    /// <param name="file">The file, that should be moved.</param>
    /// <param name="targetURL">The target directory on the ResourceManager.</param>
    /// <param name="onAnswer">The actions to be performed after the move is done.</param>
    /// <returns></returns>
/*    public IEnumerator MoveResource(VRResourceData file, string targetURL, OnRequestAnswer onAnswer)
    {

        yield return StartCoroutine(GetSession());
        string requestURL = MOVE + "source=" + file.ID + "&sourceParent=" + file.ParentPath + "&target=" + targetURL + "&session=" + _sessionID;
        yield return SendRequest(requestURL, (JsonData data) =>
        {
            string msg = "Das Verschieben von " + file.ID + "\nnach " + targetURL;

            bool success = bool.Parse(data[JSONPARAM_SUCCESS].ToString());

            if (success) msg += OP_SUCCESS_MSG;
            else msg += OP_FAILED_MSG;

            Debug.Log(msg);
            onAnswer(success);
        });
    }*/

    /// <summary>
    /// A function for copying data on the ResourceManager to an another url.
    /// </summary>
    /// <param name="file">The file, that should be copyed.</param>
    /// <param name="targetURL">The target directory on the ResourceManager.</param>
    /// <param name="onAnswer">The actions to be performed after the copy is done.</param>
    /// <returns></returns>
/*    public IEnumerator CopyResource(VRResourceData file, string targetURL, OnRequestAnswer onAnswer)
    {
        string msg = "Kopieren von " + file.ID + " nach\n" + targetURL;

        yield return StartCoroutine(GetSession());
        string requestURL = COPY + "target=" + file.ID + "&session=" + _sessionID;

        yield return StartCoroutine(SendRequest(requestURL, (JsonData data) =>
        {
            bool success = bool.Parse(data[JSONPARAM_SUCCESS].ToString());

            if (!success)
            {
                msg += OP_FAILED_MSG;
                Debug.Log(msg);
                onAnswer(false);
                return;
            }
        }));

        requestURL = PASTE + "target=" + file.ID + "&parent=" + file.ParentPath + "&session=" + _sessionID;
        yield return StartCoroutine(SendRequest(requestURL, (JsonData data) =>
        {
            bool success = bool.Parse(data[JSONPARAM_SUCCESS].ToString());

            if (success) msg += OP_SUCCESS_MSG;
            else msg += OP_FAILED_MSG;

            Debug.Log(msg);

            onAnswer(success);
        }));

    }*/

    /// <summary>
    /// Helper function, that sends a request to the given URL.
    /// </summary>
    /// <param name="requestURL">The URL of the request.</param>
    /// <param name="onResponse">The actions to be performed after the server response.</param>
    /// <returns></returns>
    private IEnumerator SendRequest(string requestURL, OnServerResponse onResponse)
    {
        UnityWebRequest webRequest = new UnityWebRequest(requestURL);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return webRequest.SendWebRequest();

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);
        onResponse(data);
    }

    /// <summary>
    /// A function for setting up the DataBrowser instance, for browsing the ResourceManager.
    /// </summary>
    /// <param name="browser">The DataBrowser instance.</param>
    /// <returns></returns>
    /*private IEnumerator SetupBrowser(DataBrowser browser)
    {
        // Close any panels animated
        browser.DataPanel.SetComponentStatus(false);
        if (browser.FilterPanel.IsActive)
        {
            if (browser.SearchPanel.IsActive) StartCoroutine(browser.FilterPanel.Activate(false));
            else yield return StartCoroutine(browser.FilterPanel.Activate(false));
        }
        if (browser.SearchPanel.IsActive) yield return StartCoroutine(browser.SearchPanel.Activate(false));

        // ============================= FILTER PANEL SETUP ============================

        // Set filters
        if (!browser.DataSpaceFilterMap.ContainsKey(Name))
        {
            browser.DataSpaceFilterMap.Add(Name, new Dictionary<string, InteractiveCheckbox.CheckboxStatus> {
                { StolperwegeHelper.FOLDER, InteractiveCheckbox.CheckboxStatus.AllChecked } ,
                { StolperwegeHelper.TEXT, InteractiveCheckbox.CheckboxStatus.AllChecked },
                { StolperwegeHelper.PICTURE, InteractiveCheckbox.CheckboxStatus.AllChecked },
                { StolperwegeHelper.AUDIO, InteractiveCheckbox.CheckboxStatus.AllChecked },
                { StolperwegeHelper.VIDEO, InteractiveCheckbox.CheckboxStatus.AllChecked },
                { StolperwegeHelper.OTHER, InteractiveCheckbox.CheckboxStatus.AllChecked } });
        }

        // Define filter update event
        browser.FilterPanel.FilterUpdater = () =>
        {
            for (int i = 0; i < browser.FilterPanel.Checkboxes.Length; i++)
            {
                browser.FilterPanel.Checkboxes[i].gameObject.SetActive((browser.FilterPanel.TypePointer + i) < browser.FilterPanel.TypeList.Count);
                if (browser.FilterPanel.Checkboxes[i].gameObject.activeInHierarchy)
                {
                    browser.FilterPanel.Checkboxes[i].ButtonValue = browser.FilterPanel.TypeList[browser.FilterPanel.TypePointer + i];
                    browser.FilterPanel.Checkboxes[i].Status = browser.FilterPanel.Types[browser.FilterPanel.TypeList[browser.FilterPanel.TypePointer + i]];
                    browser.FilterPanel.Openers[i].gameObject.SetActive(false);
                }
            }
        };

        // Set event for changing checkboxes
        browser.FilterPanel.CheckboxUpdater = (type, status) => { browser.FilterPanel.Types[type] = status; };

        // Initialize filter panel
        browser.FilterPanel.Init("File Types", browser.DataSpaceFilterMap[Name]);



        // ============================= DATA PANEL SETUP ============================
        // Root button functionality
        browser.DataPanel.Root.gameObject.SetActive(true);
        browser.DataPanel.Root.OnClick = () =>
        {
            VRResourceData root = new VRResourceData("root", ROOT, null, "root", DateTime.MaxValue, DateTime.MaxValue, VRData.SourceType.Remote);
            browser.DataPanel.Init("Loading " + root.Path);
            StartCoroutine(GetRepositoryInformations(root, () =>
            {
                browser.SetActualState(Name, root);
                if (browser.SelectedInterface.Equals(this))
                {
                    browser.DataPanel.Init(root.Path, root.GetSortedContent(browser.FilterPanel.GetSelectedItems(), browser.SearchPanel.SearchPattern.ToLower()));
                    browser.DataPanel.ParentDir.Active = false;
                    browser.DataPanel.Root.Active = false;
                }

            }));
        };

        // Parent button functionality
        browser.DataPanel.ParentDir.OnClick = () =>
        {
            VRResourceData actualDir = null;
            if (browser.LastBrowserStateMap.ContainsKey(Name) && browser.LastBrowserStateMap[Name] != null) actualDir = (VRResourceData)browser.LastBrowserStateMap[Name];
            if (actualDir == null || actualDir.Parent == null) return;
            browser.DataPanel.Init("Loading " + actualDir.Parent.Path);
            StartCoroutine(GetRepositoryInformations(actualDir.Parent, () =>
            {
                browser.SetActualState(Name, actualDir.Parent);
                if (browser.SelectedInterface.Equals(this))
                {
                    browser.DataPanel.Init(actualDir.Parent.Path, actualDir.Parent.GetSortedContent(browser.FilterPanel.GetSelectedItems(), browser.SearchPanel.SearchPattern.ToLower()));
                    browser.DataPanel.ParentDir.Active = actualDir.Parent.Parent != null;
                    browser.DataPanel.Root.Active = actualDir.Parent.Parent != null;
                }
            }));
        };

        // Define browser update event
        browser.BrowserUpdater = () =>
        {
            VRResourceData actualDir = null;
            if (browser.LastBrowserStateMap.ContainsKey(Name) && browser.LastBrowserStateMap[Name] != null) actualDir = (VRResourceData)browser.LastBrowserStateMap[Name];
            if (actualDir == null) return;
            browser.DataPanel.Init(actualDir.Path, actualDir.GetSortedContent(browser.FilterPanel.GetSelectedItems(), browser.SearchPanel.SearchPattern.ToLower()));
            browser.DataPanel.ParentDir.Active = actualDir != null;
            browser.DataPanel.Root.Active = actualDir != null;
        };

        // Define datacontainer events
        foreach (DataContainer dc in browser.DataPanel.DataContainers)
        {
            dc.OnClick = () =>
            {
                VRResourceData resource = (VRResourceData)dc.Resource;
                if (resource != null && resource.Type != VRResourceData.DataType.File)
                {
                    browser.DataPanel.Init("Loading " + resource.Path);
                    StartCoroutine(GetRepositoryInformations(resource, () =>
                    {
                        browser.SetActualState(Name, resource);
                        if (browser.SelectedInterface == this)
                        {
                            browser.DataPanel.Init(resource.Path, resource.GetSortedContent(browser.FilterPanel.GetSelectedItems(), browser.SearchPanel.SearchPattern.ToLower()));
                            browser.DataPanel.ParentDir.Active = resource.Parent != null;
                            browser.DataPanel.Root.Active = resource.Parent != null;
                        }
                    }));
                }

            };
        }

        // ============================= LOADING LAST STATE ============================ 

        StartCoroutine(browser.FilterPanel.Activate(true));
        StartCoroutine(browser.SearchPanel.Activate(true));
        VRResourceData dir = null;
        if (browser.LastBrowserStateMap.ContainsKey(Name) && browser.LastBrowserStateMap[Name] != null) dir = (VRResourceData)browser.LastBrowserStateMap[Name];
        if (dir == null) browser.DataPanel.Root.OnClick();
        else
        {
            StartCoroutine(GetRepositoryInformations(dir, () =>
            {
                browser.SetActualState(Name, dir);
                if (browser.SelectedInterface.Equals(this))
                {
                    browser.DataPanel.Init(dir.Path, dir.GetSortedContent(browser.FilterPanel.GetSelectedItems(), browser.SearchPanel.SearchPattern.ToLower()));
                    browser.DataPanel.ParentDir.Active = dir.Parent != null;
                    browser.DataPanel.Root.Active = dir.Parent != null;
                }
            }));
        }
    }*/

    // ============================== Methode f�r resources2City ====================================================
    //public IEnumerator GetRepositoryInformations(VRResourceData actualFolder, List<VRResourceData> subFolders, Dictionary<string, FolderInfo> folderInfos, 
    //                                                    HashSet<string> detectedFiles, HashSet<string> detectedEmptyFolders, CityScript city)

    //{


    //    if (city.Layout == CityScript.LayoutType.CustomGrid && actualFolder.Path != "root")
    //    {
    //        FolderInfo folderInfo;
    //        if (folderInfos.ContainsKey(actualFolder.Path))
    //            folderInfo = folderInfos[actualFolder.Path];
    //        else
    //        {
    //            folderInfo = new FolderInfo(actualFolder.Path);
    //            folderInfos.Add(actualFolder.Path, folderInfo);
    //        }
    //    }

    //    yield return StartCoroutine(GetSession());

    //    string requestURL = REPOSITORIES + "session=" + _sessionID + "&node=" + actualFolder.ID + "&documents=true";

    //    UnityWebRequest webRequest = new UnityWebRequest(requestURL);
    //    webRequest.downloadHandler = new DownloadHandlerBuffer();

    //    yield return webRequest.SendWebRequest();

    //    JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

    //    if (!bool.Parse(data[JSONPARAM_SUCCESS].ToString()))
    //    {
    //        Debug.Log("Request failed");
    //        yield break;
    //    }

    //    string parentPath = "";

    //    foreach (JsonData child in data[JSONPARAM_DATA])
    //    {
    //        string name, format;
    //        string path = child[JSONPARAM_URI].ToString();
    //        if (child.Keys.Contains(JSONPARAM_PARENT))
    //            parentPath = child[JSONPARAM_PARENT].ToString();
    //        if (actualFolder.Path == "root")
    //        {
    //            actualFolder.Path = parentPath;

    //            if (city.Layout == CityScript.LayoutType.CustomGrid)
    //            {
    //                FolderInfo folderInfo;
    //                if (folderInfos.ContainsKey(actualFolder.Path))
    //                    folderInfo = folderInfos[actualFolder.Path];
    //                else
    //                {
    //                    folderInfo = new FolderInfo(actualFolder.Path);
    //                    folderInfos.Add(actualFolder.Path, folderInfo);
    //                }
    //            }

    //        }
    //        string id = child[JSONPARAM_ID].ToString();

    //        if (child[JSONPARAM_TYPE].ToString().Equals("file"))
    //        {
    //            requestURL = DOCUMENTS + "node=" + id + "&session=" + _sessionID;
    //            webRequest = new UnityWebRequest(requestURL);
    //            webRequest.downloadHandler = new DownloadHandlerBuffer();

    //            yield return webRequest.SendWebRequest();

    //            data = JsonMapper.ToObject(webRequest.downloadHandler.text);

    //            if (!bool.Parse(data[JSONPARAM_SUCCESS].ToString()))
    //            {
    //                Debug.Log("File download failed");
    //                continue;
    //            }

    //            if (data[JSONPARAM_DATA].Count > 0)
    //            {
    //                JsonData fileInfo = data[JSONPARAM_DATA][0];

    //                name = fileInfo[JSONPARAM_NAME].ToString();
    //                format = (fileInfo.ContainsKey(JSONPARAM_MIMETYPE)) ? fileInfo[JSONPARAM_MIMETYPE].ToString() : "";
    //                long size = long.Parse(fileInfo[JSONPARAM_SIZE].ToString());
    //                DateTime created = VRResourceData.StringToDateTime(fileInfo[JSONPARAM_CR_TIME].ToString(), '-', ':', '.');
    //                DateTime modified = VRResourceData.StringToDateTime(fileInfo[JSONPARAM_LC_TIME].ToString(), '-', ':', '.');
    //                VRResourceData file = new VRResourceData(name, path, actualFolder, format, size, id, created, modified, city, city.Source);
    //                city.FilePathMap.Add(file.Path, file);
    //                actualFolder.AddFileToFormatMap(file, true);

    //                if (city.Layout == CityScript.LayoutType.CustomGrid)
    //                    detectedFiles.Add(file.Path);
    //            }
    //        }
    //        else
    //        {
    //            name = child[JSONPARAM_TEXT].ToString();
    //            VRResourceData folder = new VRResourceData(name, path, actualFolder, id, CityScript.DEFAULT_DATE, CityScript.DEFAULT_DATE, city);
    //            if (bool.Parse(child[JSONPARAM_LEAF].ToString())) {
    //                city.FolderPathMap.Add(folder.Path, folder);
    //                actualFolder.AddEmptyFolderToList(folder);
    //                if (city.Layout == CityScript.LayoutType.CustomGrid)
    //                    detectedEmptyFolders.Add(folder.Path);
    //            }
    //            else
    //                subFolders.Add(folder);              
    //        }
    //    }
    //    //System.Threading.Thread sortingThread = new System.Threading.Thread(new System.Threading.ThreadStart(actualFolder.SortAllData));
    //    //sortingThread.Start();
    //}
}
