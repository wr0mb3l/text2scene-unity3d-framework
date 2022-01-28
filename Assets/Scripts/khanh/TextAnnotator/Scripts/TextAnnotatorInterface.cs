using WebSocketSharp;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using LitJson;
using System;
//using Valve.VR;
using DT = IsoSpatialEntity.DimensionType;
using CT = IsoLocationPlace.ContinentType;
using CTV = IsoLocationPlace.CTV;
using FT = IsoSpatialEntity.FormType;
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

            if (Response["cmd"].ToString().Equals("open_tool"))
            {
                if (!Document_Map.ContainsKey(id))
                    DebugOnSocketTransfer("Document " + Response["data"]["casId"].ToString() + " that should be opened, was not found in the Document-Map");
                else if (!Response["data"].ContainsKey("toolElements"))
                    DebugOnSocketTransfer("No Toolelements found on opening a view of document " + Response["data"]["casId"].ToString());
                else
                    Document_Map[id].CreateDocument(Response["data"]["toolElements"]);
            }

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
            //doc.ViewNameMap = viewMap;
        }
    }

    private AnnotationBase _toRemove, _toChange, _parent;
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

    private JsonData featureBatch; private JsonData featureFeatureBatch; private JsonData cmd; private JsonData queue; private JsonData children;
    /// <summary>Mit dieser Methode können Anfragen an den Server geschickt werden.</summary>
    /// <param name="addrs">Die ID-Liste aller zu löschenden Textelementen.</param>
    /// <param name="features">This dictionary should contain all elements, that should be created (key: annotation-type, value: a list of feature-maps for each object).</param>
    /// <param name="updates">This dictionary should contain all elements, that should be updated (key: id of the object, value: the feature-map with the updates).</param>
    /// <param name="childrens">This should be a JSON-Array and only used for QuickTreeNodes.</param>
    public void FireWorkBatchCommand(List<string> addrs, Dictionary<string, List<Dictionary<string, object>>> features, Dictionary<string, Dictionary<string, object>> updates, JsonData childrens)
    {
        if (addrs == null && features == null && updates == null) return;

        queue = new JsonData();

        if (addrs != null)
        {
            foreach (string addr in addrs)
            {
                data = new JsonData();
                data["bid"] = "_b1_";
                data["addr"] = addr;

                cmd = new JsonData();
                cmd["cmd"] = "remove";
                cmd["data"] = data;
                queue.Add(cmd);
            }
        }

        if (features != null)
        {
            foreach (string type in features.Keys)
            {
                foreach (Dictionary<string, object> featureMap in features[type])
                {
                    featureBatch = new JsonData();
                    foreach (string key in featureMap.Keys)
                    {
                        if (featureMap[key] is null)
                            featureBatch[key] = null;
                        if (featureMap[key] is JsonData)
                            featureBatch[key] = (JsonData)featureMap[key];
                        if (featureMap[key] is int)
                            featureBatch[key] = (int)featureMap[key];
                        if (featureMap[key] is string)
                            featureBatch[key] = (string)featureMap[key];
                        if (featureMap[key] is double)
                            featureBatch[key] = (double)featureMap[key];
                        if (featureMap[key] is bool)
                            featureBatch[key] = (bool)featureMap[key];
                        if (featureMap[key] is AnnotationBase)
                            featureBatch[key] = (int)((AnnotationBase)featureMap[key]).ID;
                        if (featureMap[key] is IEnumerable<AnnotationBase>)
                        {
                            IEnumerable<AnnotationBase> elements = (IEnumerable<AnnotationBase>)featureMap[key];
                            JsonData jsonArray = new JsonData();
                            foreach (AnnotationBase ab in elements)
                                jsonArray.Add((int)ab.ID);
                            featureBatch[key] = jsonArray;
                        }
                    }

                    data = new JsonData();
                    data["bid"] = "_b0_";
                    data["_type"] = type;
                    data["features"] = featureBatch;

                    cmd = new JsonData();
                    cmd["cmd"] = "create";
                    cmd["data"] = data;

                    queue.Add(cmd);
                }
            }

            if (childrens != null)
            {
                for (int i = 0; i < childrens.Count; i++)
                {
                    featureBatch = new JsonData();
                    featureBatch["parent"] = "_b0_";

                    data = new JsonData();
                    data["features"] = featureBatch;
                    data["bid"] = "_b" + (i + 1) + "_";
                    data["addr"] = childrens[i];

                    cmd = new JsonData();
                    cmd["cmd"] = "edit";
                    cmd["data"] = data;

                    queue.Add(cmd);
                }
            }
        }

        if (updates != null)
        {
            foreach (string id in updates.Keys)
            {
                featureBatch = new JsonData();
                foreach (string key in updates[id].Keys)
                {
                    if (updates[id][key] is null)
                        featureBatch[key] = null;
                    if (updates[id][key] is JsonData)
                        featureBatch[key] = (JsonData)updates[id][key];
                    if (updates[id][key] is int)
                        featureBatch[key] = (int)updates[id][key];
                    if (updates[id][key] is string)
                        featureBatch[key] = (string)updates[id][key];
                    if (updates[id][key] is double)
                        featureBatch[key] = (double)updates[id][key];
                    if (updates[id][key] is bool)
                        featureBatch[key] = (bool)updates[id][key];
                    if (updates[id][key] is AnnotationBase)
                        featureBatch[key] = (int)((AnnotationBase)updates[id][key]).ID;
                    if (updates[id][key] is IEnumerable<AnnotationBase>)
                    {
                        IEnumerable<AnnotationBase> elements = (IEnumerable<AnnotationBase>)updates[id][key];
                        JsonData jsonArray = new JsonData();
                        foreach (AnnotationBase ab in elements)
                            jsonArray.Add((int)ab.ID);
                        featureBatch[key] = jsonArray;
                    }
                }

                data = new JsonData();
                data["bid"] = "_b0_";
                data["features"] = featureBatch;
                data["addr"] = id;

                cmd = new JsonData();
                cmd["cmd"] = "edit";
                cmd["data"] = data;

                queue.Add(cmd);
            }
        }

        FireJSONCommand(CommandType.work_batch, ActualDocument.CasId, queue, null, null, ActualDocument.View);

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

    // Vec3 methods
    #region
    public Dictionary<string, object> CreateVec3AttributeMap(float x, float y, float z)
    {
        return new Dictionary<string, object>() { { "x", (double)x }, { "y", (double)y }, { "z", (double)z } };
    }

    public void SendVec3CreatingRequest(float x, float y, float z)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        _featureMap.Add(AnnotationTypes.VEC3, new List<Dictionary<string, object>>() { CreateVec3AttributeMap(x, y, z) });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public void ChangeVec3(string id, float x, float y, float z)
    {
        Dictionary<string, Dictionary<string, object>> _updateMap = new Dictionary<string, Dictionary<string, object>>();
        _updateMap.Add(id, CreateVec3AttributeMap(x, y, z));
        FireWorkBatchCommand(null, null, _updateMap, null);
    }

    public static IsoVector3 ExtractVector3(int id, JsonData data, AnnotationDocument doc, IsoVector3 vec3 = null)
    {
        float x = data.ContainsKey("x") ? float.Parse(data["x"].ToString()) : 0;
        float y = data.ContainsKey("y") ? float.Parse(data["y"].ToString()) : 0;
        float z = data.ContainsKey("z") ? float.Parse(data["z"].ToString()) : 0;
        if (vec3 != null) vec3.SetVector(new Vector3(x, y, z));
        else vec3 = new IsoVector3(id, x, y, z, doc);
        return vec3;
    }
    #endregion

    // Vec4 methods
    #region
    public Dictionary<string, object> CreateVec4AttributeMap(float x, float y, float z, float w)
    {
        return new Dictionary<string, object>() { { "x", (double)x }, { "y", (double)y }, { "z", (double)z }, { "w", (double)w } };
    }

    public void SendVec4CreatingRequest(float x, float y, float z, float w)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        _featureMap.Add(AnnotationTypes.VEC4, new List<Dictionary<string, object>>() { CreateVec4AttributeMap(x, y, z, w) });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public void ChangeVec4(string id, float x, float y, float z, float w)
    {
        Dictionary<string, Dictionary<string, object>> _updateMap = new Dictionary<string, Dictionary<string, object>>();
        _updateMap.Add(id, CreateVec4AttributeMap(x, y, z, w));
        FireWorkBatchCommand(null, null, _updateMap, null);
    }

    public static IsoVector4 ExtractVector4(int id, JsonData data, AnnotationDocument doc, IsoVector4 vec4 = null)
    {
        float x = data.ContainsKey("x") ? float.Parse(data["x"].ToString()) : 0;
        float y = data.ContainsKey("y") ? float.Parse(data["y"].ToString()) : 0;
        float z = data.ContainsKey("z") ? float.Parse(data["z"].ToString()) : 0;
        float w = data.ContainsKey("w") ? float.Parse(data["w"].ToString()) : 0;
        if (vec4 != null) vec4.SetQuaternion(new Quaternion(x, y, z, w));
        else vec4 = new IsoVector4(id, x, y, z, w, doc);
        return vec4;
    }
    #endregion

    // Link methods
    #region
    public void SendLinkCreatingRequest(string linkdatatype, IsoEntity figure, IsoEntity ground, IsoSignal signal, string link_type, string comment = null)
    {
        Debug.Log("Create Link Request");
        IsoEntity _figure = figure.GetFirstCorefReferent();
        IsoEntity _ground = ground.GetFirstCorefReferent();
        if (_figure == _ground)
            return;
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        Dictionary<string, object> _attributes = CreateLinkAttributeMap(_figure, _ground, link_type, signal, comment: comment);
        _featureMap.Add(linkdatatype, new List<Dictionary<string, object>>() { _attributes });
        Debug.Log(_featureMap);
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public void SendOLinkCreatingRequest(IsoEntity figure, IsoEntity ground, IsoSignal signal, string frameType, IsoEntity referencePt, string link_type, string comment = null)
    {
        IsoEntity _figure = figure.GetFirstCorefReferent();
        IsoEntity _ground = ground.GetFirstCorefReferent();
        if (_figure == _ground)
            return;
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        Dictionary<string, object> _attributes = CreateOLinkAttributeMap(_figure, _ground, frameType, referencePt, link_type, signal, comment);
        _featureMap.Add(AnnotationTypes.OLINK, new List<Dictionary<string, object>>() { _attributes });
        Debug.Log(_featureMap);
        FireWorkBatchCommand(null, _featureMap, null, null);
    }


    public Dictionary<string, object> CreateLinkAttributeMap(IsoEntity figure, IsoEntity ground, string link_type, IsoSignal signalID = null, string comment = null)
    {
        Dictionary<string, object> map = new Dictionary<string, object>() { { "figure", figure.ID }, { "ground", ground.ID }, { "rel_type", link_type }, { "comment", comment } };

        if (signalID != null)
            map.Add("trigger", signalID.ID);
        return map;

    }

    public Dictionary<string, object> CreateOLinkAttributeMap(IsoEntity figure, IsoEntity ground, string frameType, IsoEntity referencePt, string link_type, IsoSignal signalID = null, string comment = null)
    {
        Dictionary<string, object> map = new Dictionary<string, object>() { { "figure", figure.ID }, { "ground", ground.ID }, { "frame_type", frameType }, { "rel_type", link_type }, { "comment", comment } };
        if (referencePt != null)
            map.Add("reference_pt", referencePt.ID);

        if (signalID != null)
            map.Add("trigger", signalID.ID);

        return map;
    }

    public static AnnotationLinkType ExtractLink<AnnotationLinkType>(int id, JsonData data, AnnotationDocument doc, AnnotationLinkType link = null)
        where AnnotationLinkType : IsoLink
    {
        IsoEntity figure = null;
        if (data.ContainsKey("figure"))
        {
            int figureID = int.Parse(data["figure"].ToString());
            figure = doc.GetElementByID<IsoEntity>(figureID, true);

        }
        IsoEntity ground = null;
        if (data.ContainsKey("ground"))
        {
            int groundID = int.Parse(data["ground"].ToString());
            ground = doc.GetElementByID<IsoEntity>(groundID, true);
        }
        IsoSignal trigger = null;
        if (data.ContainsKey("trigger"))
        {
            int triggerID;
            bool success = int.TryParse(data["trigger"].ToString(), out triggerID);
            trigger = success ? doc.GetElementByID<IsoSignal>(triggerID, true) : null;
        }
        string relType = data.ContainsKey("rel_type") ? data["rel_type"].ToString() : null;
        string comment = data.ContainsKey("comment") ? data["comment"].ToString() : null;
        string mod = data.ContainsKey("mod") ? data["mod"].ToString() : null;


        // O-Link specific attributes
        #region
        bool projective = (typeof(AnnotationLinkType).Equals(typeof(IsoOLink)) && data.ContainsKey("projective")) ? bool.Parse(data["projective"].ToString()) : false;
        string frame_type = (typeof(AnnotationLinkType).Equals(typeof(IsoOLink)) && data.ContainsKey("frame_type")) ? data["frame_type"].ToString() : null;
        IsoEntity referencePt = null;
        if (typeof(AnnotationLinkType).Equals(typeof(IsoOLink)) && data.ContainsKey("reference_pt") && !data["reference_pt"].Equals("null"))
            referencePt = doc.GetElementByID<IsoEntity>(int.Parse(data["reference_pt"].ToString()), true);
        #endregion
        if (link != null)
        {
            if (data.ContainsKey("figure")) link.SetFigure(figure);
            if (data.ContainsKey("ground")) link.SetGround(ground);
            if (data.ContainsKey("trigger")) link.SetTrigger(trigger);
            if (data.ContainsKey("rel_type")) link.SetRelType(relType);
            if (data.ContainsKey("comment")) link.SetComment(comment);
            if (data.ContainsKey("mod")) link.SetMod(mod);
            if (typeof(AnnotationLinkType).Equals(typeof(IsoOLink)))
            {
                if (data.ContainsKey("projective")) ((IsoOLink)Convert.ChangeType(link, typeof(AnnotationLinkType))).SetProjective(projective);
                if (data.ContainsKey("frame_type")) ((IsoOLink)Convert.ChangeType(link, typeof(AnnotationLinkType))).SetFrameType(frame_type);
                if (data.ContainsKey("reference_pt")) ((IsoOLink)Convert.ChangeType(link, typeof(AnnotationLinkType))).SetReferencePoint(referencePt);
            }

        }
        else
        {
            if (typeof(AnnotationLinkType).Equals(typeof(IsoQsLink)))
                link = (AnnotationLinkType)(object)new IsoQsLink(doc, id, comment, mod, figure, ground, trigger, relType);
            else if (typeof(AnnotationLinkType).Equals(typeof(IsoOLink)))
                link = (AnnotationLinkType)(object)new IsoOLink(doc, id, comment, mod, figure, ground, trigger, relType, projective, frame_type, referencePt);
            else if (typeof(AnnotationLinkType).Equals(typeof(IsoMetaLink)))
                link = (AnnotationLinkType)(object)new IsoMetaLink(doc, id, comment, mod, figure, ground, trigger, relType);
            else if (typeof(AnnotationLinkType).Equals(typeof(IsoSrLink)))
                link = (AnnotationLinkType)(object)new IsoSrLink(doc, id, comment, mod, figure, ground, trigger, relType);
            else if (typeof(AnnotationLinkType).Equals(typeof(IsoMLink)))
                link = (AnnotationLinkType)(object)new IsoMLink(doc, id, comment, mod, figure, ground, trigger, relType, null);
            else
                link = (AnnotationLinkType)new IsoLink(doc, id, comment, mod, figure, ground, trigger, relType);
        }
        return link;
    }
    #endregion

    // Signal methods
    #region
    public void SendMeasureCreatingRequest(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin, int end, string value, string unit, string comment = null, string mod = null)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        Dictionary<string, object> _attributes = CreateMeasureAttributeMap(shapeNetID, pos, rot, scale, begin, end, value, unit, comment, mod);
        _featureMap.Add(AnnotationTypes.MEASURE, new List<Dictionary<string, object>>() { _attributes });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public void SendSRelationCreatingRequest(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin, int end, string type, string cluster, string value, string comment = null, string mod = null)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        Dictionary<string, object> _attributes = CreateSRelationAttributeMap(shapeNetID, pos, rot, scale, begin, end, type, cluster, value, comment, mod);
        _featureMap.Add(AnnotationTypes.SRELATION, new List<Dictionary<string, object>>() { _attributes });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public Dictionary<string, object> CreateMeasureAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin, int end, string value, string unit, string comment = null, string mod = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, object> _attributes = new Dictionary<string, object>();

        _attributes.Add("object_id", shapeNetID);

        // position
        #region
        JsonData position = new JsonData();
        position["_type"] = AnnotationTypes.VEC3;
        JsonData posMapJSON = new JsonData();
        posMapJSON["x"] = pos.x;
        posMapJSON["y"] = pos.y;
        posMapJSON["z"] = pos.z;
        position["features"] = posMapJSON;
        _attributes.Add("position", position);
        #endregion

        // rotation
        #region
        JsonData rotation = new JsonData();
        rotation["_type"] = AnnotationTypes.VEC4;
        JsonData rotMapJSON = new JsonData();
        rotMapJSON["x"] = rot.x;
        rotMapJSON["y"] = rot.y;
        rotMapJSON["z"] = rot.z;
        rotMapJSON["w"] = rot.w;
        rotation["features"] = rotMapJSON;
        _attributes.Add("rotation", rotation);
        #endregion

        // scale
        #region
        JsonData scaleVector = new JsonData();
        scaleVector["_type"] = AnnotationTypes.VEC3;
        JsonData scaleMapJSON = new JsonData();
        scaleMapJSON["x"] = scale.x;
        scaleMapJSON["y"] = scale.y;
        scaleMapJSON["z"] = scale.z;
        scaleVector["features"] = scaleMapJSON;
        _attributes.Add("scale", scaleVector);
        #endregion

        // object features
        #region
        if (features != null)
        {
            JsonData featureData = new JsonData();
            foreach (IsoObjectAttribute feature in features)
                featureData.Add("" + feature.ID);
            if (featureData.Count > 0) _attributes.Add("object_feature_array", featureData);
        }
        #endregion

        _attributes.Add("value", value);
        _attributes.Add("unit", unit);
        if (end != 0) _attributes.Add("begin", begin);
        if (end != 0) _attributes.Add("end", end);
        if (comment != null) _attributes.Add("comment", comment);
        if (mod != null) _attributes.Add("mod", mod);
        return _attributes;
    }

    public Dictionary<string, object> CreateSRelationAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin, int end, string type, string cluster, string value, string comment = null, string mod = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, object> _attributes = new Dictionary<string, object>();

        _attributes.Add("object_id", shapeNetID);

        // position
        #region
        JsonData position = new JsonData();
        position["_type"] = AnnotationTypes.VEC3;
        JsonData posMapJSON = new JsonData();
        posMapJSON["x"] = pos.x;
        posMapJSON["y"] = pos.y;
        posMapJSON["z"] = pos.z;
        position["features"] = posMapJSON;
        _attributes.Add("position", position);
        #endregion

        // rotation
        #region
        JsonData rotation = new JsonData();
        rotation["_type"] = AnnotationTypes.VEC4;
        JsonData rotMapJSON = new JsonData();
        rotMapJSON["x"] = rot.x;
        rotMapJSON["y"] = rot.y;
        rotMapJSON["z"] = rot.z;
        rotMapJSON["w"] = rot.w;
        rotation["features"] = rotMapJSON;
        _attributes.Add("rotation", rotation);
        #endregion

        // scale
        #region
        JsonData scaleVector = new JsonData();
        scaleVector["_type"] = AnnotationTypes.VEC3;
        JsonData scaleMapJSON = new JsonData();
        scaleMapJSON["x"] = scale.x;
        scaleMapJSON["y"] = scale.y;
        scaleMapJSON["z"] = scale.z;
        scaleVector["features"] = scaleMapJSON;
        _attributes.Add("scale", scaleVector);
        #endregion

        // object features
        #region
        if (features != null)
        {
            JsonData featureData = new JsonData();
            foreach (IsoObjectAttribute feature in features)
                featureData.Add("" + feature.ID);
            if (featureData.Count > 0) _attributes.Add("object_feature_array", featureData);
        }
        #endregion

        _attributes.Add("relation_type", type);
        _attributes.Add("cluster", cluster);
        _attributes.Add("value", value);
        if (end != 0) _attributes.Add("begin", begin);
        if (end != 0) _attributes.Add("end", end);
        if (comment != null) _attributes.Add("comment", comment);
        if (mod != null) _attributes.Add("mod", mod);
        return _attributes;
    }

    public Dictionary<string, object> CreateMRelationAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin, int end, string value, string comment = null, string mod = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, object> _attributes = new Dictionary<string, object>();

        _attributes.Add("object_id", shapeNetID);

        // position
        #region
        JsonData position = new JsonData();
        position["_type"] = AnnotationTypes.VEC3;
        JsonData posMapJSON = new JsonData();
        posMapJSON["x"] = pos.x;
        posMapJSON["y"] = pos.y;
        posMapJSON["z"] = pos.z;
        position["features"] = posMapJSON;
        _attributes.Add("position", position);
        #endregion

        // rotation
        #region
        JsonData rotation = new JsonData();
        rotation["_type"] = AnnotationTypes.VEC4;
        JsonData rotMapJSON = new JsonData();
        rotMapJSON["x"] = rot.x;
        rotMapJSON["y"] = rot.y;
        rotMapJSON["z"] = rot.z;
        rotMapJSON["w"] = rot.w;
        rotation["features"] = rotMapJSON;
        _attributes.Add("rotation", rotation);
        #endregion

        // scale
        #region
        JsonData scaleVector = new JsonData();
        scaleVector["_type"] = AnnotationTypes.VEC3;
        JsonData scaleMapJSON = new JsonData();
        scaleMapJSON["x"] = scale.x;
        scaleMapJSON["y"] = scale.y;
        scaleMapJSON["z"] = scale.z;
        scaleVector["features"] = scaleMapJSON;
        _attributes.Add("scale", scaleVector);
        #endregion

        // object features
        #region
        if (features != null)
        {
            JsonData featureData = new JsonData();
            foreach (IsoObjectAttribute feature in features)
                featureData.Add("" + feature.ID);
            if (featureData.Count > 0) _attributes.Add("object_feature_array", featureData);
        }
        #endregion

        _attributes.Add("value", value);
        if (end != 0) _attributes.Add("begin", begin);
        if (end != 0) _attributes.Add("end", end);
        if (comment != null) _attributes.Add("comment", comment);
        if (mod != null) _attributes.Add("mod", mod);
        return _attributes;
    }

    public static AnnotationSignalType ExtractSignal<AnnotationSignalType>(int id, JsonData data, AnnotationDocument doc, AnnotationSignalType signal = null)
        where AnnotationSignalType : IsoSignal
    {
        int begin = data.ContainsKey("begin") ? int.Parse(data["begin"].ToString()) : 0;
        int end = data.ContainsKey("end") ? int.Parse(data["end"].ToString()) : 0;
        string objectID = data.ContainsKey("object_id") ? data["object_id"].ToString() : null;

        IsoVector3 pos = null;
        if (data.ContainsKey("position") && !data["position"].Equals("null"))
        {
            int vecID = int.Parse(data["position"].ToString());
            pos = doc.GetElementByID<IsoVector3>(vecID, false);
        }

        IsoVector4 rot = null;
        if (data.ContainsKey("rotation") && !data["rotation"].Equals("null"))
        {
            int vecID = int.Parse(data["rotation"].ToString());
            rot = doc.GetElementByID<IsoVector4>(vecID, false);
        }

        IsoVector3 scale = null;
        if (data.ContainsKey("scale") && !data["scale"].Equals("null"))
        {
            int vecID = int.Parse(data["scale"].ToString());
            scale = doc.GetElementByID<IsoVector3>(vecID, false);
        }

        List<IsoObjectAttribute> objectFeatures = null;
        IsoObjectAttribute feature = null;
        if (data.ContainsKey("object_feature_array") && !data["object_feature_array"].Equals("null"))
        {
            foreach (object featureID in data["object_feature_array"])
            {
                if (featureID is int) feature = doc.GetElementByID<IsoObjectAttribute>((int)featureID, true);
                else if (featureID is string) feature = doc.GetElementByID<IsoObjectAttribute>(int.Parse((string)featureID), true);
                else if (featureID is JsonData) feature = doc.GetElementByID<IsoObjectAttribute>(int.Parse(featureID.ToString()), true);
                if (feature != null)
                {
                    if (objectFeatures == null) objectFeatures = new List<IsoObjectAttribute>();
                    objectFeatures.Add(feature);
                }
            }
        }


        string value = data.ContainsKey("value") ? data["value"].ToString() : null;
        string unit = data.ContainsKey("unit") ? data["unit"].ToString() : null;
        string comment = data.ContainsKey("comment") ? data["comment"].ToString() : null;
        string mod = data.ContainsKey("mod") ? data["mod"].ToString() : null;
        string cluster = data.ContainsKey("cluster") ? data["cluster"].ToString() : null;
        string relationType = data.ContainsKey("relation_type") ? data["relation_type"].ToString() : null;
        string semanticType = data.ContainsKey("semantic_type") ? data["semantic_type"].ToString() : null;
        string motionSignalType = data.ContainsKey("motion_signal_type") ? data["motion_signal_type"].ToString() : null;

        if (signal != null)
        {
            if (data.ContainsKey("begin")) signal.SetBegin(begin);
            if (data.ContainsKey("end")) signal.SetEnd(end);
            //if (data.ContainsKey("begin") || data.ContainsKey("end")) signal.TerminateTextReference();


            if (data.ContainsKey("object_id")) signal.SetObjectID(objectID);
            if (pos != null) signal.SetPosition(pos);
            if (rot != null) signal.SetRotation(rot);
            if (scale != null) signal.SetScale(scale);

            if (data.ContainsKey("comment")) signal.SetComment(comment);
            if (data.ContainsKey("mod")) signal.SetMod(mod);

            if (typeof(AnnotationSignalType).Equals(typeof(IsoMeasure)))
            {
                if (data.ContainsKey("value")) ((IsoMeasure)Convert.ChangeType(signal, typeof(AnnotationSignalType))).SetValue(value);
                if (data.ContainsKey("unit")) ((IsoMeasure)Convert.ChangeType(signal, typeof(AnnotationSignalType))).SetUnit(unit);
            }

            if (typeof(AnnotationSignalType).Equals(typeof(IsoSRelation)))
            {
                if (data.ContainsKey("cluster")) ((IsoSRelation)Convert.ChangeType(signal, typeof(AnnotationSignalType))).SetCluster(cluster);
                if (data.ContainsKey("relationType")) ((IsoSRelation)Convert.ChangeType(signal, typeof(AnnotationSignalType))).SetSignalType(relationType);
                if (data.ContainsKey("value")) ((IsoSRelation)Convert.ChangeType(signal, typeof(AnnotationSignalType))).SetValue(value);
            }
            if (typeof(AnnotationSignalType).Equals(typeof(IsoMRelation)))
            {
                if (data.ContainsKey("value")) ((IsoMRelation)Convert.ChangeType(signal, typeof(AnnotationSignalType))).SetValue(value);
            }
        }
        else
        {
            if (typeof(AnnotationSignalType).Equals(typeof(IsoMeasure)))
                signal = (AnnotationSignalType)(object)new IsoMeasure(doc, id, begin, end, objectID, pos, rot, scale, objectFeatures, comment, mod, value, unit);
            else if (typeof(AnnotationSignalType).Equals(typeof(IsoSRelation)))
                signal = (AnnotationSignalType)(object)new IsoSRelation(doc, id, begin, end, objectID, pos, rot, scale, objectFeatures, comment, mod, relationType, cluster, value);
            else if (typeof(AnnotationSignalType).Equals(typeof(IsoMRelation)))
                signal = (AnnotationSignalType)(object)new IsoMRelation(doc, id, begin, end, objectID, pos, rot, scale, objectFeatures, comment, mod, value);
            else
                signal = (AnnotationSignalType)new IsoSignal(doc, id, begin, end, objectID, pos, rot, scale, objectFeatures, comment, mod);
        }
        signal.Actualize3DObject();
        return signal;
    }
    #endregion

    // Event methods
    #region

    public void SendEventCreatingRequest(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin, int end, string event_frame, string event_type, string comment = null, string mod = null, string domain = null,
                                         string lat = null, string lon = null, IsoMeasure elevation = null, bool countable = false, string gquant = null, List<IsoEntity> scopes = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        Dictionary<string, object> _attributes = CreateEventAttributeMap(shapeNetID, pos, rot, scale, begin, end, event_frame, event_type, comment, mod, domain, lat, lon, elevation, countable, gquant, scopes, features);
        _featureMap.Add(AnnotationTypes.EVENT, new List<Dictionary<string, object>>() { _attributes });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public void SendMotionCreatingRequest(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin, int end, string event_frame, string event_type, string comment = null, string mod = null, string domain = null,
                                         string lat = null, string lon = null, IsoMeasure elevation = null, bool countable = false, string gquant = null, List<IsoEntity> scopes = null,
                                         string motion_type = null, string motion_class = null, string motion_sense = null, string manner = null)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        Dictionary<string, object> _attributes = CreateMotionAttributeMap(shapeNetID, pos, rot, scale, begin, end, event_frame, event_type, comment, mod, domain, lat, lon, elevation, countable, gquant, scopes, motion_type, motion_class, motion_sense, manner);
        _featureMap.Add(AnnotationTypes.MOTION, new List<Dictionary<string, object>>() { _attributes });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public Dictionary<string, object> CreateEventAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin, int end, string event_frame, string event_type, string comment = null, string mod = null, string domain = null,
                                         string lat = null, string lon = null, IsoMeasure elevation = null, bool countable = false, string gquant = null, List<IsoEntity> scopes = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, object> _attributes = new Dictionary<string, object>();
        if (end != 0) _attributes.Add("begin", begin);
        if (end != 0) _attributes.Add("end", end);



        _attributes.Add("object_id", shapeNetID);

        // position
        #region
        JsonData position = new JsonData();
        position["_type"] = AnnotationTypes.VEC3;
        JsonData posMapJSON = new JsonData();
        posMapJSON["x"] = pos.x;
        posMapJSON["y"] = pos.y;
        posMapJSON["z"] = pos.z;
        position["features"] = posMapJSON;
        _attributes.Add("position", position);
        #endregion

        // rotation
        #region
        JsonData rotation = new JsonData();
        rotation["_type"] = AnnotationTypes.VEC4;
        JsonData rotMapJSON = new JsonData();
        rotMapJSON["x"] = rot.x;
        rotMapJSON["y"] = rot.y;
        rotMapJSON["z"] = rot.z;
        rotMapJSON["w"] = rot.w;
        rotation["features"] = rotMapJSON;
        _attributes.Add("rotation", rotation);
        #endregion

        // scale
        #region
        JsonData scaleVector = new JsonData();
        scaleVector["_type"] = AnnotationTypes.VEC3;
        JsonData scaleMapJSON = new JsonData();
        scaleMapJSON["x"] = scale.x;
        scaleMapJSON["y"] = scale.y;
        scaleMapJSON["z"] = scale.z;
        scaleVector["features"] = scaleMapJSON;
        _attributes.Add("scale", scaleVector);
        #endregion

        // object features
        #region
        if (features != null)
        {
            JsonData featureData = new JsonData();
            foreach (IsoObjectAttribute feature in features)
                featureData.Add("" + feature.ID);
            if (featureData.Count > 0) _attributes.Add("object_feature_array", featureData);
        }
        #endregion

        if (event_frame != null) _attributes.Add("event_frame", event_frame);
        if (event_type != null) _attributes.Add("event_type", event_type);
        if (comment != null) _attributes.Add("comment", comment);
        if (mod != null) _attributes.Add("mod", mod);
        // domain
        if (domain != null) _attributes.Add("domain", domain);

        // latitude
        if (lat != null) _attributes.Add("lat", lat);

        // longitude
        if (lon != null) _attributes.Add("long", lon);

        // elevation
        if (elevation != null) _attributes.Add("elevation", "" + elevation.ID);

        // countable
        _attributes.Add("countable", countable);

        //gquant
        if (gquant != null) _attributes.Add("gquant", gquant);

        // scopes
        #region
        if (scopes != null)
        {
            JsonData scopeData = new JsonData();
            foreach (IsoEntity scope in scopes)
                scopeData.Add("" + scope.ID);
            if (scopeData.Count > 0) _attributes.Add("scopes_array", scopeData);
        }
        #endregion

        return _attributes;
    }

    public Dictionary<string, object> CreateMotionAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin, int end, string event_frame, string event_type, string comment = null, string mod = null, string domain = null,
                                         string lat = null, string lon = null, IsoMeasure elevation = null, bool countable = false, string gquant = null, List<IsoEntity> scopes = null,
                                         string motion_type = null, string motion_class = null, string motion_sense = null, string manner = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, object> _attributes = CreateEventAttributeMap(shapeNetID, pos, rot, scale, begin, end, event_frame, event_type, comment, mod, domain, lat, lon, elevation, countable, gquant, scopes, features);


        if (motion_type != null) _attributes.Add("motion_type", motion_type);
        if (motion_class != null) _attributes.Add("motion_class", motion_class);
        if (motion_sense != null) _attributes.Add("motion_sense", motion_sense);
        if (manner != null) _attributes.Add("manner", manner);
        return _attributes;
    }

    public Dictionary<string, object> CreateNonMotionAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin, int end, string event_frame, string event_type, string comment = null, string mod = null, string domain = null,
                                     string lat = null, string lon = null, IsoMeasure elevation = null, bool countable = false, string gquant = null, List<IsoEntity> scopes = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, object> _attributes = CreateEventAttributeMap(shapeNetID, pos, rot, scale, begin, end, event_frame, event_type, comment, mod, domain, lat, lon, elevation, countable, gquant, scopes, features);




        return _attributes;
    }



    public static AnnotationEventType ExtractEvent<AnnotationEventType>(int id, JsonData data, AnnotationDocument doc, AnnotationEventType aEvent = null)
        where AnnotationEventType : IsoEvent
    {
        int begin = data.ContainsKey("begin") ? int.Parse(data["begin"].ToString()) : 0;
        int end = data.ContainsKey("end") ? int.Parse(data["end"].ToString()) : 0;

        string comment = data.ContainsKey("comment") ? data["comment"].ToString() : null;
        string mod = data.ContainsKey("mod") ? data["mod"].ToString() : null;

        string event_frame = data.ContainsKey("event_frame") ? data["event_frame"].ToString() : null;
        string event_type = data.ContainsKey("event_type") ? data["event_type"].ToString() : null;
        string domain = data.ContainsKey("domain") ? data["domain"].ToString() : null;
        string lat = data.ContainsKey("lat") ? data["lat"].ToString() : null;
        string lon = data.ContainsKey("long") ? data["long"].ToString() : null;

        IsoMeasure elevation = null;
        if (data.ContainsKey("elevation") && !data["elevation"].Equals("null"))
            elevation = doc.GetElementByID<IsoMeasure>(int.Parse(data["elevation"].ToString()), false);
        bool countable = data.ContainsKey("countable") ? bool.Parse(data["countable"].ToString()) : false;
        string gquant = data.ContainsKey("gquant") ? data["gquant"].ToString() : null;
        List<IsoEntity> scopes = null;
        IsoEntity scope = null;
        if (data.ContainsKey("scopes_array") && !data["scopes_array"].Equals("null"))
        {
            foreach (object scopeID in data["scopes_array"])
            {
                if (scopeID is int) scope = doc.GetElementByID<IsoEntity>((int)scopeID, true);
                else if (scopeID is string) scope = doc.GetElementByID<IsoEntity>(int.Parse((string)scopeID), true);
                else if (scopeID is JsonData) scope = doc.GetElementByID<IsoEntity>(int.Parse(scopeID.ToString()), true);
                if (scope != null)
                {
                    if (scopes == null) scopes = new List<IsoEntity>();
                    scopes.Add(scope);
                }
            }
        }


        string objectID = data.ContainsKey("object_id") ? data["object_id"].ToString() : null;
        IsoVector3 pos = null;
        if (data.ContainsKey("position") && !data["position"].Equals("null"))
        {
            int vecID = int.Parse(data["position"].ToString());
            pos = doc.GetElementByID<IsoVector3>(vecID, false);
        }
        IsoVector4 rot = null;
        if (data.ContainsKey("rotation") && !data["rotation"].Equals("null"))
        {
            int vecID = int.Parse(data["rotation"].ToString());
            rot = doc.GetElementByID<IsoVector4>(vecID, false);
        }

        IsoVector3 scale = null;
        if (data.ContainsKey("scale") && !data["scale"].Equals("null"))
        {
            int vecID = int.Parse(data["scale"].ToString());
            scale = doc.GetElementByID<IsoVector3>(vecID, false);
        }


        List<IsoObjectAttribute> objectFeatures = null;
        IsoObjectAttribute feature = null;
        if (data.ContainsKey("object_feature_array") && !data["object_feature_array"].Equals("null"))
        {
            foreach (object featureID in data["object_feature_array"])
            {
                if (featureID is int) feature = doc.GetElementByID<IsoObjectAttribute>((int)featureID, true);
                else if (featureID is string) feature = doc.GetElementByID<IsoObjectAttribute>(int.Parse((string)featureID), true);
                else if (featureID is JsonData) feature = doc.GetElementByID<IsoObjectAttribute>(int.Parse(featureID.ToString()), true);
                if (feature != null)
                {
                    if (objectFeatures == null) objectFeatures = new List<IsoObjectAttribute>();
                    objectFeatures.Add(feature);
                }
            }
        }

        string motion_type = data.ContainsKey("motion_type") ? data["motion_type"].ToString() : null;
        string motion_class = data.ContainsKey("motion_class") ? data["motion_class"].ToString() : null;
        string motion_sense = data.ContainsKey("motion_sense") ? data["motion_sense"].ToString() : null;

        IsoSRelation manner = null;
        if (data.ContainsKey("manner") && !data["manner"].Equals("null"))
            manner = doc.GetElementByID<IsoSRelation>(int.Parse(data["manner"].ToString()), true);

        IsoSpatialEntity motion_goal = null;
        if (data.ContainsKey("motion_goal") && !data["motion_goal"].Equals("null"))
            motion_goal = doc.GetElementByID<IsoSpatialEntity>(int.Parse(data["motion_goal"].ToString()), true);
        //string manner = data.ContainsKey("manner") ? data["manner"].ToString() : null;

        if (aEvent != null)
        {
            if (data.ContainsKey("begin")) aEvent.SetBegin(begin);
            if (data.ContainsKey("end")) aEvent.SetEnd(end);
            //if (data.ContainsKey("begin") || data.ContainsKey("end")) aEvent.TerminateTextReference();

            if (data.ContainsKey("comment")) aEvent.SetComment(comment);
            if (data.ContainsKey("mod")) aEvent.SetMod(mod);

            if (data.ContainsKey("event_frame")) aEvent.SetEventFrame(event_frame);
            if (data.ContainsKey("event_type")) aEvent.SetEventType(event_type);
            if (data.ContainsKey("domain")) aEvent.SetDomain(domain);
            if (data.ContainsKey("lat")) aEvent.SetLatitude(lat);
            if (data.ContainsKey("long")) aEvent.SetLongitude(lon);
            if (data.ContainsKey("elevation")) aEvent.SetElevation(elevation);
            if (data.ContainsKey("countable")) aEvent.SetCountable(countable);
            if (data.ContainsKey("gquant")) aEvent.SetGQuant(gquant);
            if (data.ContainsKey("scopes_array")) aEvent.SetScopes(scopes);

            if (data.ContainsKey("object_id")) aEvent.SetObjectID(objectID);
            if (pos != null) aEvent.SetPosition(pos);
            if (rot != null) aEvent.SetRotation(rot);
            if (scale != null) aEvent.SetScale(scale);
            if (data.ContainsKey("object_feature_array")) aEvent.SetFeatures(objectFeatures);

            if (typeof(AnnotationEventType).Equals(typeof(IsoMotion)))
            {
                if (data.ContainsKey("motion_type")) ((IsoMotion)Convert.ChangeType(aEvent, typeof(AnnotationEventType))).SetMotionType(motion_type);
                if (data.ContainsKey("motion_class")) ((IsoMotion)Convert.ChangeType(aEvent, typeof(AnnotationEventType))).SetMotionClass(motion_class);
                if (data.ContainsKey("motion_sense")) ((IsoMotion)Convert.ChangeType(aEvent, typeof(AnnotationEventType))).SetMotionSense(motion_sense);
                if (data.ContainsKey("manner")) ((IsoMotion)Convert.ChangeType(aEvent, typeof(AnnotationEventType))).SetManner(manner);
                if (data.ContainsKey("motion_goal")) ((IsoMotion)Convert.ChangeType(aEvent, typeof(AnnotationEventType))).SetGoal(motion_goal);
            }
            aEvent.Actualize3DObject();
        }
        else
        {
            if (typeof(AnnotationEventType).Equals(typeof(IsoMotion)))
                aEvent = (AnnotationEventType)(object)new IsoMotion(doc, id, begin, end, objectID, pos, rot, scale, objectFeatures, comment, mod, event_frame, event_type, domain, lat, lon, elevation, countable, gquant, scopes, motion_type, motion_class, motion_sense, manner, motion_goal);
            else if (typeof(AnnotationEventType).Equals(typeof(IsoNonMotionEvent)))
                aEvent = (AnnotationEventType)(object)new IsoNonMotionEvent(doc, id, begin, end, objectID, pos, rot, scale, objectFeatures, comment, mod, event_frame, event_type, domain, lat, lon, elevation, countable, gquant, scopes);
            else
                aEvent = (AnnotationEventType)new IsoEvent(doc, id, begin, end, objectID, pos, rot, scale, objectFeatures, comment, mod, event_frame, event_type, domain, lat, lon, elevation, countable, gquant, scopes);
        }
        return aEvent;
    }
    #endregion

    // Spatial entity methods
    #region
    public Dictionary<string, object> CreateSpatialEntityAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin = 0, int end = 0, string comment = null, string mod = null,
                                                                      string entity_type = null, DT dim = DT.none, FT form = FT.none, bool dcl = false, string domain = null, string lat = null, string longi = null,
                                                                      IsoMeasure elevation = null, bool countable = false, string gquant = null, IEnumerable<IsoEntity> scopes = null, IEnumerable<IsoObjectAttribute> features = null, List<Dictionary<string, object>> featureMap = null)
    {
        Dictionary<string, object> _attributes = new Dictionary<string, object>();
        // object_id
        _attributes.Add("object_id", shapeNetID);

        // begin
        if (end != 0) _attributes.Add("begin", begin);

        // end
        if (end != 0) _attributes.Add("end", end);

        // comment
        if (comment != null) _attributes.Add("comment", comment);

        // mod
        if (mod != null) _attributes.Add("mod", mod);

        // position
        #region
        JsonData position = new JsonData();
        position["_type"] = AnnotationTypes.VEC3;
        JsonData posMapJSON = new JsonData();
        posMapJSON["x"] = pos.x;
        posMapJSON["y"] = pos.y;
        posMapJSON["z"] = pos.z;
        position["features"] = posMapJSON;
        _attributes.Add("position", position);
        #endregion

        // rotation
        #region
        JsonData rotation = new JsonData();
        rotation["_type"] = AnnotationTypes.VEC4;
        JsonData rotMapJSON = new JsonData();
        rotMapJSON["x"] = rot.x;
        rotMapJSON["y"] = rot.y;
        rotMapJSON["z"] = rot.z;
        rotMapJSON["w"] = rot.w;
        rotation["features"] = rotMapJSON;
        _attributes.Add("rotation", rotation);
        #endregion

        // scale
        #region
        JsonData scaleVector = new JsonData();
        scaleVector["_type"] = AnnotationTypes.VEC3;
        JsonData scaleMapJSON = new JsonData();
        scaleMapJSON["x"] = scale.x;
        scaleMapJSON["y"] = scale.y;
        scaleMapJSON["z"] = scale.z;
        scaleVector["features"] = scaleMapJSON;
        _attributes.Add("scale", scaleVector);
        #endregion

        // spatial entity type
        if (entity_type != null) _attributes.Add("spatial_entitiy_type", entity_type);

        // dimensionality
        if (dim != DT.none) _attributes.Add("dimensionality", dim.ToString());

        // form
        if (form != FT.none) _attributes.Add("form", form.ToString());

        // dcl
        _attributes.Add("dcl", dcl);

        // domain
        if (domain != null) _attributes.Add("domain", domain);

        // latitude
        if (lat != null) _attributes.Add("lat", lat);

        // longitude
        if (longi != null) _attributes.Add("long", longi);

        // elevation
        if (elevation != null) _attributes.Add("elevation", "" + elevation.ID);

        // countable
        _attributes.Add("countable", countable);

        //gquant
        if (gquant != null) _attributes.Add("gquant", gquant);

        // scopes
        #region
        if (scopes != null)
        {
            JsonData scopeData = new JsonData();
            foreach (IsoEntity scope in scopes)
                scopeData.Add("" + scope.ID);
            if (scopeData.Count > 0) _attributes.Add("scopes_array", scopeData);
        }
        #endregion

        // object features
        #region
        JsonData featureData = new JsonData();
        if (features != null)
        {
            foreach (IsoObjectAttribute feature in features)
                featureData.Add("" + feature.ID);
        }
        #endregion

        // object feature array
        #region
        JsonData featureArrayData = new JsonData();

        if (featureMap != null)
        {
            foreach (Dictionary<string, object> feature in featureMap)
            {
                foreach (string featureKey in feature.Keys)
                {
                    featureBatch = new JsonData();
                    featureFeatureBatch = new JsonData();
                    featureFeatureBatch["key"] = featureKey;

                    if (feature[featureKey] is JsonData)
                        featureFeatureBatch["value"] = (JsonData)feature[featureKey];
                    else if (feature[featureKey] is int)
                        featureFeatureBatch["value"] = (int)feature[featureKey];
                    else if (feature[featureKey] is string)
                        featureFeatureBatch["value"] = (string)feature[featureKey];
                    else if (feature[featureKey] is double)
                        featureFeatureBatch["value"] = (double)feature[featureKey];
                    else if (feature[featureKey] is float)
                        featureFeatureBatch["value"] = (float)feature[featureKey];

                    featureBatch["_type"] = AnnotationTypes.OBJECT_ATTRIBUTE;
                    featureBatch["features"] = featureFeatureBatch;
                }
                featureArrayData.Add(featureBatch);
            }
            if (featureArrayData.Count > 0) _attributes.Add("object_feature_array", featureArrayData);
        }
        #endregion

        return _attributes;
    }

    public void SendSpatialEnityCreatingRequest(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin = 0, int end = 0, string comment = null, string mod = null,
                                    string entity_type = null, DT dim = DT.none, FT form = FT.none, bool dcl = false, string domain = null, string lat = null, string longi = null,
                                    IsoMeasure elevation = null, bool countable = false, string gquant = null, IEnumerable<IsoEntity> scopes = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        _featureMap.Add(AnnotationTypes.SPATIAL_ENTITY, new List<Dictionary<string, object>>() { CreateSpatialEntityAttributeMap(shapeNetID, pos, rot, scale, begin, end, comment, mod, entity_type, dim,
                                                                                                                                 form, dcl, domain, lat, longi, elevation, countable, gquant, scopes, features) });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public Dictionary<string, object> CreateLocationPathAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin = 0, int end = 0, string comment = null, string mod = null,
                                                                     string gazref = null, IsoEntity beginID = null, IsoEntity endID = null, IEnumerable<IsoEntity> mids = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, object> _attributes = new Dictionary<string, object>();
        // object_id
        _attributes.Add("object_id", shapeNetID);

        // begin
        if (end != 0) _attributes.Add("begin", begin);

        // end
        if (end != 0) _attributes.Add("end", end);

        // comment
        if (comment != null) _attributes.Add("comment", comment);

        // mod
        if (mod != null) _attributes.Add("mod", mod);

        // gazref
        if (gazref != null) _attributes.Add("gazref", gazref);

        // position
        #region
        JsonData position = new JsonData();
        position["_type"] = AnnotationTypes.VEC3;
        JsonData posMapJSON = new JsonData();
        posMapJSON["x"] = pos.x;
        posMapJSON["y"] = pos.y;
        posMapJSON["z"] = pos.z;
        position["features"] = posMapJSON;
        _attributes.Add("position", position);
        #endregion

        // rotation
        #region
        JsonData rotation = new JsonData();
        rotation["_type"] = AnnotationTypes.VEC4;
        JsonData rotMapJSON = new JsonData();
        rotMapJSON["x"] = rot.x;
        rotMapJSON["y"] = rot.y;
        rotMapJSON["z"] = rot.z;
        rotMapJSON["w"] = rot.w;
        rotation["features"] = rotMapJSON;
        _attributes.Add("rotation", rotation);
        #endregion

        // scale
        #region
        JsonData scaleVector = new JsonData();
        scaleVector["_type"] = AnnotationTypes.VEC3;
        JsonData scaleMapJSON = new JsonData();
        scaleMapJSON["x"] = scale.x;
        scaleMapJSON["y"] = scale.y;
        scaleMapJSON["z"] = scale.z;
        scaleVector["features"] = scaleMapJSON;
        _attributes.Add("scale", scaleVector);
        #endregion

        // begin entity
        if (beginID != null) _attributes.Add("beginID", (int)beginID.ID);

        // end entity
        if (endID != null) _attributes.Add("endID", (int)endID.ID);

        // middle entities
        #region
        if (mids != null)
        {
            JsonData midJSON = new JsonData();
            foreach (IsoEntity mid in mids)
                midJSON.Add("" + mid.ID);
            if (midJSON.Count > 0) _attributes.Add("midID_array", midJSON);
        }
        #endregion

        // object features
        #region
        if (features != null)
        {
            JsonData featureData = new JsonData();
            foreach (IsoObjectAttribute feature in features)
                featureData.Add("" + feature.ID);
            if (featureData.Count > 0) _attributes.Add("object_feature_array", featureData);
        }
        #endregion

        return _attributes;
    }

    public void SendLocationPathCreatingRequest(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin = 0, int end = 0, string comment = null, string mod = null,
                                    string gazref = null, IsoEntity startID = null, IsoEntity endID = null, IEnumerable<IsoEntity> mids = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        _featureMap.Add(AnnotationTypes.PATH, new List<Dictionary<string, object>>() { CreateLocationPathAttributeMap(shapeNetID, pos, rot, scale, begin, end, comment, mod, gazref, startID, endID, mids, features) });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public Dictionary<string, object> CreateLocationPlaceAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin = 0, int end = 0, string comment = null, string mod = null,
                                    string gazref = null, string country = null, string state = null, string ctv = null, string continent = null, string county = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, object> _attributes = new Dictionary<string, object>();
        // object_id
        _attributes.Add("object_id", shapeNetID);

        // begin
        if (end != 0) _attributes.Add("begin", begin);

        // end
        if (end != 0) _attributes.Add("end", end);

        // comment
        if (comment != null) _attributes.Add("comment", comment);

        // mod
        if (mod != null) _attributes.Add("mod", mod);

        // gazref
        if (gazref != null) _attributes.Add("gazref", gazref);

        // position
        #region
        JsonData position = new JsonData();
        position["_type"] = AnnotationTypes.VEC3;
        JsonData posMapJSON = new JsonData();
        posMapJSON["x"] = pos.x;
        posMapJSON["y"] = pos.y;
        posMapJSON["z"] = pos.z;
        position["features"] = posMapJSON;
        _attributes.Add("position", position);
        #endregion

        // rotation
        #region
        JsonData rotation = new JsonData();
        rotation["_type"] = AnnotationTypes.VEC4;
        JsonData rotMapJSON = new JsonData();
        rotMapJSON["x"] = rot.x;
        rotMapJSON["y"] = rot.y;
        rotMapJSON["z"] = rot.z;
        rotMapJSON["w"] = rot.w;
        rotation["features"] = rotMapJSON;
        _attributes.Add("rotation", rotation);
        #endregion

        // scale
        #region
        JsonData scaleVector = new JsonData();
        scaleVector["_type"] = AnnotationTypes.VEC3;
        JsonData scaleMapJSON = new JsonData();
        scaleMapJSON["x"] = scale.x;
        scaleMapJSON["y"] = scale.y;
        scaleMapJSON["z"] = scale.z;
        scaleVector["features"] = scaleMapJSON;
        _attributes.Add("scale", scaleVector);
        #endregion

        // country
        if (country != null) _attributes.Add("country", country);

        // state
        if (state != null) _attributes.Add("state", state);

        // state
        if (ctv != null) _attributes.Add("ctv", ctv);

        // state
        if (continent != null) _attributes.Add("continent", continent);

        // state
        if (county != null) _attributes.Add("county", county);

        // object features
        #region
        if (features != null)
        {
            JsonData featureData = new JsonData();
            foreach (IsoObjectAttribute feature in features)
                featureData.Add("" + feature.ID);
            if (featureData.Count > 0) _attributes.Add("object_feature_array", featureData);
        }
        #endregion

        return _attributes;
    }

    public Dictionary<string, object> CreateLocationAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin = 0, int end = 0, string comment = null, string mod = null,
                                string gazref = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, object> _attributes = new Dictionary<string, object>();
        // object_id
        _attributes.Add("object_id", shapeNetID);

        // begin
        if (end != 0) _attributes.Add("begin", begin);

        // end
        if (end != 0) _attributes.Add("end", end);

        // comment
        if (comment != null) _attributes.Add("comment", comment);

        // mod
        if (mod != null) _attributes.Add("mod", mod);

        // gazref
        if (gazref != null) _attributes.Add("gazref", gazref);

        // position
        #region
        JsonData position = new JsonData();
        position["_type"] = AnnotationTypes.VEC3;
        JsonData posMapJSON = new JsonData();
        posMapJSON["x"] = pos.x;
        posMapJSON["y"] = pos.y;
        posMapJSON["z"] = pos.z;
        position["features"] = posMapJSON;
        _attributes.Add("position", position);
        #endregion

        // rotation
        #region
        JsonData rotation = new JsonData();
        rotation["_type"] = AnnotationTypes.VEC4;
        JsonData rotMapJSON = new JsonData();
        rotMapJSON["x"] = rot.x;
        rotMapJSON["y"] = rot.y;
        rotMapJSON["z"] = rot.z;
        rotMapJSON["w"] = rot.w;
        rotation["features"] = rotMapJSON;
        _attributes.Add("rotation", rotation);
        #endregion

        // scale
        #region
        JsonData scaleVector = new JsonData();
        scaleVector["_type"] = AnnotationTypes.VEC3;
        JsonData scaleMapJSON = new JsonData();
        scaleMapJSON["x"] = scale.x;
        scaleMapJSON["y"] = scale.y;
        scaleMapJSON["z"] = scale.z;
        scaleVector["features"] = scaleMapJSON;
        _attributes.Add("scale", scaleVector);
        #endregion

        // object features
        #region
        if (features != null)
        {
            JsonData featureData = new JsonData();
            foreach (IsoObjectAttribute feature in features)
                featureData.Add("" + feature.ID);
            if (featureData.Count > 0) _attributes.Add("object_feature_array", featureData);
        }
        #endregion

        return _attributes;
    }

    public void SendLocationPlaceCreatingRequest(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin = 0, int end = 0, string comment = null, string mod = null,
                                                 string gazref = null, string country = null, string state = null, string ctv = null, string continent = null, string county = null, IEnumerable<IsoObjectAttribute> features = null)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        _featureMap.Add(AnnotationTypes.PLACE, new List<Dictionary<string, object>>() { CreateLocationPlaceAttributeMap(shapeNetID, pos, rot, scale, begin, end, comment, mod, gazref, country, state, ctv, continent, county, features) });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public Dictionary<string, object> CreateEventPathAttributeMap(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin = 0, int end = 0, string comment = null, string mod = null,
                                                                  string gazref = null, IsoEntity startID = null, IsoEntity endID = null, IEnumerable<IsoEntity> mids = null, IEnumerable<IsoObjectAttribute> features = null,
                                                                  IsoMotion trigger = null, List<IsoSRelation> spatial_relator = null)
    {
        Dictionary<string, object> _attributes = new Dictionary<string, object>();

        // object_id
        _attributes.Add("object_id", shapeNetID);

        // begin
        if (end != 0) _attributes.Add("begin", begin);

        // end
        if (end != 0) _attributes.Add("end", end);

        // comment
        if (comment != null) _attributes.Add("comment", comment);

        // mod
        if (mod != null) _attributes.Add("mod", mod);

        // gazref
        if (gazref != null) _attributes.Add("gazref", gazref);

        // position
        #region
        JsonData position = new JsonData();
        position["_type"] = AnnotationTypes.VEC3;
        JsonData posMapJSON = new JsonData();
        posMapJSON["x"] = pos.x;
        posMapJSON["y"] = pos.y;
        posMapJSON["z"] = pos.z;
        position["features"] = posMapJSON;
        _attributes.Add("position", position);
        #endregion

        // rotation
        #region
        JsonData rotation = new JsonData();
        rotation["_type"] = AnnotationTypes.VEC4;
        JsonData rotMapJSON = new JsonData();
        rotMapJSON["x"] = rot.x;
        rotMapJSON["y"] = rot.y;
        rotMapJSON["z"] = rot.z;
        rotMapJSON["w"] = rot.w;
        rotation["features"] = rotMapJSON;
        _attributes.Add("rotation", rotation);
        #endregion

        // scale
        #region
        JsonData scaleVector = new JsonData();
        scaleVector["_type"] = AnnotationTypes.VEC3;
        JsonData scaleMapJSON = new JsonData();
        scaleMapJSON["x"] = scale.x;
        scaleMapJSON["y"] = scale.y;
        scaleMapJSON["z"] = scale.z;
        scaleVector["features"] = scaleMapJSON;
        _attributes.Add("scale", scaleVector);
        #endregion

        // begin entity
        if (startID != null) _attributes.Add("startID", (int)startID.ID);

        // end entity
        if (endID != null) _attributes.Add("endID", (int)endID.ID);

        // middle entities

        #region
        /*
        if (mids != null)
        {
            JsonData midJSON = new JsonData();
            foreach (IsoEntity mid in mids)
                midJSON.Add("" + mid.ID);
            if (midJSON.Count > 0) _attributes.Add("midID_array", midJSON);
        }*/
        #endregion

        // trigger
        if (trigger != null) _attributes.Add("trigger", (int)trigger.ID);

        // spatial relator
        if (spatial_relator != null)
        {
            JsonData spatialRelator = new JsonData();
            foreach (IsoSRelation relator in spatial_relator)
                spatialRelator.Add("" + relator.ID);
            if (spatialRelator.Count > 0) _attributes.Add("spatial_relator_array", spatialRelator);
        }

        // object features
        #region
        if (features != null)
        {
            JsonData featureData = new JsonData();
            foreach (IsoObjectAttribute feature in features)
                featureData.Add("" + feature.ID);
            if (featureData.Count > 0) _attributes.Add("object_feature_array", featureData);
        }
        #endregion

        return _attributes;
    }

    public void SendEventPathCreatingRequest(string shapeNetID, Vector3 pos, Quaternion rot, Vector3 scale, int begin = 0, int end = 0, string comment = null, string mod = null,
                                             string gazref = null, IsoEntity beginID = null, IsoEntity endID = null, IEnumerable<IsoEntity> mids = null, IEnumerable<IsoObjectAttribute> features = null,
                                             IsoMotion trigger = null, List<IsoSRelation> spatial_relator = null)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        _featureMap.Add(AnnotationTypes.EVENT_PATH, new List<Dictionary<string, object>>() { CreateEventPathAttributeMap(shapeNetID, pos, rot, scale, begin, end, comment, mod, gazref, beginID, endID, mids, features, trigger, spatial_relator) });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public static SpatialEntityType ExtractSpatialEntity<SpatialEntityType>(int id, JsonData data, AnnotationDocument document, SpatialEntityType entity = null)
        where SpatialEntityType : IsoSpatialEntity
    {
        List<string> keylist = new List<string>();
        foreach (string k in data.Keys)
            keylist.Add(k);

        foreach (string k in keylist)
        {
            if (data[k].ToString() == "null" || data[k] == null)
            {
                data.Remove(k);
            }
        }
        #region
        string objectID = data.ContainsKey("object_id") ? data["object_id"].ToString() : null;
        int begin = data.ContainsKey("begin") ? int.Parse(data["begin"].ToString()) : 0;
        int end = data.ContainsKey("end") ? int.Parse(data["end"].ToString()) : 0;
        IsoVector3 pos = null;
        if (data.ContainsKey("position") && !data["position"].Equals("null"))
        {
            if (!data["position"].Equals("null"))
            {
                int vecID = int.Parse(data["position"].ToString());
                pos = document.GetElementByID<IsoVector3>(vecID, false);
            }
        }
        IsoVector4 rot = null;
        if (data.ContainsKey("rotation") && !data["rotation"].Equals("null"))
        {
            if (!data["rotation"].Equals("null"))
            {
                int vecID = int.Parse(data["rotation"].ToString());
                rot = document.GetElementByID<IsoVector4>(vecID, false);
            }
        }

        IsoVector3 scale = null;
        if (data.ContainsKey("scale") && !data["scale"].Equals("null"))
        {
            if (!data["scale"].Equals("null"))
            {
                int vecID = int.Parse(data["scale"].ToString());
                scale = document.GetElementByID<IsoVector3>(vecID, false);
            }
        }

        string comment = data.ContainsKey("comment") ? data["comment"].ToString() : null;
        string mod = data.ContainsKey("mod") ? data["mod"].ToString() : null;

        string spatialEntityType = data.ContainsKey("spatial_entitiy_type") ? data["spatial_entitiy_type"].ToString() : null;

        DT dim = DT.none;
        if (data.ContainsKey("dimensionality")) Enum.TryParse(data["dimensionality"].ToString(), out dim);

        FT form = FT.none;
        if (data.ContainsKey("form")) Enum.TryParse(data["form"].ToString(), out form);
        bool dcl = data.ContainsKey("dcl") && !data["dcl"].Equals("null") ? bool.Parse(data["dcl"].ToString()) : false;
        string domain = data.ContainsKey("domain") ? data["domain"].ToString() : null;
        string lat = data.ContainsKey("lat") ? data["lat"].ToString() : null;
        string lon = data.ContainsKey("long") ? data["long"].ToString() : null;

        IsoMeasure elevation = null;
        if (data.ContainsKey("elevation") && !data["elevation"].Equals("null"))
            elevation = document.GetElementByID<IsoMeasure>(int.Parse(data["elevation"].ToString()), false);
        bool countable = data.ContainsKey("countable") && !data["countable"].Equals("null") ? bool.Parse(data["countable"].ToString()) : false;
        string gquant = data.ContainsKey("gquant") ? data["gquant"].ToString() : null;
        List<IsoEntity> scopes = null;
        IsoEntity scope = null;
        if (data.ContainsKey("scopes_array") && !data["scopes_array"].Equals("null"))
        {
            foreach (object scopeID in data["scopes_array"])
            {
                if (scopeID is int) scope = document.GetElementByID<IsoEntity>((int)scopeID, true);
                else if (scopeID is string) scope = document.GetElementByID<IsoEntity>(int.Parse((string)scopeID), true);
                else if (scopeID is JsonData) scope = document.GetElementByID<IsoEntity>(int.Parse(scopeID.ToString()), true);
                if (scope != null)
                {
                    if (scopes == null) scopes = new List<IsoEntity>();
                    scopes.Add(scope);
                }
            }
        }

        List<IsoObjectAttribute> objectFeatures = null;
        IsoObjectAttribute feature = null;
        if (data.ContainsKey("object_feature_array") && !data["object_feature_array"].Equals("null"))
        {
            foreach (object featureID in data["object_feature_array"])
            {
                if (featureID is int) feature = document.GetElementByID<IsoObjectAttribute>((int)featureID, true);
                else if (featureID is string) feature = document.GetElementByID<IsoObjectAttribute>(int.Parse((string)featureID), true);
                else if (featureID is JsonData) feature = document.GetElementByID<IsoObjectAttribute>(int.Parse(featureID.ToString()), true);
                if (feature != null)
                {
                    if (objectFeatures == null) objectFeatures = new List<IsoObjectAttribute>();
                    objectFeatures.Add(feature);
                }
            }
        }
        #endregion

        // get the location-specific attribute gazref if the spatial entity type is IsoLocation
        #region
        string gazref = (typeof(SpatialEntityType).Equals(typeof(IsoLocation)) && data.ContainsKey("gazref")) ? data["gazref"].ToString() : null;
        #endregion
        // get the event-path / location-path-specific attributes beginID, MidIDs & endID if the spatial entity type is IsoLocationPath or IsoEventPath
        #region
        IsoEntity startID = null;
        if (typeof(SpatialEntityType).Equals(typeof(IsoEventPath)) && data.ContainsKey("startID") && !data["startID"].Equals("null"))
            startID = document.GetElementByID<IsoEntity>(int.Parse(data["startID"].ToString()), true);
        if (typeof(SpatialEntityType).Equals(typeof(IsoLocationPath)) && data.ContainsKey("beginID") && !data["beginID"].Equals("null"))
            startID = document.GetElementByID<IsoEntity>(int.Parse(data["beginID"].ToString()), true);
        IsoEntity endID = null;
        if ((typeof(SpatialEntityType).Equals(typeof(IsoLocationPath)) || typeof(SpatialEntityType).Equals(typeof(IsoEventPath))) &&
            data.ContainsKey("endID") && !data["endID"].Equals("null"))
            endID = document.GetElementByID<IsoEntity>(int.Parse(data["endID"].ToString()), true);
        List<IsoEntity> midIDs = null;
        IsoEntity midObject = null;
        if ((typeof(SpatialEntityType).Equals(typeof(IsoLocationPath)) || typeof(SpatialEntityType).Equals(typeof(IsoEventPath))) && data.ContainsKey("midID_array") && !data["midID_array"].Equals("null"))
        {
            foreach (object midId in data["midID_array"])
            {
                if (midId is int) midObject = document.GetElementByID<IsoEntity>((int)midId, true);
                else if (midId is string) midObject = document.GetElementByID<IsoEntity>(int.Parse((string)midId), true);
                else if (midId is JsonData) midObject = document.GetElementByID<IsoEntity>(int.Parse(midId.ToString()), true);
                if (midObject != null)
                {
                    if (midIDs == null) midIDs = new List<IsoEntity>();
                    midIDs.Add(midObject);
                }
            }
        }
        #endregion

        // get all place-specific attributes
        #region
        string country = (typeof(SpatialEntityType).Equals(typeof(IsoLocationPlace)) && data.ContainsKey("country")) ? data["country"].ToString() : null;
        string state = (typeof(SpatialEntityType).Equals(typeof(IsoLocationPlace)) && data.ContainsKey("state")) ? data["state"].ToString() : null;
        CTV ctv = CTV.none;
        if (typeof(SpatialEntityType).Equals(typeof(IsoLocationPlace)) && data.ContainsKey("ctv")) Enum.TryParse(data["ctv"].ToString(), out ctv);
        CT continent = CT.NONE;
        if (typeof(SpatialEntityType).Equals(typeof(IsoLocationPlace)) && data.ContainsKey("continent")) Enum.TryParse(data["continent"].ToString(), out continent);
        string county = (typeof(SpatialEntityType).Equals(typeof(IsoLocationPlace)) && data.ContainsKey("county")) ? data["county"].ToString() : null;
        #endregion

        // get all event-path-specific attributes
        #region
        IsoMotion trigger = null;
        if (typeof(SpatialEntityType).Equals(typeof(IsoEventPath)) && data.ContainsKey("trigger") && !data["trigger"].Equals("null"))
            trigger = document.GetElementByID<IsoMotion>(int.Parse(data["trigger"].ToString()), false);

        List<IsoSRelation> spatialRelators = null;
        IsoSRelation spatialRelator = null;
        if (typeof(SpatialEntityType).Equals(typeof(IsoEventPath)) && data.ContainsKey("spatial_relator_array") && !data["spatial_relator_array"].Equals("null"))
        {
            foreach (object spatialSignalID in data["spatial_relator_array"])
            {
                if (spatialSignalID is int) spatialRelator = document.GetElementByID<IsoSRelation>((int)spatialSignalID, false);
                else if (spatialSignalID is string) spatialRelator = document.GetElementByID<IsoSRelation>(int.Parse((string)spatialSignalID), false);
                else if (spatialSignalID is JsonData) spatialRelator = document.GetElementByID<IsoSRelation>(int.Parse(spatialSignalID.ToString()), false);
                if (spatialRelator != null)
                {
                    if (spatialRelators == null) spatialRelators = new List<IsoSRelation>();
                    spatialRelators.Add(spatialRelator);
                }
            }
        }
        #endregion
        // if the entity already exists update it, else create a new one
        if (entity != null)
        {
            // here the attributes should be only changed, if they are included in the update batch
            if (data.ContainsKey("begin")) entity.SetBegin(begin);
            if (data.ContainsKey("end")) entity.SetEnd(end);
            //if (data.ContainsKey("begin") || data.ContainsKey("end")) entity.TerminateTextReference();
            if (data.ContainsKey("object_id")) entity.SetObjectID(objectID);
            if (pos != null) entity.SetPosition(pos);
            if (rot != null) entity.SetRotation(rot);
            if (scale != null) entity.SetScale(scale);
            if (data.ContainsKey("comment")) entity.SetComment(comment);
            if (data.ContainsKey("mod")) entity.SetMod(mod);
            if (data.ContainsKey("spatial_entitiy_type")) entity.SetSpatialEntityType(spatialEntityType);
            if (data.ContainsKey("dimensionality")) entity.SetDimensionality(dim);
            if (data.ContainsKey("form")) entity.SetForm(form);
            if (data.ContainsKey("dcl")) entity.SetDcl(dcl);
            if (data.ContainsKey("domain")) entity.SetDomain(domain);
            if (data.ContainsKey("lat")) entity.SetLat(lat);
            if (data.ContainsKey("long")) entity.SetLongitude(lon);
            if (data.ContainsKey("elevation")) entity.SetElevation(elevation);
            if (data.ContainsKey("countable")) entity.SetCountable(countable);
            if (data.ContainsKey("gquant")) entity.SetGQuant(gquant);
            if (data.ContainsKey("scopes_array")) entity.SetScopes(scopes);
            if (data.ContainsKey("object_feature_array")) entity.SetFeatures(objectFeatures);


            if (typeof(SpatialEntityType).Equals(typeof(IsoLocationPath)))
            {
                if (data.ContainsKey("beginID")) ((IsoLocationPath)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetStartID(startID);
                if (data.ContainsKey("endID")) ((IsoLocationPath)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetEndID(endID);
                if (data.ContainsKey("midID_array")) ((IsoLocationPath)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetMidIDs(midIDs);
            }

            //TODO IConvertible Attila!!!!
            //if (typeof(SpatialEntityType).Equals(typeof(IsoLocation)))
            //{
            //    if (data.ContainsKey("gazref")) ((IsoLocation)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetGazref(gazref); 
            //}
            if (typeof(SpatialEntityType).Equals(typeof(IsoLocationPlace)))
            {
                if (data.ContainsKey("gazref")) ((IsoLocationPlace)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetGazref(gazref);
                if (data.ContainsKey("country")) ((IsoLocationPlace)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetCountry(country);
                if (data.ContainsKey("state")) ((IsoLocationPlace)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetState(state);
                if (data.ContainsKey("ctv")) ((IsoLocationPlace)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetCtv(ctv);
                if (data.ContainsKey("continent")) ((IsoLocationPlace)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetContinent(continent);
                if (data.ContainsKey("county")) ((IsoLocationPlace)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetCounty(county);
            }

            if (typeof(SpatialEntityType).Equals(typeof(IsoEventPath)))
            {
                if (data.ContainsKey("startID")) ((IsoEventPath)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetStartID(startID);
                if (data.ContainsKey("endID")) ((IsoEventPath)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetEndID(endID);
                if (data.ContainsKey("midID_array")) ((IsoEventPath)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetMidIDs(midIDs);
                if (data.ContainsKey("trigger")) ((IsoEventPath)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetTrigger(trigger);
                if (data.ContainsKey("spatial_relator_array")) ((IsoEventPath)Convert.ChangeType(entity, typeof(SpatialEntityType))).SetSpatialRelator(spatialRelators);
            }
            entity.Actualize3DObject();
        }
        else
        {
            if (typeof(SpatialEntityType).Equals(typeof(IsoEventPath)))
                entity = (SpatialEntityType)(object)new IsoEventPath(document, id, begin, end, comment, mod, objectID, pos, rot, scale, objectFeatures, gazref, trigger, startID, midIDs, endID, spatialRelators);
            else if (typeof(SpatialEntityType).Equals(typeof(IsoLocationPath)))
                entity = (SpatialEntityType)(object)new IsoLocationPath(document, id, begin, end, comment, mod, objectID, pos, rot, scale, objectFeatures, gazref, startID, endID, midIDs);
            else if (typeof(SpatialEntityType).Equals(typeof(IsoLocationPlace)))
                entity = (SpatialEntityType)(object)new IsoLocationPlace(document, id, begin, end, comment, mod, objectID, pos, rot, scale, objectFeatures, gazref, country, state, ctv, continent, county);
            else if (typeof(SpatialEntityType).Equals(typeof(IsoLocation)))
                entity = (SpatialEntityType)(object)new IsoLocation(document, id, begin, end, comment, mod, objectID, pos, rot, scale, objectFeatures, gazref);
            else
                entity = (SpatialEntityType)new IsoSpatialEntity(document, id, begin, end, objectID, pos, rot, scale, objectFeatures, comment, mod, spatialEntityType,
                                                                 dim, form, dcl, domain, lat, lon, elevation, countable, gquant, scopes);
        }
        return entity;
    }
    #endregion

    // quick tree node methods
    #region
    public Dictionary<string, object> CreateQuickTreeNodeAttributeMap(int begin, int end, out JsonData childrenObjects)
    {
        HashSet<QuickTreeNode> childNodes = new HashSet<QuickTreeNode>(ActualDocument.Document.GetElementsOfTypeInRange<QuickTreeNode>(begin, end, false));
        childrenObjects = new JsonData();
        foreach (QuickTreeNode node in childNodes)
            childrenObjects.Add("" + node.ID);
        return new Dictionary<string, object>() { { "begin", begin }, { "end", end }, { "children", childrenObjects } };
    }

    public void SendQuickTreeNodeCreatingRequest(int begin, int end)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        JsonData childrenObjects;
        _featureMap.Add(AnnotationTypes.QUICK_TREE_NODE, new List<Dictionary<string, object>>() { CreateQuickTreeNodeAttributeMap(begin, end, out childrenObjects) });
        FireWorkBatchCommand(null, _featureMap, null, childrenObjects);
    }

    public QuickTreeNode ExtractQuickTreeNode(int id, JsonData data, QuickTreeNode node = null)
    {
        Debug.Log("ExtractQuickTreeNode");
        int _parentNode;
        if (!data.ContainsKey("parent") || !int.TryParse(data["parent"].ToString(), out _parentNode))
            _parentNode = -1;
        int begin = data.ContainsKey("begin") ? int.Parse(data["begin"].ToString()) : 0;
        int end = data.ContainsKey("end") ? int.Parse(data["end"].ToString()) : 0;
        if (node == null)
        {
            _childNodeList = new List<int>();
            if (data.ContainsKey("children") && !data["children"].ToString().Equals("null"))
                for (int i = 0; i < data["children"].Count; i++)
                    _childNodeList.Add(int.Parse(data["children"][i].ToString()));
            if (_parent != null) node = new QuickTreeNode(id, begin, end, _parentNode, _childNodeList, (Sentence)_parent);
            else node = new QuickTreeNode(id, begin, end, _parentNode, _childNodeList, null);
        }
        else node.SetParentNode(_parentNode);
        if (node.Parent != null) node.Parent.Actualize3DObject();
        Debug.Log("End ExtractQuickTreeNode");
        return node;

    }
    #endregion

    // annotation delete-methods
    #region
    public void DeleteElement(string id)
    {
        FireWorkBatchCommand(new List<string>() { id }, null, null, null);
    }

    public void DeleteElements(List<string> elementIDs)
    {
        Debug.Log("Send Delete Elements Request!");
        FireWorkBatchCommand(elementIDs, null, null, null);
    }

    public void DeleteAllElementsOfType(Type type)
    {
        if (!ActualDocument.Document.Type_Map.ContainsKey(type)) return;
        List<string> ids = new List<string>();

        foreach (AnnotationBase element in ActualDocument.Document.Type_Map[type])
            ids.Add("" + element.ID);

        FireWorkBatchCommand(ids, null, null, null);
    }

    public void DeleteAllElementsOfTypes(HashSet<Type> types)
    {
        List<string> ids = new List<string>();
        foreach (Type type in types)
        {
            if (!ActualDocument.Document.Type_Map.ContainsKey(type)) continue;

            foreach (AnnotationBase element in ActualDocument.Document.Type_Map[type])
                ids.Add("" + element.ID);
        }
        if (ids.Count > 0) FireWorkBatchCommand(ids, null, null, null);
    }
    #endregion

    // IsoObjectAttribute methods
    #region
    public void SendIsoObjectAttributeCreatingRequest(string key, string value, int begin = 0, int end = 0)
    {
        Dictionary<string, List<Dictionary<string, object>>> _featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
        _featureMap.Add(AnnotationTypes.OBJECT_ATTRIBUTE, new List<Dictionary<string, object>>() { CreateIsoObjectAttributeMap(key, value, begin, end) });
        FireWorkBatchCommand(null, _featureMap, null, null);
    }

    public Dictionary<string, object> CreateIsoObjectAttributeMap(string key, string value, int begin = 0, int end = 0)
    {
        Dictionary<string, object> _attributes = new Dictionary<string, object>();
        // key
        _attributes.Add("key", key);

        // value
        _attributes.Add("value", value);

        // begin
        if (begin != 0) _attributes.Add("begin", begin);

        // end
        if (end != 0) _attributes.Add("end", end);

        _attributes.Add("_type", AnnotationTypes.OBJECT_ATTRIBUTE);

        return _attributes;
    }

    public void ChangeIsoObjectAttribute(string id, float x, float y, float z)
    {
        throw new NotImplementedException();
    }

    public static IsoObjectAttribute ExtractIsoObjectAttribute(int id, JsonData data, AnnotationDocument doc, IsoObjectAttribute objectAttribute = null)
    {
        string key = data.ContainsKey("key") ? data["key"].ToString() : "";
        string value = data.ContainsKey("value") ? data["value"].ToString() : "";
        int begin = data.ContainsKey("begin") ? int.Parse(data["begin"].ToString()) : 0;
        int end = data.ContainsKey("end") ? int.Parse(data["end"].ToString()) : 0;

        if (objectAttribute != null)
        {
            if (begin != 0) objectAttribute.SetBegin(begin);
            if (end != 0) objectAttribute.SetEnd(end);
            if (key != "") objectAttribute.SetKey(key);
            if (value != "") objectAttribute.SetValue(value);
        }
        else objectAttribute = new IsoObjectAttribute(id, begin, end, key, value, doc);
        return objectAttribute;
    }
    #endregion

}
