using UnityEngine;
using UnityEngine.UI;

public class DataSpaceContainer : MonoBehaviour
{   
    public Text Tag { get; private set; }
    public object ButtonValue;
    public bool ButtonOn;

    public void Awake()
    {
        Tag = transform.Find("Tag").GetComponent<Text>();
    }

    public void ChangeText(string text)
    {
        Tag.text = text;
    }
}
