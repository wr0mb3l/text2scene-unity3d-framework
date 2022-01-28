using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class QuickTreeNodeVisualizer : MonoBehaviour
{
    public GameObject MainNode;
    public GameObject ButtonTextPrefab;
    public GameObject ChildrenNodeContainer;
    public QuickTreeNode QuickTreeNode { get; private set; }
    public IsoEntity Entity { get; private set; }

    private bool showChildContainer = false;
    public void Init(QuickTreeNode token, Color color)
    {
        QuickTreeNode = token;
        setToken(color);
        var subnodes = token.TextContent.Split(' ');
        if (subnodes.Length > 1)
        {
            Button mainNodeButton = MainNode.GetComponent<Button>();
            //mainNodeButton.onClick.AddListener(toggleView);
            foreach (string subtext in subnodes)
            {
                GameObject subNode = Instantiate(ButtonTextPrefab, ChildrenNodeContainer.transform) as GameObject;
                subNode.GetComponentInChildren<TextMeshProUGUI>().text = subtext;
            }
        }
    }

    void printDictionary(IDictionary dictionary)
    {
        string text = "{";
        foreach (KeyValuePair<string, NamedEntity> kvp in dictionary)
        {
            //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            text += string.Format("{0}: {1}, ", kvp.Key, kvp.Value.ClassType);
        }
        text += "}";
        Debug.Log(text);
    }
    void setToken(Color color)
    {
        TextMeshProUGUI text = MainNode.GetComponentInChildren<TextMeshProUGUI>();
        text.text = QuickTreeNode.TextContent;
        if (color == null) text.color = Color.black;
        else text.color = color;
    }

    public bool HasEntityToken { get { return Entity != null; } }
    public bool HasQuickTreeNode { get { return QuickTreeNode != null; } }
    public IsoEntity GetEntity()
    {
        if (HasEntityToken) return Entity;
        else if (HasQuickTreeNode) return QuickTreeNode.IsoEntity;
        else return null;
    }


    private void toggleView()
    {
        showChildContainer = !showChildContainer;
        ChildrenNodeContainer.SetActive(showChildContainer);
    }
    public void OnHoverEnter()
    {
        ChildrenNodeContainer.SetActive(true);
        Debug.Log("Hover");
    }

    public void OnHoverExit()
    {
        ChildrenNodeContainer.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        ChildrenNodeContainer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
