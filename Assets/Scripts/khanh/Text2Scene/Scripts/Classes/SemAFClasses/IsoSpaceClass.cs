using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
//using Text2Scene;
using UnityEngine;

/// <summary>
/// Alle Räumlichen Entitäten (Personen, objekte, ...)
/// </summary>
public class IsoSpatialEntity : IsoEntity
{
    public const string PrettyName = "Spatial Entity";
    public const string Description = "Spatial Entity";
    public enum FormType { none, nam, nom }
    public enum DimensionType { none, point, line, area, volume }
    public string Spatial_Entity_Type { get; private set; } //Person, ect ...
    public DimensionType Dimensionality { get; private set; } //Punkt, Linie, Fläche, Volumen
    public FormType Form { get; private set; } //Namentlich, Nominal
    public bool Dcl { get; private set; } //Blickpunkt
    public string Domain { get; private set; } //Content ======> nicht anzeigen
    public string Lat { get; private set; } //Koordinaten ======> als float-Feld
    public string Lon { get; private set; } //Koordinaten ======> als float-Feld
    public IsoMeasure Elevation { get; private set; }
    public bool Countable { get; private set; } 
    public string GQuant { get; private set; } //every, some, ... ======> nicht anzeigen
    public List<IsoEntity> Scopes { get; private set; } //every chair

    //TODO
    public int cardinality { get; private set; } // one, two chairs ... 

    /**
     * Für Raumgestaltung (IsoSpatial)
     */


    public new static Color ClassColor { get; } = new Color(1, 1, 0);

    public void SetSpatialEntityType(string type) { Spatial_Entity_Type = type; }
    public void SetDimensionality(DimensionType dim) { Dimensionality = dim; }
    public void SetForm(FormType form) { Form = form; }
    public void SetDcl(bool dcl) { Dcl = dcl; }
    public void SetDomain(string domain) { Domain = domain; }
    public void SetLat(string lat) { Lat = lat; }
    public void SetLongitude(string lon) { Lon = lon; }
    public void SetElevation(IsoMeasure elevation) { Elevation = elevation; }
    public void SetCountable(bool countable) { Countable = countable; }
    public void SetGQuant(string qQuant) { GQuant = qQuant; }
    public void SetScopes(List<IsoEntity> scopes) { Scopes = scopes; }

    public void SetCardinality(int card) { cardinality = card; }

    public IsoSpatialEntity(AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position, 
                            IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string comment, string mod,
                            string spatial_entity, DimensionType dim, FormType form, bool dcl, string domain, string lat, string lon,
                            IsoMeasure elevation, bool countable, string gquant, List<IsoEntity> scopes) : 
        base(parent, ID, begin, end, comment, mod, object_ID, position, rotation, scale, object_feature, AnnotationTypes.SPATIAL_ENTITY)
    {

        SetSpatialEntityType(spatial_entity);
        SetDimensionality(dim);
        SetForm(form);
        Dcl = dcl;
        Domain = domain;
        Lat = lat;
        Lon = lon;
        Elevation = elevation;
        Countable = countable;
        GQuant = gquant;
        Scopes = scopes;
        
    }

    public IsoSpatialEntity(AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position,
                            IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string comment, string mod,
                            string spatial_entity, DimensionType dim, FormType form, bool dcl, string domain, string lat, string lon,
                            IsoMeasure elevation, bool countable, string gquant, List<IsoEntity> scopes, bool containsMultiClassTokens) :
        base(parent, ID, begin, end, comment, mod, object_ID, position, rotation, scale, object_feature, AnnotationTypes.SPATIAL_ENTITY, containsMultiClassTokens)
    {

        SetSpatialEntityType(spatial_entity);
        SetDimensionality(dim);
        SetForm(form);
        Dcl = dcl;
        Domain = domain;
        Lat = lat;
        Lon = lon;
        Elevation = elevation;
        Countable = countable;
        GQuant = gquant;
        Scopes = scopes;

    }

    public IsoSpatialEntity(AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position,
                            IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string type, string comment, string mod,
                            string spatial_entity, DimensionType dim, FormType form, bool dcl, string domain, string lat, string lon,
                            IsoMeasure elevation, bool countable, string gquant, List<IsoEntity> scopes) :
        base(parent, ID, begin, end, comment, mod, object_ID, position, rotation, scale, object_feature, type)
    {
        SetSpatialEntityType(spatial_entity);
        SetDimensionality(dim);
        SetForm(form);
        Dcl = dcl;
        Domain = domain;
        Lat = lat;
        Lon = lon;
        Elevation = elevation;
        Countable = countable;
        GQuant = gquant;
        Scopes = scopes;
    }


    public override string ToString()
    {
        return "Object: " + ID + ", Type: " + GetType() + "\nPos: " + Position + "\nRot: " + Rotation + "\nScale: " + Scale;
    }

}

//*********************************************************//
//                      LOCATION TYPES
//*********************************************************//

/// <summary>
/// Konkrete Örtlichkeiten (wird nicht direkt verwendet)
/// </summary>
public class IsoLocation : IsoSpatialEntity
{
    public new const string PrettyName = "Location";
    public string Gazref { get; private set; } // ======> anzeigen evtl. mit magnifier

    public new static Color ClassColor { get; } = new Color(1, 1, 0.25f);

    public void SetGazref(string gazref) { Gazref = gazref; }

    public IsoLocation(AnnotationBase parent, int ID, int begin, int end, string comment, string mod, string Object_ID,
                       IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string gazref) :
        base(parent, ID, begin, end, Object_ID, position, rotation, scale, object_feature, AnnotationTypes.LOCATION,
             comment, mod, null, DimensionType.none, FormType.none, false, null, null, null, null, false, null, null)
    {
        Gazref = gazref;
    }

    public IsoLocation(AnnotationBase parent, int ID, int begin, int end, string comment, string mod, string Object_ID,
                       IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string gazref, string type) :
        base(parent, ID, begin, end, Object_ID, position, rotation, scale, object_feature,
             type, comment, mod, null, DimensionType.none, FormType.none, false, null, null, null, null, false, null, null)
    {
        Gazref = gazref;
    }
}

/// <summary>
/// Path ...
/// </summary>
public class IsoLocationPath : IsoLocation
{
    public new const string PrettyName = "Path";
    public new const string Description = "Location";
    public enum EntityType { none, waterway, railway, bridge, tunnel, road, lane, passage, trail, boundary, barrier, margin, row, conduit, filament, mtn, mts }
    public IsoEntity BeginID { get; private set; }
    public IsoEntity EndID { get; private set; }
    public List<IsoEntity> MidIDs { get; private set; }
    public new static Color ClassColor { get; } = new Color(1, 1, 0.75f);

    public void SetStartID(IsoEntity beginID)
    {
        if (BeginID != null && BeginID.LinkedDirect.ContainsKey(this))
            this.BeginID.LinkedDirect.Remove(this);
        BeginID = beginID;
        if (BeginID != null)
            this.BeginID.LinkedDirect[this] = IsoLink.Connected.beginID;
    }

    public void SetEndID(IsoEntity endID)
    {
        if (EndID != null && EndID.LinkedDirect.ContainsKey(this))
            this.EndID.LinkedDirect.Remove(this);
        EndID = endID;
        if (EndID != null)
            this.EndID.LinkedDirect[this] = IsoLink.Connected.endID;
    }

    public void SetMidIDs(List<IsoEntity> midIDs)
    {
        if (MidIDs != null)
            foreach (IsoEntity ent in MidIDs)
                if (ent != null && ent.LinkedDirect.ContainsKey(this))
                    ent.LinkedDirect.Remove(this);
        MidIDs = midIDs;
        if (MidIDs != null)
            foreach (IsoEntity ent in MidIDs)
                if (ent != null && ent.LinkedDirect.ContainsKey(this))
                    ent.LinkedDirect[this] = IsoLink.Connected.midID;
    }

    public IsoLocationPath(AnnotationBase parent, int ID, int begin, int end, string comment, string mod, string Object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string gazref, IsoEntity beginID, IsoEntity endID, List<IsoEntity> mids) :
        base(parent, ID, begin, end, comment, mod, Object_ID, position, rotation, scale, object_feature, gazref, AnnotationTypes.PATH)
    {
        BeginID = beginID;
        if (beginID != null)
            beginID.LinkedDirect[this] = IsoLink.Connected.beginID;

        EndID = endID;
        if (beginID != null)
            beginID.LinkedDirect[this] = IsoLink.Connected.endID;

        MidIDs = mids;
        if(MidIDs != null)
            foreach (IsoEntity ent in MidIDs)
            {
                Debug.Log(ent);
                if (ent != null)
                    ent.LinkedDirect[this] = IsoLink.Connected.midID;
            }
    }
}

/// <summary>
/// Konkreter Ort (Deutschland, Frankfurt, Eckkneipe)
/// </summary>
public class IsoLocationPlace : IsoLocation
{
    public new const string PrettyName = "Place";
    public new const string Description = "Location";
    public enum CTV { none, city, town, village }
    public enum ContinentType { NONE, AF, AN, AS, EU, NA, SA, OC }
    public static Dictionary<ContinentType, string> ContinentFormatMap = new Dictionary<ContinentType, string>()
    {
        { ContinentType.NONE, "-" }, { ContinentType.AF, "Africa " }, { ContinentType.AN, "Antarctica " }, { ContinentType.AS, "Asia" },
        { ContinentType.EU, "Europe" }, { ContinentType.NA, "North America" }, { ContinentType.SA, "South America" }, { ContinentType.OC, "Oceania" },
    };

    public string Country { get; private set; }
    public string State { get; private set; }
    public CTV Ctv { get; private set; }
    public ContinentType Continent { get; private set; }
    public string County { get; private set; }
    public new static Color ClassColor { get; } = new Color(1, 0.75f, 0.25f) ;

    public void SetCountry(string country) { Country = country; }
    public void SetState(string state) { State = state; }
    public void SetCtv(CTV ctv) { Ctv = ctv; }
    public void SetContinent(ContinentType continent) { Continent = continent; }
    public void SetCounty(string county) { County = county; }



    public IsoLocationPlace(AnnotationBase parent, int ID, int begin, int end, string comment, string mod, string Object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string gazref, string country, string state, CTV ctv, ContinentType continent, string county) :
        base(parent, ID, begin, end, comment, mod, Object_ID, position, rotation, scale, object_feature, gazref, AnnotationTypes.PLACE)
    {
        Country = country;
        State = state;
        Ctv = ctv;
        Continent = continent;
        County = county;
    }
}

/// <summary>
/// Nach IsoSpaceV2 standert. Auslagerung aus dem Link
/// </summary>
public class IsoEventPath : IsoLocation
{
    public new const string PrettyName = "EventPath";
    public new const string Description = "EventPath";
    public IsoMotion Trigger { get; private set; }
    public IsoEntity StartID { get; private set; }
    public List<IsoEntity> MidIDs { get; private set; }
    public IsoEntity EndID { get; private set; }
    public List<IsoSRelation> Spatial_Relator { get; private set; }
    public new static Color ClassColor { get; } = new Color(1, 1, 0.25f);

    public void SetTrigger(IsoMotion trigger) { Trigger = trigger; }
    public void SetStartID(IsoEntity beginID)
    {
        if (StartID != null && StartID.LinkedDirect.ContainsKey(this))
            this.StartID.LinkedDirect.Remove(this);
        StartID = beginID;
        if (StartID != null)
            this.StartID.LinkedDirect[this] = IsoLink.Connected.startID;
    }

    public void SetEndID(IsoEntity endID)
    {
        if (EndID != null && EndID.LinkedDirect.ContainsKey(this))
            this.EndID.LinkedDirect.Remove(this);
        EndID = endID;
        if (EndID != null)
            this.EndID.LinkedDirect[this] = IsoLink.Connected.endID;
    }

    public void SetMidIDs(List<IsoEntity> midIDs)
    {
        if (MidIDs != null)
            foreach (IsoEntity ent in MidIDs)
                if (ent != null && ent.LinkedDirect.ContainsKey(this))
                    ent.LinkedDirect.Remove(this);
        MidIDs = midIDs;
        if (MidIDs != null)
            foreach (IsoEntity ent in MidIDs)
                if (ent != null && ent.LinkedDirect.ContainsKey(this))
                    ent.LinkedDirect[this] = IsoLink.Connected.midID;
    }

    public void SetSpatialRelator(List<IsoSRelation> spatialRelator) { Spatial_Relator = spatialRelator; }

    public IsoEventPath(AnnotationBase parent, int ID, int begin, int end, string comment, string mod, string Object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, 
                        string gazref, IsoMotion trigger, IsoEntity beginID, List<IsoEntity> midIDs, IsoEntity endID, List<IsoSRelation> spatial_relator) :
        base(parent, ID, begin, end, comment, mod, Object_ID, position, rotation, scale, object_feature, gazref, AnnotationTypes.EVENT_PATH)
    {
        Trigger = trigger;
        StartID = beginID;
        if (beginID != null)
            beginID.LinkedDirect[this] = IsoLink.Connected.beginID;
        EndID = endID;
        if (beginID != null)
            beginID.LinkedDirect[this] = IsoLink.Connected.endID;
        MidIDs = midIDs;
        if (MidIDs != null)
            foreach (IsoEntity ent in midIDs)
                if (ent != null)
                    ent.LinkedDirect[this] = IsoLink.Connected.midID;
        Spatial_Relator = spatial_relator;
    }


}

//*********************************************************//
//                      SIGNAL TYPES
//*********************************************************//

/// <summary>
/// Massangeben (z.B. 3m)
/// </summary>
public class IsoMeasure : IsoSignal
{
    public const string PrettyName = "Measure";
    public const string Description = "Measure";
    public string Value { get; private set; }
    public string Unit { get; private set; }
    public new static Color ClassColor { get; } = new Color(1, 0 , 0);

    public void SetValue(string value) { Value = value; }
    public void SetUnit(string unit) { Unit = unit; }

    public IsoMeasure(AnnotationBase parent, int ID, int begin, int end,
        string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature,string comment, string mod, string value, string unit) : 
        base(parent, ID, begin, end, object_ID, position, rotation, scale, object_feature, comment, mod, AnnotationTypes.MEASURE)
    {
        Value = value;
        Unit = unit;
    }

}

/*
/// <summary>
/// Ortssignal (auf, unter, nördlich)
/// </summary>
public class IsoSpatialSignal : IsoSignal
{
    public const string PrettyName = "Spatial signal";
    public const string Description = "Spatial signal";
    public string Cluster { get; private set; } //Bedeutung
    public string Semantic_Type { get; private set; }
    public new static Color ClassColor { get; } = new Color(0.5f, 0, 0f);

    public void SetCluster(string cluster) { Cluster = cluster; }
    public void SetSemanticType(string semanticType) { Semantic_Type = semanticType; }

    public IsoSpatialSignal(AnnotationBase parent, int ID, int begin, int end, string comment, string mod, string cluster, string semantic_type) :
        base(parent, ID, begin, end, comment, mod, AnnotationTypes.SPATIAL_SIGNAL)
    {
        Cluster = cluster;
        Semantic_Type = semantic_type;
    }

}*/

/*
/// <summary>
/// Signalwörter (z.B. ging "huepfend")
/// </summary>
public class IsoMotionSignal : IsoSignal
{
    public const string PrettyName = "Motion signal";
    public const string Description = "Motion signal";
    public string Motion_Signal_Type { get; private set; }
    public new static Color ClassColor { get; } = new Color(1, 0.5f, 0.5f);

    public void SetMotionSignalType(string motionSignalType) { Motion_Signal_Type = motionSignalType; }

    public IsoMotionSignal(AnnotationBase parent, int ID, int begin, int end, string comment, string mod, string motion_signal_type) :
        base(parent, ID, begin, end, comment, mod, AnnotationTypes.MOTION_SIGNAL)
    {
        Motion_Signal_Type = motion_signal_type;
    }
}*/

/// <summary>
/// Signalwort
/// </summary>
public class IsoSRelation : IsoSignal
{
    public const string PrettyName = "SRelation";
    public const string Description = "SRelation";
    public string Type { get; private set; }
    public string Cluster { get; private set; }
    public string Value { get; private set; }
    public new static Color ClassColor { get; } = new Color(1, 0.5f, 0.5f);

    public void SetSignalType(string signalType) { Type = signalType; }
    public void SetCluster(string cluster) { Cluster = cluster; }
    public void SetValue(string value) { Value = value; }

    public IsoSRelation(AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature
, string comment, string mod, string signal_type, string cluster, string value) :
        base(parent, ID, begin, end, object_ID, position, rotation, scale, object_feature, comment, mod, AnnotationTypes.SRELATION)
    {
        Type = signal_type;
        Cluster = cluster;
        Value = value;
    }
}

/// <summary>
/// Signalwort Measure Relationen
/// </summary>
public class IsoMRelation : IsoSignal
{
    public const string PrettyName = "MRelation";
    public const string Description = "MRelation";
    public string Value { get; private set; }
    public new static Color ClassColor { get; } = new Color(0.75f, 0.25f, 0.25f);

    public void SetValue(string value) { Value = value; }

    public IsoMRelation(AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature
, string comment, string mod, string value) :
        base(parent, ID, begin, end, object_ID, position, rotation, scale, object_feature, comment, mod, AnnotationTypes.SRELATION)
    {

        Value = value;
    }
}

//*********************************************************//
//                      EVENT TYPES
//*********************************************************//

/// <summary>
/// Events, die eine Bewegung ausdrücken
/// </summary>
public class IsoMotion : IsoEvent
{
    public new const string PrettyName = "Motion";
    public new const string Description = "Motion";
    public new static Color ClassColor { get; } = new Color(0f, 1, 0f);

    // the next attributes should be eventually also enums later
    public string Motion_Type { get; private set; }
    public string Motion_Class { get; private set; }
    public string Motion_Sense { get; private set; }
    public IsoEntity Manner { get; private set; } //IsoSpaceV2
    public IsoSpatialEntity MotionGoal { get; private set; } //IsoSpaceV2

    public void SetMotionType(string motion_type) { Motion_Type = motion_type; }
    public void SetMotionClass(string motion_class) { Motion_Class = motion_class; }
    public void SetMotionSense(string motion_sense) { Motion_Sense = motion_sense; }
    public void SetManner(IsoEntity manner) { Manner = manner; }
    public void SetGoal(IsoSpatialEntity goal) { MotionGoal = goal; }

    public IsoMotion(AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature
, string comment, string mod, string event_frame, string event_type, string domain, string lat, string lon, IsoMeasure elevation, bool countable, string gquant, List<IsoEntity> scopes, string motion_type, string motion_class, string motion_sense, IsoEntity manner, IsoSpatialEntity goal) : 
        base(parent, ID, begin, end, object_ID, position, rotation, scale, object_feature, comment, mod, event_frame, event_type, domain, lat, lon, elevation, countable, gquant, scopes, AnnotationTypes.MOTION)
    {
        Motion_Type = motion_type;
        Motion_Class = motion_class;
        Motion_Sense = motion_sense;
        Manner = manner;
        MotionGoal = goal;
    }

}

/// <summary>
/// Events, die keine Bewegung ausdrücken
/// </summary>
public class IsoNonMotionEvent : IsoEvent
{
    public new const string PrettyName = "NonMotionEvent";
    public new const string Description = "NonMotionEvent";
    public new static Color ClassColor { get; } = new Color(0.25f, 0.75f, 0.25f);

    public IsoNonMotionEvent(AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature
, string comment, string mod, string event_frame, string event_type, string domain, string lat, string lon, IsoMeasure elevation, bool countable, string gquant, List<IsoEntity> scopes) :
        base(parent, ID, begin, end, object_ID, position, rotation, scale, object_feature, comment, mod, event_frame, event_type, domain, lat, lon, elevation, countable, gquant, scopes, AnnotationTypes.NON_MOTION_EVENT)
    {
    }

}

//*********************************************************//
//                      LINK TYPES
//*********************************************************//

/// <summary>
/// Qualitative Spatial Link
/// </summary>
public class IsoQsLink : IsoLink
{
    public new static Color ClassColor { get; } = StolperwegeHelper.GUCOLOR.SENFGELB;
    public IsoQsLink(AnnotationBase parent, int ID, string comment, string mod, IsoEntity figure, IsoEntity ground, IsoEntity trigger, string rel_type) :
        base(parent, ID, comment, mod, figure, ground, trigger, rel_type, AnnotationTypes.QSLINK)
    {
    }
}

/// <summary>
/// Orientation link
/// </summary>
public class IsoOLink : IsoLink
{
    public bool Projective { get; private set; }
    public string Frame_Type { get; private set; }
    public IsoEntity Reference_Point { get; private set; }

    public void SetProjective(bool projective) { Projective = projective; }
    public void SetFrameType(string frameType) { Frame_Type = frameType; }
    public void SetReferencePoint(IsoEntity refPoint) {
        if (this.Reference_Point != null && this.Reference_Point.LinkedVia.ContainsKey(this))
            this.Reference_Point.LinkedVia.Remove(this);
        Reference_Point = refPoint;
        if (Reference_Point != null)
            this.Reference_Point.LinkedVia[this] = IsoLink.Connected.reference_pt;
    }

    public new static Color ClassColor { get; } = StolperwegeHelper.GUCOLOR.MAGENTA;
    public IsoOLink(AnnotationBase parent, int ID, string comment, string mod, IsoEntity figure, IsoEntity ground, IsoEntity trigger, string rel_type, bool projective, string frame_type, IsoEntity reference_point) :
        base(parent, ID, comment, mod, figure, ground, trigger, rel_type, AnnotationTypes.OLINK)
    {
        Projective = projective;
        Frame_Type = frame_type;
        Reference_Point = reference_point;
        if (reference_point != null)
            this.Reference_Point.LinkedVia[this] = IsoLink.Connected.reference_pt;
    }


}

/// <summary>
/// Verlinkung von Measure mit der entsprechenden Entity
/// </summary>
public class IsoMLink : IsoLink
{
    public List<IsoEntity> Bounds { get; private set; }

    public new static Color ClassColor { get; } = StolperwegeHelper.GUCOLOR.PURPLE;
    public IsoMLink(AnnotationBase parent, int ID, string comment, string mod, IsoEntity figure, IsoEntity ground, IsoEntity trigger, string rel_type, List<IsoEntity> bounds) :
        base(parent, ID, comment, mod, figure, ground, trigger, rel_type, AnnotationTypes.MLINK)
    {
        Bounds = bounds;
    }

}

/// <summary>
/// Signalwörter (z.B. ging "huepfend")
/// </summary>
public class IsoMoveLink : IsoLink
{
    public IsoEntity MoveSource { get; private set; }
    public IsoEntity Goal { get; private set; }
    public List<IsoEntity> Mid_Points { get; private set; }
    public IsoLocationPath Path_ID { get; private set; }
    public IsoSRelation Adjunct_ID { get; private set; }
    public IsoSRelation Motionsignal_ID { get; private set; }
    public string Goal_Reached { get; private set; }

    public IsoMoveLink(AnnotationBase parent, int ID, string comment, string mod, IsoEntity figure, IsoEntity ground, IsoEntity trigger, string rel_type,
        IsoEntity source, IsoEntity goal, List<IsoEntity> mids, IsoLocationPath pathid, IsoSRelation adjunct_id, IsoSRelation signal, string goal_reached) :
        base(parent, ID, comment, mod, figure, ground, trigger, rel_type, AnnotationTypes.MOVELINK)
    {
        MoveSource = source;
        Goal = goal;
        Mid_Points = mids;
        Path_ID = pathid;
        Adjunct_ID = adjunct_id;
        Motionsignal_ID = signal;
        Goal_Reached = goal_reached;
    }
}




