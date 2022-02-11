using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class OnClickEvent : UnityEvent { }

public class RadialSection
{
    /// <summary>
    /// The String displayed on this Button.
    /// </summary>
    public string title;
    /// <summary>
    /// The Description to this Button.
    /// </summary>
    public string description;
    /// <summary>
    /// The OnClickEvent that will be invoked when clicking this Button.
    /// </summary>
    public OnClickEvent onClick = new OnClickEvent();
    /// <summary>
    /// The Child Sections appearing when clicking this section.
    /// </summary>
    public List<RadialSection> childSections = null;
    /// <summary>
    /// Can be an action to execute when clicked or an InputType determining this menu's input type.
    /// </summary>
    public object value;

    void logClick()
    {
        Debug.Log("Clicked " + title);
    }

    public RadialSection(string title, string description, List<RadialSection> childSections, object value = null)
    {
        this.title = title;
        this.description = description;
        this.childSections = childSections;
        this.value = value;
    }
}
