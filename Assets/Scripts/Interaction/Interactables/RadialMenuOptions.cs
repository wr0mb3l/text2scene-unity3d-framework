using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("XR/Radial Menu Options")]
public class RadialMenuOptions : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The Menu to open when interacting with this object.")]
    RadialMenuData.MenuType m_MenuType;
    /// <summary>
    /// The Menu to open when interacting with this object.
    /// </summary>
    public RadialMenuData.MenuType menuType
    {
        get => m_MenuType;
        set => m_MenuType = value;
    }
}
