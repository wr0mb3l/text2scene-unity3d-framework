using UnityEngine;
using UnityEngine.UI;

public class DataSearchPanel : MonoBehaviour
{
    public string SearchPattern { get { return InputSearch.text; } }

    public InputField InputSearch { get; private set; }
    private Button DeleteButton;
    private Button SearchButton;
    private DataBrowser Browser;

    private Transform _parent;

    public void Init()
    {
        _parent = transform.parent;
        Browser = _parent.GetComponent<DataBrowser>();
        InputSearch = transform.Find("InputField").GetComponent<InputField>();
        InputSearch.onValueChanged.AddListener(delegate { inputChanged(); });

        DeleteButton = transform.Find("ButtonDelete").GetComponent<Button>();
        DeleteButton.onClick.AddListener(Delete);

        SearchButton = transform.Find("ButtonSearch").GetComponent<Button>();
        SearchButton.onClick.AddListener(Search);
    }

    private void inputChanged()
    {
        DeleteButton.interactable = (InputSearch.text != "");
    }

    private void Delete()
    {
        InputSearch.text = "";
        DeleteButton.interactable = false;
        Browser.BrowserUpdater?.Invoke();
    }

    private void Search()
    {
        Browser.BrowserUpdater?.Invoke();
    }
}
