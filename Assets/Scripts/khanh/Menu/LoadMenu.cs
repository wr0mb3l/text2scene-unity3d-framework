using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class LoadMenu : Menu
{
    public Button LoadButton;
    public Button LogoutButton;

    public GameObject FolderItemPrefab;
    public GameObject FileItemPrefab;
    public Button ReturnButton;
    public Transform BrowserContainer;
    public GameObject Breadcrumb;
    public Counter PageNavigator;

    private const int ItemPerPage = 5;
    Tuple<string, string, string> khanh = Tuple.Create("s8467169", "ahw2LohG", "27268");
    //Tuple<string, string, string> kett = Tuple.Create("kett", "Tig3rm4n", "27204");
    Tuple<string, string, string> kett = Tuple.Create("kett", "Tig3rm4n", "27081");
    public const string ROOT = "https://resources.hucompute.org/repository/2";
    private TextAnnotatorInterface textAnnotatorInterface;
    private VRResourceData currentItem;
    private IEnumerable<VRResourceData> childItemList;

    private void LoadButtonClicked()
    {
        if (textAnnotatorInterface.ActualDocument?.DocumentCreated == true)
            MenuController.SwitchMenu(MenuType.AnnotationWindow);
        else StartCoroutine(Load_Document(currentItem.ID.ToString()));
    }

    private void ReturnButtonClicked()
    {
        if (currentItem.Type == VRResourceData.DataType.File) currentItem = currentItem.Parent;
        LoadResource(currentItem.Parent);
    }
    private IEnumerator Load_Document(string documentid)
    {
        Debug.Log("Wait for Authorization ...");

        if (!textAnnotatorInterface.Authorized)
            yield return StartCoroutine(textAnnotatorInterface.StartAuthorization());

        Debug.Log("Load Document: " + documentid);
        textAnnotatorInterface.FireJSONCommand(TextAnnotatorInterface.CommandType.open_cas, documentid);

        LoadButton.interactable = false;
        LoadButton.GetComponentInChildren<Text>().text = "Loading...";

        yield return new WaitUntil(() => textAnnotatorInterface.ActualDocument?.CasId.Equals(documentid) == true);
        Debug.Log("Document loaded");
        

        Debug.Log("..........");
        foreach (string view in textAnnotatorInterface.ActualDocument.Views)
            Debug.Log(view);
        Debug.Log("..........");

        textAnnotatorInterface.FireJSONCommand(TextAnnotatorInterface.CommandType.open_tool, textAnnotatorInterface.ActualDocument.CasId, null, null, null, textAnnotatorInterface.ActualDocument.View);

        while (!textAnnotatorInterface.ActualDocument.DocumentCreated)
            yield return null;
        Debug.Log("View loaded");
        LoadButton.GetComponentInChildren<Text>().text = "View";
        LoadButton.interactable = true;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        
        SubscribeButton(LoadButton, LoadButtonClicked);
        SubscribeButton(ReturnButton, ReturnButtonClicked);
        Initialize();
    }

    private void Initialize()
    {
        textAnnotatorInterface = MenuController.GetTextAnnotatorInterface();
        VRResourceData root = new VRResourceData("root", "root", null, "root", DateTime.MaxValue, DateTime.MaxValue, VRData.SourceType.Remote);

        if (textAnnotatorInterface != null)
        {
            Debug.Log("Loading " + root.Path);
            LoadResource(root);
        }

        PageNavigator.OnValueChanged.AddListener(() => UpdateContainer());
    }

    private void LoadResource(VRResourceData parentItem)
    {
        var crumbs = new List<string>();
        var prevItem = parentItem;
        while(!prevItem.ID.Equals("root"))
        {
            crumbs.Add(prevItem.Name);
            prevItem = prevItem.Parent;
        }
        crumbs.Reverse();
        Breadcrumb.GetComponentInChildren<TextMeshProUGUI>().text = String.Join("/", crumbs);
        currentItem = parentItem;
        ErrorText.gameObject.SetActive(true);
        ErrorText.text = "Loading resources...";
        BrowserContainer.Clear();
        StartCoroutine(textAnnotatorInterface.ResourceManager.GetRepositoryInformations(parentItem, () =>
        {
            var fileList = parentItem.FileFormatMap.Values.ToList().SelectMany(d => d);
            childItemList = fileList.Concat(parentItem.NonEmptyFolders).Concat(parentItem.EmptyFolders);
            PageNavigator.Value = PageNavigator.MinValue;
            PageNavigator.MaxValue = (childItemList.Count() + ItemPerPage - 1) / ItemPerPage;
            UpdateContainer();
            ErrorText.gameObject.SetActive(false);
        }));
    }

    private void UpdateContainer()
    {
        LoadButton.interactable = currentItem.Type == VRResourceData.DataType.File;
        BrowserContainer.Clear();
        var ChildItemObjects = new List<GameObject>();
        foreach (VRResourceData childItem in childItemList.Skip((PageNavigator.Value - 1) * ItemPerPage).Take(ItemPerPage))
        {
            ChildItemObjects.Add(CreateBrowserItem(childItem));
        }
    }
    private GameObject CreateBrowserItem(VRResourceData item)
    {
        GameObject itemObject;
        if (item.Type == VRResourceData.DataType.File)
        {
            itemObject = Instantiate(FileItemPrefab, BrowserContainer);
            itemObject.GetComponent<Button>().onClick.AddListener(() => {
                currentItem = item;
                LoadButton.interactable = true;
                foreach (Transform child in BrowserContainer)
                {
                    var image = child.gameObject.GetComponentInChildren<Image>();
                    image.color = (child.gameObject.name == currentItem.Name) ? Color.blue : new Color(0.2196f, 0.2196f, 0.2196f);
                }
            });
        }
        else
        {
            itemObject = Instantiate(FolderItemPrefab, BrowserContainer);
            itemObject.GetComponent<Button>().onClick.AddListener(() => LoadResource(item));
        }
        itemObject.name = item.Name;
        TextMeshProUGUI text = itemObject.GetComponentInChildren<TextMeshProUGUI>();
        text.text = item.Name;
        return itemObject;
    }

    private void OnDisable()
    {
        LoadButton.onClick.RemoveAllListeners();
        ReturnButton.onClick.RemoveAllListeners();
        PageNavigator.OnValueChanged.RemoveAllListeners();
    }
}
