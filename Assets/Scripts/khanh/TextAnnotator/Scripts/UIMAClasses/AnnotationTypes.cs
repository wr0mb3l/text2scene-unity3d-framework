using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnnotationTypes
{

    public const string FINGERPRINT = "org.texttechnologylab.annotation.type.Fingerprint";
    public const string DOCUMENT = "uima.tcas.DocumentAnnotation";
    public const string CHAPTER = "org.texttechnologielab.annotation.type.Chapter";
    public const string PARAGRAPH = "de.tudarmstadt.ukp.dkpro.core.api.segmentation.type.Paragraph";
    public const string SENTENCE = "de.tudarmstadt.ukp.dkpro.core.api.segmentation.type.Sentence";
    public const string QUICK_TREE_NODE = "org.texttechnologylab.annotation.type.QuickTreeNode";
    public const string TOKEN = "de.tudarmstadt.ukp.dkpro.core.api.segmentation.type.Token";
    public const string LEMMA = "de.tudarmstadt.ukp.dkpro.core.api.segmentation.type.Lemma";
    public const string DDC_CATEGORY = "org.hucompute.services.type.CategoryCoveredTagged";
    public const string PART_OF_SPEECH = "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos";
    public const string SENTIMENT = "org.hucompute.services.type.Sentiment";

    // Text2Scene types
    public const string OBJECT_ATTRIBUTE = "org.texttechnologylab.annotation.semaf.IsoSpatial.ObjectAttribute";
    public const string VEC3 = "org.texttechnologylab.annotation.semaf.IsoSpatial.Vec3";
    public const string VEC4 = "org.texttechnologylab.annotation.semaf.IsoSpatial.Vec4";
    public const string ENTITY = "org.texttechnologylab.annotation.semaf.isobase.Entity";
    public const string EVENT = "org.texttechnologylab.annotation.semaf.isobase.Event";
    public const string LINK = "org.texttechnologylab.annotation.semaf.isobase.Link";
    public const string SIGNAL = "org.texttechnologylab.annotation.semaf.isobase.Signal";
    public const string META_LINK = "org.texttechnologylab.annotation.semaf.meta.MetaLink";
    public const string SR_LINK = "org.texttechnologylab.annotation.semaf.semafsr.SrLink";
    public const string SPATIAL_ENTITY = "org.texttechnologylab.annotation.semaf.isospace.SpatialEntity";
    public const string LOCATION = "org.texttechnologylab.annotation.semaf.isospace.Location";
    public const string MEASURE = "org.texttechnologylab.annotation.semaf.isospace.Measure";
    public const string SPATIAL_SIGNAL = "org.texttechnologylab.annotation.semaf.isospace.SpatialSignal";
    public const string MLINK = "org.texttechnologylab.annotation.semaf.isospace.MLink";
    public const string MOTION = "org.texttechnologylab.annotation.semaf.isospace.Motion";
    public const string NON_MOTION_EVENT = "org.texttechnologylab.annotation.semaf.isospace.NonMotionEvent";
    public const string MOTION_SIGNAL = "org.texttechnologylab.annotation.semaf.isospace.MotionSignal";
    public const string OLINK = "org.texttechnologylab.annotation.semaf.isospace.OLink";
    public const string QSLINK = "org.texttechnologylab.annotation.semaf.isospace.QsLink";
    public const string PATH = "org.texttechnologylab.annotation.semaf.isospace.Path";
    public const string EVENT_PATH = "org.texttechnologylab.annotation.semaf.isospace.EventPath";
    public const string PLACE = "org.texttechnologylab.annotation.semaf.isospace.Place";
    public const string MOVELINK = "org.texttechnologylab.annotation.semaf.isospace.MoveLink";
    public const string SRELATION = "org.texttechnologylab.annotation.semaf.isospace.SRelation";
    public const string MRELATION = "org.texttechnologylab.annotation.semaf.isospace.MRelation";

    /// <summary>
    /// Das Wörterbuch der (Typesystem, Eltern-Typesystem)-Paare.
    /// </summary>
    public static Dictionary<string, Type> TypeParentTable = new Dictionary<string, Type>()
    {
        { PART_OF_SPEECH + "ADJ", typeof(AnnotationToken)}, { PART_OF_SPEECH + "ADV", typeof(AnnotationToken)},
        { PART_OF_SPEECH + "ART", typeof(AnnotationToken)}, { PART_OF_SPEECH + "CARD", typeof(AnnotationToken)},
        { PART_OF_SPEECH + "CONJ", typeof(AnnotationToken)}, { PART_OF_SPEECH + "N", typeof(AnnotationToken)},
        { PART_OF_SPEECH + "NN", typeof(AnnotationToken)}, { PART_OF_SPEECH + "NP", typeof(AnnotationToken)},
        { PART_OF_SPEECH + "O", typeof(AnnotationToken)}, { PART_OF_SPEECH + "PP", typeof(AnnotationToken)},
        { PART_OF_SPEECH + "PR", typeof(AnnotationToken)}, { PART_OF_SPEECH + "PRT", typeof(AnnotationToken)},
        { PART_OF_SPEECH + "PUNC", typeof(AnnotationToken)}, { PART_OF_SPEECH + "V", typeof(AnnotationToken)},
        { CHAPTER, typeof(AnnotationDocument) }, { PARAGRAPH, typeof(Chapter)},
        { SENTENCE, typeof(Paragraph) }, { TOKEN, typeof(Sentence)},
        { QUICK_TREE_NODE, typeof(Sentence) }, { SENTIMENT, typeof(Sentence) }
    };

    /// <summary>
    /// Das Wörterbuch der (Typesystem, C#-Klassentype)-Paare.
    /// </summary>
    public static Dictionary<string, Type> TypesystemClassTable = new Dictionary<string, Type>()
    {
        { PART_OF_SPEECH, typeof(PartOfSpeech)},
        { CHAPTER, typeof(Chapter) }, { PARAGRAPH, typeof(Paragraph)},
        { SENTENCE, typeof(Sentence) }, { TOKEN, typeof(AnnotationToken)},
        { QUICK_TREE_NODE, typeof(QuickTreeNode) }, { SENTIMENT, typeof(Sentiment)},
        { ENTITY, typeof(IsoEntity) }, { EVENT, typeof(IsoEvent) }, { LINK, typeof(IsoLink)},
        { SIGNAL, typeof(IsoSignal) }, { META_LINK, typeof(IsoMetaLink) }, { SR_LINK, typeof(IsoSrLink)},
        { SPATIAL_ENTITY, typeof(IsoSpatialEntity) }, { LOCATION, typeof(IsoLocation) }, { MEASURE, typeof(IsoMeasure)},
        { MLINK, typeof(IsoMLink) }, { MOTION, typeof(IsoMotion)},
        { NON_MOTION_EVENT, typeof(IsoNonMotionEvent) }, { OLINK, typeof(IsoOLink)},
        { QSLINK, typeof(IsoQsLink) }, { PATH, typeof(IsoLocationPath) }, { EVENT_PATH, typeof(IsoEventPath)},
        { PLACE, typeof(IsoLocationPlace) }, { MOVELINK, typeof(IsoMoveLink)}, { VEC3, typeof(IsoVector3) }, { VEC4, typeof(IsoVector4) },
        { OBJECT_ATTRIBUTE, typeof(IsoObjectAttribute) },
        { SRELATION, typeof(IsoSRelation) }, { MRELATION, typeof(IsoMRelation) }
    };

    /// <summary>
    /// Das Wörterbuch der (C#-Klassentyp, Typesystem)-Paare.
    /// </summary>
    public static Dictionary<Type, string> ClassTypesystemTable = new Dictionary<Type, string>()
    {
        { typeof(PartOfSpeech), PART_OF_SPEECH },
        { typeof(Chapter), CHAPTER }, { typeof(Paragraph), PARAGRAPH },
        { typeof(Sentence), SENTENCE }, { typeof(AnnotationToken), TOKEN },
        { typeof(QuickTreeNode), QUICK_TREE_NODE }, { typeof(Sentiment), SENTIMENT },
        { typeof(IsoEntity), ENTITY }, { typeof(IsoEvent), EVENT }, { typeof(IsoLink), LINK },
        { typeof(IsoSignal), SIGNAL }, { typeof(IsoMetaLink), META_LINK }, { typeof(IsoSrLink), SR_LINK},
        { typeof(IsoSpatialEntity), SPATIAL_ENTITY }, { typeof(IsoLocation), LOCATION }, { typeof(IsoMeasure), MEASURE },
        { typeof(IsoMLink), MLINK }, { typeof(IsoMotion), MOTION },
        { typeof(IsoNonMotionEvent), NON_MOTION_EVENT }, { typeof(IsoOLink), OLINK },
        { typeof(IsoQsLink), QSLINK }, { typeof(IsoLocationPath), PATH }, { typeof(IsoEventPath), EVENT_PATH },
        { typeof(IsoLocationPlace), PLACE }, { typeof(IsoMoveLink), MOVELINK }, { typeof(IsoVector3), VEC3 }, { typeof(IsoVector4), VEC4 },
        { typeof(IsoObjectAttribute), OBJECT_ATTRIBUTE },
        { typeof(IsoSRelation), SRELATION }, { typeof(IsoMRelation), MRELATION }
    };

    internal static List<string> SortTypes(HashSet<string> keys)
    {
        List<string> result = new List<string>();
        if (keys.Contains(FINGERPRINT)) result.Add(FINGERPRINT);
        if (keys.Contains(VEC3)) result.Add(VEC3);
        if (keys.Contains(VEC4)) result.Add(VEC4);
        if (keys.Contains(OBJECT_ATTRIBUTE)) result.Add(OBJECT_ATTRIBUTE);
        foreach (string key in keys)
        {
            if (key.Equals(VEC3) || key.Equals(VEC4) || key.Equals(FINGERPRINT) || key.Equals(OBJECT_ATTRIBUTE)) continue;
            if (TypesystemClassTable.ContainsKey(key)) result.Add(key);
        }
        return result;
    }
}
