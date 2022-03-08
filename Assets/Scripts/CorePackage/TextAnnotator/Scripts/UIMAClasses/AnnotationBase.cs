using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnnotationBase : VRData
{
    
    /// <summary>
    /// Der Anfangsindex des Elements im Text.
    /// </summary>
    public int Begin { get; protected set; }

    /// <summary>
    /// Der Abschlussindex des Elements im Text.
    /// </summary>
    public int End { get; protected set; }

    /// <summary>
    /// Der Typ des Textelements.
    /// </summary>
    public string ClassType { get; set; }

    /// <summary>
    /// Das Elternelement.
    /// </summary>
    public AnnotationBase Parent { get; private set; }

    /// <summary>
    /// Das ID des übergeordneten Textelement.
    /// </summary>
    public string ParentID 
    {
        get
        {
            if (Parent == null) return null;
            else return Parent.ID is string ? (string)Parent.ID : "" + (int)Parent.ID;
        }
    }

    /// <summary>
    /// Die Länge des Textinhalts.
    /// </summary>
    public int Size
    {
        get
        {
            if (TextContent == null) return 0;
            else return TextContent.Length;
        }
    }

    /// <summary>
    /// IEnumerable-Liste der Kinder
    /// </summary>
    public IEnumerable<AnnotationBase> ChildElements;

    /// <summary>
    /// Die Anzahl der Tokens, die Nachfahren dieses Textelements sind.
    /// </summary>
    public int TokenCount
    {
        get
        {
            return AdjCount + AdvCount + ConjCount + DetCount + NounCount +
                   NumCount + PartCount + PronCount + PrePPostPCount +
                   PunctCount + VerbCount + OtherCount;
        }
    }

    /// <summary>
    /// Die Anzahl der Sätze, die Nachfahren dieses Textelements sind.
    /// </summary>
    public int SentenceCount
    {
        get
        {
            int res = 0;
            if (this is Sentence)
                res = 1;
            if (this is Paragraph ||  this is  Chapter || 
                this is AnnotationDocument)
            {
                foreach (AnnotationBase ab in ChildElements)
                    res += ab.SentenceCount;
            }            
            return res;
        }
    }

    /// <summary>
    /// Die Anzahl der Absätze, die Nachfahren dieses Textelements sind.
    /// </summary>
    public int ParagraphCount
    {
        get
        {
            int res = 0;
            if (this is Paragraph)
                res = 1;
            if (this is Chapter || this is AnnotationDocument)
            {
                foreach (AnnotationBase ab in ChildElements)
                    res += ab.ParagraphCount;
            }
            return res;
        }
    }

    /// <summary>
    /// Die Anzahl der Kapitel, die Nachfahren dieses Textelements sind.
    /// </summary>
    public int ChapterCount
    {
        get
        {
            int res = 0;
            if (this is Chapter)
                res = 1;
            if (this is AnnotationDocument)
            {
                foreach (AnnotationBase ab in ChildElements)
                    res += ab.ChapterCount;
            }
            return res;
        }
    }

    /// <summary>
    /// Die Anzahl der Adjektive im Textelement.
    /// </summary>
    public int AdjCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl der Adverbe im Textelement.
    /// </summary>
    public int AdvCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl der Konjunktionen im Textelement.
    /// </summary>
    public int ConjCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl der Bestimmungswörter im Textelement.
    /// </summary>
    public int DetCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl der Hauptwörter im Textelement.
    /// </summary>
    public int NounCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl der numerischen Wörter im Textelement.
    /// </summary>
    public int NumCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl der Partikel im Textelement.
    /// </summary>
    public int PartCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl der Pronomen im Textelement.
    /// </summary>
    public int PronCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl der Prä- und Postpositionen im Textelement.
    /// </summary>
    public int PrePPostPCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl der Punktuierungen im Textelement.
    /// </summary>
    public int PunctCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl der Verbe im Textelement.
    /// </summary>
    public int VerbCount { get; private set; } = 0;

    /// <summary>
    /// Die Anzahl von Wortarten im Textelement, die in keine anderen Wortarttypen eingeordnet werden können.
    /// </summary>
    public int OtherCount { get; private set; } = 0;

    /// <summary>
    /// Verweis auf die DDC-Kategorie-Instanz, die diesem Element zugewiesen wurde.
    /// </summary>
    public DDC_Category DDC_Category { get; private set; }

    /// <summary>
    /// Der Sentimentwert dieser Textkomponente.
    /// </summary>
    public float SentimentValue { get; private set; }

    /// <summary>
    /// Die zu dem Elternobjekt relative Position dieser Textkomponente.
    /// </summary>
    public int RelativePosition;

    /// <summary>
    /// Speichert den Zeitstempel der letzten Änderung.
    /// </summary>
    public DateTime TextFileLastChangedOn { get; private set; }

    public const string EmptyRepresentation = "[-]";
    public override string TextContent
    {
        get 
        {
            if (Begin == 0 && End == 0) return EmptyRepresentation;
            if (_textContent == null || _textContent.Length == 0)
                _textContent = DetermineDocument().TextContent.Substring(Begin, End - Begin);
            return _textContent; 
        }
    }

    public void SetBegin(int begin) { Begin = begin; }
    public void SetEnd(int end) { End = end; }

    /// <summary>Der Konstruktor initialisiert die Basiseigenschaften einer Textkomponente.</summary>
    /// <param name="id">Die Identifikationsnummer des Textkomponents</param>
    /// <param name="begin">Der Anfangsindex des Textkomponenten im Text</param>
    /// <param name="end">Der Abschlussindex des Textkomponenten im Text</param>
    /// <param name="type">Die Typesystem-Bezeichnung des Textkomponenten</param>
    public AnnotationBase(int id, int begin, int end, string type, AnnotationBase parent)
    {
        ID = id;
        Begin = begin;
        End = end;
        ClassType = type;
        Parent = parent;
        if (Parent != null) DetermineDocument().AddTextElement(this);
    }

    /// <summary>In dieser Methode kann definiert werden, wie und wann die 3D-Repräsentation der Textkomponente aktualisiert werden soll.</summary>
    public abstract void Actualize3DObject();

    /// <summary>Die Methode entfernt dieses Element aus dem Dokument.</summary>
    public virtual void RemoveElement()
    {
        AnnotationDocument doc = DetermineDocument();
        if (doc != null)
            doc.RemoveTextDataFromMaps(this);
    }

    /// <summary>Die Methode setzt die Liste der Kinderelementen dieser Textkomponente.</summary>
    /// <param name="elements">Die Liste der Elementen, die dieser Textkompontente als Kinder zugewiesen werden sollen.</param>
    public void SetChildElements(IEnumerable<AnnotationBase> elements)
    {
        List <AnnotationBase> childElements = new List<AnnotationBase>(elements);
        childElements.Sort((x, y) => x.Begin.CompareTo(y.Begin));
        for (int i = 0; i < childElements.Count; i++)
            childElements[i].RelativePosition = i + 1;
        ChildElements = childElements;
    }

    /// <summary>Die Methode weist dieser Textkomponente ein neues Kindelement zu.</summary>
    /// <param name="data">Das Textelement, das als Kind zugewiesen werden soll.</param>
    public void AddComponent(AnnotationBase data)
    {
        List<AnnotationBase> childElements = new List<AnnotationBase>(ChildElements);
        childElements.Add(data);
        childElements.Sort((x, y) => x.Begin.CompareTo(y.Begin));
        ChildElements = childElements;
    }

    /// <summary>Die Methode erhöht die Anzahl einer bestimmten Wortart in diesem Textelement.</summary>
    /// <param name="type">Der Typ der Wortart, deren Anzahl erhöht werden soll.</param>
    public void IncreasePOSTypeCount(PartOfSpeech.POSType type)
    {
        switch (type)
        {
            case (PartOfSpeech.POSType.Adjective):
                AdjCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            case (PartOfSpeech.POSType.Adverb):
                AdvCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            case (PartOfSpeech.POSType.Conjunction):
                ConjCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            case (PartOfSpeech.POSType.Determiner):
                DetCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            case (PartOfSpeech.POSType.Noun):
                NounCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            case (PartOfSpeech.POSType.Numeral):
                NumCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            case (PartOfSpeech.POSType.Particle):
                PartCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            case (PartOfSpeech.POSType.Pronoun):
                PronCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            case (PartOfSpeech.POSType.Prepos_Postpos):
                PrePPostPCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            case (PartOfSpeech.POSType.Punctuation):
                PunctCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            case (PartOfSpeech.POSType.Verb):
                VerbCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
            default:
                OtherCount += 1;
                if (Parent != null)
                    Parent.IncreasePOSTypeCount(type);
                break;
        }
    }

    /// <summary>Die Methode addiert einen bestimmten Wert zu dem aktuellen Sentimentwert hinzu.</summary>
    /// <param name="sV">Der zu addierende Sentimentwert.</param>
    public void AddSentimentValue(float sV)
    {
        SentimentValue += sV;
        if (Parent != null)
            Parent.AddSentimentValue(sV);
    }

    /// <summary>Eine Methode zur Zuweisung einer DDC-Kategorie.</summary>
    /// <param name="category">Die Instanz der DdcCategory, die diesem Element zugewiesen werden soll.</param>
    public void AddCategory(DDC_Category category)
    {
        DDC_Category = category;
        CategoryID = DDC_Category.CategoryID;
    }

    /// <summary>Die Methode gibt die Klasse und den Textinhalt des Objekts als String zurück.</summary>
    public override string ToString()
    {
        return "Type: " + ClassType + " Text: " + TextContent;
    }

    /// <summary>Die Methode überprüft, ob das übergebene Objekt gleich mit dieser Textkomponente ist. Vergleich nach ID.</summary>
    /// <param name="obj">Das zu vergleichende Objekt</param>
    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is AnnotationBase)) return false;
        AnnotationBase data = (AnnotationBase)obj;
        return data.ID.Equals(ID);
    }

    /// <summary>Die Methode generiert den Hashcode aus den Attributen ID, Begin, End und ClassType des Objekts.</summary>
    public override int GetHashCode()
    {
        var hashCode = 1858081855;
        hashCode = hashCode * -1521134295 + ID.GetHashCode();
        hashCode = hashCode * -1521134295 + Begin.GetHashCode();
        hashCode = hashCode * -1521134295 + End.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ClassType);
        return hashCode;
    }

    public AnnotationDocument DetermineDocument()
    {
        if (this is AnnotationDocument) return (AnnotationDocument)this;
        AnnotationBase parent = Parent;
        while (parent != null && !(parent is AnnotationDocument))
            parent = parent.Parent;
        return (AnnotationDocument)parent;
    }

}
