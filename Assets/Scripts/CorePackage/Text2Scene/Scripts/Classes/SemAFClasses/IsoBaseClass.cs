using System.Collections.Generic;
using UnityEngine;
using LitJson;
//Alle toString würde ich rekursiv und Jsonbezogen aufschlüsseln, sobald eine Json Bibliothek im Projekt ist.




/// <summary>
/// Standart Entity (alles was im Text irgendwie Markierbar ist)
/// </summary>
public class IsoEntity : AnnotationBase
{
    public string Object_ID { get; private set; } //ShapenetId
    public IsoVector3 Position { get; private set; }
    public IsoVector4 Rotation { get; private set; }
    public IsoVector3 Scale { get; private set; }
    public List<IsoObjectAttribute> Object_Feature { get; private set; }

    //public AnnotationObjectPanel Panel { get; set; }
    public string Comment { get; private set; }
    public string Mod { get; private set; }


    public void SetObjectID(string id) { Object_ID = id; }
    public void SetPosition(IsoVector3 pos) { Position = pos; }
    public void SetRotation(IsoVector4 rot) { Rotation = rot; }
    public void SetScale(IsoVector3 scale) { Scale = scale; }
    public void SetFeatures(List<IsoObjectAttribute> features) { Object_Feature = features; }

    public void SetComment(string comment) { Comment = comment; }
    public void SetMod(string mod) { Mod = mod; }

    public static Color ClassColor { get; } = new Color(1, 1, 1);
    public QuickTreeNode TextReference { get; private set; }
    //public TokenObject TokenObject;

    public Dictionary<IsoLink, IsoLink.Connected> LinkedVia; //LinkID, How(Figure, Ground, ect ....)
    public Dictionary<IsoEntity, IsoLink.Connected> LinkedDirect;

    //public InteractiveShapeNetObject InteractiveShapeNetObject { get {return  Object3D != null ? Object3D.GetComponent<InteractiveShapeNetObject>() : null;}}

    public override string TextContent
    {
        get
        {
            if (Begin == 0 && End == 0)
            {
                if (UMAISOEntity.AVATAR_TYPE_NAMES.Contains(Object_ID))
                    return Object_ID;
                else if(Comment != null && !Comment.Equals(""))
                    return Comment.Trim();
                //else if (InteractiveShapeNetObject != null && InteractiveShapeNetObject.ShapeNetModel != null)
                //    return InteractiveShapeNetObject.ShapeNetModel.Name;
                else
                    return EmptyRepresentation;
            }
            if (_textContent == null || _textContent.Length == 0)
                _textContent = DetermineDocument().TextContent.Substring(Begin, End - Begin).Trim();
            return _textContent;
        }
    }


    protected IsoEntity(AnnotationBase parent, int ID, int begin, int end, string comment, string mod, 
        string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string class_type) : 
        base(ID, begin, end, class_type, parent)
    {
        LinkedVia = new Dictionary<IsoLink, IsoLink.Connected>();
        LinkedDirect = new Dictionary<IsoEntity, IsoLink.Connected>();
        Object_ID = object_ID;
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Object_Feature = object_feature;

        Comment = comment;
        Mod = mod;
        //TerminateTextReference();
        //UpdateAnnotationWindow();
    }

    protected IsoEntity(AnnotationBase parent, int ID, int begin, int end, string comment, string mod,
        string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string class_type, bool containsMultiClassTokens) :
        base(ID, begin, end, class_type, parent)
    {
        LinkedVia = new Dictionary<IsoLink, IsoLink.Connected>();
        LinkedDirect = new Dictionary<IsoEntity, IsoLink.Connected>();
        Object_ID = object_ID;
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Object_Feature = object_feature;

        Comment = comment;
        Mod = mod;
        //TerminateTextReference(containsMultiClassTokens);
        //UpdateAnnotationWindow();
    }

    public List<Dictionary<string, object>> GetObjectFeatureDict()
    {
        //Nicht wirklich getestet .....
        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

        foreach (IsoObjectAttribute attr in Object_Feature)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict[attr.Key] = attr.Value;
            list.Add(dict);
        }
        return list;
    }

    public IsoEntity _createrequest_ground = null;
    public string _createrequest_linkdatatype = null;
    public string _createrequest_frameType = null;
    public string _createrequest_comment = null;
    /*public void SendLinkRequest(string datatype, IsoEntity ground, string type, string comment = null)
    {
        _createrequest_linkdatatype = datatype;
        _createrequest_ground = ground;
        _createrequest_comment = comment;
        SendLinkRequest(type);
    }

    public void SendLinkRequest(string type)
    {
        Debug.Log("SendLinkRequest");
        SceneBuilderSceneScript.WaitingForResponse = true;
        SceneController.GetInterface<TextAnnotatorInterface>().OnElemCreated = (entity) =>
        {
            IsoLink link = (IsoLink)entity;

            link.CreateInteractiveLinkObject();


            StolperwegeHelper.VRWriter.Interface.DoneClicked -= SendLinkRequest;
            _createrequest_ground = null;
            _createrequest_linkdatatype = null;
            _createrequest_comment = null;
            _createrequest_frameType = null;
            SceneBuilderSceneScript.WaitingForResponse = false;
        };

        if (type.Equals(AnnotationTypes.OLINK))
            SceneController.GetInterface<TextAnnotatorInterface>().SendOLinkCreatingRequest(this, _createrequest_ground, null, _createrequest_frameType, null, type, _createrequest_comment);
        else
            SceneController.GetInterface<TextAnnotatorInterface>().SendLinkCreatingRequest(_createrequest_linkdatatype, this, _createrequest_ground, null, type, comment: _createrequest_comment);
    }*/

    public IsoEntity GetFirstCorefReferent()
    {
        foreach (KeyValuePair<IsoLink, IsoLink.Connected> attachStat in LinkedVia)
            if (attachStat.Value == IsoLink.Connected.figure && attachStat.Key.Rel_Type.Equals("Coreference"))
                return attachStat.Key.Ground.GetFirstCorefReferent();
        return this;
    }

    public List<IsoEntity> GetAllLinkedCoref()
    {
        List<IsoEntity> coreflist = new List<IsoEntity>();
        foreach (KeyValuePair<IsoLink, IsoLink.Connected> attachStat in LinkedVia)
            if (attachStat.Value == IsoLink.Connected.ground && attachStat.Key.Rel_Type.Equals("Coreference"))
                coreflist.Add(attachStat.Key.Figure);
        return coreflist;
    }

    /*public void SendOLinkRequest(IsoEntity ground, string frameType, IsoEntity referencePt, string relType, string comment)
    {
        SceneBuilderSceneScript.WaitingForResponse = true;
        SceneController.GetInterface<TextAnnotatorInterface>().OnElemCreated = (entity) =>
        {
            IsoOLink link = (IsoOLink)entity;

            link.CreateInteractiveLinkObject();

            SceneBuilderSceneScript.WaitingForResponse = false;
        };

        SceneController.GetInterface<TextAnnotatorInterface>().SendOLinkCreatingRequest(this, ground, null, frameType, referencePt, relType, comment);
    }*/


    /* private void UpdateAnnotationWindow()
     {
         if (SceneController.ActiveSceneScript is SceneBuilderSceneScript)
         {
             SceneBuilder builder = ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder;
             if (builder == null) return;
             QuickAnnotatorTool annoTab = builder.GetTab<QuickAnnotatorTool>();
             if (annoTab != null && annoTab.AnnotationWindow != null && annoTab.AnnotationWindow.Active)
             {
                 if (Begin == 0 && End == 0)
                     annoTab.AnnotationWindow.UpdateEmptyTokenContainer(true);
                 else annoTab.AnnotationWindow.UpdateTokenContainer();
             }
         }
     }*/

    /*public override void RemoveElement()
    {
        if (Object3D != null)
        {
            if (InteractiveShapeNetObject != null)
                InteractiveShapeNetObject.Destroy();
            else UnityEngine.Object.Destroy(Object3D);
        }

        if (Panel != null)
            Panel.Destroy();


        if (TokenObject != null)
        {
            TokenObject.DeleteEntityInToken();
            //if (TokenObject.AnnotationWindow != null)
            //{
            //    if (Begin == 0 && End == 0)
            //        TokenObject.AnnotationWindow.UpdateEmptyTokenContainer(true);
            //    else TokenObject.AnnotationWindow.UpdateTokenContainer();
            //}
            //if (TokenObject.QuickAnnoTool != null)
            //    TokenObject.QuickAnnoTool.UpdateTokenContainer();
        }
        else if (TextReference != null && TextReference.TokenObjects != null)
        {
            TokenObject quickAnnoToken = null;
            TokenObject annoWindowToken = null;
            foreach (TokenObject to in TextReference.TokenObjects)
            {
                if (annoWindowToken == null && to.AnnotationWindow != null)
                    annoWindowToken = to;
                if (quickAnnoToken == null && to.QuickAnnoTool != null)
                    quickAnnoToken = to;

                if (quickAnnoToken != null && annoWindowToken != null)
                    break;
            }
            if (quickAnnoToken != null) quickAnnoToken.QuickAnnoTool.UpdateTokenContainer();
            if (annoWindowToken != null)
            {
                if (Begin == 0 && End == 0)
                    annoWindowToken.AnnotationWindow.UpdateEmptyTokenContainer(true);
                else annoWindowToken.AnnotationWindow.UpdateTokenContainer();
            }
        }

        SceneBuilder builder = ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder;
        if (builder != null)
        {
            QuickAnnotatorTool qat = builder.GetTab<QuickAnnotatorTool>();
            qat.UpdateTokenContainer();
            if (qat.AnnotationWindow != null)
            {
                if (Begin == 0 && End == 0) qat.AnnotationWindow.UpdateEmptyTokenContainer(true);
                else qat.AnnotationWindow.UpdateTokenContainer();
            }
        }

        base.RemoveElement();
    }*/

    /*public void TerminateTextReference()
    {
        QuickTreeNode result;
        if (DetermineDocument() != null && DetermineDocument().ExistsElementOfTypeInRange(Begin, End, out result))
        {
            if (TextReference != null && !Equals(TextReference.IsoEntity))
                TextReference.IsoEntity.Uncouple();
            TextReference = result;
            TextReference.IsoEntity = this;
        }
        else
        {
            if (TextReference != null && Equals(TextReference.IsoEntity))
                TextReference.IsoEntity = null;
            TextReference = null;
        }
    }*/

    /*public void TerminateTextReference(bool containsMultiClassTokens)
    {
        QuickTreeNode result;
        if (!containsMultiClassTokens) return;
        if (DetermineDocument().ExistsElementOfTypeInRange(Begin, End, out result))
        {
            if (TextReference != null && !Equals(TextReference.IsoEntity))
                TextReference.IsoEntity.Uncouple();
            TextReference = result;
            TextReference.IsoEntity = this;
        }
        else
        {
            if (TextReference != null && Equals(TextReference.IsoEntity))
                TextReference.IsoEntity = null;
            TextReference = null;
        }
    }*/

    public override void Actualize3DObject()
    {
        /*if (TokenObject != null)
        {
            if (TokenObject.AnnotationWindow != null)
            {
                if (Begin == 0 && End == 0)
                    TokenObject.AnnotationWindow.UpdateEmptyTokenContainer(true);
                else TokenObject.AnnotationWindow.UpdateTokenContainer();
            }
            //if (TokenObject.QuickAnnoTool != null)
            //    TokenObject.QuickAnnoTool.UpdateTokenContainer();
        }
        else if (TextReference != null && TextReference.TokenObjects != null)
        {
            TokenObject quickAnnoToken = null;
            TokenObject annoWindowToken = null;
            foreach (TokenObject to in TextReference.TokenObjects)
            {
                if (annoWindowToken == null && to.AnnotationWindow != null)
                    annoWindowToken = to;
                if (quickAnnoToken == null && to.QuickAnnoTool != null)
                    quickAnnoToken = to;

                if (quickAnnoToken != null && annoWindowToken != null)
                    break;
            }
            if (quickAnnoToken != null) quickAnnoToken.QuickAnnoTool.UpdateTokenContainer();
            if (annoWindowToken != null)
            {
                if (Begin == 0 && End == 0)
                    annoWindowToken.AnnotationWindow.UpdateEmptyTokenContainer(true);
                else annoWindowToken.AnnotationWindow.UpdateTokenContainer();
            }
        }

        if (InteractiveShapeNetObject != null)
        {
            InteractiveShapeNetObject.ActualizeLabel();
            Object3D.transform.position = Position.Vector;
            Object3D.transform.rotation = Rotation.Quaternion;
            Object3D.transform.localScale = Scale.Vector;
        }
        else
        {

        }
        // TODO do the object updates
*/
    }

    public HashSet<IsoEntity> AllRelatedQSLinkFigures()
    {
        HashSet<IsoEntity> allEntities = new HashSet<IsoEntity>();
        List<IsoEntity> quarry = new List<IsoEntity>();
        quarry.Add(this);
        while (quarry.Count > 0)
        {
            foreach (IsoLink link in quarry[0].LinkedVia.Keys)
            {
                if (link.GetType() == typeof(IsoQsLink) && link.Figure.ID != ID && !allEntities.Contains(link.Figure))
                {
                    quarry.Add(link.Figure);
                    allEntities.Add(link.Figure);
                }
            }
            quarry.RemoveAt(0);
        }
        return allEntities;
    }

    protected Dictionary<string, Dictionary<string, object>> updates;
    /*public virtual void Uncouple()
    {
        if (updates == null) updates = new Dictionary<string, Dictionary<string, object>>();
        else updates.Clear();

        updates.Add("" + ID, new Dictionary<string, object>() { { "begin", 0 }, { "end", 0 } });
        SceneController.GetInterface<TextAnnotatorInterface>().FireWorkBatchCommand(null, null, updates, null);
    }*/

    /*public void OverrideEntity(IsoEntity entity)
    {
        //"entity" gets overwritten by "this"
        if (updates == null) updates = new Dictionary<string, Dictionary<string, object>>();
        else updates.Clear();
        updates.Add("" + ID, new Dictionary<string, object>() { { "begin", entity.Begin }, { "end", entity.End } });

        List<string> toRemove = new List<string>();
        toRemove.Add("" + entity.ID);

        List<IsoLink> linkIDs = new List<IsoLink>(entity.LinkedVia.Keys);

        foreach (IsoLink link in linkIDs)
        {
            Dictionary<string, object> linkUpdate = new Dictionary<string, object>();
            linkUpdate.Add(entity.LinkedVia[link].ToString(), this.ID);
            updates.Add("" + link.ID, linkUpdate);
            entity.LinkedVia.Remove(link);
        }

        
        SceneController.GetInterface<TextAnnotatorInterface>().ChangeEventMap.Add((int)ID, (updated) =>
        {
            Debug.Log("ChangeEvent!");
            IsoEntity u = (IsoEntity)updated;
            if(u.InteractiveShapeNetObject != null)
                u.InteractiveShapeNetObject.RepositionAllCorespondingLinks();
            SceneController.GetInterface<TextAnnotatorInterface>().DeleteElements(toRemove);
        });
        SceneBuilderSceneScript.WaitingForResponse = true;
        SceneController.GetInterface<TextAnnotatorInterface>().FireWorkBatchCommand(null, null, updates, null);        
    }*/

    /*public void OverrideObj(IsoEntity entity)
    {
        //"entity" gets overwritten by "this".
        //mergeLinks or overwrite them
        if (updates == null) updates = new Dictionary<string, Dictionary<string, object>>();
        else updates.Clear();

        Dictionary<string, object> _attributes = new Dictionary<string, object>();
        _attributes.Add("object_id", Object_ID);
        _attributes.Add("position", Position.ID);
        _attributes.Add("rotation", Rotation.ID);
        _attributes.Add("scale", Scale.ID);

        // object features
        *//*
        _attributes.Add("object_feature_array", null);
        JsonData featureData = new JsonData();
        if (Object_Feature != null) 
        {
            foreach (IsoObjectAttribute feature in Object_Feature)
                featureData.Add("" + feature.ID);
            _attributes["object_feature_array"] =  featureData;
        }*//*

        updates.Add("" + entity.ID, _attributes);

        //List<string> toRemove = new List<string>();
        //toRemove.Add("" + ID);

        List<IsoLink> linkIDs = new List<IsoLink>(LinkedVia.Keys);
        foreach (IsoLink link in linkIDs)
        {
            Dictionary<string, object> linkUpdate = new Dictionary<string, object>();
            linkUpdate.Add(LinkedVia[link].ToString(), entity.ID);
            updates.Add("" + link.ID, linkUpdate);
        }


        SceneController.GetInterface<TextAnnotatorInterface>().ChangeEventMap.Add((int)entity.ID, (updated) =>
        {
            IsoEntity u = (IsoEntity)updated;
            if(u.Object3D != null)
                u.InteractiveShapeNetObject.Destroy();
            ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<ObjectTab>().CreateObject(u);
            u.InteractiveShapeNetObject.RepositionAllCorespondingLinks();

            SceneController.GetInterface<TextAnnotatorInterface>().DeleteElement("" + ID);
        });
        SceneBuilderSceneScript.WaitingForResponse = true;
        SceneController.GetInterface<TextAnnotatorInterface>().FireWorkBatchCommand(null, null, updates, null);
    }*/

    /*public virtual void DeleteObjRequest()
    {
        if (updates == null) updates = new Dictionary<string, Dictionary<string, object>>();
        else updates.Clear();

        Dictionary<string, object> _attributes = new Dictionary<string, object>();
        _attributes.Add("object_id", null);
        _attributes.Add("position", null);
        _attributes.Add("rotation", null);
        _attributes.Add("scale", null);
        _attributes.Add("object_feature_array", null);
        updates.Add("" + ID, _attributes);

        SceneController.GetInterface<TextAnnotatorInterface>().ChangeEventMap.Add((int)ID, (updated) =>
        {
            IsoEntity u = (IsoEntity)updated;
            if (u.Object3D != null)
                u.InteractiveShapeNetObject.Destroy();
        });

        SceneController.GetInterface<TextAnnotatorInterface>().FireWorkBatchCommand(null, null, updates, null);
    }*/
}


/// <summary>
/// Events sind in der Regel Verben im Text, die eine Handlung markieren
/// </summary>
public class IsoEvent : IsoEntity
{
    public const string PrettyName = "Event";
    public const string Description = "Event";
    public new static Color ClassColor { get; } = new Color(0, 1, 0);
    /*
     * Aus SRType
     */
    public string Event_Frame { get; private set; } //Verbsense
    public string Event_Type { get; private set; }

    /*
    * Aus IsoSpace
    */

    public string Domain { get; private set; }
    public string Lat { get; private set; }
    public string Lon { get; private set; }
    public IsoMeasure Elevation { get; private set; }
    public bool Countable { get; private set; }
    public string GQuant { get; private set; } 
    public List<IsoEntity> Scopes { get; private set; }

    public void SetEventFrame(string event_frame) { Event_Frame = event_frame; }
    public void SetEventType(string event_type) { Event_Type = event_type; }
    public void SetDomain(string domain) { Domain = domain; }
    public void SetLatitude(string latitude) { Lat = latitude; }
    public void SetLongitude(string longitude) { Lon = longitude; }
    public void SetElevation(IsoMeasure elevation) { Elevation = elevation; }
    public void SetCountable(bool countable) { Countable = countable; }
    public void SetGQuant(string gquant) { GQuant = gquant; }
    public void SetScopes(List<IsoEntity> scopes) { Scopes = scopes; }


    public IsoEvent(AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature,
        string comment, string mod,string event_frame, string event_type, string domain, string lat, string lon, IsoMeasure elevation, bool countable, string gquant, List<IsoEntity> scopes) : 
        base(parent, ID, begin, end, comment, mod, object_ID, position, rotation, scale, object_feature, AnnotationTypes.EVENT)
    {
        Event_Frame = event_frame;
        Event_Type = event_type;
        Domain = domain;
        Lat = lat;
        Lon = lon;
        Elevation = elevation;
        Countable = countable;
        GQuant = gquant;
        Scopes = scopes;
    }

    protected IsoEvent(AnnotationBase parent, int ID, int begin, int end,string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature,
        string comment, string mod, string event_frame, string event_type, string domain, string lat, string lon, IsoMeasure elevation, bool countable, string gquant, List<IsoEntity> scopes, string class_type) : 
        base(parent, ID, begin, end, comment, mod, object_ID, position, rotation, scale, object_feature, class_type)
    {
        Event_Frame = event_frame;
        Event_Type = event_type;
        Domain = domain;
        Lat = lat;
        Lon = lon;
        Elevation = elevation;
        Countable = countable;
        GQuant = gquant;
        Scopes = scopes;
    }

}


/// <summary>
/// Signalwörter sind meistens Pronomen, die Links triggern
/// </summary>
public class IsoSignal : IsoEntity
{
    public IsoSignal(AnnotationBase parent, int ID, int begin, int end,
        string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string comment, string mod) : 
        base(parent, ID, begin, end, comment, mod, object_ID, position, rotation, scale, object_feature, AnnotationTypes.SIGNAL)
    {
    }

    protected IsoSignal(AnnotationBase parent, int ID, int begin, int end,
        string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string comment, string mod, string class_type) : 
        base(parent, ID, begin, end, comment, mod, object_ID, position, rotation, scale, object_feature, class_type)
    {
    }
}


public class IsoLink : AnnotationBase
{   //TODO: Besitzt eigenlich weder Parent, noch brgin/end ....
    //Links erbt "nicht" von Entities. Links verbinden verschiedene Entities und haben dementsprechend auch kein Äquivalent im Text (Kein TextData)

    public enum Connected { figure, ground, trigger, reference_pt, beginID, endID, midID, startID}; //Die sollten so benannt werden, wie die UIMA Attribute, diese als "ToString()" weiterverwendet werden.
    public string Comment { get; private set; }
    public string Mod { get; private set; }

    // figure ---rel_type---> ground
    public IsoEntity Figure { get; private set; } 
    public IsoEntity Ground { get; private set; }

    public IsoEntity Trigger { get; private set; } //Was den Link auslöst. Meistens Events oder Signals
    public string Rel_Type { get; private set; } //Verschiedene Links haben verschiedene Relationstypen.

    public void SetFigure(IsoEntity figure) {
        if (this.Figure != null && this.Figure.LinkedVia.ContainsKey(this))
            this.Figure.LinkedVia.Remove(this);
        Figure = figure;
        if (Figure != null)
            this.Figure.LinkedVia[this] = IsoLink.Connected.figure;
    }
    public void SetGround(IsoEntity ground) {
        if (this.Ground != null && this.Ground.LinkedVia.ContainsKey(this))
            this.Ground.LinkedVia.Remove(this);
        Ground = ground;
        if (Ground != null)
            this.Ground.LinkedVia[this] = IsoLink.Connected.ground;
    }

    public void SetTrigger(IsoEntity trigger) {
        if (this.Trigger != null && this.Trigger.LinkedVia.ContainsKey(this))
            this.Trigger.LinkedVia.Remove(this);
        Trigger = trigger;
        if (Trigger != null)
            this.Trigger.LinkedVia[this] = IsoLink.Connected.trigger;
    }

    public void SetRelType(string type) { Rel_Type = type; }
    public void SetComment(string comment) { Comment = comment; }
    public void SetMod(string mod) { Mod = mod; }

    //public LinkAnnotationPanel Panel { get; set; }

    public static Color ClassColor { get; } = new Color(0, 0, 0);
    public IsoLink(AnnotationBase parent, int ID, string comment, string mod, IsoEntity figure, IsoEntity ground, IsoEntity trigger, string rel_type) : 
        base(ID, 0, 0, AnnotationTypes.LINK, parent)
    {
        this.Figure = figure;
        if (figure != null)
            this.Figure.LinkedVia[this] = IsoLink.Connected.figure;

        this.Ground = ground;
        if (ground != null)
            this.Ground.LinkedVia[this] = IsoLink.Connected.ground;

        this.Trigger = trigger;
        if (trigger != null)
            this.Trigger.LinkedVia[this] = IsoLink.Connected.trigger;

        this.Rel_Type = rel_type;
        this.Comment = comment;
        this.Mod = mod;
    }

    protected IsoLink(AnnotationBase parent, int ID, string comment, string mod, IsoEntity figure, IsoEntity ground, IsoEntity trigger, string rel_type, string class_type) : 
        base(ID, 0, 0, class_type, parent)
    {
        this.Figure = figure;
        if (figure != null)
            this.Figure.LinkedVia[this] = IsoLink.Connected.figure;

        this.Ground = ground;
        if (ground != null)
            this.Ground.LinkedVia[this] = IsoLink.Connected.ground;

        this.Trigger = trigger;
        if (trigger != null)
            this.Trigger.LinkedVia[this] = IsoLink.Connected.trigger;
            
        
        this.Rel_Type = rel_type;
        this.Comment = comment;
        this.Mod = mod;
    }

    /*public void CreateInteractiveLinkObject()
    {
        if (this is IsoMetaLink && Figure is IsoSpatialEntity iFigure && UMAISOEntity.AVATAR_TYPE_NAMES.Contains(iFigure.Object_ID))
            return;

        if (Figure.Object3D != null && Ground.Object3D != null)
        {
            GameObject _objectInstance = new GameObject("Link_" + ID);
            _objectInstance.SetActive(true);
            _objectInstance.AddComponent<InteractiveLinkObject>().Init(this);

            _objectInstance.transform.SetParent(((SceneBuilderSceneScript)SceneController.ActiveSceneScript).ObjectContainer.transform, true);

        }
    }

    public override void RemoveElement()
    {
        base.RemoveElement();
        if (Object3D != null)
        {
            if (Object3D.GetComponent<InteractiveLinkObject>() != null)
                Object3D.GetComponent<InteractiveLinkObject>().Destroy();
            else 
                Object.Destroy(Object3D);
        }

        if (Panel != null)
            Panel.Destroy();

        if (Figure != null && Figure.LinkedVia.ContainsKey(this))
        {
            Figure.LinkedVia.Remove(this);
            if (Figure.Panel != null && Figure.Panel.ActiveTab == AnnotationObjectPanel.PanelTab.Links)
                Figure.Panel.ChangeLinkPage();
        }

        if (Ground != null && Ground.LinkedVia.ContainsKey(this))
        {
            Ground.LinkedVia.Remove(this);
            if (Ground.Panel != null && Ground.Panel.ActiveTab == AnnotationObjectPanel.PanelTab.Links)
                Ground.Panel.ChangeLinkPage();
        }
            

        if (Trigger != null && Trigger.LinkedVia.ContainsKey(this))
        {
            Trigger.LinkedVia.Remove(this);
            if (Trigger.Panel != null && Trigger.Panel.ActiveTab == AnnotationObjectPanel.PanelTab.Links)
                Trigger.Panel.ChangeLinkPage();
        }
    }*/


    public override void Actualize3DObject()
    {
        //throw new System.NotImplementedException();
    }

    /*
    public override string ToString()
    {
        return " figure: " + Figure.ID + ", ground: " + Ground.ID;
    }*/

}