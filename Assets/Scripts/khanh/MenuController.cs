using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEditor;

public class MenuController : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public Button LoginButton;

    public InputField filePathInput;
    public Button BrowseButton;
    public Button ViewButton;
    public Button LogoutButton;

    List<GameObject> menuList;
    GameObject lastActiveMenu;

    TextAnnotatorInterface textAnnotatorInterface;
    ResourceManagerInterface resourceManagerInterface;

    bool isLoaded;

    private void Init_Interfaces()
    {
        Debug.Log("Init Interfaces");
        textAnnotatorInterface = gameObject.AddComponent<TextAnnotatorInterface>();
        StartCoroutine(textAnnotatorInterface.Initialize());

        resourceManagerInterface = gameObject.AddComponent<ResourceManagerInterface>();
        textAnnotatorInterface.ResourceManager = resourceManagerInterface;
    }

    private void LoginButtonClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        if (ValidateInput(usernameInput) && ValidateInput(passwordInput))
        {
            //StartCoroutine(Login());
        }
        //StartCoroutine(textAnnotatorInterface.LoginWithCredential(username, password));
        StartCoroutine(Login());
    }

    private void ViewButtonClicked()
    {
        var documentId = "27268";
        if (!isLoaded)
        {
            StartCoroutine(Load_Document(documentId));
        }
    }

    private IEnumerator Login()
    {
        string username = "s8467169";
        string password = "ahw2LohG";
        StartCoroutine(textAnnotatorInterface.LoginWithCredential(username, password));
        yield return new WaitUntil(() => textAnnotatorInterface.Authorized);
        SwitchMenu("Browse");

    }
    private IEnumerator Load_Document(string documentid)
    {
        Debug.Log("Wait for Authorization ...");

        if (!textAnnotatorInterface.Authorized)
            yield return StartCoroutine(textAnnotatorInterface.StartAuthorization());

        Debug.Log("Load Document: " + documentid);
        textAnnotatorInterface.FireJSONCommand(TextAnnotatorInterface.CommandType.open_cas, documentid);


        //while (textAnnotatorInterface.ActualDocument == null || (!textAnnotatorInterface.ActualDocument.CasId.Equals(documentid) && !textAnnotatorInterface.ActualDocument.ViewsLoaded))
        //while (textAnnotatorInterface.ActualDocument == null || !textAnnotatorInterface.ActualDocument.CasId.Equals(documentid))
        //    yield return null;
        yield return new WaitUntil(() => textAnnotatorInterface.ActualDocument?.CasId.Equals(documentid) == true);
        Debug.Log("Document loaded");
        isLoaded = true;
        ViewButton.GetComponentInChildren<Text>().text = "View";

        Debug.Log("..........");
        foreach (string view in textAnnotatorInterface.ActualDocument.Views)
            Debug.Log(view);
        Debug.Log("..........");

        //textAnnotatorInterface.FireJSONCommand(TextAnnotatorInterface.CommandType.open_tool, textAnnotatorInterface.ActualDocument.CasId, null, null, null, view);

        //while (!textAnnotatorInterface.ActualDocument.DocumentCreated)
        //    yield return null;

        //Debug.Log("View loaded");

        //StartCoroutine(Create_Scene());
    }

    private void SwitchMenu(string tag)
    {
        Debug.Log(tag);
        if (lastActiveMenu != null)
        {
            lastActiveMenu.SetActive(false);
        }
        
        GameObject desiredMenu = menuList.Find(menuObject => menuObject.name == tag);
        if (desiredMenu != null)
        {
            desiredMenu.SetActive(true);
            lastActiveMenu = desiredMenu;
        }
        else
        {
            Debug.LogWarning("The desired menu is not found");
        }
    }    

    private bool ValidateInput(InputField input)
    {
        var textObject = input.placeholder.GetComponent<Text>();
        if (input.text.Trim().Length == 0)
        {
            textObject.text = $"{input.name} must not be empty";
            textObject.color = Color.red;
            return false;
        }
        else
        {
            textObject.color = Color.black;
            return true;
        }
    }
    private void DownloadFile()
    {
        Debug.Log(filePathInput.text);
        ValidateInput(filePathInput);
        
    }
    // Start is called before the first frame update
    void Start()
    {
        Init_Interfaces();

    }                                                                                                                                                         

    private void Awake()
    {
        menuList = GameObject.FindGameObjectsWithTag("Menu").ToList();
        menuList.ForEach(menuObject => menuObject.SetActive(false));
        SwitchMenu("Login");
        LoginButton.onClick.AddListener(LoginButtonClicked);
        ViewButton.onClick.AddListener(ViewButtonClicked);
        LogoutButton.onClick.AddListener(() => SwitchMenu("Login"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
