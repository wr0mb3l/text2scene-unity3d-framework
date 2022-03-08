using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

/// <summary>
/// Use this class on an Input Action Manager for keyboard inputs so that the keyboard inputs are disabled when
/// tyiping something into an InputField.
/// </summary>
public class KeyboardInputManager : MonoBehaviour
{
    [SerializeField]
    InputActionManager m_InputActionManager;
    /// <summary>
    /// The Input System Action Manager that should be enabled/disabled when using Keyboard otherwise.
    /// </summary>
    public InputActionManager inputActionManager
    {
        get => m_InputActionManager;
        set => m_InputActionManager = value;
    }

    private bool m_InputActive = true;

    // Update is called once per frame
    void Update()
    {
        // When InputFields have focus, they will occupy the EventSystem.current.currentSelectedGameObject property.
        var currentSelected = EventSystem.current.currentSelectedGameObject;
        if (m_InputActive && currentSelected != null && currentSelected.GetComponent<InputField>() != null)
        {
            m_InputActionManager.DisableInput();
            m_InputActive = false;
        }
        else if (!m_InputActive && currentSelected == null)
        {
            m_InputActionManager.EnableInput();
            m_InputActive = true;
        }
    }
}
