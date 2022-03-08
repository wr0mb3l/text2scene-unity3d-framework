using System.IO;
using UnityEngine;
using Dummiesman;

public class ObjectLoader : MonoBehaviour
{
    public static GameObject LoadObject(string objpath, string mtlpath)
    {
        if (!File.Exists(objpath))
        {
            Debug.Log(objpath + " obj doesn't exist.");
        }
        else
        {
            if (!File.Exists(mtlpath))
            {
                Debug.Log(mtlpath + " mtl doesn't exist.");
            }

            OBJLoader loader = new OBJLoader();
            GameObject obj = loader.Load(objpath);
            return obj;
        }
        return null;
    }
    
    public static GameObject Reorientate_Obj(GameObject obj, Vector3 up, Vector3 front, float scale)
    {
        obj.transform.localScale = new Vector3(scale, scale, scale);

        //Rotate Obj
        Quaternion rotation_up = Quaternion.FromToRotation(up, Vector3.up);
        obj.transform.rotation = rotation_up;

        Quaternion rotation_front = Quaternion.FromToRotation(front, -obj.transform.forward);
        obj.transform.rotation *= rotation_front;

        //Reposition Obj
        Renderer obj_renderer = obj.GetComponentInChildren<Renderer>();
        Vector3 render_position_med = -obj_renderer.bounds.center;
        obj.transform.localPosition = render_position_med;

        GameObject oriented_obj = new GameObject(obj.name+"_center");
        obj.transform.SetParent(oriented_obj.transform,false);
        return oriented_obj;
    }
}
