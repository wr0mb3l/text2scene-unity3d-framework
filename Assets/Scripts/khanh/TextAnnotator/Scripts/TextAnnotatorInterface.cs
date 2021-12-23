using WebSocketSharp;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using LitJson;
using System;
//using Valve.VR;
//using DT = IsoSpatialEntity.DimensionType;
//using CT = IsoLocationPlace.ContinentType;
//using CTV = IsoLocationPlace.CTV;
//using FT = IsoSpatialEntity.FormType;
//using Text2Scene;

/// <summary>
/// Diese Klasse stellt eine Websocket-Verbindung zum TextAnnotator-Service her und wickelt die Kommunikation ab.
/// </summary>
public class TextAnnotatorInterface : Interface
{


    /// <summary>
    /// Eine Liste mit allen Named Entity Typen.
    /// </summary>
    public static List<string> NamedEntityTypes;
    /// <summary>
    /// Das Wörterbuch der Named Entity Typen und mit zugehöriger Farbe.
    /// </summary>
    public static Dictionary<string, Color> NamedEntityColorMap;

    /// <summary>
    /// Die Adresse des TextAnnotator-Service.
    /// </summary>
    public string ANNOTATION_SERVICE { get; private set; } = "textannotator.texttechnologylab.org/uima";
    //public string ANNOTATION_SERVICE { get; private set; } = "localhost:4567/uima";
    //public string ANNOTATION_SERVICE { get; private set; } = "192.168.0.1:8080/uima";

    /// <summary>
    /// Gibt an, ob das Client authorisiert wurde.
    /// </summary>
    public bool Authorized { get; private set; }

    /// <summary>
    /// In dieser Variable werden relevanten Daten der Server-Antwort abgespeichert.
    /// </summary>
    public static JsonData Response { get; private set; }

    /// <summary>
    /// In diese Warteschalnge werden die vom Server erhaltene Änderungsbefehle eingereiht.
    /// </summary>
    private static Queue<JsonData> ChangeQueue;

    /// <summary>
    /// In diese Warteschalnge werden die vom Server erhaltene Änderungsbefehle eingereiht.
    /// </summary>
    private static Queue<TextAnnotatorDataContainer> DocumentViewQueue;
    private static Dictionary<string, string> FingerprintMap;

    /// <summary>
    /// Das Wörterbuch der in dieser Session geöffneten Dokumente als (ID, Dokumentdaten)-Paare.
    /// </summary>
    public Dictionary<int, TextAnnotatorDataContainer> Document_Map = new Dictionary<int, TextAnnotatorDataContainer>();

    /// <summary>
    /// Das aktuell geöffnete Dokument.
    /// </summary>
    public TextAnnotatorDataContainer ActualDocument;

    /// <summary>
    /// Die enum-Typen für die TextAnnotator-Befehle.
    /// </summary>
    public enum CommandType
    {
        session, create_db_cas, change_cas, open_tool,
        open_cas, close_cas, save_cas, download_cas,
        load_types, work_batch, undo, redo, convert
    };

    /// <summary>Ein delegate-Funktion die nach Änderungen am Dokument aufgerufen wird.</summary>
    /// <param name="deletedData">Die Menge aller Textelement-IDs, die gelöscht werden sollen.</param>
    /// <param name="addedData">Die Menge aller Textelemente, die dem Dokument hinzugefügt werden.</param>
    //public delegate void OnDocumentChange(HashSet<string> deletedData, HashSet<AnnotationBase> addedData);

    /// <summary>
    /// Die delegate-Methode zum Überschreiben bei Änderungen am Dokument.
    /// </summary>
    /*public OnDocumentChange OnChanges;

    public delegate void OnElementCreated(AnnotationBase created);
    public OnElementCreated OnElemCreated;

    public delegate void OnElementsCreated(HashSet<AnnotationBase> createdElements);
    public OnElementsCreated OnElemsCreated;

    public delegate void OnElementRemoved();
    public Dictionary<int, OnElementRemoved> RemoveEventMap = new Dictionary<int, OnElementRemoved>();

    public delegate void OnElementChanged(AnnotationBase changed);
    public Dictionary<int, OnElementChanged> ChangeEventMap = new Dictionary<int, OnElementChanged>();*/

    public WebSocket Client { get; private set; }
    private string _sessionID;
    private Regex Regex;
    private Coroutine ViewGetter;

    private ResourceManagerInterface _resourceManager;

    public ResourceManagerInterface ResourceManager
    {
        get
        {
            //if (_resourceManager == null)
            //    _resourceManager = SceneController.GetInterface<ResourceManagerInterface>();
            return _resourceManager;
        }
        set
        {
            _resourceManager = value;
        }
    }

    private void WriteToText(string json)
    {
        var fileName = "json.txt";
        var sr = File.CreateText(fileName);
        sr.WriteLine(json);
        sr.Close();
    }
    protected override IEnumerator InitializeInternal()
    {
        Name = "TextAnnotator";
        Client = new WebSocket("ws://" + ANNOTATION_SERVICE);

        Client.OnMessage += (ss, ee) =>
        {
            Response = JsonMapper.ToObject(ee.Data);
            if (Response["cmd"].ToString().Equals("meta")) return;

            Debug.Log(Response.ToJson());
            //WriteToText(Response.ToJson());

            if (!Authorized && Response.Keys.Contains("cmd") && Response["cmd"].ToString().Equals("session"))
                Authorized = true;
                

            int id = int.Parse(Response["data"]["casId"].ToString());
            if (Response["cmd"].ToString().Equals("create_db_cas") ||
                    Response["cmd"].ToString().Equals("open_cas"))
            {
                ActualDocument = new TextAnnotatorDataContainer(Response);
                if (Document_Map.ContainsKey(id))
                    Document_Map[id] = ActualDocument;
                else
                    Document_Map.Add(id, ActualDocument);
                DocumentViewQueue.Enqueue(ActualDocument);
            }

            //if (Response["cmd"].ToString().Equals("change_cas") && Document_Map.ContainsKey(id))
            //    ChangeQueue.Enqueue(Response["data"]);

            /*if (Response["cmd"].ToString().Equals("open_tool"))
            {
                if (!Document_Map.ContainsKey(id))
                    DebugOnSocketTransfer("Document " + Response["data"]["casId"].ToString() + " that should be opened, was not found in the Document-Map");
                else if (!Response["data"].ContainsKey("toolElements"))
                    DebugOnSocketTransfer("No Toolelements found on opening a view of document " + Response["data"]["casId"].ToString());
                else
                    Document_Map[id].CreateDocument(Response["data"]["toolElements"]);


            }*/

        };
        Client.OnClose += (ss, ee) =>
        {
            if (ViewGetter != null) StopCoroutine(ViewGetter);
            ChangeQueue.Clear();
            DocumentViewQueue.Clear();
            FingerprintMap.Clear();
            ActualDocument = null;
            Document_Map.Clear();
            Debug.Log(ee.Code + " - " + ee.Reason);
        };
        ChangeQueue = new Queue<JsonData>();
        DocumentViewQueue = new Queue<TextAnnotatorDataContainer>();
        Regex = new Regex("[a-z][A-Z]");
        FingerprintMap = new Dictionary<string, string>();

        // TODO load named entities from online source
        //LoadNamedEntities();
        yield break;
    }

    public void CloseSocketConnection()
    {
        if (Client != null && Client.IsAlive)
            Client.Close();
    }

    Dictionary<string, string> viewMap; List<string> viewList, annoviews;
    private IEnumerator LoadDocumentViews(TextAnnotatorDataContainer doc)
    {
        if (doc.Json.ContainsKey("views"))
        {
            viewMap = new Dictionary<string, string>();
            viewList = new List<string>();
            annoviews = new List<string>();
            for (int i = 0; i < doc.Json["views"].Count; i++)
            {
                if (doc.Json["views"][i].ContainsKey("view"))
                {
                    if (doc.Json["views"][i]["view"].ToString().Contains("https://resources.hucompute.org/annoview/"))
                        annoviews.Add(doc.Json["views"][i]["view"].ToString());
                    else if (!doc.Json["views"][i]["view"].ToString().Contains("https://authority.hucompute.org/user/"))
                        viewMap.Add(doc.Json["views"][i]["view"].ToString(), doc.Json["views"][i]["view"].ToString());
                    viewList.Add(doc.Json["views"][i]["view"].ToString());
                }
            }

            bool usersGetted = false; int viewsGetted = 0;
            StartCoroutine(ResourceManager.GetUserList((JsonData response) =>
            {
                if (response.ContainsKey("success") && bool.Parse(response["success"].ToString()))
                {
                    Dictionary<string, string> userMap = new Dictionary<string, string>();
                    for (int i = 0; i < response["users"].Count; i++)
                        if (viewList.Contains(response["users"][i]["uri"].ToString()))
                            viewMap.Add(response["users"][i]["uri"].ToString(), response["users"][i]["description"].ToString());
                    usersGetted = true;
                }
            }));

            while (!usersGetted) yield return null;

            foreach (string view in annoviews)
            {
                StartCoroutine(ResourceManager.GetAnnoview(view, (JsonData response) =>
                {
                    if (response.ContainsKey("success") && bool.Parse(response["success"].ToString()))
                    {
                        if (response["annoview"].ContainsKey("name"))
                            viewMap.Add(response["annoview"]["uri"].ToString(), response["annoview"]["description"].ToString());
                    }
                    else
                        viewList.Remove(view);
                    viewsGetted += 1;
                }));
            }


            while (viewsGetted < annoviews.Count)
            {
                yield return null;
            }

            doc.Views = viewList;
            doc.ViewNameMap = viewMap;
        }
    }

    //private AnnotationBase _toRemove, _toChange, _parent;
    private int _id, _begin, _end, _parentNode;
    private bool _docUpdated;
    private TextAnnotatorDataContainer _editedDoc;
    private JsonData _updates, _features, _fingerprintObj;
    //private HashSet<string> _deletedData; private HashSet<AnnotationBase> _addedData;
    //private HashSet<AnnotationBase> _createdElements;
    private List<int> _childNodeList;
    public HashSet<int> MarkedForRemoving;
    /// <summary>Die Methode wird aufgerufen, wenn ein change_cas-Befehl empfangen wurde.</summary>
    /// <param name="jsonData">Der Änderungsbefehl.</param>
    /*private void Change(JsonData jsonData)
    {
        Debug.Log("Change Request: " + jsonData.ToJson());
        if (MarkedForRemoving == null) MarkedForRemoving = new HashSet<int>();
        else MarkedForRemoving.Clear();

        if (_deletedData == null) _deletedData = new HashSet<string>();
        else _deletedData.Clear();

        if (_addedData == null) _addedData = new HashSet<AnnotationBase>();
        else _addedData.Clear();

        if (_createdElements == null) _createdElements = new HashSet<AnnotationBase>();
        else _createdElements.Clear();

        _editedDoc = Document_Map[int.Parse(jsonData["casId"].ToString())];
        if (jsonData.ContainsKey("undo"))
            _editedDoc.Document.CanUndo = bool.Parse(jsonData["undo"].ToString());
        if (jsonData.ContainsKey("redo"))
            _editedDoc.Document.CanRedo = bool.Parse(jsonData["redo"].ToString());

        List<string> types = AnnotationTypes.SortTypes(new HashSet<string>(jsonData["updates"].Keys));

        foreach (string type in types)
        {
            // Get fingerprints
            if (type.Equals(AnnotationTypes.FINGERPRINT))
            {
                foreach (string key in jsonData["updates"][type].Keys)
                {
                    if (!jsonData["updates"][type][key].ContainsKey("features")) continue;
                    _fingerprintObj = jsonData["updates"][type][key]["features"];
                    FingerprintMap.Add(_fingerprintObj["reference"].ToString(), _fingerprintObj["user"].ToString());
                }
            }
            else
            {
                _updates = jsonData["updates"][type];
                foreach (string id in _updates.Keys)
                {
                    _id = int.Parse(id);
                    // updates
                    if (_updates[id].Keys.Contains("features"))
                    {
                        _features = _updates[id]["features"];
                        if (_editedDoc.Document.Text_ID_Map.ContainsKey(_id))
                        {
                            _toChange = _editedDoc.Document.Text_ID_Map[_id];
                            _docUpdated = true;

                            if (_toChange is IsoVector3) ExtractVector3(_id, _features, _editedDoc.Document, (IsoVector3)_toChange);

                            if (_toChange is IsoVector4) ExtractVector4(_id, _features, _editedDoc.Document, (IsoVector4)_toChange);

                            if (_toChange is IsoLocationPath) ExtractSpatialEntity(_id, _features, _editedDoc.Document, (IsoLocationPath)_toChange);

                            if (_toChange is IsoLocationPlace) ExtractSpatialEntity(_id, _features, _editedDoc.Document, (IsoLocationPlace)_toChange);

                            if (_toChange is IsoLocation) ExtractSpatialEntity(_id, _features, _editedDoc.Document, (IsoLocation)_toChange);

                            if (_toChange is IsoEventPath) ExtractSpatialEntity(_id, _features, _editedDoc.Document, (IsoEventPath)_toChange);

                            if (_toChange is IsoSpatialEntity) ExtractSpatialEntity(_id, _features, _editedDoc.Document, (IsoSpatialEntity)_toChange);

                            if (_toChange is QuickTreeNode) ExtractQuickTreeNode(_id, _features, (QuickTreeNode)_toChange);

                            if (_toChange is IsoQsLink) ExtractLink(_id, _features, _editedDoc.Document, (IsoQsLink)_toChange);

                            if (_toChange is IsoOLink) ExtractLink(_id, _features, _editedDoc.Document, (IsoOLink)_toChange);

                            if (_toChange is IsoMLink) ExtractLink(_id, _features, _editedDoc.Document, (IsoMLink)_toChange);

                            if (_toChange is IsoMetaLink) ExtractLink(_id, _features, _editedDoc.Document, (IsoMetaLink)_toChange);

                            if (_toChange is IsoSrLink) ExtractLink(_id, _features, _editedDoc.Document, (IsoSrLink)_toChange);

                            if (_toChange is IsoOLink) ExtractLink(_id, _features, _editedDoc.Document, (IsoOLink)_toChange);

                            //if (_toChange is IsoLink) ExtractLink(_id, _features, _editedDoc.Document, (IsoLink)_toChange);
                            //if (_toChange is IsoLink) Debug.LogError("Change IsoLink");

                            if (_toChange is IsoMeasure) ExtractSignal(_id, _features, _editedDoc.Document, (IsoMeasure)_toChange);

                            //if (_toChange is IsoSpatialSignal) ExtractSignal(_id, _features, _editedDoc.Document, (IsoSpatialSignal)_toChange);

                            //if (_toChange is IsoMotionSignal) ExtractSignal(_id, _features, _editedDoc.Document, (IsoMotionSignal)_toChange);

                            //if (_toChange is IsoSignal) ExtractSignal(_id, _features, _editedDoc.Document, (IsoSignal)_toChange);
                            //if (_toChange is IsoSignal) Debug.LogError("Change IsoSignal");

                            if (_toChange is IsoMotion) ExtractEvent(_id, _features, _editedDoc.Document, (IsoMotion)_toChange);

                            if (_toChange is IsoNonMotionEvent) ExtractEvent(_id, _features, _editedDoc.Document, (IsoNonMotionEvent)_toChange);

                            if (_toChange is IsoEvent) ExtractEvent(_id, _features, _editedDoc.Document, (IsoEvent)_toChange);

                            if (_toChange is IsoObjectAttribute) ExtractIsoObjectAttribute(_id, _features, _editedDoc.Document, (IsoObjectAttribute)_toChange);
                            
                            if (_toChange is IsoSRelation) ExtractSignal(_id, _features, _editedDoc.Document, (IsoSRelation)_toChange);

                            if (_toChange is IsoMRelation) ExtractSignal(_id, _features, _editedDoc.Document, (IsoMRelation)_toChange);

                            if (ChangeEventMap.ContainsKey(_id))
                            {
                                ChangeEventMap[_id]?.Invoke(_toChange);
                                ChangeEventMap.Remove(_id);
                            }
                        }
                        else
                        {
                            if (_features.ContainsKey("begin"))
                            {
                                _begin = int.Parse(_features["begin"].ToString());
                                _end = int.Parse(_features["end"].ToString());
                                _parent = _editedDoc.Document.GetParentOfType(type, _begin, _end);
                            }
                            if (type.Equals(AnnotationTypes.DOCUMENT))
                            {
                                Chapter chapter = new Chapter(_id, _begin, _end, _editedDoc.Document);
                                _editedDoc.Document.AddComponent(chapter);
                                _addedData.Add(chapter);
                                Debug.Log("ADDED: " + chapter.ToString());
                            }

                            if (type.Equals(AnnotationTypes.PARAGRAPH))
                            {
                                Paragraph paragraph = new Paragraph(_id, _begin, _end, (Chapter)_parent);
                                _parent.AddComponent(paragraph);
                                _addedData.Add(paragraph);
                                Debug.Log("ADDED: " + paragraph.ToString());
                            }

                            if (type.Equals(AnnotationTypes.SENTENCE))
                            {
                                Sentence sentence = new Sentence(_id, _begin, _end, (Paragraph)_parent);
                                _parent.AddComponent(sentence);
                                _addedData.Add(sentence);
                                Debug.Log("ADDED: " + sentence.ToString());
                            }

                            if (type.Equals(AnnotationTypes.TOKEN))
                            {
                                AnnotationToken token = new AnnotationToken(_id, _begin, _end, (Sentence)_parent);
                                _parent.AddComponent(token);
                                _addedData.Add(token);
                                Debug.Log("ADDED: " + token.ToString());
                            }

                            if (type.Equals(AnnotationTypes.QUICK_TREE_NODE))
                            {
                                QuickTreeNode quick = ExtractQuickTreeNode(_id, _features);
                                _addedData.Add(quick);
                                if (FingerprintMap.ContainsKey(id) &&
                                    FingerprintMap[id].Equals(SceneController.GetInterface<ResourceManagerInterface>().UserID))
                                {
                                    OnElemCreated?.Invoke(quick);
                                    OnElemCreated = null;
                                    _createdElements.Add(quick);
                                }
                            }
                                

                            //if (type.Equals(AnnotationTypes.NAMED_ENTITY))
                            //{
                            //    NamedEntity namedEntity = new NamedEntity(_id, _begin, _end, type, (Sentence)_parent);
                            //    _editedDoc.Document.AddTextElement(namedEntity);
                            //    namedEntity.Actualize3DObject();
                            //    //if (!UpdateQueue.Contains(namedEntity))
                            //    //    UpdateQueue.Enqueue(namedEntity);
                            //    _addedData.Add(namedEntity);
                            //    Debug.Log("ADDED: " + namedEntity.ToString());
                            //}


                            // ============================= TODO DDC Category ===================================
                            //if (AnnotationTypes.TypeClassTable[type] == typeof(DdcCategory))
                            //{

                            //    DdcCategory category = new DdcCategory(_parent, _id, _begin, _end, _updates[id]["features"]["value"].ToString(), _editedDoc.Document.BelongsToCity);
                            //    _editedDoc.Document.AddTextDataToMaps(category);
                            //    _addedData.Add(category);
                            //    Debug.Log("ADDED: " + category.ToString());
                            //}
                            // ============================= END DDC Category ====================================

                            // 3D-vectors
                            if (type.Equals(AnnotationTypes.VEC3))
                            {
                                //_addedData.Add(ExtractVector3(_id, _features, _editedDoc.Document));
                                IsoVector3 vec = ExtractVector3(_id, _features, _editedDoc.Document);
                                _addedData.Add(vec);
                                if (FingerprintMap.ContainsKey(id) &&
                                    FingerprintMap[id].Equals(SceneController.GetInterface<ResourceManagerInterface>().UserID))
                                {
                                    OnElemCreated?.Invoke(vec);
                                    OnElemCreated = null;
                                    _createdElements.Add(vec);
                                }
                            }


                            // 4D-vectors
                            if (type.Equals(AnnotationTypes.VEC4))
                            {
                                //_addedData.Add(ExtractVector4(_id, _features, _editedDoc.Document));
                                IsoVector4 vec = ExtractVector4(_id, _features, _editedDoc.Document);
                                _addedData.Add(vec);
                                if (FingerprintMap.ContainsKey(id) &&
                                    FingerprintMap[id].Equals(SceneController.GetInterface<ResourceManagerInterface>().UserID))
                                {
                                    OnElemCreated?.Invoke(vec);
                                    OnElemCreated = null;
                                    _createdElements.Add(vec);
                                }
                            }


                            // spatial entities
                            if (type.Equals(AnnotationTypes.SPATIAL_ENTITY) || type.Equals(AnnotationTypes.PATH) ||
                                type.Equals(AnnotationTypes.PLACE) || type.Equals(AnnotationTypes.EVENT_PATH) || 
                                type.Equals(AnnotationTypes.LOCATION))
                            {
                                IsoSpatialEntity spatialEntity;
                                Debug.Log("Spatial Entity Created");
                                if (type.Equals(AnnotationTypes.SPATIAL_ENTITY))
                                    spatialEntity = ExtractSpatialEntity<IsoSpatialEntity>(_id, _features, _editedDoc.Document);
                                else if (type.Equals(AnnotationTypes.PATH))
                                    spatialEntity = ExtractSpatialEntity<IsoLocationPath>(_id, _features, _editedDoc.Document);
                                else if (type.Equals(AnnotationTypes.PLACE))
                                    spatialEntity = ExtractSpatialEntity<IsoLocationPlace>(_id, _features, _editedDoc.Document);
                                else if (type.Equals(AnnotationTypes.LOCATION))
                                    spatialEntity = ExtractSpatialEntity<IsoLocation>(_id, _features, _editedDoc.Document);
                                else
                                    spatialEntity = ExtractSpatialEntity<IsoEventPath>(_id, _features, _editedDoc.Document);
                                _addedData.Add(spatialEntity);
                                if (FingerprintMap.ContainsKey(id) &&
                                    FingerprintMap[id].Equals(SceneController.GetInterface<ResourceManagerInterface>().UserID))
                                {
                                    OnElemCreated?.Invoke(spatialEntity);
                                    OnElemCreated = null;
                                    _createdElements.Add(spatialEntity);
                                }

                            }

                            // links
                            if (type.Equals(AnnotationTypes.QSLINK) || type.Equals(AnnotationTypes.OLINK) || type.Equals(AnnotationTypes.META_LINK) || type.Equals(AnnotationTypes.LINK))
                            {
                                IsoLink link = null;
                                if (type.Equals(AnnotationTypes.QSLINK))
                                    link = ExtractLink<IsoQsLink>(_id, _features, _editedDoc.Document);
                                else if (type.Equals(AnnotationTypes.OLINK))
                                    link = ExtractLink<IsoOLink>(_id, _features, _editedDoc.Document);
                                else if (type.Equals(AnnotationTypes.META_LINK))
                                    link = ExtractLink<IsoMetaLink>(_id, _features, _editedDoc.Document);
                                else link = ExtractLink<IsoLink>(_id, _features, _editedDoc.Document);
                                _addedData.Add(link);
                                if (FingerprintMap.ContainsKey(id) &&
                                    FingerprintMap[id].Equals(SceneController.GetInterface<ResourceManagerInterface>().UserID))
                                {
                                    OnElemCreated?.Invoke(link);
                                    OnElemCreated = null;
                                    _createdElements.Add(link);
                                }
                            }

                            // signals
                            if (type.Equals(AnnotationTypes.MEASURE) || type.Equals(AnnotationTypes.SPATIAL_SIGNAL) ||
                                type.Equals(AnnotationTypes.MOTION_SIGNAL) || type.Equals(AnnotationTypes.SIGNAL) ||
                                type.Equals(AnnotationTypes.SRELATION) || type.Equals(AnnotationTypes.MRELATION))
                            {
                                IsoSignal signal = null;
                                if (type.Equals(AnnotationTypes.MEASURE))
                                    signal = ExtractSignal<IsoMeasure>(_id, _features, _editedDoc.Document);
                                else if (type.Equals(AnnotationTypes.SPATIAL_SIGNAL))
                                    Debug.LogError("There should no longer be SPATIAL_SIGNAL");
                                //signal = ExtractSignal<IsoSpatialSignal>(_id, _features, _editedDoc.Document);
                                else if (type.Equals(AnnotationTypes.MOTION_SIGNAL))
                                    Debug.LogError("There should no longer be MOTION_SIGNAL");
                                //signal = ExtractSignal<IsoMotionSignal>(_id, _features, _editedDoc.Document);
                                else if (type.Equals(AnnotationTypes.SRELATION))
                                    signal = ExtractSignal<IsoSRelation>(_id, _features, _editedDoc.Document);
                                else if (type.Equals(AnnotationTypes.MRELATION))
                                    signal = ExtractSignal<IsoMRelation>(_id, _features, _editedDoc.Document);
                                else signal = ExtractSignal<IsoSignal>(_id, _features, _editedDoc.Document);

                                _addedData.Add(signal);
                                if (FingerprintMap.ContainsKey(id) &&
                                    FingerprintMap[id].Equals(SceneController.GetInterface<ResourceManagerInterface>().UserID))
                                {
                                    OnElemCreated?.Invoke(signal);
                                    OnElemCreated = null;
                                    _createdElements.Add(signal);
                                }
                            }

                            // events
                            if (type.Equals(AnnotationTypes.NON_MOTION_EVENT) || type.Equals(AnnotationTypes.MOTION) || type.Equals(AnnotationTypes.EVENT))
                            {
                                IsoEvent isoEvent;
                                if (type.Equals(AnnotationTypes.MOTION))
                                    isoEvent = ExtractEvent<IsoMotion>(_id, _features, _editedDoc.Document);
                                else if(type.Equals(AnnotationTypes.NON_MOTION_EVENT))
                                    isoEvent = ExtractEvent<IsoNonMotionEvent>(_id, _features, _editedDoc.Document);
                                else
                                    isoEvent = ExtractEvent<IsoEvent>(_id, _features, _editedDoc.Document);
                                _addedData.Add(isoEvent);
                                if (FingerprintMap.ContainsKey(id) &&
                                    FingerprintMap[id].Equals(SceneController.GetInterface<ResourceManagerInterface>().UserID))
                                {
                                    OnElemCreated?.Invoke(isoEvent);
                                    OnElemCreated = null;
                                    _createdElements.Add(isoEvent);
                                }
                            }

                            // object attributes
                            if (type.Equals(AnnotationTypes.OBJECT_ATTRIBUTE))
                            {
                                IsoObjectAttribute objectAttribute;
                                objectAttribute = ExtractIsoObjectAttribute(_id, _features, _editedDoc.Document);
                                _addedData.Add(objectAttribute);
                                if (FingerprintMap.ContainsKey(id) &&
                                    FingerprintMap[id].Equals(SceneController.GetInterface<ResourceManagerInterface>().UserID))
                                {
                                    OnElemCreated?.Invoke(objectAttribute);
                                    OnElemCreated = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_editedDoc.Document.Text_ID_Map.ContainsKey(_id))
                            if (!MarkedForRemoving.Contains(_id))
                                MarkedForRemoving.Add(_id);
                    }
                }
            }
        }

        foreach (int id in MarkedForRemoving)
        {
            _toRemove = _editedDoc.Document.Text_ID_Map[id];
            _deletedData.Add("" + id);
            _toRemove.RemoveElement();
            if (RemoveEventMap.ContainsKey(id))
            {
                RemoveEventMap[id]?.Invoke();
                RemoveEventMap.Remove(id);
            }
            _toRemove = null;
        }

        _editedDoc.Document.HasChanges = _editedDoc.Document.HasChanges || _deletedData.Count > 0 || _addedData.Count > 0 || _docUpdated;

        if (_editedDoc.Document.HasChanges && SceneController.ActiveSceneScript is SceneBuilderSceneScript &&
            ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder != null &&
            ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<DocumentTab>().Active)
            ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<DocumentTab>().SaveDocumentBtn.Active = true;

        OnChanges?.Invoke(_deletedData, _addedData);
        OnElemsCreated?.Invoke(_createdElements);
    }*/

/*
    bool _viewsPrinted; IsoSpatialEntity e1, e2;
    public void Update()
    {
        if (ChangeQueue != null && ChangeQueue.Count > 0)
            Change(ChangeQueue.Dequeue());
        if (DocumentViewQueue != null && DocumentViewQueue.Count > 0)
            ViewGetter = StartCoroutine(LoadDocumentViews(DocumentViewQueue.Dequeue()));
        if (Input.GetKeyDown(KeyCode.A) || (SteamVR_Actions.default_right_action1.activeBinding &&
            SteamVR_Actions.default_right_action1.GetStateDown(SteamVR_Input_Sources.RightHand)))
            StartCoroutine(AutoLogin());
        if (Input.GetKeyDown(KeyCode.P))
        {
            List<string> todelete = new List<string>();
            foreach(IsoSpatialEntity e in ActualDocument.Document.GetElementsOfType<IsoSpatialEntity>())
            {
                todelete.Add("" + e.ID);
            }
            foreach (IsoObjectAttribute e in ActualDocument.Document.GetElementsOfType<IsoObjectAttribute>())
            {
                todelete.Add("" + e.ID);
            }
            foreach (IsoMetaLink e in ActualDocument.Document.GetElementsOfType<IsoMetaLink>())
            {
                todelete.Add("" + e.ID);
            }
            FireWorkBatchCommand(todelete, null, null, null);
        }
        if (Input.GetKeyDown(KeyCode.S))
            TakeScreenshot();
        if (Input.GetKeyDown(KeyCode.O)) FireJSONCommand(CommandType.open_cas, "23112");
        if (Input.GetKeyDown(KeyCode.V)) ActualDocument.OpenTools();
        if (Input.GetKeyDown(KeyCode.T))
        {
            JsonData transferData = new JsonData();
            transferData["addr"] = "" + e1.ID;
            transferData["targetType"] = AnnotationTypes.PLACE;
            FireJSONCommand(CommandType.convert, ActualDocument.CasId, transferData);
        }
    }*/

    private void TakeScreenshot()
    {
        string folderPath = Directory.GetCurrentDirectory() + "/Screenshots/"; ;

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string screenshotName = "Screenshot_" + DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".png";
        ScreenCapture.CaptureScreenshot(Path.Combine(folderPath, screenshotName), 100);
        Debug.Log(folderPath + screenshotName);
    }

    /// <summary>Die Methode, die den Client mit dem Server verbindet.</summary>
    public IEnumerator StartAuthorization()
    {
        _sessionID = null;
        StartCoroutine(ResourceManager.GetSession(null, (string res) => { _sessionID = res; }));
        Debug.Log(Client);
        Client.ConnectAsync();
        while (Client.ReadyState != WebSocketState.Open || _sessionID == null)
            yield return null;

        FireJSONCommand(CommandType.session, _sessionID);
    }

    public void OnDestroy()
    {
        CloseSocketConnection();
    }


    public void Redo()
    {
        if (ActualDocument == null || !ActualDocument.DocumentCreated) return;
        FireJSONCommand(CommandType.redo, ActualDocument.CasId, null, null, null, ActualDocument.View);
    }

    public void Undo()
    {
        if (ActualDocument == null || !ActualDocument.DocumentCreated) return;
        FireJSONCommand(CommandType.undo, ActualDocument.CasId, null, null, null, ActualDocument.View);
    }

    private static JsonData data;
    private static JsonData command;
    /// <summary>Die Methode zur Kommunikation mit dem Server.</summary>
    /// <param name="type">Typ des zu ausführende TextAnnotator-Befehls.</param>
    /// <param name="stringData">Bei create_cas-Befehl der XMI-File-Inhalt, sonst die ID des Dokuments.</param>
    /// <param name="arg">Weitere Argumente.</param>
    /// /// <param name="fileName">Nur bei Erstellung eines neuen Dokuments relevant. Gibt an, wie das Dokument in der Datenbank genannt werden soll.</param>
    //public void FireJSONCommand(CommandType type, string stringData, JsonData arg=null, string fileName=null, string parent="3959", string view=null)
    public void FireJSONCommand(CommandType type, string stringData, JsonData arg = null, string fileName = null, string parent = "3959", string view = null, string text = null)
    {
        command = new JsonData();
        command["cmd"] = type.ToString();
        data = new JsonData();
        switch (type)
        {
            case CommandType.session:
                data[type.ToString()] = stringData;
                break;
            case CommandType.create_db_cas:
                data["xmi"] = stringData;
                data["name"] = (fileName != null) ? fileName : "UnnamedDocument.xmi";
                data["description"] = "";
                data["parent"] = parent;
                if (text != null) data["text"] = text;
                break;
            case CommandType.open_cas:
                data["casId"] = stringData;
                break;
            case CommandType.save_cas:
                data["casId"] = stringData;
                break;
            case CommandType.close_cas:
                data["casId"] = stringData;
                FingerprintMap.Clear();
                ChangeQueue.Clear();
                DocumentViewQueue.Clear();
                break;
            case CommandType.open_tool:
                data["casId"] = stringData;
                data["view"] = view == null ? "_InitialView" : view;
                data["toolName"] = "quickpanel";
                break;
            case CommandType.load_types:
                data["casId"] = stringData;
                data["types"] = arg;
                break;
            case CommandType.work_batch:
                data["casId"] = stringData;
                data["view"] = view == null ? "_InitialView" : view;
                data["toolName"] = "quickpanel";
                data["queue"] = arg;
                break;
            case CommandType.undo:
            case CommandType.redo:
                data["casId"] = stringData;
                data["view"] = view == null ? "_InitialView" : view;
                data["toolName"] = "quickpanel";
                break;
            case CommandType.convert:
                data["casId"] = stringData;
                data["view"] = view == null ? "_InitialView" : view;
                data["toolName"] = "quickpanel";
                data["conversionData"] = arg;
                break;
            default:
                data["casId"] = stringData;
                break;
        }

        command["data"] = data;
        Debug.Log(command.ToJson());
        Client.Send(command.ToJson());
    }

    string res; MatchCollection matches;
    /// <summary>Die Methode formattiert den Typesystem-Typ von Named-Entities.</summary>
    /// <param name="toFormat">Der zu formattierende Typesystem-Typ.</param>
    public string PrettyFormatNamedEntity(string toFormat)
    {
        res = toFormat.Substring(toFormat.LastIndexOf('.') + 1);
        res = res.Replace("_", ", ");
        matches = Regex.Matches(res);
        for (int i = 0; i < matches.Count; i++)
            res = res.Insert(matches[i].Index + 1, " ");
        return res;
    }

    /// <summary>Die Methode lädt alle Named-Entity-Typen und ihre Farben aus der lokalen NamedEntities.txt-Datei.</summary>
    /*private void LoadNamedEntities()
    {
        string fileContent = File.ReadAllText("Assets//Text2City//NamedEntities.txt");
        string[] lines = fileContent.Replace("\r\n", " ").Split(' ');
        string[] lineSplit; string[] colorSplit; string concept; Color color;
        NamedEntityTypes = new List<string>();
        NamedEntityColorMap = new Dictionary<string, Color>();
        foreach (string line in lines)
        {
            lineSplit = line.Split('|');
            colorSplit = lineSplit[1].Split(',');
            concept = lineSplit[0].Insert(lineSplit[0].LastIndexOf('.'), ".concept");
            color = new Color(float.Parse(colorSplit[0]) / 255f, float.Parse(colorSplit[1]) / 255f, float.Parse(colorSplit[2]) / 255f);
            NamedEntityTypes.Add(lineSplit[0]);
            NamedEntityTypes.Add(concept);
            NamedEntityColorMap.Add(lineSplit[0], color);
            NamedEntityColorMap.Add(concept, color);
            AnnotationTypes.TypesystemClassTable.Add(lineSplit[0], typeof(NamedEntity));
            AnnotationTypes.TypesystemClassTable.Add(concept, typeof(NamedEntity));
            AnnotationTypes.TypeParentTable.Add(lineSplit[0], typeof(Sentence));
            AnnotationTypes.TypeParentTable.Add(concept, typeof(Sentence));
        }
    }*/

    public void OnApplicationQuit()
    {
        if (Client != null && Client.IsAlive)
            Client.Close();
    }

    public IEnumerator AutoLogin()
    {
        yield return StartCoroutine(ResourceManager.AutoLogin());
        if (!Authorized) yield return StartCoroutine(StartAuthorization());        
        //StolperwegeHelper.textAnnotatorClient.FireJSONCommand(CommandType.open_cas, "16503");
    }

    public IEnumerator LoginWithCredential(string username, string password)
    {
        yield return StartCoroutine(ResourceManager.LoginWithCredential(username, password));
        if (!Authorized) yield return StartCoroutine(StartAuthorization());
    }

    private static void DebugOnSocketTransfer(string message)
    {
        Debug.Log(message);
    }
 }
