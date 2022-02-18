using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Diese Klasse repräsentiert eine Wortart und erbt von VRTextData.
/// </summary>
public class NamedEntity : AnnotationBase
{

    /// <summary>
    /// Die Typesystem-Bezeichnung des Named Entity.
    /// </summary>
    public string Type;

    /// <summary>
    /// Gibt an, ob das Named Entity konkret (false), oder Konzept ist (true).
    /// </summary>
    public bool IsConcept;

    private string _prettyType;
    /// <summary>
    /// Der wohlgeformte Name des Wortarttyps.
    /// </summary>
    //public string PrettyType
    //{
    //    get
    //    {
    //        if (_prettyType == null)
    //            _prettyType = StolperwegeHelper.textAnnotatorClient.PrettyFormatNamedEntity(Type);
    //        return _prettyType;
    //    }
    //}

    /// <summary>
    /// Diese Variable verweist auf den Multi-Token, der mit diesem Named Entity verbunden sein soll.
    /// </summary>
    public QuickTreeNode MultiToken;

    /// <summary>Dieser Konstruktor initialisiert die Named Entity-Klasse.</summary>
    /// <param name="id">Die Identifikationsnummer des Named-Entity.</param>
    /// <param name="begin">Der Anfangsindex des Named-Entity im Text.</param>
    /// <param name="end">Der Abschlussindex des Named-Entity im Text.</param>
    /// <param name="sentence">Der Eltern-Satz des Named-Entity.</param>
    /// <param name="type">Die Typesystem-Bezeichnung des Named-Entity.</param>
    /// <param name="city">Nullable. Hier kann eine Cityscript zugewiesen werden, falls vorhanden.</param>    
    public NamedEntity(int id, int begin, int end, string type, Sentence sentence) :
                       base(id, begin, end, type, sentence)
    {

        Type = type;
        QuickTreeNode result;
        AnnotationDocument doc = DetermineDocument();
        if (doc != null && doc.ExistsElementOfTypeInRange(Begin, End, out result))
        {
            MultiToken = result;
            MultiToken.NamedEntities.Add(Type, this);
        }

    }

    /// <summary>Die Methode entfernt dieses Named-Entity aus dem Dokument und löscht alle eventuelle Verweise zu Multi-Tokens.</summary>
    public override void RemoveElement()
    {
        if (MultiToken != null) MultiToken.NamedEntities.Remove(Type);
        base.RemoveElement();
        if (MultiToken != null && MultiToken.Object3D != null)
            Actualize3DObject();
    }

    public override string ToString()
    {
        return "Named entity: " + TextContent + ", Type: " + Type.ToString();
    }

    /// <summary>Diese Methode aktualisiert die 3D-Repräsentation des Named-Entity, falls letztere aktiv ist.</summary>
    public override void Actualize3DObject()
    {
        if (MultiToken.Object3D == null) return;
        //if (MultiToken.Object3D.GetComponent<TokenObject>() != null)
        //    MultiToken.Object3D.GetComponent<TokenObject>().Actualize();
    }
}