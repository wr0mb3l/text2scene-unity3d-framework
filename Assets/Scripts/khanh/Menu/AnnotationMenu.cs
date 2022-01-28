using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public static class TransformEx
{
    public static Transform Clear(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        return transform;
    }
}

public class AnnotationMenu : Menu
{
    AnnotationDocument Document;
    IsoSignal Signals;
    IsoEvent Events;
    IsoSpatialEntity SpatialEntitys;
    IsoLocation Locations;

    public Counter PageNavigator;
    public Counter SentenceCounter;
    public GameObject QuickTreeNodePrefab;
    public GameObject TextButtonPrefab;

    private List<Sentence> Sentences;
    public Transform AnnotationContent;
    public Transform ColorPallete;

    private Type[] Isotypes;
    GameObject NewLine;
    string[] TestString = new string[] { "Datmaniac", "is", "da best", "in the house", "regweg", "rwgrw", "."};
    string[] TestString2 = new string[] { "Furthermore", ",", "none is better" };
    void OnEnable()
    {
        var textAnnotatorInterface = MenuController.GetTextAnnotatorInterface();
        if (textAnnotatorInterface == null) return;
        LoadDocument(textAnnotatorInterface.ActualDocument.Document);
        PageNavigator.OnValueChanged.AddListener(UpdateTokenContainer);
        SentenceCounter.OnValueChanged.AddListener(UpdateTokenContainer);
        
        
    }

    public void LoadDocument(AnnotationDocument doc)
    {
        Document = doc;
        Sentences = new List<Sentence>(Document.GetElementsOfType<Sentence>());
        var excludedTypes = new System.Type[] { typeof(AnnotationToken), typeof(QuickTreeNode) };
        Isotypes = Document.Type_Map.Keys.Where(type => !excludedTypes.Contains(type)).ToArray();
        //GenerateColorPalete();
        UpdateTokenContainer();
    }

    void GenerateAnnotationText(QuickTreeNode token, Color color)
    {
        GameObject QuickTreeNodeObj = Instantiate(QuickTreeNodePrefab, AnnotationContent) as GameObject;
        QuickTreeNodeObj.name = token.TextContent;
        var visualizer = QuickTreeNodeObj.GetComponent<QuickTreeNodeVisualizer>();
        visualizer.Init(token, color);
    }

    void GenerateColorPalete()
    {
        foreach (var type in Isotypes)
        {
            Debug.Log(type.Name);
            var color = type?.GetProperty("ClassColor")?.GetValue(null);
            if (color != null)
            {
                var GO = Instantiate(TextButtonPrefab, ColorPallete);
                Text txt = GO.GetComponentInChildren<Text>();
                Image img = GO.GetComponent<Image>();
                txt.text = type.Name;
                txt.color = Color.white;
                img.color = (Color) color;
            }
        }
    }

    void GenerateNewLine()
    {
        var NewLine = new GameObject("NewLine", typeof(RectTransform));
        NewLine.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 5);
        Instantiate(NewLine, AnnotationContent);
    }
    void OnDisable()
    {
        PageNavigator.OnValueChanged.RemoveAllListeners();
        SentenceCounter.OnValueChanged.RemoveAllListeners();
    }
    void UpdateTokenContainer()
    {
        AnnotationContent.transform.Clear();
        var sentencePerPage = SentenceCounter.Value;
        PageNavigator.MaxValue = (Document.SentenceCount + sentencePerPage - 1) / sentencePerPage;
        if (PageNavigator.Value > PageNavigator.MaxValue) PageNavigator.Value = PageNavigator.MaxValue;
        var currentPage = PageNavigator.Value;
        //Debug.Log("Page " + currentPage);
        //Debug.Log("Count " + sentencePerPage);
        
        var sentences = Sentences.Skip((currentPage - 1) * sentencePerPage).Take(sentencePerPage);
        foreach (Sentence st in sentences)
        {
            var nodes = st.GetVisibleQuickTreeNodes();
            var emptyNodes = nodes.Where(node => node.Begin == 0 && node.End == 0);
            Debug.Log(emptyNodes.Count());
            foreach (var node in nodes)
            {
                Color color;
                var element = Document.GetElementsOfTypes(Isotypes, node.Begin, node.End);
                if (element == null) color = Color.white;
                else color = (Color) element?.GetType().GetProperty("ClassColor").GetValue(null);
                GenerateAnnotationText(node, color);
            }
            GenerateNewLine();
        }
        RefreshLayoutGroupsImmediateAndRecursive(AnnotationContent.gameObject);
    }

    public void RefreshLayoutGroupsImmediateAndRecursive(GameObject root)
    {
        foreach (var layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
    }
}
