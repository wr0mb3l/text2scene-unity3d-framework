using System.Collections.Generic;
using UnityEngine;

public class VRData
{
    public string Name { get; protected set; }

    /// <summary>
    /// Die Identifikationsnummer.
    /// </summary>
    public object ID { get; protected set; }    

    /// <summary>
    /// Enum zur Einordnung des VRData Objekts.
    /// </summary>
    public enum SourceType { Local, Remote }

    /// <summary>
    /// Die Quellentyp, dem das VRData-Objekt zugeornet wird.
    /// </summary>
    public SourceType Source;

    public Color CategoryColor { get; protected set; }

    /// <summary>
    /// Die vom Textinhalt des Objekts ermittelte Sprache.
    /// </summary>
    public string Language;

    /// <summary>
    /// Die Bezeichnung (ID) der Haupt-DDC-Kategorie des Objekts (erste Dezimalstelle).
    /// </summary>
    public string MainCategory { get; private set; } = "none";

    /// <summary>
    /// Die Bezeichnung (ID) der DDC-Kategorie des Objekts (zweite Dezimalstelle).
    /// </summary>
    public string SubCategory { get; private set; } = "none";

    /// <summary>
    /// Variable zur Überprüfung, ob die DDC-Kategorie des Objekts bereits ermittelt wurde.
    /// </summary>
    public bool CategorySetted { get; private set; }

    public string _catID = "none";
    /// <summary>
    /// Die ID der bestimmten DDC-Category.
    /// </summary>
    public string CategoryID
    {
        get
        {
            return _catID;
        }
        set
        {
            _catID = value;
            CategorySetted = true;
            if (_catID.Equals("none")) return;
            string mainCat = _catID[0] + "00";
            //MainCategory = FileCategorizer.ID_CATEGORY_MAP[mainCat];
            //SubCategory = FileCategorizer.ID_CATEGORY_MAP[_catID];
            //CategoryColor = FileCategorizer.CATEGORY_COLORS[int.Parse(_catID[0].ToString())];
            //if (this is VRResourceData)
            //{
            //    if (BelongsToCity.CategoryMap.ContainsKey(MainCategory)) BelongsToCity.CategoryMap[MainCategory].Add(this);
            //    else BelongsToCity.CategoryMap.Add(MainCategory, new List<VRData>() { this });
            //}
        }
    }    

    /// <summary>
    /// Variable zur Überprüfung, ob der Textinhalt des Objekts bereits ermittelt wurde.
    /// </summary>
    public bool TextContentSetted { get; private set; }

    protected string _textContent = "";
    /// <summary>
    /// Der Textinhalt dieses Objekts.
    /// </summary>
    public virtual string TextContent
    {
        get { return _textContent; }
        set
        {
            _textContent = value;
            TextContentSetted = true;
        }
    }

    /// <summary>
    /// Die 3D-Repräsentation des Elements.
    /// </summary>
    public GameObject Object3D;

    /// <summary>
    /// Alle untergeordnete GameObjects.
    /// </summary>
    public List<GameObject> ChildObjects;
    
    /// <summary>
    /// Stores variables for other packages
    /// </summary>
    //public readonly Blackboard Blackboard = new Blackboard();

    //public DataContainer DataContainer;

   /* public virtual void SetupDataContainer(DataContainer container)
    {
        // base method does nothing
    }*/

    public virtual void Setup3DObject()
    {
        // base method does nothing
    }

}
