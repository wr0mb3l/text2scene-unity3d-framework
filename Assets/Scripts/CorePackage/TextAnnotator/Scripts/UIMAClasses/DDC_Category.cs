using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDC_Category : AnnotationBase
{

    private static string _ddcTrash = "__label_ddc__";

    /// <summary>Dieser Konstruktor initialisiert die DdcCategory-Klasse.</summary>
    /// <param name="id">Die Identifikationsnummer der DDC-Kateogrie.</param>
    /// <param name="begin">Der Anfangsindex des DDC-Kateogrie im Text.</param>
    /// <param name="end">Der Abschlussindex des DDC-Kateogrie im Text.</param>
    /// <param name="parent">Das Element dem die Kategorie zugeordnet werden soll.</param>
    /// <param name="categoryID">Die Zeichenkette, in der das Kennzeichen der Kategorie enthalten ist.</param>
    public DDC_Category(AnnotationBase parent, int id, int begin, int end, string categoryID) :
        base(id, begin, end, AnnotationTypes.DDC_CATEGORY, parent)
    {
        CategoryID = categoryID.Replace(_ddcTrash, "");
        parent.AddCategory(this);
    }

    public override void Actualize3DObject()
    {
        
    }
}
