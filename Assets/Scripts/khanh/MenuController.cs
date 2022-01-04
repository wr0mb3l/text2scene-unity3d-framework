using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEditor;

public class MenuController : MonoBehaviour
{


    private Button LoadButton;
 
    List<GameObject> menuList;
    GameObject lastActiveMenu;

    TextAnnotatorInterface textAnnotatorInterface;
    ResourceManagerInterface resourceManagerInterface;

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
        var usernameInput = lastActiveMenu.transform.Find("UsernameInput").GetComponent<InputField>();
        var passwordInput = lastActiveMenu.transform.Find("PasswordInput").GetComponent<InputField>();
        string username = "s8467169";
        string password = "ahw2LohG";
        usernameInput.text = username;
        passwordInput.text = password;
        if (ValidateInput(usernameInput) && ValidateInput(passwordInput))
        {
            StartCoroutine(Login(usernameInput.text, passwordInput.text));
        }
    }

    private void LoadButtonClicked()
    {
        var documentUrlInput = lastActiveMenu.transform.Find("DocumentUrlInput").GetComponent<InputField>();
        var documentId = "27268";
        documentUrlInput.text = documentId;
        if (ValidateInput(documentUrlInput))
        {
            //StartCoroutine(Load_Document(documentUrlInput.text));
            StartCoroutine(Load_Document(documentId));
        }
    }

    private IEnumerator Login(string username, string password)
    {
        //string username = "s8467169";
        //string password = "ahw2LohG";
        StartCoroutine(textAnnotatorInterface.LoginWithCredential(username, password));
        yield return new WaitUntil(() => textAnnotatorInterface.Authorized);
        SwitchMenu("LoadMenu");

    }
    private IEnumerator Load_Document(string documentid)
    {
        Debug.Log("Wait for Authorization ...");

        if (!textAnnotatorInterface.Authorized)
            yield return StartCoroutine(textAnnotatorInterface.StartAuthorization());

        Debug.Log("Load Document: " + documentid);
        textAnnotatorInterface.FireJSONCommand(TextAnnotatorInterface.CommandType.open_cas, documentid);


        yield return new WaitUntil(() => textAnnotatorInterface.ActualDocument?.CasId.Equals(documentid) == true);
        Debug.Log("Document loaded");
        LoadButton.GetComponentInChildren<Text>().text = "View";

        /*Debug.Log("..........");
        foreach (string view in textAnnotatorInterface.ActualDocument.Views)
            Debug.Log(view);
        Debug.Log("..........");*/

        textAnnotatorInterface.FireJSONCommand(TextAnnotatorInterface.CommandType.open_tool, textAnnotatorInterface.ActualDocument.CasId, null, null, null, view);

        while (!textAnnotatorInterface.ActualDocument.DocumentCreated)
            yield return null;

        Debug.Log("View loaded");

        //StartCoroutine(Create_Scene());
    }

    private void SwitchMenu(string menuName)
    {
        Debug.Log(menuName);
        if (lastActiveMenu != null)
        {
            lastActiveMenu.SetActive(false);
        }
        
        GameObject desiredMenu = menuList.Find(menuObject => menuObject.name == menuName);
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
        var errorText = lastActiveMenu.transform.Find("ErrorText").GetComponent<Text>();
        if (input.text.Trim().Length == 0)
        {
            errorText.text = $"{input.name} must not be empty";
            return false;
        }
        else
        {
            errorText.text = "";
            return true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Init_Interfaces();
        Init_UI();
    }                                                                                                                                                         

    private void Init_UI()
    {
        Button[] buttons = gameObject.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            
            if (button.name.Equals("LoginButton"))
            {
                button.onClick.AddListener(LoginButtonClicked);
            }
                
            if (button.name.Equals("LoadButton"))
            {
                button.onClick.AddListener(LoadButtonClicked);
                LoadButton = button;
            }
            if (button.name.Equals("LogoutButton")) 
                button.onClick.AddListener(() => SwitchMenu("LoginMenu"));
        }
        menuList = GameObject.FindGameObjectsWithTag("Menu").ToList();
        menuList.ForEach(menuObject => menuObject.SetActive(false));
        SwitchMenu("LoginMenu");
    }

    // Update is called once per frame
    void Update()
    {
    }
}
