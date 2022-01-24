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
    [Tooltip("Keyboard Radial Menu.")]
    bool m_Keyboard;
    /// <summary>
    /// Keyboard Radial Menu."
    /// </summary>
    public bool keyboard => m_Keyboard;

    [SerializeField]
    [Tooltip("The reference to the action of choosing a radial menu option for this controller.")]
    InputActionReference m_menuSelectAxis;
    /// <summary>
    /// The reference to the action of choosing a radial menu option for this controller."
    /// </summary>
    public InputActionReference menuSelectAxis
    {
        get => m_menuSelectAxis;
        set => m_menuSelectAxis = value;
    }

    [SerializeField]
    [Tooltip("The reference to the action of clicking a menu button.")]
    InputActionReference m_menuClick;
    /// <summary>
    /// The reference to the action of clicking a menu button.
    /// </summary>
    public InputActionReference menuClick => m_menuClick;

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
    public GameObject selectedObject => m_SelectedObject;

    [Space]
    [Header("Menu Items")]
    [SerializeField]
    [Tooltip("The cursor gameObject.")]
    GameObject m_cursor;
    /// <summary>
    /// The cursor gameObject.
    /// </summary>
    public GameObject cursor => m_cursor;

    [SerializeField]
    [Tooltip("The heading gameObject.")]
    GameObject m_heading;
    /// <summary>
    /// The heading gameObject.
    /// </summary>
    public GameObject heading => m_heading;

    [SerializeField]
    [Tooltip("The selected sprite gameObject.")]
    GameObject m_selected;
    /// <summary>
    /// The selected sprite gameObject.
    /// </summary>
    public GameObject selected => m_selected;

    [SerializeField]
    [Tooltip("The previous page Button in the radial menu.")]
    GameObject m_previousPage;
    /// <summary>
    /// The cursor gameObject.
    /// </summary>
    public GameObject previousPageText => m_previousPage;

    [SerializeField]
    [Tooltip("The next page Button in the radial menu.")]
    GameObject m_nextPage;
    /// <summary>
    /// The cursor gameObject.
    /// </summary>
    public GameObject nextPageText => m_nextPage;

    [SerializeField]
    [Tooltip("The radial button gameObjects.")]
    List<GameObject> m_RadialButtons;
    /// <summary>
    /// The radial button gameObjects.
    /// </summary>
    public List<GameObject> radialButtons => m_RadialButtons;


    private const int SECTIONS = 8;
    private float degreeIncrement = 360.0f / SECTIONS;

    private int hoveredSection = -1;
    private int currentPage = 0;

    private List<RadialSection> radialSections = new List<RadialSection>();
    private List<RadialSection> currentSections = new List<RadialSection>();
    private string prettyHeading = "heading";

    // Stores the layers and their corresponding menus.
    // private Dictionary<int, RadialSection> allMenus = RadialMenuData.layerMenus;

    private void OnEnable()
    {
        // Detect hit object
        List<XRBaseInteractable> targets = new List<XRBaseInteractable>();
        m_Interactor.GetHoverTargets(targets);

        m_SelectedObject = targets[0].transform.gameObject;

        // Get appropriate menu according to layer
        radialSections = RadialMenuData.GetMenuFromInteractionLayerMask(targets[0]);

        currentSections = radialSections;

        Show(true);

        DisableAllSelections();

        ShowPage();
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
        var menuSelectAction = GetInputAction(m_menuSelectAxis);

        if (menuSelectAction != null)
        {
            Vector2 pos = menuSelectAction.ReadValue<Vector2>();
            if (pos.magnitude >= 0.6)
            {
                float rotation = GetDegree(pos);
                hoveredSection = GetNearestSection(rotation);

                SelectButton(m_RadialButtons[hoveredSection]);
            }
            else if (pos.magnitude <= 0.4)
            {
                if (pos.x < 0)
                {
                    SelectButton(m_previousPage);
                    hoveredSection = SECTIONS;
                } else if (pos.x > 0)
                {
                    SelectButton(m_nextPage);
                    hoveredSection = SECTIONS + 1;
                }
            }
            else
            {
                hoveredSection = -1;
                DisableAllSelections();
            }

            SetCursorPosition(pos);
            SetCursorActive(true);
        }
        else
        {
            hoveredSection = -1;
            SetCursorActive(false);
        }

        var menuClickAction = GetInputAction(m_menuClick);
        if (menuClickAction != null && menuClickAction.triggered)
        {
            // Handle Click
            if (hoveredSection != -1 && hoveredSection < SECTIONS)
            {
                // Get sections at virtual index
                var vindex = SECTIONS * currentPage + hoveredSection;
                if (vindex < currentSections.Count)
                {
                    var children = currentSections[vindex].childSections;
                    if (children != null)
                    {
                        prettyHeading = currentSections[hoveredSection].title;
                        currentSections = children;
                        ShowPage();
                    }
                    else if (currentSections[vindex].title == "CANCEL")
                    {
                        CloseMenu();
                    }
                }
            }
            else if (hoveredSection == SECTIONS)
            {
                // Go to prev page
                if (currentPage > 0)
                {
                    currentPage -= 1;
                    ShowPage();
                }
            }
            else if (hoveredSection == SECTIONS + 1)
            {
                if (SECTIONS * (currentPage + 1) < currentSections.Count)
                {
                    // Go to next page
                    currentPage += 1;
                    ShowPage();
                }
            }
        }

    }

    private void ShowPage()
    {
        for (int i = 0; i < SECTIONS; i++)
        {
            var virtualIndex = SECTIONS * currentPage + i;
            if (virtualIndex < currentSections.Count && currentSections[virtualIndex] != null)
                m_RadialButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentSections[virtualIndex].title;
            else
                m_RadialButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
        m_heading.GetComponent<TextMeshProUGUI>().text = prettyHeading;
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
        return Mathf.RoundToInt(rotation / degreeIncrement) % 8;
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

    private void SetCursorActive(bool value) => m_cursor.SetActive(value);

    private void SetCursorPosition(Vector2 position) => m_cursor.transform.localPosition = 200 * position;

    private void CloseMenu()
    {
        gameObject.GetComponent<Canvas>().enabled = false;
    }

    static InputAction GetInputAction(InputActionReference actionReference)
    {
        return actionReference != null ? actionReference.action : null;
    }
}
