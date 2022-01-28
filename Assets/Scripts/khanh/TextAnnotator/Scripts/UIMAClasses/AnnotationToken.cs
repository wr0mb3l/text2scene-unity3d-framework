using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Diese Klasse repräsentiert eine Zeichenkette und erbt von AnnotationBase.
/// </summary>
public class AnnotationToken : AnnotationBase
{

    /// <summary>
    /// Die Part-of-speech-Instanz, die diesem TextToken zugewiesen ist.
    /// </summary>
    public PartOfSpeech POS { get; private set; }

    /// <summary>
    /// Die Sentiment-Instanz, die diesem TextToken zugewiesen ist.
    /// </summary>
    public Sentiment Sentiment { get; private set; }

    /// <summary>
    /// Die Liste aller Multi-Tokens von denen dieses TextToken ein Teil ist.
    /// </summary>
    public List<QuickTreeNode> PartOfQuickTreeUnits { get; private set; }

    /// <summary>Dieser Konstruktor initialisiert die TextToken-Klasse.</summary>
    /// <param name="id">Die Identifikationsnummer des TextTokens.</param>
    /// <param name="begin">Der Anfangsindex des TextTokens im Text.</param>
    /// <param name="end">Der Abschlussindex des TextTokens im Text.</param>
    /// <param name="sentence">Der Elternsatz.</param>
    public AnnotationToken(int id, int begin, int end, Sentence sentence) :
                     base(id, begin, end, AnnotationTypes.TOKEN, sentence)
    {
        PartOfQuickTreeUnits = new List<QuickTreeNode>();
    }

    public override string ToString()
    {
        return "Token: " + TextContent;
    }

    /// <summary>
    /// Die Methode verlinkt dieses TextToken mit dem übergebenen Sentiment.
    /// </summary>
    /// <param name="sentiment">Die Sentiment-Instanz zum Verlinken.</param>
    public void AddSentiment(Sentiment sentiment)
    {
        Sentiment = sentiment;
        AddSentimentValue(Sentiment.Value);
    }

    /// <summary>
    /// Die Methode verlinkt dieses TextToken mit dem übergebenen Part-of-speech.
    /// </summary>
    /// <param name="pos">Die POS-Instanz zum Verlinken.</param>
    public void AddPOS(PartOfSpeech pos)
    {
        POS = pos;
        IncreasePOSTypeCount(POS.Type);
    }

    public override void Actualize3DObject()
    {
        
    }

}
