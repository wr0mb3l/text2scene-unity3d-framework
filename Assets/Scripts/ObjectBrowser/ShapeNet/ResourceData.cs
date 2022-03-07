using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ResourceData : Data
{
    public enum DataType { File, Folder }
    public DataType Type { get; protected set; }
    public string NameWithFormat { get; private set; }
    public string Path;

    public ResourceData Parent { get; private set; }

    public string Format { get; private set; }
    public DateTime LastChange { get; private set; }
    public DateTime CreatedOn { get; private set; }

    public Dictionary<string, List<List<ResourceData>>> FileGroups;
    public Dictionary<string, List<ResourceData>> FileFormatMap;
    public List<ResourceData> EmptyFolders;
    public List<ResourceData> NonEmptyFolders;

    public bool IsDrive
    {
        get
        {
            if (Source != SourceType.Local) return false;
            return new DirectoryInfo(Path).Parent == null;
        }
    }

    public ResourceData(string name, string path, ResourceData parent, object id, DateTime createdOn,
                          DateTime lastChange, SourceType source)
    {
        Type = DataType.Folder;
        Name = name;
        Path = path;
        Parent = parent;
        ID = id;
        FileGroups = new Dictionary<string, List<List<ResourceData>>>();
        FileFormatMap = new Dictionary<string, List<ResourceData>>();
        EmptyFolders = new List<ResourceData>();
        NonEmptyFolders = new List<ResourceData>();
        ChildObjects = new List<GameObject>();
        LastChange = lastChange;
        CreatedOn = createdOn;
        NameWithFormat = Name;
        Source = source;
    }

    public override void SetupDataContainer(DataContainer container)
    {
        DataContainer = container;
        if (DataContainer == null) return;
        DataContainer.Name.text = Name;
        if (Type == DataType.File)
        {
            DataContainer.DataType.text = "Format: ";
            DataContainer.DataType.text += (Format == "") ? "unknown" : Format;
        }
        else
        {
            if (IsDrive)
                DataContainer.DataType.text = (new DriveInfo(Path)).DriveType.ToString();
            else
            {
                if (Source == SourceType.Local)
                    DataContainer.DataType.text = "Folder, Childs: " + (new DirectoryInfo(Path)).GetFileSystemInfos().Length;
                else
                    DataContainer.DataType.text = "Repository";
            }
        }
        DataContainer.Thumbnail.enabled = false;
    }
}