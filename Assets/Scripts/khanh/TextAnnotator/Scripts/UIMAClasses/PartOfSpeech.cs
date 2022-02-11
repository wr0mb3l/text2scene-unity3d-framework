using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Diese Klasse repräsentiert eine Wortart und erbt von VRTextData.
/// </summary>
public class PartOfSpeech : AnnotationBase
{

    /// <summary>
    /// Die enum-Typen der Wortarten.
    /// </summary>
    public enum POSType
    {
        Adjective, Adverb, Determiner, Numeral, Conjunction, Noun,
        Other, Prepos_Postpos, Pronoun, Particle, Punctuation, Verb
    }

    /// <summary>
    /// Das Wörterbuch in dem den Typesystem-Zeichenketten die passenden Wortart-enum-Typen zugeorndet werden.
    /// </summary>
    public static Dictionary<string, POSType> POSTypeMap = new Dictionary<string, POSType>()
    {
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.ADJ", POSType.Adjective},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_ADJ", POSType.Adjective},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_ADP", POSType.Adjective},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.ADV", POSType.Adverb},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_ADV", POSType.Adverb},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.ART", POSType.Determiner},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_DET", POSType.Determiner},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.CARD", POSType.Numeral},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_NUM", POSType.Numeral},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.CONJ", POSType.Conjunction},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_CONJ", POSType.Conjunction},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.N", POSType.Noun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.NNS", POSType.Noun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.NN", POSType.Noun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.NP", POSType.Noun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.NPS", POSType.Noun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.NNPS", POSType.Noun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.NNP", POSType.Noun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_NOUN", POSType.Noun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_PROPN", POSType.Noun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.O", POSType.Other},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.PP", POSType.Prepos_Postpos},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_X", POSType.Prepos_Postpos},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.PR", POSType.Pronoun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_PRON", POSType.Pronoun},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.PRT", POSType.Particle},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.PUNC", POSType.Punctuation},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_SYM", POSType.Punctuation},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_PUNCT", POSType.Punctuation},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.V", POSType.Verb},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_VERB", POSType.Verb},
        { "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos.POS_INTJ", POSType.Verb},
    };

    /// <summary>
    /// Der Typ dieser Part-of-speech.
    /// </summary>
    public POSType Type
    {
        //get { return POSTypeMap[ClassType]; }
        get {
            POSType p;
            if (POSTypeMap.TryGetValue(ClassType, out p)) return p;
            else return POSType.Other;
        }
    }

    /// <summary>
    /// Die referenzierte TextToken-Instanz.
    /// </summary>
    public AnnotationBase ReferencedTextData { get; private set; }

    /// <summary>Dieser Konstruktor initialisiert die PartOfSpeech-Klasse.</summary>
    /// <param name="id">Die Identifikationsnummer der POS-Instanz.</param>
    /// <param name="begin">Der Anfangsindex des POS-Instanz im Text.</param>
    /// <param name="end">Der Abschlussindex des POS-Instanz im Text.</param>
    /// <param name="textToken">Das TextToken-Element als Eltern.</param>
    /// <param name="type">Der Typesystem-Zeichenkette der POS-Instanz.</param>
    public PartOfSpeech(AnnotationBase textToken, int id, int begin, int end, string type) :
        base(id, begin, end, type, textToken)
    {
        Connect();
    }

    /// <summary>
    /// Die Methode sucht das TextToken, auf das diese Part-of-speech-Instanz verweist und verbindet sie miteinander.
    /// </summary>
    private void Connect()
    {
        AnnotationDocument doc = DetermineDocument();
        if (doc == null) return;
        ReferencedTextData = doc.GetElementsOfTypes(new Type[] { typeof(AnnotationToken) }, Begin, End);
        if (ReferencedTextData != null && (ReferencedTextData is AnnotationToken))
            ((AnnotationToken)ReferencedTextData).AddPOS(this);

    }

    public override string ToString()
    {
        return "Text: " + TextContent + ", Type: " + Type.ToString();
    }

    public override void Actualize3DObject()
    {
        
    }
}
