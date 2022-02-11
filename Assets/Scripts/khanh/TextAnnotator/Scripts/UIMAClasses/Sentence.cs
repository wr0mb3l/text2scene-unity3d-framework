using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using AnnotationBaseComparers;

/// <summary>
/// Diese Klasse repräsentiert einen Satz und erbt von VRTextData.
/// </summary>
public class Sentence : AnnotationBase
{

    /// <summary>
    /// Alle Multi-Tokens dieses Satzes.
    /// </summary>
    private HashSet<QuickTreeNode> AllQuickTreeNodes = new HashSet<QuickTreeNode>();

    /// <summary>
    /// Alle Basis-QuickTreeNodes (Single Tokens).
    /// </summary>
    private List<QuickTreeNode> BaseQuickTreeNodes;

    /// <summary>
    /// Der Annotator-Tool des Text-Raumes
    /// </summary>
    //public AnnotatorTool AnnotatorTool;
    //public QuickAnnotatorTool QuickAnnotatorTool;
    //public AnnotationWindow AnnotationWindow;


    /// <summary>Dieser Konstruktor initialisiert die Sentence-Klasse.</summary>
    /// <param name="id">Die Identifikationsnummer des Satzes.</param>
    /// <param name="begin">Der Anfangsindex des Satzes im Text.</param>
    /// <param name="end">Der Abschlussindex des Satzes im Text.</param>
    /// <param name="paragraph">Der Elternabsatz.</param>
    public Sentence(int id, int begin, int end, Paragraph paragraph) :
                    base(id, begin, end, AnnotationTypes.SENTENCE, paragraph)
    {

    }

    /// <summary>
    /// Diese Methode fügt ein neues Multi-Token dem Satz hinzu.
    /// </summary>
    /// <param name="unit">Das Multi-Token, das eingefügt werden soll.</param>
    public void AddQuickTreeUnit(QuickTreeNode unit)
    {
        AllQuickTreeNodes.Add(unit);
    }

    private List<QuickTreeNode> _visibleQuickTreeNodes;
    /// <summary>
    /// Diese Methode sucht alle Multi-Tokens des Satzes, die gerade visualisiert werden.
    /// </summary>
    public List<QuickTreeNode> GetVisibleQuickTreeNodes()
    {
        _visibleQuickTreeNodes = new List<QuickTreeNode>();
        foreach (QuickTreeNode node in AllQuickTreeNodes)
            if (node.ParentNode == -1)
                _visibleQuickTreeNodes.Add(node);
        SortQuickTreeUnits(_visibleQuickTreeNodes);
        return _visibleQuickTreeNodes;
    }

    public List<QuickTreeNode> GetBaseQuickTreeNodes()
    {
        if (BaseQuickTreeNodes == null) BaseQuickTreeNodes = new List<QuickTreeNode>();
        else BaseQuickTreeNodes.Clear();
        foreach (QuickTreeNode node in AllQuickTreeNodes)
            if (node.ChildNodes.Count == 0) BaseQuickTreeNodes.Add(node);
        return BaseQuickTreeNodes;
    }

    ///// <summary>
    ///// Diese Methode sortiert alle Multi-Tokens nach ihrem Anfangsindex.
    ///// </summary>
    public static void SortQuickTreeUnits(List<QuickTreeNode> quickTreeNodes)
    {
        quickTreeNodes.Sort((a, b) => 
        {
            if (a.End == -1 && b.End > -1) return 1;
            if (a.End > -1 && b.End == -1) return -1;
            return a.Begin.CompareTo(b.Begin); });
    }

    ///// <summary>
    ///// Diese Methode entfernt ein bestimmtes Multi-Token aus dem Satz.
    ///// </summary>
    ///// <param name="unit">Das Multi-Token, das entfernt werden soll.</param>
    public void RemoveQuickTreeUnit(QuickTreeNode unit)
    {
        AllQuickTreeNodes.Remove(unit);
    }

    ///// <summary>Diese Methode aktualisiert die 3D-Repräsentation des Satzes, falls vorhanden und aktiv.</summary>
    public override void Actualize3DObject()
    {
        //    //if (AnnotatorTool != null)
        //    //    AnnotatorTool.UpdateTokenContainer();
        //if (QuickAnnotatorTool != null)
        //    QuickAnnotatorTool.UpdateTokenContainer();
        //if (AnnotationWindow != null)
        //{
        //    AnnotationWindow.DetermineMaxSentenceLength();
        //    AnnotationWindow.UpdateTokenContainer();
        //}

    }

    public override string ToString()
    {
        return "Text: " + TextContent + ", Tokens: " + new List<AnnotationBase>(ChildElements).Count;
    }

}