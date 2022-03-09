using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Diese Klasse repräsentiert ein Kapitel und erbt von VRTextData.
/// </summary>
public class Chapter : AnnotationBase
{

    ///// <summary>
    ///// Alle Absätze des Kapitels.
    ///// </summary>
    public IEnumerable<Paragraph> Paragraphs
    {
        get { return ChildElements as IEnumerable<Paragraph>; }
    }

    /// <summary>
    /// Die Absatzgruppen, die für text2City erstellt und dort als Hochhäuser dargestellt werden.
    /// </summary>
    public IEnumerable<IEnumerable<Paragraph>> ParagraphGroups { get; private set; }

    public string _title = null;
    /// <summary>
    /// Titel des Kapitels (entweder der Überschirft oder der Kapiteltext selbst.
    /// </summary>
    public string Title
    {
        get
        {
            if (_title == null)
                _title = new List<AnnotationBase>(ChildElements)[0].TextContent;
            return _title;
        }
    }

    /// <summary>Dieser Konstruktor initialisiert die Chapter-Klasse.</summary>
    /// <param name="id">Die Identifikationsnummer des Kapitels.</param>
    /// <param name="begin">Der Anfangsindex des Kapitels im Text.</param>
    /// <param name="end">Der Abschlussindex des Kapitels im Text.</param>
    /// <param name="textDoc">Das Elterndokument.</param>
    public Chapter(int id, int begin, int end, AnnotationDocument textDoc) :
                   base(id, begin, end, AnnotationTypes.CHAPTER, textDoc)
    {

    }

    public override void Actualize3DObject()
    {

    }

    /// <summary>
    /// Diese Methode generiert aus den Absätzen maximal 9er-Absatzgruppen (Reihenfolge bleibt unverändert).
    /// </summary>
    //public void BuildParagraphGroups()
    //{
    //    int index = 0;
    //    List<List<Paragraph>> paragraphGroups = new List<List<Paragraph>> { new List<Paragraph>() };
    //    foreach (Paragraph paragraph in Components)
    //    {
    //        if (paragraphGroups[index].Count < CityScript.TEXT_FLOOR_LIMIT - 1)
    //            paragraphGroups[index].Add(paragraph);
    //        else
    //        {

    //            paragraphGroups.Add(new List<Paragraph> { paragraph });
    //            index += 1;
    //        }

    //    }
    //    ParagraphGroups = paragraphGroups;
    //}

}
