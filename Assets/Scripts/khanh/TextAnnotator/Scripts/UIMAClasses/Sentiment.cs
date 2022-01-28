using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Diese Klasse repräsentiert ein Sentiment und erbt von VRTextData.
/// </summary>
public class Sentiment : AnnotationBase
{

    /// <summary>
    /// Der Wert des Sentiments.
    /// </summary>
    public float Value { get; private set; }

    /// <summary>
    /// Die Farbe des Sentiments (negative Zahl = rot, 0 = weiss, positive Zahl = grün).
    /// </summary>
    public Vector4 Color { get; private set; }

    /// <summary>
    /// Die referenzierte TextToken-Instanz.
    /// </summary>
    public AnnotationBase ReferencedTextData { get; private set; }

    /// <summary>Dieser Konstruktor initialisiert die Sentiment-Klasse.</summary>
    /// <param name="id">Die Identifikationsnummer der Sentiment-Instanz.</param>
    /// <param name="begin">Der Anfangsindex des Sentiment-Instanz im Text.</param>
    /// <param name="end">Der Abschlussindex des Sentiment-Instanz im Text.</param>
    /// <param name="textToken">Das TextToken-Element als Eltern.</param>
    /// <param name="value">Der Wert der Sentiment-Instanz.</param>
    public Sentiment(AnnotationBase textToken, int id, int begin, int end, float value) :
        base(id, begin, end, AnnotationTypes.SENTIMENT, textToken)
    {
        Value = value;
        Color = (Value < 0) ? new Vector4(Mathf.Abs(Value), 0, 0, 1) : (Value == 0) ? Vector4.one : new Vector4(0, Value, 0, 1);
        Connect();
    }

    /// <summary>
    /// Die Methode sucht das TextToken, auf das diese Sentiment-Instanz verweist und verbindet sie miteinander.
    /// </summary>
    private void Connect()
    {
        AnnotationDocument doc = DetermineDocument();
        if (doc == null) return;
        ReferencedTextData = doc.GetElementsOfTypes(new Type[] {typeof(AnnotationToken) }, Begin, End);
        if (ReferencedTextData != null)
            if (ReferencedTextData is AnnotationToken) ((AnnotationToken)ReferencedTextData).AddSentiment(this);

    }

    public override string ToString()
    {
        return "Text: " + TextContent + ", Value: " + Value;
    }

    public override void Actualize3DObject()
    {
        
    }
}
