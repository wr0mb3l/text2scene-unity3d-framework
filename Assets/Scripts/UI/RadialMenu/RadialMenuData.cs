using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RadialMenuData
{
    public enum MenuType
    {
        Link, SpatialEntity, Form, Dimension, Ctv, Continent, OLinkType, QsLinkType,
        IsoEntities, IsoLinks, IsoSRelationTypes, IsoMotionType, IsoMotionClass, IsoMotionSense,
        IsoSpatialEntityTypes, IsoPlaceTypes, IsoPathTypes,
        IsoSpatialDimension, IsoSpatialForm, IsoPlaceCTV, IsoPlaceContinent
    }

    public static Dictionary<MenuType, List<RadialSection>> m_radialMenuMap = new Dictionary<MenuType, List<RadialSection>>();
    public static Dictionary<MenuType, List<RadialSection>> RadialMenuMap
    {
        get
        {
            if (m_radialMenuMap == null) Init();
            return m_radialMenuMap;
        }
    }


    static RadialMenuData()
    {
        Init();
    }

    private static void Init()
    {
        m_radialMenuMap = new Dictionary<MenuType, List<RadialSection>>();
        // Link menu
        #region
        //MainSection
        List<RadialSection> linkMenuData = new List<RadialSection>();

        RadialSection qsLinkSection = new RadialSection("QsLink", "Qualitative Spatial Link", null);


        RadialSection oLinkSection = new RadialSection("OLink", "Orientation Link", null);

        linkMenuData.Add(null);
        linkMenuData.Add(qsLinkSection);
        linkMenuData.Add(oLinkSection);

        // QsLink Section 
        List<RadialSection> qsLinkSections = new List<RadialSection>()
        {
            new RadialSection("DC", "QsLink", null), new RadialSection("EC", "QsLink", null),
            new RadialSection("PO", "QsLink", null), new RadialSection("TPP", "QsLink", null),
            new RadialSection("NTTP", "QsLink", null), new RadialSection("EQ", "QsLink", null),
            new RadialSection("IN", "QsLink", null), new RadialSection("RETURN", "to MainMenu", linkMenuData)
        };//oLinkSection
        List<RadialSection> oLinkSections = new List<RadialSection>()
        {
            new RadialSection("Other", "OLink", null), new RadialSection("On", "OLink", null),
            new RadialSection("Above", "OLink", null), new RadialSection("Under", "OLink", null),
            new RadialSection("Next To", "OLink", null), new RadialSection("Behind", "OLink", null),
            new RadialSection("In Front of", "OLink", null), new RadialSection("RETURN", "to MainMenu", linkMenuData)
        };

        qsLinkSection.childSections = qsLinkSections;
        oLinkSection.childSections = oLinkSections;

        m_radialMenuMap.Add(MenuType.OLinkType, oLinkSections);
        m_radialMenuMap.Add(MenuType.QsLinkType, qsLinkSections);
        m_radialMenuMap.Add(MenuType.Link, linkMenuData);
        #endregion

        // SpatialEntity menu
        #region
        List<RadialSection> spatialSection = new List<RadialSection>()
        {
            new RadialSection("Spatial Entity", "", null),
            new RadialSection("Location Path", "", null),
            new RadialSection("Location Place", "", null),
            new RadialSection("Event Path","", null)
        };

        m_radialMenuMap.Add(MenuType.SpatialEntity, spatialSection);
        #endregion

        // string prettyTitle;


        // SpatialEntity-dimensions menu
        #region
        List<RadialSection> dimensionSection = new List<RadialSection>();
        /*
        List<DT> dtValues = new List<DT>(Enum.GetValues(typeof(DT)).Cast<DT>());
        foreach (DT value in dtValues)
        {
            prettyTitle = char.ToUpper(value.ToString()[0]) + value.ToString().Substring(1);
            dimensionSection.Add(new RadialSection(prettyTitle, null, null, value));
        }
        */

        m_radialMenuMap.Add(MenuType.Dimension, dimensionSection);
        #endregion

        // SpatialEntity-forms menu
        #region
        List<RadialSection> formSection = new List<RadialSection>();
        /*
        prettyTitle = char.ToUpper(FT.none.ToString()[0]) + FT.none.ToString().Substring(1);
        formSection.Add(new RadialSection(prettyTitle, null, null, FT.none));
        formSection.Add(new RadialSection(FT.nam.ToString(), "By name", null, FT.nam));
        formSection.Add(new RadialSection(FT.nom.ToString(), "Nominally", null, FT.nam));
        */

        m_radialMenuMap.Add(MenuType.Form, formSection);
        #endregion

        // Ctv
        #region
        List<RadialSection> ctvSection = new List<RadialSection>();
        /*
        List<CTV> ctvValues = new List<CTV>(Enum.GetValues(typeof(CTV)).Cast<CTV>());
        foreach (CTV value in ctvValues)
        {
            prettyTitle = char.ToUpper(value.ToString()[0]) + value.ToString().Substring(1);
            ctvSection.Add(new RadialSection(prettyTitle, null, null, value));
        }
        */

        m_radialMenuMap.Add(MenuType.Ctv, ctvSection);
        #endregion

        // Continent
        #region
        List<RadialSection> continentSection = new List<RadialSection>();
        /*
        List<CT> continentValues = new List<CT>(Enum.GetValues(typeof(CT)).Cast<CT>());
        foreach (CT value in continentValues)
        {
            continentSection.Add(new RadialSection(value.ToString(), IsoLocationPlace.ContinentFormatMap[value], null, value));
        }
        */

        m_radialMenuMap.Add(MenuType.Continent, continentSection);
        #endregion


        Init_IsoLinks();

        Init_IsoEntities();

        Init_SRelationTypes();

        Init_MotionTypes();

        Init_MotionClasses();

        Init_MotionSenses();

        //////////////////////

        Init_SpatialEntityTypes();

        Init_LocationTypes();

        Init_PathTypes();

        Init_SpatialDimensions();

        Init_SpatialForm();

        /////

        Init_PlaceCTV();

        Init_PlaceContinent();
    }
    private static void Init_IsoLinks()
    {
        List<RadialSection> linkingSection = new List<RadialSection>();


        // QsLink Section 
        List<RadialSection> qsLinkingSections = new List<RadialSection>()
        {
            new RadialSection("DC", "QsLink", null), new RadialSection("EC", "QsLink", null),
            new RadialSection("PO", "QsLink", null), new RadialSection("TPP", "QsLink", null),
            new RadialSection("RETURN", "to MainMenu", linkingSection), new RadialSection("NTTP", "QsLink", null),
            new RadialSection("EQ", "QsLink", null),new RadialSection("IN", "QsLink", null)
        };

        List<RadialSection> mLinkingSections = new List<RadialSection>()
        {
            new RadialSection("Other", "MLink", null), new RadialSection("distance", "MLink", null),
            new RadialSection("length", "MLink", null), new RadialSection("width", "MLink", null),
            new RadialSection("RETURN", "to MainMenu", linkingSection), new RadialSection("height", "MLink", null),
            new RadialSection("generalDimension", "MLink", null), null
        };

        List<RadialSection> srLinkingSections = new List<RadialSection>()
        {
            new RadialSection("Other", "SRLink", null), new RadialSection("Arg0", "SRLink", null),
            new RadialSection("Arg1", "SRLink", null), new RadialSection("Arg2", "SRLink", null),
            new RadialSection("RETURN", "to MainMenu", linkingSection), new RadialSection("Arg3", "SRLink", null),
            new RadialSection("Arg4", "SRLink", null), new RadialSection("Arg5", "SRLink", null)
        };

        List<RadialSection> metaLinkingSections = new List<RadialSection>()
        {
            new RadialSection("Other", "SRLink", null), new RadialSection("Coreference", "metaLink", null),
            new RadialSection("SubCoreference", "metaLink", null), new RadialSection("SplitCoreference", "metaLink", null),
            new RadialSection("RETURN", "to MainMenu", linkingSection), null,
            null, new RadialSection("PartCoreference", "metaLink", null),
        };


        //////////////////////////////////////////////////////

        List<RadialSection> oLinkingSections = new List<RadialSection>();


        List<RadialSection> oLinkingAbsoluteSections = new List<RadialSection>()
        {
            new RadialSection("North", "absolut", null), new RadialSection("NorthEast", "absolut", null),
            new RadialSection("East", "absolut", null), new RadialSection("SouthEast", "absolut", null),
            new RadialSection("South", "absolut", null), new RadialSection("SouthWest", "absolut", null),
            new RadialSection("West", "absolut", null), new RadialSection("NorthWest", "absolut", null)
        };

        List<RadialSection> oLinkingIntrinsicSections = new List<RadialSection>()
        {
            new RadialSection("Other", "intrinsic", null), new RadialSection("ON", "intrinsic", null),
            new RadialSection("ABOVE", "intrinsic", null), new RadialSection("BENEATH", "intrinsic", null),
            new RadialSection("RETURN", "to MainMenu", oLinkingSections),  new RadialSection("BELOW", "intrinsic", null),
            new RadialSection("NEXT_TO", "intrinsic", null), new RadialSection("IN_FRONT_OF", "intrinsic", null), 
            // new RadialSection("BEHIND", "intrinsic", null, AnnotationTypes.OLINK), new RadialSection("NEAR", "intrinsic", null, AnnotationTypes.OLINK)
        };

        List<RadialSection> oLinkingRelativeSections = new List<RadialSection>()
        {
            new RadialSection("Other", "relative", null), new RadialSection("NEXT_TO", "relative", null),
            new RadialSection("LEFT", "relative", null), new RadialSection("RIGHT", "relative", null),
            new RadialSection("RETURN", "to MainMenu", oLinkingSections),  new RadialSection("IN_FRONT_OF", "relative", null),
            new RadialSection("BEHIND", "relative", null), new RadialSection("ACROSS", "relative", null)
        };

        List<RadialSection> oLinkingUndefinedSections = new List<RadialSection>()
        {
            new RadialSection("Other", "undefined", null), new RadialSection("NEXT_TO", "undefined", null),
            new RadialSection("LEFT", "undefined", null), new RadialSection("RIGHT", "undefined", null),
            new RadialSection("RETURN", "to MainMenu", oLinkingSections),  new RadialSection("IN_FRONT_OF", "undefined", null),
            new RadialSection("BEHIND", "undefined", null), new RadialSection("ACROSS", "undefined", null)
        };

        oLinkingSections.Add(null);
        oLinkingSections.Add(new RadialSection("Absolut", "OLink", oLinkingAbsoluteSections));
        oLinkingSections.Add(new RadialSection("Intrinsic", "OLink", oLinkingIntrinsicSections));
        oLinkingSections.Add(null);
        oLinkingSections.Add(new RadialSection("RETURN", "to MainMenu", linkingSection));
        oLinkingSections.Add(null);
        oLinkingSections.Add(new RadialSection("Relative", "OLink", oLinkingRelativeSections));
        oLinkingSections.Add(new RadialSection("Undefined", "OLink", oLinkingUndefinedSections));


        ///////////////////////////////////////////////////////

        List<RadialSection> overwriteSections = new List<RadialSection>()
        {
            null,
            new RadialSection("3DObject", "Overwrite", null),
            null,
            null,
            new RadialSection("RETURN", "to MainMenu", linkingSection),
            null,
            null,
            new RadialSection("Entity", "Overwrite", null),
        };


        linkingSection.Add(new RadialSection("Overwrite", "Overwrite", overwriteSections));  //Titel ist wichtig. Wird wo anders als Abfrage verwendet
        linkingSection.Add(new RadialSection("QsLink", "QsLink", qsLinkingSections));
        linkingSection.Add(new RadialSection("OLink", "OLink", oLinkingSections));
        linkingSection.Add(new RadialSection("MeasureLink", "MeasureLink", mLinkingSections));
        linkingSection.Add(new RadialSection("CANCEL", "Close the menu", null));
        linkingSection.Add(new RadialSection("SrLink", "SrLink", srLinkingSections));
        linkingSection.Add(new RadialSection("MetaLink", "MetaLink", metaLinkingSections));
        //linkingSection.Add(new RadialSection("Search 3D Object", "Search 3D Object", null));

        m_radialMenuMap.Add(MenuType.IsoLinks, linkingSection);
    }


    private static void Init_IsoEntities()
    {
        List<RadialSection> isoEntitiesSection = new List<RadialSection>();



        // SpatialEntity menu
        #region
        List<RadialSection> spatialEntitiesSection = new List<RadialSection>()
        {
            null,
            new RadialSection("Spatial Entitiy", "Spatial Entity", null),
            new RadialSection("Location", "Location", null),
            new RadialSection("Location Place", "Location Place", null),

            new RadialSection("RETURN", "to MainMenu",isoEntitiesSection,null),
            null,
            new RadialSection("Event Path", "Event Path", null),
            new RadialSection("Location Path", "Location Path", null)
        };


        List<RadialSection> sRelationsSection = new List<RadialSection>()
        {
            null,
            new RadialSection("topological", "topological", null),
            new RadialSection("directional", "directional", null),
            new RadialSection("topoDirectional", "topoDirectional", null),
            new RadialSection("CANCEL", "to MainMenu",isoEntitiesSection,null),
            new RadialSection("manner", "manner", null),
            new RadialSection("goalDefining", "goalDefining", null),
            new RadialSection("pathDefining", "pathDefining", null)
        };


        List<RadialSection> measuresSection = new List<RadialSection>()
        {
            null,
            null,
            new RadialSection("Measure", "Measure", null),
            null,
            new RadialSection("RETURN", "Close the menu",isoEntitiesSection,null),
            null,
            new RadialSection("MRelation", "MRelation", null)
        };



        List<RadialSection> eventsSection = new List<RadialSection>()
        {
            null,
            null,
            new RadialSection("Motion", "Motion", null),
            null,
            new RadialSection("RETURN", "to MainMenu",isoEntitiesSection,null),
            null,
            new RadialSection("Non Motion Event", "Non Motion Event", null)
        };
        #endregion

        isoEntitiesSection.Add(new RadialSection("Multitoken", "Build Multitoken", null));
        isoEntitiesSection.Add(new RadialSection("SpatialEntities", "Spatial Entitie of any kind.", spatialEntitiesSection));
        isoEntitiesSection.Add(new RadialSection("sRelation", "Signal words of any kind.", sRelationsSection));
        isoEntitiesSection.Add(new RadialSection("Events", "Signal words of any kind.", eventsSection));
        isoEntitiesSection.Add(new RadialSection("CANCEL", "Close the menu", null));
        isoEntitiesSection.Add(new RadialSection("Measures", "Signal words of any kind.", measuresSection));
        isoEntitiesSection.Add(null);
        //isoEntitiesSection.Add(new RadialSection("Search 3D Object", "Search 3D Object", null));


        m_radialMenuMap.Add(MenuType.IsoEntities, isoEntitiesSection);
    }


    private static void Init_SRelationTypes()
    {

        List<RadialSection> sRelationsSection = new List<RadialSection>()
        {
            null,
            new RadialSection("topological", "topological", null),
            new RadialSection("directional", "directional", null),
            new RadialSection("topoDirectional", "topoDirectional", null),
            new RadialSection("CANCEL", "Close the menu", null),
            new RadialSection("manner", "manner", null),
            new RadialSection("goalDefining", "goalDefining", null),
            new RadialSection("pathDefining", "pathDefining", null)
        };

        m_radialMenuMap.Add(MenuType.IsoSRelationTypes, sRelationsSection);
    }

    private static void Init_MotionTypes()
    {
        List<RadialSection> motiontypeSection = new List<RadialSection>()
        {
            null,
            new RadialSection("manner", "manner", null),
            new RadialSection("path", "path", null),
            new RadialSection("compound", "compound", null),
            new RadialSection("CANCEL", "Close the menu",null),
        };

        m_radialMenuMap.Add(MenuType.IsoMotionType, motiontypeSection);
    }

    private static void Init_MotionClasses()
    {
        List<RadialSection> motiontclassSection = new List<RadialSection>()
        {
            new RadialSection("move", "move", null),
            new RadialSection("moveExternal", "moveExternal", null),
            new RadialSection("moveInternal", "moveInternal", null),
            new RadialSection("leave", "leave", null),
            new RadialSection("CANCEL", "Close the menu",null),
            new RadialSection("reach", "reach", null),
            new RadialSection("cross", "cross", null),
            new RadialSection("detach", "detach", null),
            new RadialSection("hit", "hit", null),
            new RadialSection("follow", "follow", null),
            new RadialSection("deviate", "deviate", null),
            new RadialSection("stay", "stay", null),
        };

        m_radialMenuMap.Add(MenuType.IsoMotionClass, motiontclassSection);
    }


    private static void Init_MotionSenses()
    {
        List<RadialSection> motiontsenseSection = new List<RadialSection>()
        {
           null,
            new RadialSection("literal", "literal", null),
            new RadialSection("fictive", "fictive", null),
            new RadialSection("intrinsicChange", "intrinsicChange", null),
            new RadialSection("CANCEL", "Close the menu",null),

        };

        m_radialMenuMap.Add(MenuType.IsoMotionSense, motiontsenseSection);
    }



    private static void Init_SpatialEntityTypes()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("Other", "Other", null),
            new RadialSection("facility", "facility", null),
            new RadialSection("vehicle", "vehicle", null),
            new RadialSection("person", "person", null),
            new RadialSection("CANCEL", "Close the menu",null),
            new RadialSection("dynamicEvent", "dynamicEvent", null),
            new RadialSection("artefact", "artefact", null)
        };

        m_radialMenuMap.Add(MenuType.IsoSpatialEntityTypes, entitytypes);
    }

    private static void Init_LocationTypes()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("Other", "Other", null),
            new RadialSection("water", "water", null),
            new RadialSection("celestial", "celestial", null),
            new RadialSection("civil", "civil", null),
            new RadialSection("CANCEL", "Close the menu",null),
            new RadialSection("country", "country", null),
            new RadialSection("grid", "grid", null),
            new RadialSection("latLong", "latLong", null),
            new RadialSection("mtn", "mountain", null),
            new RadialSection("mts", "mountain range", null),
            new RadialSection("postalCode", "postalCode", null),
            new RadialSection("postBox", "postBox", null),
            new RadialSection("ppl", "populated place", null),
            new RadialSection("ppla", "capital of sub-country", null),
            new RadialSection("pplc", "capital of country", null),
            new RadialSection("rgn", "non-political", null),
            new RadialSection("state", "state", null),
            new RadialSection("UTM", "UTM", null),
        };

        m_radialMenuMap.Add(MenuType.IsoPlaceTypes, entitytypes);
    }

    private static void Init_PathTypes()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("Other", "Other", null),
            new RadialSection("waterway", "waterway", null),
            new RadialSection("railway", "railway", null),
            new RadialSection("bridge", "bridge", null),
            new RadialSection("CANCEL", "Close the menu",null),
            new RadialSection("tunnel", "tunnel", null),
            new RadialSection("road", "road", null),
            new RadialSection("lane", "lane", null),
            new RadialSection("passage", "passage", null),
            new RadialSection("trail", "trail", null),
            new RadialSection("boundary", "boundary", null),
            new RadialSection("barrier", "barrier", null),
            new RadialSection("margin", "margin", null),
            new RadialSection("row", "row", null),
            new RadialSection("conduit", "conduit", null),
            new RadialSection("filament", "filament", null),
            new RadialSection("mtn", "mtn", null),
            new RadialSection("mts", "mts", null),
        };

        m_radialMenuMap.Add(MenuType.IsoPathTypes, entitytypes);
    }


    private static void Init_SpatialDimensions()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("point", "point", null),
            new RadialSection("line", "line", null),
            new RadialSection("area", "area", null),
            new RadialSection("volume", "volume", null),
            new RadialSection("CANCEL", "Close the menu",null)
        };

        m_radialMenuMap.Add(MenuType.IsoSpatialDimension, entitytypes);
    }

    private static void Init_SpatialForm()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("nam", "nam", null),
            new RadialSection("nom", "nom", null),
            null,
            null,
            new RadialSection("CANCEL", "Close the menu",null)
        };

        m_radialMenuMap.Add(MenuType.IsoSpatialForm, entitytypes);
    }


    private static void Init_PlaceCTV()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("Other", "Other", null),
            new RadialSection("city", "city", null),
            new RadialSection("town", "town", null),
            new RadialSection("village", "village", null),
            new RadialSection("CANCEL", "Close the menu",null)
        };

        m_radialMenuMap.Add(MenuType.IsoPlaceCTV, entitytypes);
    }

    private static void Init_PlaceContinent()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("None", "-", null),
            new RadialSection("AF", "Africe", null),
            new RadialSection("AN", "Antarctica", null),
            new RadialSection("AS", "Asia", null),
            new RadialSection("CANCEL", "Close the menu", null),
            new RadialSection("EU", "Europe", null),
            new RadialSection("NA", "North America", null),
            new RadialSection("SA", "South America", null),
            new RadialSection("OC", "Oceania", null),
        };
        m_radialMenuMap.Add(MenuType.IsoPlaceContinent, entitytypes);
    }

    public static List<RadialSection> GetMenu(MenuType type) => m_radialMenuMap[type];
}
