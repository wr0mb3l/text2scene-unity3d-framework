using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Threading;

public class ShapeNetTexture : ShapeNetObject
{
    public ShapeNetInterface shapeNetInterface;
    public int XSize { get; private set; }
    public int YSize { get; private set; }
    public Dictionary<string, string> Files { get; private set; }
    public string ThumbnailFileName { get; private set; }
    public Material Material { get; private set; }

    public ShapeNetTexture(JsonData json) : base(json)
    {
        XSize = json.Keys.Contains("xsize") ? (int)json["xsize"] : 256;
        YSize = json.Keys.Contains("ysize") ? (int)json["ysize"] : 256;
        Files = new Dictionary<string, string>();
        if (json.Keys.Contains("files"))
            foreach (string key in json["files"].Keys)
            {
                Files.Add(key, json["files"][key].ToString());
                if (key.Equals("thumbnail")) ThumbnailFileName = Files[key];
            }       
    }

    public delegate void OnMaterialLoaded(Material material); bool _requested;
    public IEnumerator GetMaterial(MonoBehaviour requester, OnMaterialLoaded onMaterialLoaded)
    {
        if (shapeNetInterface == null)
            shapeNetInterface = GameObject.Find("ShapeNetInterface").gameObject.GetComponent<ShapeNetInterface>();

        if (!shapeNetInterface.CachedTexturePathMap.ContainsKey((string)ID) && !_requested)
        {
            _requested = true;
            ShapeNetInterface.RequestTexture((string)ID, null);
        }
            

        while (!shapeNetInterface.CachedTexturePathMap.ContainsKey((string)ID))
            yield return null;

        Material = TextureLoader.LoadMaterialFile(shapeNetInterface.CachedTexturePathMap[(string)ID] + "\\", Files);

        onMaterialLoaded(Material);
    }
}
