using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coreferenz Links (inkls. Part of, ect.)
/// </summary>
public class IsoMetaLink: IsoLink
{

    public new static Color ClassColor { get; } = StolperwegeHelper.GUCOLOR.HELLESGRUEN;
    public IsoMetaLink(AnnotationBase parent, int ID, string comment, string mod, IsoEntity figure, IsoEntity ground, IsoEntity trigger, string rel_type) : 
        base(parent, ID, comment, mod, figure, ground, trigger, rel_type, AnnotationTypes.META_LINK)
    {
        if (rel_type.Equals("Coreference"))
        {
            if (figure.Object3D == null)
                figure.Object3D = ground.Object3D;
        }
    }

    public override void RemoveElement()
    {
        if (Figure.Object3D == Ground.Object3D)
            Figure.Object3D = null;

        base.RemoveElement();
    }
}
