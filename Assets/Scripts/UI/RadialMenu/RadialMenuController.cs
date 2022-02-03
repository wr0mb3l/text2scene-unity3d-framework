using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class RadialMenuController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Is this used on a Keyboard Rig?")]
    bool m_Keyboard;
    /// <summary>
    /// Is this used on a Keyboard Rig?
    /// </summary>
    public bool keyboard
    {
        get => m_Keyboard;
        set => m_Keyboard = value;
    }

    [SerializeField]
    [Tooltip("The reference to the action of choosing a radial menu option for this controller.")]
    InputActionReference m_MenuSelectAxis;
    /// <summary>
    /// The reference to the action of choosing a radial menu option for this controller."
    /// </summary>
    public InputActionReference menuSelectAxis
    {
        get => m_MenuSelectAxis;
        set => m_MenuSelectAxis = value;
    }

    [SerializeField]
    [Tooltip("The reference to the action of clicking a menu button.")]
    InputActionReference m_MenuClick;
    /// <summary>
    /// The reference to the action of clicking a menu button.
    /// </summary>
    public InputActionReference menuClick => m_MenuClick;

    [Space]
    [Header("Interactor")]

    [SerializeField]
    [Tooltip("The Ray Interactor to use for options.")]
    XRBaseInteractor m_Interactor;
    /// <summary>
    /// (Read Only) The Ray Interactor to use for options."
    /// </summary>
    public XRBaseInteractor interactor => m_Interactor;

    [SerializeField]
    [Tooltip("The current selected gameObject.")]
    GameObject m_SelectedObject;
    /// <summary>
    /// (Read Only) The current selected gameObject.
    /// </summary>
    public GameObject selectedObject
    {
        set => m_SelectedObject = value;
        get => m_SelectedObject;
    }

    [Space]
    [Header("Menu Items")]
    [SerializeField]
    [Tooltip("The cursor gameObject.")]
    GameObject m_Cursor;
    /// <summary>
    /// The cursor gameObject.
    /// </summary>
    public GameObject cursor => m_Cursor;

    [SerializeField]
    [Tooltip("The heading gameObject.")]
    GameObject m_Heading;
    /// <summary>
    /// The heading gameObject.
    /// </summary>
    public GameObject heading => m_Heading;

    [SerializeField]
    [Tooltip("The previous page Button in the radial menu.")]
    GameObject m_PreviousPage;
    /// <summary>
    /// The cursor gameObject.
    /// </summary>
    public GameObject previousPageText => m_PreviousPage;

    [SerializeField]
    [Tooltip("The next page Button in the radial menu.")]
    GameObject m_NextPage;
    /// <summary>
    /// The cursor gameObject.
    /// </summary>
    public GameObject nextPageText => m_NextPage;

    [SerializeField]
    [Tooltip("The radial button gameObjects.")]
    List<GameObject> m_RadialButtons;
    /// <summary>
    /// The radial button gameObjects.
    /// </summary>
    public List<GameObject> radialButtons => m_RadialButtons;


    private const int SECTIONS = 8;
    private float m_DegreeIncrement = 360.0f / SECTIONS;

    private int m_HoveredSection = -1;
    private int m_CurrentPage = 0;

    private Vector2 m_CursorPos;

    private List<RadialSection> m_RadialSections = new List<RadialSection>();
    private List<RadialSection> m_CurrentSections = new List<RadialSection>();
    private string m_PrettyHeading = "heading";

    // Stores the layers and their corresponding menus.
    // private Dictionary<int, RadialSection> allMenus = RadialMenuData.layerMenus;

    private void OnEnable()
    {
        // Detect hit object
        List<XRBaseInteractable> targets = new List<XRBaseInteractable>();
        m_Interactor.GetHoverTargets(targets);
        m_SelectedObject = targets[0].transform.gameObject;

        // Get appropriate menu according to layer
        m_RadialSections = RadialMenuData.GetMenuFromInteractionLayerMask(targets[0]);

        m_CurrentSections = m_RadialSections;

        m_CursorPos = new Vector2(0, 0);

        DisableAllSelections();

        ShowPage();

        Show(true);
    }

    private void OnDisable()
    {
        Show(false);
    }

    public void Show(bool value)
    {
        GetComponent<Canvas>().enabled = value;
    }

    public void Update()
    {
        var menuSelectAction = GetInputAction(m_MenuSelectAxis);

        if (menuSelectAction != null)
        {
            Vector2 pos = menuSelectAction.ReadValue<Vector2>();

            if (keyboard)
            {
                // Add new Input to current position
                m_CursorPos += Time.deltaTime * pos;
                if (m_CursorPos.magnitude > 1) m_CursorPos.Normalize();
            }
            else
            {
                m_CursorPos = pos;
            }

            SetCursorPosition(m_CursorPos);
            SetCursorActive(true);

            // Determine and highlight corresponding Section
            if (m_CursorPos != new Vector2(0, 0))
            {
                if (m_CursorPos.magnitude >= 0.6)
                {
                    float rotation = GetDegree(m_CursorPos);
                    m_HoveredSection = GetNearestSection(rotation);

                    SelectButton(m_RadialButtons[m_HoveredSection]);
                }
                else if (m_CursorPos.magnitude <= 0.3)
                {
                    if (m_CursorPos.x < 0)
                    {
                        SelectButton(m_PreviousPage);
                        m_HoveredSection = SECTIONS;
                    }
                    else if (m_CursorPos.x > 0)
                    {
                        SelectButton(m_NextPage);
                        m_HoveredSection = SECTIONS + 1;
                    }
                }
                else
                {
                    m_HoveredSection = -1;
                    DisableAllSelections();
                }
            }
            else
            {
                m_HoveredSection = -1;
                DisableAllSelections();
            }
        }
        else
        {
            m_HoveredSection = -1;
            SetCursorActive(false);
        }

        var menuClickAction = GetInputAction(m_MenuClick);
        if (menuClickAction != null && menuClickAction.triggered)
        {
            // Handle Click
            if (m_HoveredSection != -1 && m_HoveredSection < SECTIONS)
            {
                // Get sections at virtual index
                var vindex = SECTIONS * m_CurrentPage + m_HoveredSection;
                if (vindex < m_CurrentSections.Count && m_CurrentSections[vindex] != null)
                {
                    var children = m_CurrentSections[vindex].childSections;
                    if (children != null)
                    {
                        m_PrettyHeading = m_CurrentSections[m_HoveredSection].title;
                        m_CurrentSections = children;
                        ShowPage();
                    }
                    else if (m_CurrentSections[vindex].title == "CANCEL") // TODO: Find better Check
                    {
                        CloseMenu();
                    }
                }
            }
            else if (m_HoveredSection == SECTIONS)
            {
                // Go to prev page
                if (m_PreviousPage.activeSelf)
                {
                    m_CurrentPage -= 1;
                    ShowPage();
                }
            }
            else if (m_HoveredSection == SECTIONS + 1)
            {
                if (m_NextPage.activeSelf)
                {
                    // Go to next page
                    m_CurrentPage += 1;
                    ShowPage();
                }
            }
        }
    }

    private void ShowPage()
    {
        for (int i = 0; i < SECTIONS; i++)
        {
            var virtualIndex = SECTIONS * m_CurrentPage + i;
            if (virtualIndex < m_CurrentSections.Count && m_CurrentSections[virtualIndex] != null)
                m_RadialButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = m_CurrentSections[virtualIndex].title;
            else
                m_RadialButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
        m_Heading.GetComponent<TextMeshProUGUI>().text = m_PrettyHeading;

        // Activate/Deactivate Prev/Next Buttons
        m_NextPage.SetActive(SECTIONS * (m_CurrentPage + 1) < m_CurrentSections.Count);
        m_PreviousPage.SetActive(m_CurrentPage > 0);
    }

    private float GetDegree(Vector2 direction)
    {
        float value = Mathf.Atan2(direction.x, direction.y);
        value *= Mathf.Rad2Deg;

        if (value < 0)
            value += 360.0f;

        return value;
    }

    private int GetNearestSection(float rotation)
    {
        return Mathf.RoundToInt(rotation / m_DegreeIncrement) % 8;
    }

    private void SelectButton(GameObject obj)
    {
        var buttonComponent = obj.GetComponent<Button>();
        buttonComponent.Select();
    }
    
    private void DisableAllSelections()
    {
        GameObject myEventSystem = GameObject.Find("EventSystem");
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    private void SetCursorActive(bool value) => m_Cursor.SetActive(value);

    private void SetCursorPosition(Vector2 position) => m_Cursor.transform.localPosition = 200 * position;

    private void CloseMenu()
    {
        DisableAllSelections();
        gameObject.GetComponent<Canvas>().enabled = false;
    }

    static InputAction GetInputAction(InputActionReference actionReference)
    {
        return actionReference != null ? actionReference.action : null;
    }
}
