using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class RadialMenuProvider : MonoBehaviour
{
    [Serializable]
    public class OnClickEvent : UnityEvent { }

    public class RadialMenuItem
    {
        string m_Text;
        /// <summary>
        /// The String displayed on this Button.
        /// </summary>
        public string text
        {
            get => m_Text;
            set => m_Text = value;
        }
        OnClickEvent m_OnClick = new OnClickEvent();
        /// <summary>
        /// The OnClickEvent that will be invoked when clicking this Button.
        /// </summary>
        public OnClickEvent onClick
        {
            get => m_OnClick;
            set => m_OnClick = value;
        }
        void logClick() {
            Debug.Log("Clicked " + text);
        }

        public RadialMenuItem(string str)
        {
            text = str;
            onClick.AddListener(logClick);
        }
    }

    public class RadialMenuPage : RadialMenuItem
    {
        List<RadialMenuPage> m_subPages = new List<RadialMenuPage>();
        /// <summary>
        /// The subpages that are accessible from this menu page.
        /// </summary>
        public List<RadialMenuPage> subPages
        {
            get => m_subPages;
            set => m_subPages = value;
        }

        List<RadialMenuItem> m_Buttons = new List<RadialMenuItem>();
        /// <summary>
        /// The buttons that are accessible from this menu page.
        /// </summary>
        public List<RadialMenuItem> buttons
        {
            get => m_Buttons;
            set => m_Buttons = value;
        }

        public List<RadialMenuItem> items
        {
            get
            {
                List<RadialMenuItem> m_items = new List<RadialMenuItem>();
                foreach (RadialMenuPage subPage in subPages)
                {
                    var sp = new RadialMenuItem(subPage.text);
                    sp.onClick = subPage.onClick;
                    m_items.Add(sp);
                }
                foreach (RadialMenuItem button in buttons)
                    m_items.Add(button);
                return m_items;
            }
        }

        public RadialMenuPage(string str) : base(str)
        {

        }
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

    [Space]
    [Header("Interactor")]

    [SerializeField]
    [Tooltip("The Ray Interactor to use for options.")]
    XRRayInteractor m_EditInteractor;
    /// <summary>
    /// (Read Only) The Ray Interactor to use for options."
    /// </summary>
    public XRRayInteractor editInteractor => m_EditInteractor;

    [SerializeField]
    [Tooltip("The current selected gameObject.")]
    GameObject m_SelectedObject;
    /// <summary>
    /// (Read Only) The current selected gameObject.
    /// </summary>
    public GameObject selectedObject => m_SelectedObject;

    [SerializeField]
    [Tooltip("The cursor gameObject.")]
    GameObject m_cursor;
    /// <summary>
    /// The cursor gameObject.
    /// </summary>
    public GameObject cursor => m_cursor;

    [Header("Menu Items")]

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
    [Tooltip("The previous page text gameObject in the radial menu.")]
    GameObject m_previousPage;
    /// <summary>
    /// The cursor gameObject.
    /// </summary>
    public GameObject previousPageText => m_previousPage;

    [SerializeField]
    [Tooltip("The next page text gameObject in the radial menu.")]
    GameObject m_nextPage;
    /// <summary>
    /// The cursor gameObject.
    /// </summary>
    public GameObject nextPageText => m_nextPage;

    [SerializeField]
    [Tooltip("The text field gameObjects in the radial menu.")]
    List<GameObject> m_texts;
    /// <summary>
    /// The text field gameObjects in the radial menu.
    /// </summary>
    public List<GameObject> texts => m_texts;


    // Variables that store the current state of the menu
    RadialMenuPage m_currentPage;
    int m_currentIndex;
    // Detect Hover
    int m_hoveredItem;

    // Small sample Menu page
    RadialMenuPage menuPage;

    private void OnEnable()
    {
        m_EditInteractor = transform.parent.GetComponent<XRRayInteractor>();

        RaycastHit target;
        m_EditInteractor.TryGetCurrent3DRaycastHit(out target);

        m_SelectedObject = target.transform.gameObject;

        // Small default menu
        RadialMenuPage subPage0 = new RadialMenuPage("Subpage 0");
        subPage0.onClick.AddListener(SwitchToSubPage);
        subPage0.buttons.Add(new RadialMenuItem("S0B0"));
        subPage0.buttons.Add(new RadialMenuItem("S0B1"));
        RadialMenuPage subPage1 = new RadialMenuPage("Subpage 1");
        subPage1.onClick.AddListener(SwitchToSubPage);
        subPage1.buttons.Add(new RadialMenuItem("S1B0"));
        subPage1.buttons.Add(new RadialMenuItem("S1B1"));
        subPage1.buttons.Add(new RadialMenuItem("S1B2"));

        menuPage = new RadialMenuPage("Edit menu");
        menuPage.subPages.Add(subPage0);
        menuPage.subPages.Add(subPage1);
        menuPage.buttons.Add(new RadialMenuItem("B0"));
        menuPage.buttons.Add(new RadialMenuItem("B1"));
        menuPage.buttons.Add(new RadialMenuItem("B2"));
        menuPage.buttons.Add(new RadialMenuItem("B3"));
        menuPage.buttons.Add(new RadialMenuItem("B4"));
        menuPage.buttons.Add(new RadialMenuItem("B5"));
        menuPage.buttons.Add(new RadialMenuItem("B6"));

        m_currentIndex = 0;
        m_hoveredItem = -1;
        m_currentPage = null;

        SwitchToSubPage();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var menuSelectAction = GetInputAction(m_MenuSelectAxis);
        var canvas = GetComponent<RectTransform>();
        if (menuSelectAction != null)
        {
            Vector2 pos = menuSelectAction.ReadValue<Vector2>();
            m_cursor.transform.localPosition = new Vector3(canvas.rect.height*pos.x/2, canvas.rect.width*pos.y/2, 0);

            bool inputRelease = menuSelectAction.phase == InputActionPhase.Waiting;
            if (!inputRelease )
            {
                // Calculate hit button
                if (pos.magnitude >= 0.625)
                {
                    if (pos.x == 0)
                    {
                        m_hoveredItem = (pos.y >= 0) ? 3 : 7;
                    }
                    else
                    {
                        var angle = Math.Atan(pos.y / pos.x);
                        if (pos.x > 0)
                            m_hoveredItem = (int)(112.5 - angle * (180 / Math.PI)) / 45;
                        else
                            m_hoveredItem = (int)(292.5 - angle * (180 / Math.PI)) / 45;
                        m_hoveredItem = (m_hoveredItem + 1) % 8;
                    }
                    // TODO: Highlight hovered item
                    var image = m_selected.GetComponent<Image>();
                    image.transform.localEulerAngles = new Vector3(0, 0, 45 - 45 * m_hoveredItem);
                    image.enabled = true;
                }
                else
                {
                    m_hoveredItem = -1;
                    m_selected.GetComponent<Image>().enabled = false;
                }
            }
            else
            {
                m_selected.GetComponent<Image>().enabled = false;
            }
            if (inputRelease && m_hoveredItem != -1)
            {
                // Handle release
                if (m_hoveredItem < m_currentPage.items.Count)
                    m_currentPage.items[m_hoveredItem].onClick.Invoke();
                m_hoveredItem = -1;
            }
        }
    }

    void SwitchToSubPage()
    {
        if (m_currentPage == null)
            m_currentPage = menuPage;
        else
            m_currentPage = m_currentPage.subPages[m_hoveredItem - (m_hoveredItem) / 3 ];

        m_currentIndex = -1;
        nextPage();
    }

    void nextPage()
    {
        m_currentIndex += 1;
        if (m_currentIndex >= 1)
            m_previousPage.SetActive(true);
        else
            m_previousPage.SetActive(false);

        var maxIndex = (m_currentPage.subPages.Count + m_currentPage.buttons.Count) / m_texts.Count;
        if (m_currentIndex >= maxIndex)
            m_nextPage.SetActive(false);
        else
            m_nextPage.SetActive(true);
        showPage();
    }
    void prevPage()
    {
        m_currentIndex -= 1;
        m_nextPage.SetActive(true);

        if (m_currentIndex == 0)
            m_previousPage.SetActive(false);
        showPage();

    }
    void showPage()
    {
        var items = m_currentPage.items;
        for (int i = 0; i < m_texts.Count; i++)
        {
            if (i < items.Count)
                m_texts[i].GetComponent<Text>().text = items[i].text;
            else
                m_texts[i].GetComponent<Text>().text = null;
        }
        m_heading.GetComponent<Text>().text = m_currentPage.text;
    }

    static InputAction GetInputAction(InputActionReference actionReference)
    {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
        return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
    }
}
