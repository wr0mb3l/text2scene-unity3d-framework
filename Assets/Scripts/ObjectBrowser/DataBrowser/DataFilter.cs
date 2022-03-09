using UnityEngine;
using UnityEngine.UI;

public class DataFilter : MonoBehaviour
{
    private string _buttonValue;
    public string ButtonValue
    {
        get { return _buttonValue; }
        set
        {
            _buttonValue = value;
            transform.Find("Tag").GetComponent<Text>().text = value;
        }
    }

    private DataBrowser.CheckboxStatus _status;
    public DataBrowser.CheckboxStatus Status
    {
        get { return _status; }
        set
        {
            _status = value;
            if (_status == DataBrowser.CheckboxStatus.AllChecked) transform.Find("Checkbox").GetComponent<Image>().sprite = FindObjectOfType<DataBrowserFilterController>().Checked;
            if (_status == DataBrowser.CheckboxStatus.NoneChecked) transform.Find("Checkbox").GetComponent<Image>().sprite = FindObjectOfType<DataBrowserFilterController>().Unchecked;
            if (_status == DataBrowser.CheckboxStatus.PartsChecked) transform.Find("Checkbox").GetComponent<Image>().sprite = FindObjectOfType<DataBrowserFilterController>().Mixed;
        }
    }
}
