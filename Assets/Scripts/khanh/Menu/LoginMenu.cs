using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginMenu : Menu
{
    public InputField UsernameInput;
    public InputField PasswordInput;
    public Button LoginButton;

    Tuple<string, string, string> khanh = Tuple.Create("s8467169", "ahw2LohG", "27268");
    Tuple<string, string, string> kett = Tuple.Create("kett", "Tig3rm4n", "27019");
    private void LoginButtonClicked()
    {
        UsernameInput.text = kett.Item1;
        PasswordInput.text = kett.Item2;
        if (ValidateInput(UsernameInput) && ValidateInput(PasswordInput))
        {
            StartCoroutine(Login(UsernameInput.text, PasswordInput.text));
        }
    }

    private IEnumerator Login(string username, string password)
    {
        var textAnnotatorInterface = MenuController.GetTextAnnotatorInterface();
        StartCoroutine(textAnnotatorInterface.LoginWithCredential(username, password));
        yield return new WaitUntil(() => textAnnotatorInterface.Authorized);
        MenuController.SwitchMenu(MenuType.DocumentBrowserMenu);
    }

    private void OnEnable()
    {
        SubscribeButton(LoginButton, LoginButtonClicked);
    }

    private void OnDisable()
    {
        LoginButton.onClick.RemoveAllListeners();
    }
}
