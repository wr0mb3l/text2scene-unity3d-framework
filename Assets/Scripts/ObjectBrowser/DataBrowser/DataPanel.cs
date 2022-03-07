using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataPanel : MonoBehaviour
{
    public Text Title { get; private set; }
    private Text SiteIndicator;
    public List<Data> Datas { get; private set; }
    public DataContainer[] DataContainers { get; private set; }
    private int MaxSites;
    private Button NextSite;
    private Button PreviousSite;
    public Button ParentDir { get; private set; }
    public Button Root { get; private set; }
    public DataBrowser Browser { get; private set; }

    private int _containerPointer;
    public int ContainerPointer
    {
        get { return _containerPointer; }
        set
        {
            SetComponentStatus(Datas != null);
            if (Datas == null) return;
            _containerPointer = Mathf.Max(0, Mathf.Min(value, (Datas.Count / DataContainers.Length) * DataContainers.Length));
            ActualizeDataContainers();
            ActualizeSiteVariables();
        }
    }

    private bool _baseInit;
    private void BaseInit()
    {
        Browser = transform.parent.GetComponent<DataBrowser>();
        Title = transform.Find("Title").GetComponent<Text>();
        SiteIndicator = transform.Find("SiteIndicator").GetComponent<Text>();
        DataContainers = GetComponentsInChildren<DataContainer>();
        PreviousSite = transform.Find("ButtonPrevious").GetComponent<Button>();
        PreviousSite.onClick.AddListener(PreviousClick);
        NextSite = transform.Find("ButtonNext").GetComponent<Button>();
        NextSite.onClick.AddListener(NextClick);
        ParentDir = transform.Find("ButtonParent").GetComponent<Button>();
        Root = transform.Find("ButtonRoot").GetComponent<Button>();

        _baseInit = true;
    }

    public void Init(string title = null, IEnumerable<Data> datas = null)
    {
        if (!_baseInit) BaseInit();
        Datas = (datas == null) ? null : new List<Data>(datas);
        if (title == null)
            Title.text = "Nothing to show, please choose a data space.";
        else
            Title.text = title;

        if (Datas != null)
            MaxSites = Mathf.CeilToInt(Datas.Count / (float)DataContainers.Length);

        ContainerPointer = 0;
    }

    public void SetComponentStatus(bool status)
    {
        SiteIndicator.gameObject.SetActive(status);
        PreviousSite.gameObject.SetActive(status);
        NextSite.gameObject.SetActive(status);
        ParentDir.gameObject.SetActive(status);
        foreach (DataContainer dc in DataContainers)
            dc.gameObject.SetActive(status);
    }

    private void ActualizeSiteVariables()
    {
        SiteIndicator.text = "Site " + ((ContainerPointer / DataContainers.Length) + 1) + " of " + Mathf.Max(1, MaxSites);
        PreviousSite.interactable = ContainerPointer > 0;
        NextSite.interactable = (ContainerPointer + DataContainers.Length) < Datas.Count;
    }

    private void ActualizeDataContainers()
    {
        for (int i = 0; i < DataContainers.Length; i++)
        {
            DataContainers[i].gameObject.SetActive((_containerPointer + i) < Datas.Count);
            if (DataContainers[i].gameObject.activeInHierarchy)
            {
                DataContainers[i].Resource = Datas[i + _containerPointer];
                if ((string)DataContainers[i].Resource.ID != "")
                {
                    DataContainers[i].GetComponent<Button>().onClick.RemoveAllListeners();
                    if (i == 0)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(0); });
                    else if (i == 1)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(1); });
                    else if (i == 2)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(2); });
                    else if (i == 3)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(3); });
                    else if (i == 4)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(4); });
                    else if (i == 5)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(5); });
                    else if (i == 6)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(6); });
                }
            }
        }
    }

    private void DataContainerClicked(int i)
    {
        LoadObject((string)DataContainers[i].Resource.ID);
    }
    private void NextClick()
    {
        ContainerPointer += DataContainers.Length;
    }
    private void PreviousClick()
    {
        ContainerPointer -= DataContainers.Length;
    }

    public void LoadObject(string ID)
    {
        Debug.Log("Load Object: " + ID);
        ShapeNetInterface inter = GameObject.Find("ShapeNetInterface").gameObject.GetComponent<ShapeNetInterface>();

        ShapeNetModel shapeObj = inter.ShapeNetModels[ID];

        StartCoroutine(inter.GetModel((string)shapeObj.ID, (path) =>
        {
            Debug.Log("Scale & Reorientate Obj");
            GameObject GameObject = ObjectLoader.LoadObject(path + "\\" + shapeObj.ID + ".obj", path + "\\" + shapeObj.ID + ".mtl");
            GameObject GhostObject = ObjectLoader.Reorientate_Obj(GameObject, shapeObj.Up, shapeObj.Front, shapeObj.Unit);
            GhostObject.transform.GetChild(0).GetChild(0).gameObject.AddComponent<MeshCollider>().convex = true;
            GhostObject.AddComponent<Rigidbody>();
            GhostObject.AddComponent<XREditInteractable>();
            GhostObject.transform.position = this.transform.position;
        }));
    }
}
