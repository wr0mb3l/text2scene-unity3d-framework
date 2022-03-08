using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UMAISOEntity
{
    public static UMAISOEntity ROOT = new UMAISOEntity("Root");
    public static UMAISOEntity HEAD = new UMAISOEntity("Head", ROOT);
    public static UMAISOEntity TORSO = new UMAISOEntity("Torso", ROOT);
    public static UMAISOEntity LEGS = new UMAISOEntity("Legs", ROOT);
    public static UMAISOEntity WARDROBE = new UMAISOEntity("Wardrobe", ROOT);
    public static UMAISOEntity ARMS = new UMAISOEntity("Arms", TORSO);
    public static UMAISOEntity EARS = new UMAISOEntity("Ears", HEAD);
    public static UMAISOEntity FACE = new UMAISOEntity("Face", HEAD);
    public static UMAISOEntity EYES = new UMAISOEntity("Eyes", FACE);
    public static UMAISOEntity NOSE = new UMAISOEntity("Nose", FACE);
    public static UMAISOEntity MOUTH = new UMAISOEntity("Mouth", FACE);

    public static List<UMAISOEntity> AVATARTYPES = new List<UMAISOEntity>
    {
        ROOT, HEAD, TORSO, LEGS, ARMS, EARS, FACE, EYES, NOSE, MOUTH, WARDROBE
    };
    public static List<string> AVATAR_TYPE_NAMES = AVATARTYPES.ConvertAll(new System.Converter<UMAISOEntity, string>(ToString));

    private IsoSpatialEntity _isoSpatialEntity;
    public IsoSpatialEntity Entity {
        get { return _isoSpatialEntity; }
        set
        {
            if (_isoSpatialEntity == null) _isoSpatialEntity = value;
        }
    }
    public UMAISOEntity Parent { get; private set; }
    public List<UMAISOEntity> Children { get; private set; }
    public string Name { get; private set; }
    public Dictionary<UMAProperty, string> Properties { get; private set; } 

    /*private TextAnnotatorInterface TextAnnotator
    {
        get { return SceneController.GetInterface<TextAnnotatorInterface>() ; }
    }*/

    public UMAISOEntity(IsoSpatialEntity entity)
    {
        Entity = entity;
        Name = entity.Object_ID;
        Properties = new Dictionary<UMAProperty, string>();
        if (entity.Object_Feature != null) entity.Object_Feature.ForEach(p => Properties.Add(UMAProperty.Find(p.Key), p.Value));
    }

    public UMAISOEntity(string name, UMAISOEntity parent = null, Dictionary<UMAProperty, string> properties = null)
    {
        Name = name;
        Properties = properties ?? new Dictionary<UMAProperty, string>();
        if (parent != null) parent.AddChild(this);
    }

    public void AddChild(UMAISOEntity child)
    {
        if (Children == null) Children = new List<UMAISOEntity>();
        Children.Add(child);
        child.Parent = this;
    }

    public bool RemoveChild(UMAISOEntity child)
    {
        if (Children == null) return false;
        bool retVal = Children.Remove(child);
        if (retVal) child.Parent = null;
        return retVal;
    }
    
    public int GetDepth()
    {
        return Parent == null ? 0 : Parent.GetDepth() + 1;
    }

    private Dictionary<UMAProperty, string> _childProperties;
    public Dictionary<UMAProperty, string> GetChildProperties()
    {
        _childProperties = new Dictionary<UMAProperty, string>();
        Children.ForEach(c => c.GetChildProperties(_childProperties));
        return _childProperties;
    }

    public void SetProperty(UMAProperty property, string value)
    {
        if (Properties.ContainsKey(property)) Properties[property] = value;
        else Properties.Add(property, value);
        IsoObjectAttribute feature = _isoSpatialEntity.Object_Feature.Find(f => f.Key.Equals(property.Value));
        if (feature == null) Debug.LogError("ERROR: Could not find key" + property.Value + " in spatial entity: " + this.Name);
        else feature.Value = value;
    }

    private Dictionary<UMAProperty, string> GetChildProperties(Dictionary<UMAProperty, string> temp)
    {
        foreach (UMAProperty property in Properties.Keys)
        {
            if(!temp.ContainsKey(property)) temp.Add(property, Properties[property]);
        }
        if (Children?.Count > 0)
        {
            foreach (UMAISOEntity child in Children)
            {
                child.GetChildProperties(temp);
            }
        }
        return temp;
    }

    public static string ToString(UMAISOEntity entity)
    {
        return entity.Name;
    }

    /*public void GetChanges(Dictionary<string, Dictionary<string, object>> changes)
    {
        Entity.Object_Feature.ForEach(f =>
        {
            changes.Add(f.ID + "", TextAnnotator.CreateIsoObjectAttributeMap(f.Key, f.Value));
        });
        if (Children != null) Children.ForEach(c => c.GetChanges(changes));
    }*/
}
