using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// //TODO: Das muss auf jeden Fall noch irgendwie geändert werden ....
/// Eigendlich nur ein Dictionary. Evtl. entsprechend ändern
/// 
/// </summary>
public class IsoObjectAttribute : AnnotationBase
{   
    public string Key { get; private set; }
    public string Value;

    public IsoObjectAttribute(int id, int begin, int end, string key, string value, AnnotationDocument doc) : 
          base(id, begin, end, AnnotationTypes.OBJECT_ATTRIBUTE, doc)
    {
        ID = id;
        Key = key;
        Value = value;
    }

    public void SetKey(string key)
    {
        Key = key;
    }

    public void SetValue(string value)
    {
        Value = value;
    }

    public override void Actualize3DObject()
    {
        
    }
}

/// <summary>
/// Eigendlich unity Vextor3
/// </summary>
public class IsoVector3 : AnnotationBase
{   

    public Vector3 Vector { get; private set; }
    //public double X { get; private set; }
    //public double Y { get; private set; }
    //public double Z { get; private set; }

    public IsoVector3(int id, float x, float y, float z, AnnotationDocument doc) : base(id, 0, 0, AnnotationTypes.VEC3, doc)
    {
        Vector = new Vector3(x, y, z);
    }

    public void SetVector(Vector3 vec3) 
    { 
        this.Vector = vec3; 
    }

    public override void Actualize3DObject()
    {
        //throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        return "x: " + Vector.x + " y: " + Vector.y + " z: " + Vector.z;
    }

}

/// <summary>
/// Eigendlich unity Vextor4
/// </summary>
public class IsoVector4 : AnnotationBase
{
    public Quaternion Quaternion { get; private set; }

    public IsoVector4(int id, float x, float y, float z, float w, AnnotationDocument doc) : base(id, 0, 0, AnnotationTypes.VEC4, doc)
    {
        Quaternion = new Quaternion(x, y, z, w);
    }

    public void SetQuaternion(Quaternion rotation)
    {
        this.Quaternion = rotation;
    }

    public override void Actualize3DObject()
    {
        //throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        return "x: " + Quaternion.x + " y: " + Quaternion.y + " z: " + Quaternion.z + " w: " + Quaternion.w;
    }
}