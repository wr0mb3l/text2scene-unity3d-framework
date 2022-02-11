using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading;
using LitJson;

public class ShapeNetInterface : Interface
{
    public static string UserFolder { get { return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); } }
    public enum CheckboxStatus { AllChecked, NoneChecked, PartsChecked };

    public const string WS = "http://shapenet.texttechnologylab.org/";
    private const string CACHE_DIR = "\\Documents\\text2scene\\";

    public const string MAIN_CATEGORIES = "Main categories";
    public const string SUB_CATEGORIES = "Subcategories";

    // MODELS
    public const string MODELS = WS + "loadedobjects";
    public const string OBJECT_THUMBNAILS = WS + "thumbnails";
    public const string OBJECT_THUMBNAIL_INFO = WS + "thumbnailsInfos";
    public const string OBJECT_TAXONOMY = WS + "taxonomy";
    public const string GET_OBJECT_ID = WS + "get?id=";
    public const string SEARCH_OBJECTS = WS + "search?search=";

    private const string CACHED_OBJECT_DIR = CACHE_DIR + "objects\\";
    private const string CACHED_OBJECT_FILES = CACHED_OBJECT_DIR + "models\\";
    private const string CACHED_OBJECT_THUMBNAILS = CACHED_OBJECT_DIR + "thumbnails\\";
    public const string FORMATTED_MODEL_NAME = "ShapeNet Models";

    public bool _objectListLoaded;
    public string _objectListError;
    private bool _objectTaxonomyLoaded;
    private string _objectTaxonomyError;
    private bool _objectThumbnailsActualized;
    private string _objectThumbnailError;

    public Dictionary<string, ShapeNetModel> ShapeNetModels;
    public Dictionary<string, ShapeNetTaxonomyEntry> ModelTaxonomies;
    public Dictionary<string, string> ObjectSubCategoryMap;
    public Dictionary<string, CheckboxStatus> ModelMainCategories;
    public Dictionary<string, CheckboxStatus> ModelSubCategories;
    public Dictionary<string, string> CachedObjectPathMap { get; private set; }

    // Textures
    public const string TEXTURES = WS + "loadedTextures";
    public const string TEXTURE_TAXONOMY = WS + "textureTaxonomy";

    private const string CACHED_TEXTURE_DIR = CACHE_DIR + "textures\\";
    private const string CACHED_TEXTURE_FILES = CACHED_TEXTURE_DIR + "textureFiles\\";
    private const string CACHED_TEXTURE_THUMBNAILS = CACHED_TEXTURE_DIR + "thumbnails\\";
    public const string FORMATTED_TEXTURE_NAME = "ShapeNet Textures";

    private bool _textureListLoaded;
    private string _textureListError;
    private bool _textureTaxonomyLoaded;
    private string _textureTaxonomyError;

    public Dictionary<string, ShapeNetTexture> ShapeNetTextures;
    public Dictionary<string, ShapeNetTaxonomyEntry> TextureTaxonomies;
    public Dictionary<string, string> TextureSubCategoryMap;
    public Dictionary<string, CheckboxStatus> TextureMainCategories;
    public Dictionary<string, CheckboxStatus> TextureSubCategories;

    public Dictionary<string, string> CachedTexturePathMap { get; private set; }

    private const string ThumbnailZip = "thumbnails.zip";
    private const string ThumbnailInfosOld = "thumbnailLastUpdate.txt";
    private const string ThumbnailInfos = "thumbnailInfos.json";
    private string _path;
    private byte[] bytes;

    private DirectoryInfo dir;
    private FileStream fileStream;
    private StreamReader streamReader;
    private UnityWebRequest request;
    private FastZip zip;
    private Thread _unzippingThread;
    private FileInfo[] ActualFiles;
    private long ActualSize;
    private long UnzippedSize;
    private long StoredSize;
    private long JSONSize;
    private long StoredTimestamp;
    private long JSONtimestamp;
    private JsonData storedInfos;
    private JsonData data;
    private JsonData objectList;
    private JsonData taxonomyObject;
    private ShapeNetModel shapeNetModel;
    private ShapeNetTexture shapeNetTexture;
    public string InitStatus;
    private static Queue<TextureRequest> TextureQueue;

    private string taxonomyName;
    private List<string> keys;

    public struct TextureRequest
    {

        public string ID { get; private set; }
        public OnObjectLoaded Event { get; private set; }
        public TextureRequest(string id, OnObjectLoaded onLoaded)
        {
            ID = id;
            Event = onLoaded;
        }
    }

    public delegate void OnThumbnailLoaded();
    public delegate void OnObjectLoaded(string filePath);

    IEnumerator Start()
    {
        Name = "ShapeNet";
        OnSetupBrowser = SetupBrowser;
        TextureQueue = new Queue<TextureRequest>();

        ShapeNetModels = new Dictionary<string, ShapeNetModel>();
        ModelTaxonomies = new Dictionary<string, ShapeNetTaxonomyEntry>();
        ModelMainCategories = new Dictionary<string, CheckboxStatus>();
        ModelSubCategories = new Dictionary<string, CheckboxStatus>();
        ObjectSubCategoryMap = new Dictionary<string, string>();
        _objectListLoaded = false;
        _objectTaxonomyLoaded = false;
        _objectThumbnailsActualized = false;
        _objectListError = null;
        _objectTaxonomyError = null;
        _objectThumbnailError = null;

        ShapeNetTextures = new Dictionary<string, ShapeNetTexture>();
        TextureTaxonomies = new Dictionary<string, ShapeNetTaxonomyEntry>();
        TextureMainCategories = new Dictionary<string, CheckboxStatus>();
        TextureSubCategories = new Dictionary<string, CheckboxStatus>();
        TextureSubCategoryMap = new Dictionary<string, string>();
        _textureListLoaded = false;
        _textureListError = null;
        _textureTaxonomyLoaded = false;
        _textureTaxonomyError = null;

        InitializeCache();

        if (!_objectListLoaded)
        {
            InitStatus = "Loading Shapenet-Model-List...";
            yield return StartCoroutine(LoadModelList());
            if (_objectListError == null) InitStatus = "ShapeNet objects loaded.";
            else InitStatus = "ShapeNet objects cannot be loaded: " + _objectListError;
            _objectListLoaded = _objectListError == null;
        }
        if (!_objectTaxonomyLoaded)
        {
            InitStatus = "Loading Shapenet-Model-Taxonomies...";
            yield return StartCoroutine(LoadObjectTaxonomy());
            if (_objectTaxonomyError == null) InitStatus = "ShapeNet object-taxonomy loaded.";
            else InitStatus = "ShapeNet object-taxonomy cannot be loaded: " + _objectTaxonomyError;
            _objectTaxonomyLoaded = _objectTaxonomyError == null;
        }
        if (!_objectThumbnailsActualized)
        {
            InitStatus = "Loading Shapenet-Model-Thumbnails...";
            yield return StartCoroutine(CheckThumbnails(CACHED_OBJECT_DIR, CACHED_OBJECT_THUMBNAILS, CACHED_OBJECT_FILES, OBJECT_THUMBNAIL_INFO, OBJECT_THUMBNAILS, "Object"));
            if (_objectThumbnailError == null) InitStatus = "ShapeNet object-thumbnails actualized.";
            else InitStatus = "ShapeNet object-thumbnails cannot be actualized: " + _objectThumbnailError;
            _objectThumbnailsActualized = _objectThumbnailError == null;
        }
        if (!_textureListLoaded)
        {
            InitStatus = "Loading Shapenet-Texture-List...";
            yield return StartCoroutine(LoadTextureList());
            if (_textureListError == null) InitStatus = "ShapeNet textures loaded.";
            else InitStatus = "ShapeNet textures cannot be loaded: " + _textureListError;
            _textureListLoaded = _textureListError == null;
        }
        if (!_textureTaxonomyLoaded)
        {
            InitStatus = "Loading Shapenet-Texture-Taxonomies...";
            yield return StartCoroutine(LoadTextureTaxonomy());
            if (_textureTaxonomyError == null) InitStatus = "ShapeNet texture-taxonomy loaded.";
            else InitStatus = "ShapeNet texture-taxonomy cannot be loaded: " + _textureTaxonomyError;
            _textureTaxonomyLoaded = _textureTaxonomyError == null;
        }
    }

    protected override IEnumerator InitializeInternal()
    {
        Name = "ShapeNet";
        yield break;
    }

    private void InitializeCache()
    {
        InitStatus = "Initializing cached model map...";
        _path = UserFolder + CACHED_OBJECT_FILES;
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);
        CachedObjectPathMap = new Dictionary<string, string>();
        string[] paths = Directory.GetDirectories(_path);
        for (int i = 0; i < paths.Length; i++)
        {
            dir = new DirectoryInfo(paths[i]);
            CachedObjectPathMap.Add(dir.Name, dir.FullName);
        }

        InitStatus = "Initializing cached texture map...";
        _path = UserFolder + CACHED_TEXTURE_FILES;
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);
        CachedTexturePathMap = new Dictionary<string, string>();
        paths = Directory.GetDirectories(_path);
        for (int i = 0; i < paths.Length; i++)
        {
            dir = new DirectoryInfo(paths[i]);
            CachedTexturePathMap.Add(dir.Name, dir.FullName);
        }
    }

    private IEnumerator LoadModelList()
    {
        request = UnityWebRequest.Get(MODELS);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            _objectListError = request.error;
        }
        else
        {
            data = JsonMapper.ToObject(request.downloadHandler.text);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("ShapeNetObj"))
            {
                _objectListError = "Downloading object list failed.";
                yield break;
            }
            objectList = data["ShapeNetObj"];
            for (int i = 0; i < objectList.Count; i++)
            {
                if (!objectList[i].Keys.Contains("id"))
                    Debug.Log("Object without id.");
                else
                {
                    shapeNetModel = new ShapeNetModel(objectList[i]);
                    ShapeNetModels.Add((string)shapeNetModel.ID, shapeNetModel);
                }
            }
        }
    }

    private IEnumerator LoadObjectTaxonomy()
    {
        request = UnityWebRequest.Get(OBJECT_TAXONOMY);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            _objectTaxonomyError = request.error;
        else
        {
            data = JsonMapper.ToObject(request.downloadHandler.text);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("taxonomy"))
            {
                _objectTaxonomyError = "Downloading object taxonomy failed.";
                yield break;
            }
            objectList = data["taxonomy"];
            for (int i = 0; i < objectList.Count; i++)
            {
                taxonomyObject = objectList[i];
                keys = new List<string>(taxonomyObject.Keys);
                taxonomyName = keys[0];
                if (!ModelMainCategories.ContainsKey(taxonomyName))
                {
                    ModelTaxonomies.Add(taxonomyName, new ShapeNetTaxonomyEntry(taxonomyName, ShapeNetTaxonomyEntry.TaxonomyType.Object, taxonomyObject[taxonomyName], this));
                    ModelMainCategories.Add(taxonomyName, CheckboxStatus.AllChecked);
                }
            }
        }
    }

    private IEnumerator LoadTextureTaxonomy()
    {
        request = UnityWebRequest.Get(TEXTURE_TAXONOMY);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            _textureTaxonomyError = request.error;
        else
        {
            data = JsonMapper.ToObject(request.downloadHandler.text);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("taxonomy"))
            {
                _textureTaxonomyError = "Downloading texture taxonomy failed.";
                yield break;
            }
            objectList = data["taxonomy"];
            for (int i = 0; i < objectList.Count; i++)
            {
                taxonomyObject = objectList[i];
                keys = new List<string>(taxonomyObject.Keys);
                taxonomyName = keys[0];
                if (!TextureMainCategories.ContainsKey(taxonomyName))
                {
                    TextureTaxonomies.Add(taxonomyName, new ShapeNetTaxonomyEntry(taxonomyName, ShapeNetTaxonomyEntry.TaxonomyType.Texture, taxonomyObject[taxonomyName], this));
                    TextureMainCategories.Add(taxonomyName, CheckboxStatus.AllChecked);
                }
            }

        }
    }

    private IEnumerator LoadTextureList()
    {
        request = UnityWebRequest.Get(TEXTURES);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            _textureListError = request.error;
        else
        {
            data = JsonMapper.ToObject(request.downloadHandler.text);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("Textures"))
            {
                _textureListError = "Downloading texture list failed.";
                yield break;
            }
            objectList = data["Textures"];
            for (int i = 0; i < objectList.Count; i++)
            {
                if (!objectList[i].Keys.Contains("id"))
                    Debug.Log("Object without id.");
                else
                {
                    shapeNetTexture = new ShapeNetTexture(objectList[i]);
                    ShapeNetTextures.Add((string)shapeNetTexture.ID, shapeNetTexture);
                }

            }
        }
    }

    public static IEnumerator LoadThumbnail(ShapeNetObject sObj, OnThumbnailLoaded onThumbnail)
    {
        string url = "file://" + UserFolder;
        if (sObj is ShapeNetModel) url += CACHED_OBJECT_THUMBNAILS + sObj.ID + "-7.png";
        if (sObj is ShapeNetTexture) url += CACHED_TEXTURE_THUMBNAILS + ((ShapeNetTexture)sObj).ThumbnailFileName;
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.error == null)
        {
            sObj.Thumbnail = DownloadHandlerTexture.GetContent(request);
            onThumbnail?.Invoke();
        }
    }

    private IEnumerator CheckThumbnails(string cacheFolder, string thumbnailCacheFolder, string dataCacheFolder, string infoURL, string thumbnailURL, string thumbnailType)
    {
        _path = UserFolder + thumbnailCacheFolder;
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);

        bool update = false;
        request = UnityWebRequest.Get(infoURL);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            _objectThumbnailError = request.error;
        else
        {

            // getting the timestamp of thumbnails from server
            data = JsonMapper.ToObject(request.downloadHandler.text);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("timestamp") || !data.Keys.Contains("size"))
            {
                _objectThumbnailError = "Downloading " + thumbnailType.ToLower() + " timestamp failed.";
                yield break;
            }

            JSONtimestamp = long.Parse(data["timestamp"].ToString());
            JSONSize = long.Parse(data["size"].ToString());

            // get rid of deprecated file
            _path = UserFolder + cacheFolder + ThumbnailInfosOld;
            if (File.Exists(_path))
                File.Delete(_path);
            // creating the timestamp file if needed, otherwise comparing the timestamps
            _path = UserFolder + cacheFolder + ThumbnailInfos;
            if (!File.Exists(_path))
            {
                Debug.Log(thumbnailType + " timestamp-file missing. Updating " + thumbnailType.ToLower() + " thumbnails...");
                fileStream = new FileStream(_path, FileMode.Create);
                update = true;
            }
            else
            {
                try
                {
                    fileStream = new FileStream(_path, FileMode.Open);
                    streamReader = new StreamReader(fileStream);
                    storedInfos = JsonMapper.ToObject(streamReader.ReadToEnd());
                    streamReader.Close();
                    fileStream.Close();
                    ActualSize = 0;
                    ActualFiles = new DirectoryInfo(UserFolder + thumbnailCacheFolder).GetFiles();
                    for (int i = 0; i < ActualFiles.Length; i++)
                        ActualSize += ActualFiles[i].Length;
                    update = !long.TryParse(storedInfos["timestamp"].ToString(), out StoredTimestamp) || JSONtimestamp != StoredTimestamp ||
                             !long.TryParse(storedInfos["size"].ToString(), out StoredSize) || JSONSize != StoredSize ||
                             !long.TryParse(storedInfos["unzippedSize"].ToString(), out UnzippedSize) || UnzippedSize != ActualSize;

                    if (update) Debug.Log(thumbnailType + " thumbnails out of date. Updating " + thumbnailType.ToLower() + " thumbnails...");
                    else Debug.Log(thumbnailType + " thumbnails up-to-date.");
                }
                catch (Exception e)
                {
                    Debug.Log("Could not parse Thumbnails. Updating " + thumbnailType.ToLower() + " thumbnails...");
                    fileStream = new FileStream(_path, FileMode.Create);
                    update = true;
                }
            }

            if (update)
            {
                _path = UserFolder + thumbnailCacheFolder;
                request = UnityWebRequest.Get(thumbnailURL);
                request.SendWebRequest();
                while (!request.isDone)
                {
                    InitStatus = "Downloading " + thumbnailType.ToLower() + " thumbnails:\n" +
                                     (int)(request.downloadedBytes / Mathf.Pow(10, 6) * 100) / 100f + " MB of " +
                                     (int)(JSONSize / Mathf.Pow(10, 6) * 100) / 100f + " MB";
                    yield return null;
                }
                InitStatus = "Downloading " + thumbnailType.ToLower() + " thumbnails:\n" +
                                     (int)(request.downloadedBytes / Mathf.Pow(10, 6) * 100) / 100f + " MB of " +
                                     (int)(JSONSize / Mathf.Pow(10, 6) * 100) / 100f + " MB";
                if (request.isNetworkError || request.isHttpError)
                    _objectThumbnailError = request.error;
                else
                {
                    InitStatus = "Unzipping " + thumbnailType.ToLower() + " thumbnails...";
                    fileStream = new FileStream(_path + ThumbnailZip, FileMode.Create);
                    fileStream.Write(request.downloadHandler.data, 0, request.downloadHandler.data.Length);
                    fileStream.Close();
                    _unzippingThread = new Thread(() => { UnzipFile(_path + ThumbnailZip, _path); });
                    _unzippingThread.Start();
                    while (_unzippingThread.IsAlive)
                        yield return null;
                    File.Delete(_path + ThumbnailZip);
                }
                ActualSize = 0;
                ActualFiles = new DirectoryInfo(UserFolder + thumbnailCacheFolder).GetFiles();
                for (int i = 0; i < ActualFiles.Length; i++)
                    ActualSize += ActualFiles[i].Length;
                storedInfos = new JsonData();
                storedInfos["timestamp"] = data["timestamp"];
                storedInfos["size"] = data["size"];
                storedInfos["unzippedSize"] = ActualSize;
                bytes = System.Text.Encoding.UTF8.GetBytes(storedInfos.ToJson());
                _path = UserFolder + cacheFolder + ThumbnailInfos;
                fileStream = new FileStream(_path, FileMode.Open);
                fileStream.SetLength(0);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Close();
            }

            if (streamReader != null) streamReader.Close();
            if (fileStream != null) fileStream.Close();
        }

    }

    private void UnzipFile(string filePath, string targetDir)
    {
        FastZip zip = new FastZip();
        zip.ExtractZip(filePath, targetDir, null);
    }

    public IEnumerator GetModel(string id, OnObjectLoaded onLoaded)
    {
        _path = UserFolder + CACHED_OBJECT_FILES;

        if (CachedObjectPathMap.ContainsKey(id))
        {
            onLoaded(CachedObjectPathMap[id]);
            Debug.Log("Searched model was cached.");
            yield break;

        }

        Debug.Log("Searched model was not cached, downloading it...");
        request = UnityWebRequest.Get(GET_OBJECT_ID + id);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
            Debug.Log(request.error);
        else
        {
            if (!Directory.Exists(_path + id))
                Directory.CreateDirectory(_path + id);
            string zipFile = _path + id + "\\" + id + ".zip";
            fileStream = new FileStream(zipFile, FileMode.Create);
            fileStream.Write(request.downloadHandler.data, 0, request.downloadHandler.data.Length);
            fileStream.Close();
            _unzippingThread = new Thread(() => { UnzipFile(zipFile, _path + id); });
            _unzippingThread.Start();
            while (_unzippingThread.IsAlive)
                yield return null;
            try
            {
                CachedObjectPathMap.Add(id, _path + id);
            }
            catch
            {

            }
            //File.Delete(zipFile);
            onLoaded(CachedObjectPathMap[id]);
        }
    }

    public static void RequestTexture(string id, OnObjectLoaded onLoaded)
    {
        TextureRequest req = new TextureRequest(id, onLoaded);
        Debug.Log("Enqueued: " + id);
        TextureQueue.Enqueue(req);
    }

    private IEnumerator SetupBrowser(DataBrowser browser)
    {
        // ============================= FILTER PANEL SETUP ============================

        // Define filter update event
        browser.FilterPanel.FilterUpdater = () =>
        {
            ResourceData actualSpace = (ResourceData)browser.LastBrowserStateMap[Name];
            for (int i = 0; i < browser.FilterPanel.Checkboxes.Length; i++)
            {
                browser.FilterPanel.Checkboxes[i].transform.parent.GetComponent<DataFilter>().gameObject.SetActive((browser.FilterPanel.TypePointer + i) < browser.FilterPanel.TypeList.Count);
                if (browser.FilterPanel.Checkboxes[i].gameObject.activeInHierarchy)
                {
                    string label = browser.FilterPanel.TypeList[browser.FilterPanel.TypePointer + i];
                    browser.FilterPanel.Checkboxes[i].transform.parent.GetComponent<DataFilter>().ButtonValue = label;
                    browser.FilterPanel.Checkboxes[i].transform.parent.GetComponent<DataFilter>().Status = browser.FilterPanel.Types[browser.FilterPanel.TypeList[browser.FilterPanel.TypePointer + i]];
                    bool hasSubcategories = !browser.FilterPanel.ShowingSubTypes &&
                                            ((actualSpace.Name.Equals(FORMATTED_MODEL_NAME) && ModelTaxonomies[label].SubCategories.Count > 0) ||
                                             (actualSpace.Name.Equals(FORMATTED_TEXTURE_NAME) && TextureTaxonomies[label].SubCategories.Count > 0));
                    browser.FilterPanel.Openers[i].gameObject.SetActive(hasSubcategories);
                }
            }
        };
        // Define event for subcategory opener button        
        for (int i = 0; i < browser.FilterPanel.Openers.Length; i++)
        {
            Button opener = browser.FilterPanel.Openers[i];
            Button cb = browser.FilterPanel.Checkboxes[i];
            opener.onClick.AddListener(() =>
            {
                browser.FilterPanel.ShowingSubTypes = true;
                ResourceData actualSpace = (ResourceData)browser.LastBrowserStateMap[Name];
                Dictionary<string, CheckboxStatus> filters = new Dictionary<string, CheckboxStatus>();
                if (actualSpace.Name.Equals(FORMATTED_MODEL_NAME))
                {
                    foreach (string subCategorie in ModelTaxonomies[(string)cb.transform.parent.GetComponent<DataFilter>().ButtonValue].SubCategories)
                        filters.Add(subCategorie, ModelSubCategories[subCategorie]);
                    browser.FilterPanel.Init(SUB_CATEGORIES, filters);
                }
                else
                {
                    foreach (string subCategorie in TextureTaxonomies[(string)cb.transform.parent.GetComponent<DataFilter>().ButtonValue].SubCategories)
                        filters.Add(subCategorie, TextureSubCategories[subCategorie]);
                    browser.FilterPanel.Init(SUB_CATEGORIES, filters);
                }
            });
        }

        // Define event for back-button on subcategory page
        browser.FilterPanel.Back.onClick.AddListener(() =>
        {
            SetupFilterPanel(browser, ((ResourceData)browser.LastBrowserStateMap[Name]).Name, false);
        });

        // Set event for changing checkboxes
        browser.FilterPanel.CheckboxUpdater = (type, status) =>
        {
            ResourceData actualSpace = (ResourceData)browser.LastBrowserStateMap[Name];
            string _mainCat; int checkedSubCategories; CheckboxStatus _mainCatStatus;
            if (browser.FilterPanel.ShowingSubTypes)
            {
                if (actualSpace.Name.Equals(FORMATTED_MODEL_NAME))
                {
                    ModelSubCategories[type] = status;
                    _mainCat = ObjectSubCategoryMap[type];
                    checkedSubCategories = 0;
                    foreach (string subCat in ModelTaxonomies[_mainCat].SubCategories)
                        if (ModelSubCategories[subCat] == CheckboxStatus.AllChecked)
                            checkedSubCategories += 1;
                    _mainCatStatus = (checkedSubCategories == 0) ? CheckboxStatus.NoneChecked :
                                     (checkedSubCategories == ModelTaxonomies[_mainCat].SubCategories.Count) ?
                                     CheckboxStatus.AllChecked : CheckboxStatus.PartsChecked;
                    ModelMainCategories[_mainCat] = _mainCatStatus;
                }
                else
                {
                    TextureSubCategories[type] = status;
                    _mainCat = TextureSubCategoryMap[type];
                    checkedSubCategories = 0;
                    foreach (string subCat in TextureTaxonomies[_mainCat].SubCategories)
                        if (TextureSubCategories[subCat] == CheckboxStatus.AllChecked)
                            checkedSubCategories += 1;
                    _mainCatStatus = (checkedSubCategories == 0) ? CheckboxStatus.NoneChecked :
                                     (checkedSubCategories == TextureTaxonomies[_mainCat].SubCategories.Count) ?
                                     CheckboxStatus.AllChecked : CheckboxStatus.PartsChecked;
                    TextureMainCategories[_mainCat] = _mainCatStatus;
                }
            }
            else
            {
                if (actualSpace.Name.Equals(FORMATTED_MODEL_NAME))
                {
                    ModelMainCategories[type] = status;
                    if (ModelTaxonomies.ContainsKey(type))
                    {
                        foreach (string sub in ModelTaxonomies[type].SubCategories)
                            ModelSubCategories[sub] = status;
                    }
                }
                else
                {
                    TextureMainCategories[type] = status;
                    if (TextureTaxonomies.ContainsKey(type))
                    {
                        foreach (string sub in TextureTaxonomies[type].SubCategories)
                            TextureSubCategories[sub] = status;
                    }
                }
            }
            browser.FilterPanel.Types[type] = status;
        };

        // ============================= DATA PANEL SETUP ============================

        //Parent button functionality
        browser.DataPanel.ParentDir.onClick.AddListener(() =>
        {
            ResourceData actualDir = null;
            if (browser.LastBrowserStateMap.ContainsKey(Name) && browser.LastBrowserStateMap[Name] != null) actualDir = (ResourceData)browser.LastBrowserStateMap[Name];
            if (actualDir == null) return;
            browser.SetActualState(Name, null);
            SetupMainMenu(browser);
        });

        // Define browser update event
        browser.BrowserUpdater = () =>
        {
            ResourceData actualDir = null;
            if (browser.LastBrowserStateMap.ContainsKey(Name) && browser.LastBrowserStateMap[Name] != null) actualDir = (ResourceData)browser.LastBrowserStateMap[Name];
            if (actualDir == null) return;

            if (actualDir.Name.Equals(FORMATTED_MODEL_NAME))
            {
                StartCoroutine(ObjectSearchRequest(browser.SearchPanel.SearchPattern.ToLower(), browser, actualDir.Name));
            }
            else if (actualDir.Name.Equals(FORMATTED_TEXTURE_NAME))
                browser.DataPanel.Init(actualDir.Name, GetObjectList(ShapeNetTextures.Values, browser.SearchPanel.SearchPattern.ToLower()));

            browser.DataPanel.ParentDir.interactable = true;
            browser.DataPanel.Root.gameObject.SetActive(false);
        };
        // ============================= LOADING LAST STATE ============================ 

        if (!browser.LastBrowserStateMap.ContainsKey(Name) || browser.LastBrowserStateMap[Name] == null)
        {
            SetupMainMenu(browser);
        }
        yield return null;
    }

    private void SetupMainMenu(DataBrowser browser)
    {
        browser.FilterPanel.showFilterPanelItems(false);
        browser.DataPanel.Init(Name, GetShapeNetSpaces());
        browser.DataPanel.Root.gameObject.SetActive(false);
        browser.DataPanel.ParentDir.interactable = false;
        foreach (DataContainer dc in browser.DataPanel.DataContainers)
        {
            dc.GetComponent<Button>().onClick.RemoveAllListeners();
            dc.GetComponent<Button>().onClick.AddListener(() =>
            {
                ResourceData resource = (ResourceData)dc.Resource;
                if (resource != null && resource.Type != ResourceData.DataType.File)
                {
                    foreach (DataContainer dCont in browser.DataPanel.DataContainers)
                        dCont.GetComponent<Button>().onClick.RemoveAllListeners();
                    browser.SetActualState(Name, resource);
                    if (resource.Name.Equals(FORMATTED_MODEL_NAME))
                    {
                        browser.FilterPanel.showFilterPanelItems(true);
                        browser.DataPanel.Init(resource.Name, GetObjectList(ShapeNetModels.Values, browser.SearchPanel.SearchPattern.ToLower()));
                    }
                    else if (resource.Name.Equals(FORMATTED_TEXTURE_NAME))
                    {
                        browser.FilterPanel.showFilterPanelItems(true);
                        browser.DataPanel.Init(resource.Name, GetObjectList(ShapeNetTextures.Values, browser.SearchPanel.SearchPattern.ToLower()));
                    }
                    SetupFilterPanel(browser, resource.Name, false);
                    browser.DataPanel.Root.gameObject.SetActive(false);
                    browser.DataPanel.ParentDir.interactable = true;
                }
            });
        }
    }

    private void SetupFilterPanel(DataBrowser browser, string space, bool showingSubTypes)
    {
        browser.FilterPanel.ShowingSubTypes = showingSubTypes;
        if (space.Equals(FORMATTED_MODEL_NAME))
            browser.FilterPanel.Init(MAIN_CATEGORIES, ModelMainCategories);
        else
            browser.FilterPanel.Init(MAIN_CATEGORIES, TextureMainCategories);
    }

    private IEnumerator ObjectSearchRequest(string search, DataBrowser browser, string name)
    {
        Debug.Log("Searchrequest: " + search);
        if (search.Equals(""))
        {
            browser.DataPanel.Init(name, GetObjectList(ShapeNetModels.Values, ""));
        }
        else
        {
            List<string> foundObjects = new List<string>();
            request = UnityWebRequest.Get(SEARCH_OBJECTS + search);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
                _objectTaxonomyError = request.error;
            else
            {
                data = JsonMapper.ToObject(request.downloadHandler.text);
                Debug.Log("Search return: " + data);
                if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                    !data.Keys.Contains("term") || !data.Keys.Contains("results"))
                {
                    _objectTaxonomyError = "Downloading object taxonomy failed.";
                    yield break;
                }

                foreach (JsonData obj in data["results"])
                {
                    foundObjects.Add(obj["id"].ToString());
                }
                Debug.Log("Search Final: " + foundObjects);
            }

            if (foundObjects != null)
            {
                browser.DataPanel.Init(name, GetObjectList(ShapeNetModels, foundObjects));
            }
        }
    }

    private List<ShapeNetObject> GetObjectList(IEnumerable<ShapeNetObject> objects, string pattern)
    {
        List<ShapeNetObject> res = new List<ShapeNetObject>();

        foreach (ShapeNetObject obj in objects)
            if (CheckPatternMatch(obj, pattern) && ProofCategories(obj))
                res.Add(obj);
        return res;
    }

    private List<ShapeNetObject> GetObjectList(Dictionary<string, ShapeNetModel> ShapeNetModels, List<string> filterlist)
    {
        List<ShapeNetObject> res = new List<ShapeNetObject>();

        foreach (string obj in filterlist)
            if (ShapeNetModels.ContainsKey(obj))
                res.Add(ShapeNetModels[obj]);
        return res;
    }

    private List<ResourceData> GetShapeNetSpaces()
    {
        List<ResourceData> spaces = new List<ResourceData>();
        spaces.Add(new ResourceData(FORMATTED_MODEL_NAME, MODELS, null, "", DateTime.MinValue, DateTime.MinValue, Data.SourceType.Remote));
        spaces.Add(new ResourceData(FORMATTED_TEXTURE_NAME, TEXTURES, null, "", DateTime.MinValue, DateTime.MinValue, Data.SourceType.Remote));
        return spaces;
    }

    private bool CheckPatternMatch(ShapeNetObject snO, string pattern)
    {
        if (snO is ShapeNetModel)
        {
            ShapeNetModel snM = (ShapeNetModel)snO;
            if (pattern == "") return true;
            if (snM.Name.ToLower().Contains(pattern)) return true;
            if (snM.Lemmas.Contains(pattern)) return true;
            return false;
        }
        else if (snO is ShapeNetTexture)
        {
            ShapeNetTexture snT = (ShapeNetTexture)snO;
            if (pattern == "") return true;
            if (snT.Name.ToLower().Contains(pattern)) return true;
            return false;
        }
        else return false;
    }

    private bool ProofCategories(ShapeNetObject snObj)
    {
        foreach (string category in snObj.Categories)
        {

            if (snObj is ShapeNetModel)
            {
                if (!ModelSubCategories.ContainsKey(category)) continue;
                if (ModelSubCategories[category] == CheckboxStatus.AllChecked)
                    return true;
            }
            if (snObj is ShapeNetTexture)
            {
                if (!TextureSubCategories.ContainsKey(category)) continue;
                if (TextureSubCategories[category] == CheckboxStatus.AllChecked)
                    return true;
            }
        }
        return false;
    }
}
