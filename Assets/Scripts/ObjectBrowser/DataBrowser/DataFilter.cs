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

    private ShapeNetInterface.CheckboxStatus _status;
    public ShapeNetInterface.CheckboxStatus Status
    {
        get { return _status; }
        set
        {
            _status = value;
            if (_status == ShapeNetInterface.CheckboxStatus.AllChecked) transform.Find("Checkbox").GetComponent<Image>().sprite = FindObjectOfType<DataBrowserFilterController>().Checked;
            if (_status == ShapeNetInterface.CheckboxStatus.NoneChecked) transform.Find("Checkbox").GetComponent<Image>().sprite = FindObjectOfType<DataBrowserFilterController>().Unchecked;
            if (_status == ShapeNetInterface.CheckboxStatus.PartsChecked) transform.Find("Checkbox").GetComponent<Image>().sprite = FindObjectOfType<DataBrowserFilterController>().Mixed;

        }
    }
}
