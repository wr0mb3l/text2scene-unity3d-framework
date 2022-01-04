using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;
//using Text2Scene;
//using Text2Scene.NeuralNetwork;
using UnityEngine;

/// <summary>
/// Diese Klasse stellt eine Websocket-Verbindung zum TextAnnotator-Service her und wickelt die Kommunikation ab.
/// </summary>
public class TextAnnotatorDataContainer
{

    public const string JSON_TEXT = "text";
    public const string JSON_DATA = "data";
    public const string JSON_FEATURE = "feature";
    public const string JSON_FEATURES = "features";
    public const string JSON_ID = "id";
    public const string JSON_CHAPTERS = "chapters";
    public const string JSON_PARAGRAPHS = "paragraphs";
    public const string JSON_SENTENCES = "sentences";
    public const string JSON_LEMMAS = "lemmas";
    public const string JSON_LEMMA_TYPE = "type";
    public const string JSON_BEGIN = "begin";
    public const string JSON_END = "end";
    public const string JSON_DDC = "ddc";
    public const string JSON_REF = "ref";
    public const string JSON_KEY = "key";
    public const string JSON_VALUE = "value";
    public const string JSON_POS = "pos";
    public const string JSON_POS_VALUE = "PosValue";
    public const string JSON_TYPE = "_type";
    public const string JSON_CASID = "casId";
    public const string JSON_TYPESYSTEM = "typesystem";
    public const string JSON_NAME = "name";
    public const string JSON_PARENT = "parent";
    public const string JSON_CHILDREN = "children";
    public const string JSON_FATHER = "fatherObject";
    public const string JSON_NEXT = "nextTimeObject";
    public const string JSON_PREV = "prevTimeObject";
    public const string JSON_TIME_REF = "timeReference";
    public const string JSON_SHAPE_ID = "shapeNetID";
    public const string JSON_SCALE = "scale";
    public const string JSON_LOCATION = "location";
    public const string JSON_ROTATION = "rotation";
    public const string JSON_OBJ_FEATURE = "objectFeature";
    public const string JSON_VECTORS = "vectorlist";
    public const string JSON_HEIGHT = "height";
    public const string JSON_VIEWS = "views";

    /// <summary>
    /// Die ID des Dokuments.
    /// </summary>
    public string CasId;

    /// <summary>
    /// Wörterbuch mit uri-Name Paaren, die die Views des Dokuments darstellen.
    /// </summary>
    public Dictionary<string, string> ViewNameMap;

    /// <summary>
    /// Die Viewliste des Dokuments.
    /// </summary>
    public List<string> Views;

    /// <summary>
    /// Das aktuelle Views des Dokuments.
    /// </summary>
    public string View;

    /// <summary>
    /// Der Text des Dokuments.
    /// </summary>
    public string Text;

    /// <summary>
    /// Die Klasseninstanz des Dokuments.
    /// </summary>
    //public AnnotationDocument Document;
    public JsonData Json { get; private set; }
    private JsonData TypesJson;
    private HashSet<string> TypeMap = new HashSet<string>();
    private HashSet<string> PosTypes = new HashSet<string>();
    private HashSet<JsonData> NamedEntities = new HashSet<JsonData>();

    /// <summary>
    /// Wird auf true gesetzt, sobald alle Informationen aus dem JSON ausgelesen wurde und alle Textelement-Instanzen erstellt wurden.
    /// </summary>
    public bool DocumentCreated { get; private set; }

    int id, begin, end, loc, rot, featID;
    string name, shapeID, key, value;
    int father, prev, next, timeRef;
    List<int> attrs, features;
    List<int> vectors;
    float x, y, z, w, height;
    /// <summary>
    /// Speichert alle eventuelle Fehler, die beim Auslesen auftreten können.
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// Speichert alle eventuelle Warnings, die beim Auslesen auftreten können.
    /// </summary>
    public string WarningMessage { get; private set; }

    /// <summary>
    /// Gibt an, ob die Views des Dokuments geladen wurden.
    /// </summary>
    public bool ViewsLoaded { get { return ViewNameMap != null; } }

    /// <summary>Der Konstruktor initialisiert den TextAnnotatorDataContainer.</summary>
    /// <param name="json">Die Daten vom  nach dem create_db_cas, oder open_cas in JSON</param>
    public TextAnnotatorDataContainer(JsonData json)
    {
        Json = json[JSON_DATA];
        List<string> userList = new List<string>();
        CasId = Json[JSON_CASID].ToString();
        //Text = Json[JSON_TEXT].ToString();
        //Json[JSON_VIEWS].SetJsonType(JsonType.Array);
        TypeMap = new HashSet<string>(Json[JSON_TYPESYSTEM].Keys);
    }

    /*public void OpenTools(string view = "_InitialView")
    {
        SceneController.GetInterface<TextAnnotatorInterface>().FireJSONCommand(TextAnnotatorInterface.CommandType.open_tool, CasId, null, null, null, view);
    }*/

    /// <summary>Die Methode erstellt das Dokument und alle zugehörigen Textelemente aus dem JSON.</summary>
    /// <param name="data">Der JSON nach dem Öffnen des Tools.</param>
    /*public void CreateDocument(JsonData data)
    {
        Debug.Log("Create: Document");

        TypesJson = data;
        GetPosTypeMap();
        //GetNamedEntities();

        ErrorMessage = "";
        WarningMessage = "";
        if (!TypesJson.Keys.Contains(AnnotationTypes.DOCUMENT))
            ErrorMessage += "No document was found.\n";
        if (!TypesJson.Keys.Contains(AnnotationTypes.CHAPTER))
            WarningMessage += "No chapters was found.\n";
        if (!TypesJson.Keys.Contains(AnnotationTypes.PARAGRAPH))
            WarningMessage += "No paragraphs was found.\n";
        if (!TypesJson.Keys.Contains(AnnotationTypes.SENTENCE))
            ErrorMessage += "No sentences was found.\n";
        if (!TypesJson.Keys.Contains(AnnotationTypes.TOKEN))
            ErrorMessage += "No tokens was found.";
        if (!TypesJson.Keys.Contains(AnnotationTypes.QUICK_TREE_NODE))
            WarningMessage += "No multitokens was found.\n";
        if (!TypesJson.Keys.Contains(AnnotationTypes.DDC_CATEGORY))
            WarningMessage += "No categories was found.\n";
        //if (!TypesJson.Keys.Contains(TextAnnotatorClient.ROOM_OBJECT_TYPE))
        //    WarningMessage += "No rooms was found.";

        JsonData doc = TypesJson[AnnotationTypes.DOCUMENT];
        List<string> keys = new List<string>(doc.Keys);
        id = int.Parse(keys[0]);
        begin = int.Parse(doc[keys[0]][JSON_FEATURES][JSON_BEGIN].ToString());
        end = int.Parse(doc[keys[0]][JSON_FEATURES][JSON_END].ToString());
        JsonData objList;
        AnnotationBase element;

        Document = new AnnotationDocument(id, Text);
        *//************************************************************************************************************
        get chapters
        ************************************************************************************************************//*
        if (TypesJson.Keys.Contains(AnnotationTypes.CHAPTER))
            objList = TypesJson[AnnotationTypes.CHAPTER];
        else
        {
            objList = new JsonData();
            JsonData chp = new JsonData();
            JsonData feat = new JsonData();
            feat[JSON_BEGIN] = begin;
            feat[JSON_END] = end;
            chp[JSON_FEATURES] = feat;
            objList["9999999"] = chp;
        }

        Debug.Log("- Create: Chapter");
        List<Chapter> chapters = new List<Chapter>();
        Chapter chapter;
        foreach (string cId in objList.Keys)
        {
            id = int.Parse(cId);
            begin = int.Parse(objList[cId][JSON_FEATURES][JSON_BEGIN].ToString());
            end = int.Parse(objList[cId][JSON_FEATURES][JSON_END].ToString());
            chapter = new Chapter(id, begin, end, Document);
            chapters.Add(chapter);
        }
        // set&sort chapters
        Document.SetChildElements(chapters);

        if (ErrorMessage.Length > 0) Debug.Log("ERROR:\n" + ErrorMessage);
        if (WarningMessage.Length > 0) Debug.Log("WARNING:\n" + WarningMessage);

        *//************************************************************************************************************
        get paragraphs
        ************************************************************************************************************//*
        Debug.Log("- Create: Paragraph");
        if (TypesJson.Keys.Contains(AnnotationTypes.PARAGRAPH))
            objList = TypesJson[AnnotationTypes.PARAGRAPH];
        else
        {
            objList = new JsonData();
            JsonData par = new JsonData();
            JsonData feat = new JsonData();
            feat[JSON_BEGIN] = begin;
            feat[JSON_END] = end;
            par[JSON_FEATURES] = feat;
            objList["9999991"] = par;
        }
        Dictionary<Chapter, List<Paragraph>> paragraphs = new Dictionary<Chapter, List<Paragraph>>();
        Paragraph paragraph;
        foreach (string pID in objList.Keys)
        {
            id = int.Parse(pID);
            begin = int.Parse(objList[pID][JSON_FEATURES][JSON_BEGIN].ToString());
            end = int.Parse(objList[pID][JSON_FEATURES][JSON_END].ToString());

            chapter = Document.GetElementOfTypeInRangeGreaterEqual<Chapter>(begin, end);
            paragraph = new Paragraph(id, begin, end, chapter);
            if (!paragraphs.ContainsKey(chapter)) paragraphs.Add(chapter, new List<Paragraph>());
            paragraphs[chapter].Add(paragraph);
        }

        // set&sort paragraphs in each chapter
        foreach (Chapter c in paragraphs.Keys)
        {
            c.SetChildElements(paragraphs[c]);
            //c.BuildParagraphGroups();
        }

        *//************************************************************************************************************
        get sentences
        ************************************************************************************************************//*
        Debug.Log("- Create: Sentence");
        objList = TypesJson[AnnotationTypes.SENTENCE];
        Dictionary<Paragraph, List<Sentence>> sentences = new Dictionary<Paragraph, List<Sentence>>();
        Sentence sentence;
        foreach (string sID in objList.Keys)
        {

            id = int.Parse(sID);
            begin = int.Parse(objList[sID][JSON_FEATURES][JSON_BEGIN].ToString());
            end = int.Parse(objList[sID][JSON_FEATURES][JSON_END].ToString());

            paragraph = Document.GetElementOfTypeInRangeGreaterEqual<Paragraph>(begin, end);
            sentence = new Sentence(id, begin, end, paragraph);
            if (!sentences.ContainsKey(paragraph)) sentences.Add(paragraph, new List<Sentence>());
            sentences[paragraph].Add(sentence);
        }

        // set&sort sentences in each paragraph
        foreach (Paragraph p in sentences.Keys)
            p.SetChildElements(sentences[p]);

        *//************************************************************************************************************
        get tokens
        ************************************************************************************************************//*
        Debug.Log("- Create: Token");
        objList = TypesJson[AnnotationTypes.TOKEN];
        Dictionary<Sentence, List<AnnotationToken>> tokens = new Dictionary<Sentence, List<AnnotationToken>>();
        AnnotationToken token;
        foreach (string tID in objList.Keys)
        {
            id = int.Parse(tID);
            begin = int.Parse(objList[tID][JSON_FEATURES][JSON_BEGIN].ToString());
            end = int.Parse(objList[tID][JSON_FEATURES][JSON_END].ToString());

            sentence = Document.GetElementOfTypeInRangeGreaterEqual<Sentence>(begin, end);
            token = new AnnotationToken(id, begin, end, sentence);
            if (!tokens.ContainsKey(sentence)) tokens.Add(sentence, new List<AnnotationToken>());
            tokens[sentence].Add(token);
        }

        // sort sentences in each paragraph
        foreach (Sentence s in tokens.Keys)
            s.SetChildElements(tokens[s]);

        *//************************************************************************************************************
        get multi-tokens
        ************************************************************************************************************//*
        Debug.Log("- Create: Quick Tree Nodes");
        if (TypesJson.Keys.Contains(AnnotationTypes.QUICK_TREE_NODE))
        {
            objList = TypesJson[AnnotationTypes.QUICK_TREE_NODE];
            QuickTreeNode quickTreeNode;
            foreach (string mtID in objList.Keys)
            {
                id = int.Parse(mtID);
                begin = int.Parse(objList[mtID][JSON_FEATURES][JSON_BEGIN].ToString());
                end = int.Parse(objList[mtID][JSON_FEATURES][JSON_END].ToString());
                sentence = Document.GetElementOfTypeInRangeGreaterEqual<Sentence>(begin, end);
                int parentNode;
                if (!objList[mtID][JSON_FEATURES].ContainsKey(JSON_PARENT) ||
                    !int.TryParse(objList[mtID][JSON_FEATURES][JSON_PARENT].ToString(), out parentNode))
                    parentNode = -1;
                List<int> childNodes = new List<int>();
                if (objList[mtID][JSON_FEATURES].ContainsKey(JSON_CHILDREN) &&
                    !objList[mtID][JSON_FEATURES][JSON_CHILDREN].ToString().Equals("null"))
                {
                    foreach (object cnIDString in objList[mtID][JSON_FEATURES][JSON_CHILDREN])
                    {
                        if (cnIDString != null) childNodes.Add(int.Parse(cnIDString.ToString()));
                    }
                }
                if (sentence != null)
                {
                    quickTreeNode = new QuickTreeNode(id, begin, end, parentNode, childNodes, sentence);
                }
                else
                {
                    quickTreeNode = new QuickTreeNode(id, begin, end, parentNode, childNodes, null);
                }
            }
        }

        *//************************************************************************************************************
        get named-entities
        ************************************************************************************************************//*
        Debug.Log("- Create: Named Entity");
        NamedEntity namedEntity;
        foreach (JsonData type in NamedEntities)
        {
            foreach (string nerID in type.Keys)
            {
                id = int.Parse(nerID);
                begin = int.Parse(type[nerID][JSON_FEATURES][JSON_BEGIN].ToString());
                end = int.Parse(type[nerID][JSON_FEATURES][JSON_END].ToString());

                sentence = Document.GetElementOfTypeInRangeGreaterEqual<Sentence>(begin, end);
                namedEntity = new NamedEntity(id, begin, end, type[nerID][JSON_TYPE].ToString(), sentence);
            }
        }

        *//************************************************************************************************************
        get DDC-categories
        ************************************************************************************************************//*
        Debug.Log("- Create: DDC");
        if (TypesJson.Keys.Contains(AnnotationTypes.DDC_CATEGORY))
        {
            objList = TypesJson[AnnotationTypes.DDC_CATEGORY];
            DDC_Category ddc;
            string catID;
            foreach (string cID in objList.Keys)
            {
                id = int.Parse(cID);
                begin = int.Parse(objList[cID][JSON_FEATURES][JSON_BEGIN].ToString());
                end = int.Parse(objList[cID][JSON_FEATURES][JSON_END].ToString());

                catID = objList[cID][JSON_FEATURES][JSON_VALUE].ToString();

                element = Document.GetElementsOfTypes(new Type[]
                {
                    typeof(AnnotationDocument), typeof(Chapter),
                    typeof(Paragraph), typeof(Sentence)
                }, begin, end);

                if (element != null)
                {
                    if (element is Sentence) Debug.Log("SENTENCE DDC");
                    ddc = new DDC_Category(element, id, begin, end, catID);
                }

            }
        }
        *//************************************************************************************************************
        get sentiments
        ************************************************************************************************************//*
        Debug.Log("- Create: Sentiments");
        if (TypesJson.Keys.Contains(AnnotationTypes.SENTIMENT))
        {
            objList = TypesJson[AnnotationTypes.SENTIMENT];
            Sentiment sentiment;
            foreach (string smtID in objList.Keys)
            {
                id = int.Parse(smtID);
                begin = int.Parse(objList[smtID][JSON_FEATURES][JSON_BEGIN].ToString());
                end = int.Parse(objList[smtID][JSON_FEATURES][JSON_END].ToString());

                float value = float.Parse(objList[smtID][JSON_FEATURES][JSON_VALUE].ToString());

                element = Document.GetElementOfTypeInRangeGreaterEqual<AnnotationToken>(begin, end);
                sentiment = new Sentiment(element, id, begin, end, value);

            }
        }

        *//************************************************************************************************************
       get part-of-speeches
       ************************************************************************************************************//*
        Debug.Log("- Create: PoS");
        PartOfSpeech pos;
        foreach (string type in PosTypes)
        {
            if (!TypesJson.Keys.Contains(type)) continue;
            objList = TypesJson[type];
            foreach (string pID in objList.Keys)
            {
                id = int.Parse(pID);
                begin = int.Parse(objList[pID][JSON_FEATURES][JSON_BEGIN].ToString());
                end = int.Parse(objList[pID][JSON_FEATURES][JSON_END].ToString());

                element = Document.GetElementOfTypeInRangeGreaterEqual<AnnotationToken>(begin, end);
                pos = new PartOfSpeech(element, id, begin, end, type);
            }
        }

        *//************************************************************************************************************
        get vectors
        ************************************************************************************************************/
        /*Debug.Log("- Create: Vectors");
        if (TypesJson.Keys.Contains(AnnotationTypes.VEC3))
        {
            objList = TypesJson[AnnotationTypes.VEC3];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                TextAnnotatorInterface.ExtractVector3(id, objList[oId][JSON_FEATURES], Document);
            }
        }

        if (TypesJson.Keys.Contains(AnnotationTypes.VEC4))
        {
            objList = TypesJson[AnnotationTypes.VEC4];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                TextAnnotatorInterface.ExtractVector4(id, objList[oId][JSON_FEATURES], Document);
            }
        }*//*

        //IsoObjectAttribute objectAttribute;
        *//************************************************************************************************************
        get ObjectAttributes (has to be extracted before spatial entities because of data race)
        ************************************************************************************************************/
        /*if (TypesJson.Keys.Contains(AnnotationTypes.OBJECT_ATTRIBUTE))
        {
            objList = TypesJson[AnnotationTypes.OBJECT_ATTRIBUTE];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                objectAttribute = TextAnnotatorInterface.ExtractIsoObjectAttribute(id, objList[oId][JSON_FEATURES], Document);
            }
        }*/

        /************************************************************************************************************
        get spatial entities
        ************************************************************************************************************/
        /*Debug.Log("- Create: Spatial Entity");
        if (TypesJson.Keys.Contains(AnnotationTypes.SPATIAL_ENTITY))
        {
            bool addedObjects = false;
            objList = TypesJson[AnnotationTypes.SPATIAL_ENTITY];
            foreach (string oId in objList.Keys)
            {
                    id = int.Parse(oId);
                    begin = int.Parse(objList[oId][JSON_FEATURES][JSON_BEGIN].ToString());
                    end = int.Parse(objList[oId][JSON_FEATURES][JSON_END].ToString());
                    TextAnnotatorInterface.ExtractSpatialEntity<IsoSpatialEntity>(id, objList[oId][JSON_FEATURES], Document);
            }
        }*/

        /************************************************************************************************************
        get location paths
        ************************************************************************************************************/
        /*Debug.Log("- Create: Path");
        if (TypesJson.Keys.Contains(AnnotationTypes.PATH))
        {
            objList = TypesJson[AnnotationTypes.PATH];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                TextAnnotatorInterface.ExtractSpatialEntity<IsoLocationPath>(id, objList[oId][JSON_FEATURES], Document);
            }
        }*/

        /************************************************************************************************************
        get location places
        ************************************************************************************************************/
        /*Debug.Log("- Create: Place");
        if (TypesJson.Keys.Contains(AnnotationTypes.PLACE))
        {
            objList = TypesJson[AnnotationTypes.PLACE];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                TextAnnotatorInterface.ExtractSpatialEntity<IsoLocationPlace>(id, objList[oId][JSON_FEATURES], Document);
            }
        }*/

        /************************************************************************************************************
        get locations
        ************************************************************************************************************/
        /*Debug.Log("- Create: Path");
        if (TypesJson.Keys.Contains(AnnotationTypes.LOCATION))
        {
            objList = TypesJson[AnnotationTypes.LOCATION];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                TextAnnotatorInterface.ExtractSpatialEntity<IsoLocation>(id, objList[oId][JSON_FEATURES], Document);
            }
        }*/

        /************************************************************************************************************
        get event paths
        ************************************************************************************************************/
        /*Debug.Log("- Create: Event Path");
        if (TypesJson.Keys.Contains(AnnotationTypes.EVENT_PATH))
        {
            objList = TypesJson[AnnotationTypes.EVENT_PATH];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                TextAnnotatorInterface.ExtractSpatialEntity<IsoEventPath>(id, objList[oId][JSON_FEATURES], Document);
            }
        }*//*



        //IsoSignal signal;
        *//************************************************************************************************************
        get IsoMeasures
        ************************************************************************************************************/
        /*Debug.Log("- Create: Measure");
        if (TypesJson.Keys.Contains(AnnotationTypes.MEASURE))
        {
            objList = TypesJson[AnnotationTypes.MEASURE];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                signal = TextAnnotatorInterface.ExtractSignal<IsoMeasure>(id, objList[oId][JSON_FEATURES], Document);
            }
        }*/

        /************************************************************************************************************
        get IsoSpatialSignals
        ************************************************************************************************************//*
        //if (TypesJson.Keys.Contains(AnnotationTypes.SPATIAL_SIGNAL))
        //{
        //    Debug.LogError("Spatial_Signal found");
        *//*
        objList = TypesJson[AnnotationTypes.SPATIAL_SIGNAL];
        foreach (string oId in objList.Keys)
        {
            id = int.Parse(oId);
            signal = TextAnnotatorInterface.ExtractSignal<IsoSpatialSignal>(id, objList[oId][JSON_FEATURES], Document);
        }*//*
        //}
        *//************************************************************************************************************
        get IsoMotionSignals
        ************************************************************************************************************/
        /* if (TypesJson.Keys.Contains(AnnotationTypes.MOTION_SIGNAL))
         {
             Debug.LogError("Motion_Signal found");

             objList = TypesJson[AnnotationTypes.MOTION_SIGNAL];
             foreach (string oId in objList.Keys)
             {
                 id = int.Parse(oId);
                 signal = TextAnnotatorInterface.ExtractSignal<IsoSRelation>(id, objList[oId][JSON_FEATURES], Document);
             }
         }
         Debug.Log("16.5");*/
        /************************************************************************************************************
        get IsoSRelation
        ************************************************************************************************************/
        /*Debug.Log("- Create: SRelation");
        if (TypesJson.Keys.Contains(AnnotationTypes.SRELATION))
        {
            objList = TypesJson[AnnotationTypes.SRELATION];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                signal = TextAnnotatorInterface.ExtractSignal<IsoSRelation>(id, objList[oId][JSON_FEATURES], Document);
            }
        }*/

        /************************************************************************************************************
        get IsoMRelation
        ************************************************************************************************************/
        /*Debug.Log("- Create: MRelation");
        if (TypesJson.Keys.Contains(AnnotationTypes.MRELATION))
        {
            objList = TypesJson[AnnotationTypes.MRELATION];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                signal = TextAnnotatorInterface.ExtractSignal<IsoMRelation>(id, objList[oId][JSON_FEATURES], Document);
            }
        }*/

        /************************************************************************************************************
        get IsoSignals
        ************************************************************************************************************/
        /*if (TypesJson.Keys.Contains(AnnotationTypes.SIGNAL))
        {
            Debug.LogError("There should net be a signal");
            *//*
            objList = TypesJson[AnnotationTypes.SIGNAL];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                signal = TextAnnotatorInterface.ExtractSignal<IsoSignal>(id, objList[oId][JSON_FEATURES], Document);
            }*//*
        }*//*

        //IsoEvent isoEvent;
        *//************************************************************************************************************
        get IsoMotions
        ************************************************************************************************************/
        /*Debug.Log("- Create: Motions");
        if (TypesJson.Keys.Contains(AnnotationTypes.MOTION))
        {
            objList = TypesJson[AnnotationTypes.MOTION];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                isoEvent = TextAnnotatorInterface.ExtractEvent<IsoMotion>(id, objList[oId][JSON_FEATURES], Document);
            }
        }*/

        /************************************************************************************************************
        get IsoEvents
        ************************************************************************************************************/
        /*Debug.Log("- Create: NonMotion");
        if (TypesJson.Keys.Contains(AnnotationTypes.NON_MOTION_EVENT))
        {
            objList = TypesJson[AnnotationTypes.NON_MOTION_EVENT];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                isoEvent = TextAnnotatorInterface.ExtractEvent<IsoNonMotionEvent>(id, objList[oId][JSON_FEATURES], Document);
            }
        }*/

        /************************************************************************************************************
        get iso links
        ************************************************************************************************************//*
        Debug.Log("- Create: Links");
        List<string> brokenLinks = new List<string>();
        //IsoLink link;
        if (TypesJson.Keys.Contains(AnnotationTypes.LINK))
        {
            Debug.LogError("Error. There should not be any link.");
            *//*
            objList = TypesJson[AnnotationTypes.LINK];            
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                Debug.Log(id);
                link = TextAnnotatorInterface.ExtractLink<IsoLink>(id, objList[oId][JSON_FEATURES], Document);
                if (link.Ground == null || link.Figure == null)
                {
                    Debug.Log("" + link.GetType() + " " + oId + " was broken.");
                    brokenLinks.Add(oId);
                }
            }*//*
        }

        *//************************************************************************************************************
        get iso QsLinks
        ************************************************************************************************************/
        /*Debug.Log("- Create: ´QSLink");
        if (TypesJson.Keys.Contains(AnnotationTypes.QSLINK))
        {
            objList = TypesJson[AnnotationTypes.QSLINK];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                link = TextAnnotatorInterface.ExtractLink<IsoQsLink>(id, objList[oId][JSON_FEATURES], Document);
                if (link.Ground == null || link.Figure == null)
                {
                    Debug.Log("" + link.GetType() + " " + oId + " was broken.");
                    brokenLinks.Add(oId);
                }

            }
        }*/

        /************************************************************************************************************
        get iso OLinks
        ************************************************************************************************************/
        /*Debug.Log("- Create: OLINK");
        if (TypesJson.Keys.Contains(AnnotationTypes.OLINK))
        {
            objList = TypesJson[AnnotationTypes.OLINK];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                link = TextAnnotatorInterface.ExtractLink<IsoOLink>(id, objList[oId][JSON_FEATURES], Document);
                if (link.Ground == null || link.Figure == null)
                {
                    Debug.Log("" + link.GetType() + " " + oId + " was broken.");
                    brokenLinks.Add(oId);
                }
            }
        }*/

        /************************************************************************************************************
        get iso SRLinks
        ************************************************************************************************************/
        /*Debug.Log("- Create: SRLink");
        if (TypesJson.Keys.Contains(AnnotationTypes.SR_LINK))
        {
            objList = TypesJson[AnnotationTypes.SR_LINK];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                link = TextAnnotatorInterface.ExtractLink<IsoSrLink>(id, objList[oId][JSON_FEATURES], Document);
                if (link.Ground == null || link.Figure == null)
                {
                    Debug.Log("" + link.GetType() + " " + oId + " was broken.");
                    brokenLinks.Add(oId);
                }
            }
        }*/

        /************************************************************************************************************
        get iso MetaLinks
        ************************************************************************************************************/
        /*Debug.Log("- Create: MetaLink");
        if (TypesJson.Keys.Contains(AnnotationTypes.META_LINK))
        {
            objList = TypesJson[AnnotationTypes.META_LINK];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                Debug.Log(id);
                link = TextAnnotatorInterface.ExtractLink<IsoMetaLink>(id, objList[oId][JSON_FEATURES], Document);
                Debug.Log("Link extracted");
                if (objList[oId][JSON_FEATURES]["rel_type"].ToString() == "MASK")
                {
                    int reference = int.Parse(objList[oId][JSON_FEATURES]["figure"].ToString());
                    string word = objList[oId][JSON_FEATURES]["comment"].ToString().Length > 0 ? objList[oId][JSON_FEATURES]["comment"].ToString() + " " : objList[oId][JSON_FEATURES]["comment"].ToString();
                    ClassifiedObject classifiedObject = Document.GetElementByID<ClassifiedObject>(reference, true);
                    classifiedObject.Praefix = word + classifiedObject.Praefix;
                }
                if (link.Ground == null || link.Figure == null)
                {
                    Debug.Log("" + link.GetType() + " " + oId + " was broken.");
                    brokenLinks.Add(oId);
                }
            }
        }*/

        /************************************************************************************************************
        get iso MLink
        ************************************************************************************************************/
        /*Debug.Log("- Create: MLink");
        if (TypesJson.Keys.Contains(AnnotationTypes.MLINK))
        {
            objList = TypesJson[AnnotationTypes.MLINK];
            foreach (string oId in objList.Keys)
            {
                id = int.Parse(oId);
                link = TextAnnotatorInterface.ExtractLink<IsoMLink>(id, objList[oId][JSON_FEATURES], Document);
                if (link.Ground == null || link.Figure == null)
                {
                    Debug.Log("" + link.GetType() + " " + oId + " was broken.");
                    brokenLinks.Add(oId);
                }
            }
        }

        if (brokenLinks.Count > 0)
        {
            Debug.Log("Removing broken links...");
            SceneController.GetInterface<TextAnnotatorInterface>().DeleteElements(brokenLinks);
        }
        else Debug.Log("No broken links was found.");*//*

        foreach (Type type in Document.Type_Map.Keys)
            Document.Type_Map[type].Sort((x, y) => x.Begin.CompareTo(y.Begin));

        //Document.CreateRegulableInfoContainer();

        //while (!Document.InfoContainerCreated)
        //    yield return null;
        //yield return null;
        DocumentCreated = true;

        Debug.Log("Document created");
    }*/

    HashSet<string[]> res;
    /// <summary>Die Methode gibt alle Elemente eines bestimmten Typs zwischen dem gewünschten Start- und Endindex als (ID, Begin, End)-Tripel zurück.</summary>
    /// <param name="type">Der gewünschte Typesytem-Typ.</param>
    /// <param name="_begin">Der Startindex.</param>
    /// <param name="_end">Der Endindex.</param>
    /*public HashSet<string[]> GetElementsInRangeByType(Type type, int _begin, int _end)
    {
        res = new HashSet<string[]>();
        if (TypesJson.Keys.Contains(AnnotationTypes.ClassTypesystemTable[type]))
        {
            foreach (string id in TypesJson[AnnotationTypes.ClassTypesystemTable[type]].Keys)
            {
                begin = int.Parse(TypesJson[AnnotationTypes.ClassTypesystemTable[type]][id][JSON_FEATURES][JSON_BEGIN].ToString());
                end = int.Parse(TypesJson[AnnotationTypes.ClassTypesystemTable[type]][id][JSON_FEATURES][JSON_END].ToString());
                if (begin >= _begin && end <= _end && Document.Text_ID_Map.ContainsKey(int.Parse(id)))
                    res.Add(new string[] { id, "" + begin, "" + end });

            }
        }
        else
        {
            foreach (AnnotationBase data in Document.Type_Map[type])
                if (data.Begin >= _begin && data.End <= _end)
                    res.Add(new string[] { "" + data.ID, "" + data.Begin, "" + data.End });
        }

        return res;
    }*/

    /// <summary>Die Methode lädt alle Part-of-speech Typen des Dokuments.</summary>
    /*private void GetPosTypeMap()
    {
        foreach (string key in TypesJson.Keys)
        {
            if (key.Contains(AnnotationTypes.PART_OF_SPEECH))
                PosTypes.Add(key);
        }
    }*/

    /// <summary>Die Methode lädt alle Named-Entity Typen des Dokuments.</summary>
    private void GetNamedEntities()
    {
        foreach (string nerType in TextAnnotatorInterface.NamedEntityTypes)
            if (TypesJson.Keys.Contains(nerType))
                NamedEntities.Add(TypesJson[nerType]);

    }
}