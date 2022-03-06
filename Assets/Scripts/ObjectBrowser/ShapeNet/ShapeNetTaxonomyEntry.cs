using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class ShapeNetTaxonomyEntry
{
    public enum TaxonomyType { Object, Texture }

    public TaxonomyType Type;
    public string Name;
    public HashSet<string> Objects;
    public HashSet<string> SubCategories;

    public ShapeNetTaxonomyEntry(string name, TaxonomyType type, JsonData data, ShapeNetInterface shapeNetInterface = null)
    {
        Name = name;
        Type = type;
        Objects = new HashSet<string>();
        SubCategories = new HashSet<string>();
        if (data.Keys.Contains("objects"))
            for (int i = 0; i < data["objects"].Count; i++)
                Objects.Add(data["objects"][i].ToString());

        if (shapeNetInterface == null)
            shapeNetInterface = GameObject.Find("ShapeNetInterface").gameObject.GetComponent<ShapeNetInterface>();

        if (data.Keys.Contains("subcategories"))
        {
            JsonData objectList = data["subcategories"];
            for (int i = 0; i < objectList.Count; i++)
            {
                foreach (string _name in objectList[i].Keys)
                {
                    if ((objectList[i][_name].Keys.Contains("objects") && objectList[i][_name]["objects"].Count > 0))
                    {
                        if (Type == TaxonomyType.Object)
                        {
                            if (!shapeNetInterface.ModelSubCategories.ContainsKey(_name))
                            {
                                shapeNetInterface.ModelSubCategories.Add(_name, ShapeNetInterface.CheckboxStatus.AllChecked);
                                shapeNetInterface.ObjectSubCategoryMap.Add(_name, Name);
                                SubCategories.Add(_name);
                            }
                        }
                        else
                        {
                            if (!shapeNetInterface.TextureSubCategories.ContainsKey(_name))
                            {
                                shapeNetInterface.TextureSubCategories.Add(_name, ShapeNetInterface.CheckboxStatus.AllChecked);
                                shapeNetInterface.TextureSubCategoryMap.Add(_name, Name);
                                SubCategories.Add(_name);
                            }
                        }
                    }
                }
            }
        }
    }
}
