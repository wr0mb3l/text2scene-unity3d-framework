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
    /// The Child Sections appearing when clicking this section.
    /// </summary>
    public List<RadialSection> childSections = null;

    public RadialSection(string title, string description, List<RadialSection> childSections, object value = null)
    {
        this.title = title;
        this.description = description;
        this.childSections = childSections;
    }
}
