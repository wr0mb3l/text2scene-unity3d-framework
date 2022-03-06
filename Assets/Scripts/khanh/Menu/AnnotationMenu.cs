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

    public Counter PageNavigator;
    public Counter SentenceCounter;
    public GameObject QuickTreeNodePrefab;
    public GameObject TextButtonPrefab;
    public List<IsoEntity> EmptyTokenObjects { get; private set; }
    public Transform EmptyTokenContainer;
    public Transform TokenContainer;

    private int EmptyTokenPointer = 0;
    private List<Sentence> Sentences;
    private Type[] Isotypes;

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
        EmptyTokenObjects = new List<IsoEntity>(Document.GetElementsOfTypeFromTo<IsoEntity>(0, 0, true));
        Debug.Log(EmptyTokenObjects.Count());
        UpdateTokenContainer();
        UpdateEmptyTokenContainer();
    }

    GameObject GenerateTokenObject(AnnotationBase token, Color color, Transform container)
    {
        GameObject QuickTreeNodeObj = Instantiate(QuickTreeNodePrefab, container) as GameObject;
        QuickTreeNodeObj.name = token.TextContent;
        var visualizer = QuickTreeNodeObj.GetComponent<QuickTreeNodeVisualizer>();
        visualizer.Init(token, color);
        return QuickTreeNodeObj;
    }

    void GenerateNewLine()
    {
        var NewLine = new GameObject("NewLine", typeof(RectTransform));
        NewLine.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 5);
        Instantiate(NewLine, TokenContainer);
        Destroy(NewLine);
    }
    void OnDisable()
    {
        PageNavigator.OnValueChanged.RemoveAllListeners();
        SentenceCounter.OnValueChanged.RemoveAllListeners();
    }
    void UpdateTokenContainer()
    {
        TokenContainer.transform.Clear();
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
            foreach (var node in nodes)
            {
                Color color;
                var element = Document.GetElementsOfTypes(Isotypes, node.Begin, node.End);
                if (element == null) color = Color.white;
                else color = (Color) element?.GetType().GetProperty("ClassColor").GetValue(null);
                GenerateTokenObject(node, color, TokenContainer);
            }
            GenerateNewLine();
        }
        RefreshLayoutGroupsImmediateAndRecursive(TokenContainer.gameObject);
    }

    void UpdateEmptyTokenContainer()
    {
        EmptyTokenContainer.transform.Clear();
        foreach (IsoEntity et in EmptyTokenObjects)
        {
            GenerateTokenObject(et, Color.white, EmptyTokenContainer);
        } 
        
    }

    void AddEmptyToken()
    {
        // AnnotationBase parent, int ID, int begin, int end, string comment, string mod, 
        // string object_ID, IsoVector3 position, IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string class_type
        // var emptyToken = new 
        UpdateEmptyTokenContainer();
    }

    public void RefreshLayoutGroupsImmediateAndRecursive(GameObject root)
    {
        foreach (var layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
    }
}
