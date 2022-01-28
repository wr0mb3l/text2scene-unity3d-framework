using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class LoadMenu : Menu
{
    public InputField DocumentUrlInput;
    public Button LoadButton;
    public Button LogoutButton;
    
    Tuple<string, string, string> khanh = Tuple.Create("s8467169", "ahw2LohG", "27268");
    //Tuple<string, string, string> kett = Tuple.Create("kett", "Tig3rm4n", "27204");
    Tuple<string, string, string> kett = Tuple.Create("kett", "Tig3rm4n", "23112");
    private TextAnnotatorInterface textAnnotatorInterface;
    private void LoadButtonClicked()
    {
        textAnnotatorInterface = MenuController.GetTextAnnotatorInterface();
        DocumentUrlInput.text = kett.Item3;
        if (ValidateInput(DocumentUrlInput))
        {
            if (textAnnotatorInterface.ActualDocument?.DocumentCreated == true) 
                MenuController.SwitchMenu(MenuType.AnnotationWindow);
            else StartCoroutine(Load_Document(DocumentUrlInput.text));
        }
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
        SubscribeButton(LogoutButton, () => MenuController.SwitchMenu(MenuType.LoginMenu));
    }

    private void OnDisable()
    {
        LoadButton.onClick.RemoveAllListeners();
        LogoutButton.onClick.RemoveAllListeners();
    }
}
