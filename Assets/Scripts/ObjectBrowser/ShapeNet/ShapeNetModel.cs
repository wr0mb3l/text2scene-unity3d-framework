using LitJson;
using System.Collections.Generic;
using UnityEngine;

public class ShapeNetModel : ShapeNetObject
{
    public string SynSet { get; private set; }

    public HashSet<string> Lemmas { get; private set; }
    public HashSet<string> Tags { get; private set; }
    public float SolidVolume { get; private set; }
    public bool IsContainer { get; private set; }
    public float SurfaceVolume { get; private set; }
    public float Unit { get; private set; }
    public float SupportSurfaceArea { get; private set; }
    public Vector3 AlignedDimensions { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Front { get; private set; }

    public ShapeNetModel(JsonData json) : base(json)
    {
        SynSet = json.Keys.Contains("wnsynset") ? json["wnsynset"].ToString() : "";
        SolidVolume = json.Keys.Contains("solidVolume") ? NumericHelper.ParseFloat(json["solidVolume"].ToString()) : 0;
        IsContainer = json.Keys.Contains("isContainer") ? bool.Parse(json["isContainer"].ToString()) : false;
        SurfaceVolume = json.Keys.Contains("surfaceVolume") ? NumericHelper.ParseFloat(json["surfaceVolume"].ToString()) : 0;
        Unit = json.Keys.Contains("unit") ? NumericHelper.ParseFloat(json["unit"].ToString()) : 0;
        SupportSurfaceArea = json.Keys.Contains("supportSurfaceArea") ? NumericHelper.ParseFloat(json["supportSurfaceArea"].ToString()) : 0;
        AlignedDimensions = json.Keys.Contains("alignedDims") ? ParseVector(json["alignedDims"].ToString()) : Vector3.one;
        Up = json.Keys.Contains("up") ? ParseVector(json["up"].ToString()) : Vector3.up;
        Front = json.Keys.Contains("front") ? ParseVector(json["front"].ToString()) : Vector3.forward;

        if (Up == Front) //Need to be fixed in the ShapeNet Service ....
        {
            Up = new Vector3(0f, 0, 1f);
            Front = new Vector3(0f, -1f, 0f);
        }

        Tags = new HashSet<string>();
        if (json.Keys.Contains("tags"))
        {
            for (int i = 0; i < json["tags"].Count; i++)
                Tags.Add(json["tags"][i].ToString().ToLower());

        }

        Lemmas = new HashSet<string>();
        if (json.Keys.Contains("wnlemmas"))
        {
            for (int i = 0; i < json["wnlemmas"].Count; i++)
                Lemmas.Add(json["wnlemmas"][i].ToString().ToLower());
        }
    }

    private static Vector3 ParseVector(string input)
    {
        string[] vectorSplit = input.Replace("[", "").Replace("]", "").Replace(" ", "").Split(',');
        return new Vector3(NumericHelper.ParseFloat(vectorSplit[0]), NumericHelper.ParseFloat(vectorSplit[1]), NumericHelper.ParseFloat(vectorSplit[2]));
    }
}