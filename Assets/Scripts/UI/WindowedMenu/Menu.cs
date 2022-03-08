using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Menu : MonoBehaviour
{
    public Text ErrorText;
    public MenuType Type;
    protected bool ValidateInput(InputField input)
    {
        if (input.text.Trim().Length == 0)
        {
            ErrorText.text = $"{input.name} must not be empty";
            return false;
        }
        else
        {
            ErrorText.text = "";
            return true;
        }
    }

    protected void SubscribeButton(Button button, UnityEngine.Events.UnityAction callback)
    {
        button.onClick.AddListener(callback);
    }
}
