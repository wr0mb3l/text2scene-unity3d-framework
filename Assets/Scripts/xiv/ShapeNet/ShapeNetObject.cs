using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class ShapeNetObject : Data
{
    public HashSet<string> Categories { get; private set; }
    public string PrettyCategories { get; private set; }

    // TODO call the LoadThumbnail if thumbnail is null
    public Texture2D Thumbnail;

    public ShapeNetObject(JsonData json)
    {
        ID = json["id"].ToString();
        Name = json.Keys.Contains("name") ? json["name"].ToString() : "no_name";
        Categories = new HashSet<string>();
        if (json.Keys.Contains("categories"))
        {
            PrettyCategories = "";
            for (int i = 0; i < json["categories"].Count; i++)
            {
                Categories.Add(json["categories"][i].ToString());
                PrettyCategories += json["categories"][i].ToString();
                if (i < json["categories"].Count - 1) PrettyCategories += ", ";
            }

        }
    }

    public override void SetupDataContainer(DataContainer container)
    {
        DataContainer = container;
        if (DataContainer == null) return;
        DataContainer.Name.text = Name;
        DataContainer.DataType.text = PrettyCategories;
        // DataContainer.DataTextIcon.gameObject.SetActive(false);
        DataContainer.Thumbnail.enabled = true;
        if (Thumbnail != null)
        {
            DataContainer.Thumbnail.enabled = false;
            DataContainer.Thumbnail.material.SetTexture("_MainTex", Thumbnail);
            DataContainer.Thumbnail.enabled = true;
        }
        else
            DataContainer.StartCoroutine(ShapeNetInterface.LoadThumbnail(this, () =>
            {
                if (DataContainer == null) return;
                DataContainer.Thumbnail.enabled = false;
                DataContainer.Thumbnail.material.SetTexture("_MainTex", Thumbnail);
                DataContainer.Thumbnail.enabled = true;
            }));
    }
}