using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class Counter : MonoBehaviour
{
    [SerializeField]
    public int MaxValue;

    [SerializeField]
    public int MinValue;

    [SerializeField]
    public int Value {
        get
        {
            return _value;
        } 
        set {
            _value = value;
            ValueText.text = _value.ToString();
        } 
    }

    private int _value; 
    public Button IncrementButton;
    public Button DecrementButton;
    public Button ValueDisplay;
    private Text ValueText;
    public UnityEvent OnValueChanged;

    private void Increment()
    {
        if (Value >= MaxValue) return;
        Value++;
        OnValueChanged.Invoke();
    }

    private void Decrement()
    {
        if (Value <= MinValue) return;
        Value--;
        OnValueChanged.Invoke();
    }

    private void Awake()
    {
        ValueDisplay.enabled = false;
        ValueText = ValueDisplay.GetComponentInChildren<Text>();
        if (ValueText != null) Value = MinValue;
        IncrementButton.onClick.AddListener(Increment);
        DecrementButton.onClick.AddListener(Decrement);
    }


}
