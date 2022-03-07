using UnityEngine;
using UnityEngine.UI;

public class DataContainer : MonoBehaviour
{
    private Image Outline = null;
    private DataBrowser Browser = null;
    private Button button = null;
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
        button = transform.GetComponent<Button>();
    }

    void Update()
    {
        if (Outline == null)
        {
            Outline = transform.Find("Outline").GetComponent<Image>();
        }
        if (Browser == null)
        {
            Browser = GameObject.Find("DataBrowser").GetComponent<DataBrowser>();
        }
    }
}
