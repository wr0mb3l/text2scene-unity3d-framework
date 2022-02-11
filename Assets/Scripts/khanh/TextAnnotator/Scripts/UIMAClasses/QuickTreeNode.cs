using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Diese Klasse repräsentiert ein Multi-Token und erbt von VRTextData.
/// </summary>
public class QuickTreeNode : AnnotationBase
{

    /// <summary>
    /// Alle Named-Entities, die auf dieses Multi-Token verweisen im Wörterbuch einsortiert nach ihren Typ.
    /// </summary>
    public Dictionary<string, NamedEntity> NamedEntities = new Dictionary<string, NamedEntity>();

    /// <summary>
    /// Die Liste aller TextTokens, die Teil dieses Multi-Tokens sind
    /// </summary>
    public List<AnnotationToken> Tokens;

    /// <summary>
    /// Die ID-Liste aller untergeordneten QuickTreeNodes
    /// </summary>
    public List<int> ChildNodes { get; private set; }

    /// <summary>
    /// Die ID-Liste aller untergeordneten QuickTreeNodes
    /// </summary>
    public int ParentNode { get; private set; }

    public IsoEntity IsoEntity;
    //public HashSet<TokenObject> TokenObjects { get; private set; }


    /// <summary>Dieser Konstruktor initialisiert die QuickTreeUnit-Klasse (Multi-Token).</summary>
    /// <param name="id">Die Identifikationsnummer des Multi-Tokens.</param>
    /// <param name="begin">Der Anfangsindex des Multi-Tokens im Text.</param>
    /// <param name="end">Der Abschlussindex des Multi-Tokens im Text.</param>
    /// <param name="sentence">Der Satz zu dem das QuickTreeNode gehört.</param>  
    public QuickTreeNode(int id, int begin, int end, int parentNode, List<int> childNodes, Sentence sentence) :
                     base(id, begin, end, AnnotationTypes.QUICK_TREE_NODE, sentence)
    {
        ParentNode = parentNode;
        ChildNodes = childNodes;
        Tokens = new List<AnnotationToken>();
        //TokenObjects = new HashSet<TokenObject>();
        foreach (AnnotationToken token in sentence.ChildElements)
        {
            if (token.Begin >= Begin && token.End <= End)
            {
                Tokens.Add(token);
                token.PartOfQuickTreeUnits.Add(this);
            }
        }
        sentence.AddQuickTreeUnit(this);
    }

    /// <summary>Die Methode entfernt dieses Multi-Tokens aus dem Dokument und löscht alle eventuelle Verweise zu TextTokens und Named-Entities.</summary>
    /*public override void RemoveElement()
    {
        foreach (string type in NamedEntities.Keys)
            NamedEntities[type].MultiToken = null;
        foreach (AnnotationToken token in Tokens)
            token.PartOfQuickTreeUnits.Remove(this);
        ((Sentence)Parent).RemoveQuickTreeUnit(this);
        if (IsoEntity != null && IsoEntity is IsoEntity)
            ((IsoEntity)IsoEntity).Uncouple();
        base.RemoveElement();
        Parent.Actualize3DObject();
    }*/

    public override string ToString()
    {
        string res = "Quick Tree Node: " + TextContent;
        if (NamedEntities.Count > 0)
        {
            List<string> types = new List<string>(NamedEntities.Keys);
            res += "\nNamed entities: ";
            for (int i = 0; i < types.Count; i++)
            {
                res += types[i];
                if (i < types.Count - 1) res += ", ";
            }

        }
        return res;
    }

    public void SetParentNode(int parentNode) { ParentNode = parentNode; }

    public override void Actualize3DObject()
    {
        
    }
}
