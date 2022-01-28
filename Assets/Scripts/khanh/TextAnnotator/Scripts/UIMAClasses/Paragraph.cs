using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Diese Klasse repräsentiert einen Absatz und erbt von VRTextData.
/// </summary>
public class Paragraph : AnnotationBase
{
    /// <summary>
    /// Alle Sätze des Absatzes.
    /// </summary>
    public IEnumerable<Sentence> Sentences
    {
        get { return ChildElements as IEnumerable<Sentence>; }
    }

    /// <summary>Dieser Konstruktor initialisiert die Paragraph-Klasse.</summary>
    /// <param name="id">Die Identifikationsnummer des Absatzes.</param>
    /// <param name="begin">Der Anfangsindex des Absatzes im Text.</param>
    /// <param name="end">Der Abschlussindex des Absatzes im Text.</param>
    /// <param name="chapter">Das Elternkapitel.</param>
    public Paragraph(int id, int begin, int end, Chapter chapter) :
                     base(id, begin, end, AnnotationTypes.PARAGRAPH, chapter)
    {

    }

    public override void Actualize3DObject()
    {
        
    }

}
