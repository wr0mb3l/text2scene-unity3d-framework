using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataContainer : MonoBehaviour
{   
    public Text Name { get; private set; }
    public Text DataType { get; private set; }
    public Image Thumbnail { get; private set; }
    public Material thumbnailMat { get; private set; }

    private Data _data;
    public Data Resource
    {
        get { return _data; }
        set
        {
            if (_data != null)
            {
                if (_data.Equals(value)) return;
                _data.SetupDataContainer(null);
            }
            _data = value;
            _data.SetupDataContainer(this);
        }
    }

    public void Awake()
    {
        Name = transform.Find("Name").GetComponent<Text>();
        DataType = transform.Find("Type").GetComponent<Text>();
        Thumbnail = transform.Find("Thumbnail").GetComponent<Image>();
        thumbnailMat = new Material(Shader.Find("UI/Default"));
        Thumbnail.material = thumbnailMat;
    }
}
