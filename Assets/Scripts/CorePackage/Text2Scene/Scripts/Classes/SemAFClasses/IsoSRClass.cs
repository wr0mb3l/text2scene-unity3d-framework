using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Semantic Role Labeling Links
/// </summary>
public class IsoSrLink : IsoLink
{
    public new static Color ClassColor { get; } = StolperwegeHelper.GUCOLOR.LICHTBLAU;
    public IsoSrLink(AnnotationBase parent, int ID, string comment, string mod, IsoEntity figure, IsoEntity ground, IsoEntity trigger, string rel_type) : 
        base(parent, ID, comment, mod, figure, ground, trigger, rel_type, AnnotationTypes.SR_LINK)
    {

    }

    protected IsoSrLink(AnnotationBase parent, int ID, string comment, string mod, IsoEntity figure, IsoEntity ground, IsoEntity trigger, string rel_type, string class_type) : 
        base(parent, ID, comment, mod, figure, ground, trigger, rel_type, class_type)
    {

    }
}
