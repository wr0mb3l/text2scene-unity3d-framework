using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
/***********************************************************************
    TokenObjectScript controls display of token
***********************************************************************/
public class TokenObjectScript : MonoBehaviour
{
    public AnnotationBase _token { get; private set; }

    public void Init(AnnotationBase token, Color color)
    {
        _token = token;
        setToken(color);
    }

    void setToken(Color color)
    {
        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        text.text = string.Join(" ", _token.TextContent.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        if (color != null)
            text.color = color;
        else
            text.color = Color.white;
    }
}
