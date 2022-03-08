using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Security;

public class VRResourceData : VRData {

    // Messages
    public const string UNEXPECTED_ERROR = "Operation wegen einem unerwarteten Fehler felhgeschlagen.";
    public const string NOT_EXISTS_MSG = " existiert nicht länger.";
    public const string UNAUTHORIZED_ACCES_MSG = "Zugriff wurde verweigert. Keine Rechte\nim Ordner, oder an der Datei";
    public const string FILE_IN_USE_MSG = "Datei wird von einem anderen Programm verwendet.";
    public const string FOLDER_NOT_FOUND = " Zielordner wurde nicht gefunden";
    public const string FOLDER_IN_USE_MSG = "Ordner wird von einem anderen Programm verwendet";
    public const string FOLDER_NOT_EMPTY_MSG = "Ordner ist nicht leer";
    public const string ALREADY_EXISTS_MSG = " existiert bereits in ";
    public const string DELETE_SUCCEED_MSG = " wurde erfolgreich gelöscht.";
    public const string COPY_SUCCEED_MSG = " wurde erfolgreich kopiert nach:\n";
    public const string MOVE_SUCCEED_MSG = " wurde erfolgreich verschoben nach:\n";

    public enum DataType { File, Folder }
    public DataType Type { get; protected set; }    
    public string NameWithFormat { get; private set; }
    public string Path;

    public VRResourceData Parent { get; private set; }
    public string ParentName
    {
        get
        {
            if (Parent == null) return "";
            return Parent.Name;
        }
    }
    public string ParentPath
    {
        get
        {
            if (Parent == null) return "";
            return Parent.Path;
        }
    }
    public object ParentID
    {
        get
        {
            if (Parent == null) return "";
            return Parent.ID;
        }
    }
    public string Format { get; private set; }
    public string IconChar { get; private set; }
    public long Size { get; private set; }
    public DateTime LastChange { get; private set; }
    public DateTime CreatedOn { get; private set; }

    private string _url = "";
    public string URL
    {
        get
        {
            if (Format != "url") return "No url.";
            if (_url == "")  _url = File.ReadAllText(Path).Replace("URL=", "*").Split('*')[1];
            return _url;
            
        }
    }

    public bool ImageLoaded { get; private set; }
    public Texture2D ImageTexture { get; private set; }

    //public bool HasTextPreview { get { return (FileCategorizer.SUPPORTED_TEXT_FORMATS.Contains(Format.ToLower()) || Format == "") && Size > 0; } }

    //public bool HasImagePreview { get { return ((FileCategorizer.SUPPORTED_PICTURE_FORMATS.Contains(Format.ToLower()))); } }

    public int FileCount { get; private set; }
    public int FolderGroupCount { get; private set; }
    public int FolderCount
    {
        get
        {
            Debug.Log(Type);
            if (Type != DataType.Folder) return 0;
            else return EmptyFolders.Count + NonEmptyFolders.Count;
        }
    }


    public Dictionary<string, List<List<VRResourceData>>> FileGroups;
    public Dictionary<string, List<VRResourceData>> FileFormatMap;
    public List<VRResourceData> EmptyFolders;
    public List<VRResourceData> NonEmptyFolders;

    public bool IsEmptyFolder
    {
        get
        {
            return Type == DataType.Folder && FileGroups.Count == 0 && EmptyFolders.Count == 0 &&
                   NonEmptyFolders.Count == 0;
        }
    }

    public bool IsShortcutLink { get { return Format != null && Format.Equals("lnk"); } }
    public bool ContainsShortcutLink { get; private set; }

    public bool IsWebLink { get { return Format != null && Format.Equals("url"); } }
    public bool ContainsWebLink { get; private set; }

    public bool IsLink { get { return IsWebLink || IsShortcutLink; } }

    public bool IsDrive {
        get
        {
            if (Source != SourceType.Local) return false;
            return new DirectoryInfo(Path).Parent == null;
        }
    }
    public bool IsParentDrive { get { return new DirectoryInfo(ParentPath).Parent == null; } }

    public float ElapsedTimeSinceLastActualization;

    public bool IsUIMADocument { get { return Format.Equals("application/bson"); } }
    public string TextObjectContent { get; set; }

    //public VRResourceData(string name, string path, VRResourceData parent,  string format, long size, string id, 
    //                      DateTime lastChange, DateTime createdOn, CityScript city, CityScript.SourceType source)
    public VRResourceData(string name, string path, VRResourceData parent, string format, long size, object id,
                          DateTime lastChange, DateTime createdOn, SourceType source)
    {
        Type = DataType.File;
        Name = name;
        Path = path;
        Parent = parent;
        Format = format;
        Size = size;
        ID = id;
        LastChange = lastChange;
        CreatedOn = createdOn;
        NameWithFormat = (Format != "") ? Name + "." + Format : Name;
        CategoryColor = Color.black;        
        Source = source;
        IconChar = (StolperwegeHelper.FORMAT_ICON_MAP.ContainsKey(format)) ? "" + StolperwegeHelper.FORMAT_ICON_MAP[format] : 
                                                                           "" + StolperwegeHelper.FORMAT_ICON_MAP["file"];

        // ============================== resources2City Kram folgt ====================================================
        //if (city != null) BelongsToCity = city;
        //if (IsShortcutLink && BelongsToCity != null) BelongsToCity.LinkMap.Add(this, null);
        //if (BelongsToCity != null && Source != CityScript.SourceType.ResourceManager && HasTextPreview)
        //    StolperwegeHelper.categorizer.EnqueueTextFile(this);
    }

    //public VRResourceData(string name, string path, VRResourceData parent, string id, DateTime createdOn, 
    //                      DateTime lastChange, CityScript city, CityScript.SourceType source = CityScript.SourceType.None)
    public VRResourceData(string name, string path, VRResourceData parent, object id, DateTime createdOn, 
                          DateTime lastChange, SourceType source)
    {
        Type = DataType.Folder;
        Name = name;
        Path = path;
        Parent = parent;
        ID = id;
        FileGroups = new Dictionary<string, List<List<VRResourceData>>>();
        FileFormatMap = new Dictionary<string, List<VRResourceData>>();
        EmptyFolders = new List<VRResourceData>();
        NonEmptyFolders = new List<VRResourceData>();
        ChildObjects = new List<GameObject>();
        LastChange = lastChange;
        CreatedOn = createdOn;
        NameWithFormat = Name;
        Source = source;
        IconChar = (IsDrive) ? "\xf0a0" : "\xf07b";

        // ============================== resources2City Kram folgt ====================================================
        //if (city != null)
        //{
        //    BelongsToCity = city;
        //    Source = BelongsToCity.Source;
        //}
        //else if (source != CityScript.SourceType.None)
        //Source = source;


    }

    public delegate void OnSizeDetermined(long size);
    public IEnumerator GetFolderSize(OnSizeDetermined onSizeDetermined)
    {
        if (Type == DataType.File) yield break;

        long size = 0;
        foreach (VRResourceData nonEmpty in NonEmptyFolders)
        {
            yield return nonEmpty.GetFolderSize((long res) => { size += res; });
        }
        foreach (List<List<VRResourceData>> fileGroups in FileGroups.Values)
        {
            foreach (List<VRResourceData> fileGroup in fileGroups)
            {
                foreach (VRResourceData file in fileGroup)
                {
                    if (file.Size < 0) continue;
                    size += file.Size;
                }
            }
            yield return new WaitForEndOfFrame();
        }

        Size = size;
        onSizeDetermined(size);
    }

    public List<VRResourceData> GetSortedContent(HashSet<string> types, string searchPattern)
    {
        if (Type == DataType.File) return null;
        List<VRResourceData> content = new List<VRResourceData>();
        
        if (types.Contains(StolperwegeHelper.FOLDER))
        {
            List<VRResourceData> folders = new List<VRResourceData>();
            for (int i = 0; i < NonEmptyFolders.Count; i++)
                if (searchPattern == null || searchPattern == "" ||
                    NonEmptyFolders[i].Name.ToLower().Contains(searchPattern))
                    folders.Add(NonEmptyFolders[i]);

            for (int i = 0; i < EmptyFolders.Count; i++)
                if (searchPattern == null || searchPattern == "" ||
                    EmptyFolders[i].Name.ToLower().Contains(searchPattern))
                    folders.Add(EmptyFolders[i]);
            folders.Sort((VRResourceData x, VRResourceData y) => { return x.Name.CompareTo(y.Name); });
            content.AddRange(folders);
        }
        
        
        List<VRResourceData> files = new List<VRResourceData>();
        foreach (string format in FileFormatMap.Keys)
        {
            if ((StolperwegeHelper.FileExtensionTypeMap.ContainsKey(format) && !types.Contains(StolperwegeHelper.FileExtensionTypeMap[format])) ||
                (!StolperwegeHelper.FileExtensionTypeMap.ContainsKey(format) && !types.Contains(StolperwegeHelper.OTHER))) continue;
            for (int i=0; i<FileFormatMap[format].Count; i++)
                if (FileFormatMap[format][i].NameWithFormat.ToLower().Contains(searchPattern))
                    files.Add(FileFormatMap[format][i]);
        }
            
        files.Sort((VRResourceData x, VRResourceData y) => { return x.Name.CompareTo(y.Name); });        
        content.AddRange(files);

        return content;
    }

    private void GetFileSize()
    {
        FileInfo file = new FileInfo(Path);
        Size = file.Length;
        Debug.Log(Size);
    }

    public void AddFileToFormatMap(VRResourceData file, bool groupByFormat)
    {
        if (file.Type != DataType.File) return;
        if (groupByFormat)
        {
            if (FileFormatMap.ContainsKey(file.Format))
                FileFormatMap[file.Format].Add(file);
            else
                FileFormatMap.Add(file.Format, new List<VRResourceData>() { file });
        } else
        {
            if (!file.Format.Equals("lnk"))
            {
                ContainsWebLink = ContainsWebLink || file.Format.Equals("url");
                if (FileFormatMap.ContainsKey("files"))
                    FileFormatMap["files"].Add(file);
                else
                    FileFormatMap.Add("files", new List<VRResourceData>() { file });
            } else
            {
                ContainsShortcutLink = true;
                if (FileFormatMap.ContainsKey("links"))
                    FileFormatMap["links"].Add(file);
                else
                    FileFormatMap.Add("links", new List<VRResourceData>() { file });
            }
            
        }
        
        FileCount += 1;
    }
    

    public string GetFileSizeAsString()
    {
        string res = "Size: ";
        string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        int unit = 0;
        if (Size < 0) return res + "N.N.";
        long size = Size;
        while (size >= 1024)
        {
            size /= 1024;
            unit += 1;
        }
        return "" + res + size + " " + units[unit];
    }

    public static DateTime StringToDateTime(string text, char dateSeparator, char timeSeparator, char secondSeparator)
    {
        string[] dateTimeSplit = text.Split(' ');
        string[] dateSplit = dateTimeSplit[0].Split(dateSeparator);
        string[] timeSplit = dateTimeSplit[1].Split(timeSeparator);
        string[] secondsSplit = timeSplit[2].Split(secondSeparator);
        int year = int.Parse(dateSplit[0]);
        int month = int.Parse(dateSplit[1]);
        int day = int.Parse(dateSplit[2]);
        int hour = int.Parse(timeSplit[0]);
        int minute = int.Parse(timeSplit[1]);
        int second = int.Parse(secondsSplit[0]);
        int millisecond = int.Parse(secondsSplit[1]);
        return new DateTime(year, month, day, hour, minute, second, millisecond);
    }

    public static string DateTimeToString(DateTime dateTime, char dateSeparator, char timeSeparator, char secondSeparator)
    {
        string res = "";
        res += dateTime.Year.ToString() + dateSeparator;
        res += dateTime.Month.ToString() + dateSeparator;
        res += dateTime.Day.ToString() + " ";
        res += dateTime.Hour.ToString() + timeSeparator;
        res += dateTime.Minute.ToString() + timeSeparator;
        res += dateTime.Second.ToString() + secondSeparator;
        res += dateTime.Millisecond.ToString();
        
        return res;
    }

    public override bool Equals(object other)
    {
        if (other == null || !other.GetType().Equals(GetType())) return false;
        return ((VRResourceData)other).Path.Equals(Path);
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    public override string ToString()
    {
        string res = "Name: " + Name + "\nPath: " + ParentPath + "\nDatatype: " + Type;
        if (Type == DataType.File) res += "\nFormat: " + Format + ", " + GetFileSizeAsString();
        return res;
    }

    private bool CopyFile(FileInfo file, string targetPath)
    {
        bool success = false;
        string msg;
        try
        {
            file.CopyTo(targetPath + "\\" + file.Name);
            msg = file.Name + COPY_SUCCEED_MSG + targetPath;
            success = true;
        }
        catch (Exception e)
        {
            if (e is IOException) msg = file.Name + ALREADY_EXISTS_MSG + targetPath;
            else if (e is SecurityException || e is UnauthorizedAccessException) msg = UNAUTHORIZED_ACCES_MSG;
            else if (e is DirectoryNotFoundException) msg = targetPath + "\n" + FOLDER_NOT_FOUND;
            else msg = UNEXPECTED_ERROR;
        }
        //StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(msg, 3, MessageBoxScript.PriorityType.High));
        Debug.Log(msg);
        return success;
    }

    public void CopyDir(DirectoryInfo source, DirectoryInfo target, HashSet<string> copyFailures)
    {

        if (source.FullName.ToLower() == target.FullName.ToLower())
            return;

        // Check if the target directory exists, if not, create it.
        if (!Directory.Exists(target.FullName))
            Directory.CreateDirectory(target.FullName);

        // Copy each file into it's new directory.
        foreach (FileInfo fi in source.GetFiles())
        {
            if (!CopyFile(fi, target.FullName))
                copyFailures.Add(fi.FullName);
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyDir(diSourceSubDir, nextTargetSubDir, copyFailures);
        }
    }

    private bool MoveFile(FileInfo file, string targetPath)
    {
        string msg;
        bool success = false;
        try
        {
            file.MoveTo(targetPath + "\\" + file.Name);

            msg = file.Name + MOVE_SUCCEED_MSG;

            success = true;
        }
        catch (Exception e)
        {

            if (e is IOException)
                msg = file.Name + ALREADY_EXISTS_MSG + "\n" + targetPath;
            else if (e is SecurityException || e is UnauthorizedAccessException)
                msg = UNAUTHORIZED_ACCES_MSG;
            else
                msg = "Unbekannter Fehler beim Verschieben von\n" + file.Name;

        }
        //StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(msg, 3, MessageBoxScript.PriorityType.High));
        Debug.Log(msg);
        return success;
    }

    public bool MoveDir(DirectoryInfo source, string targetPath)
    {
        bool success = false;
        // Check if the target directory exists, if not, create it.
        if (!Directory.Exists(targetPath))
            Directory.CreateDirectory(targetPath);

        try
        {
            source.MoveTo(targetPath + "\\" + source.Name);

            //MessageBoxScript.Message msg = new MessageBoxScript.Message(source.Name + MOVE_SUCCEED_MSG, 3, MessageBoxScript.PriorityType.High);
            //StolperwegeHelper.messageBox.AddMessage(msg);
            Debug.Log(MOVE_SUCCEED_MSG);

            success = true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            if (e is IOException || e is SecurityException)
            {
                //MessageBoxScript.Message msg = new MessageBoxScript.Message(UNAUTHORIZED_ACCES_MSG, 3, MessageBoxScript.PriorityType.High);
                //StolperwegeHelper.messageBox.AddMessage(msg);
                Debug.Log(e.Message);
            }
        }

        return success;
    }

    /*public override void SetupDataContainer(DataContainer container)
    {
        DataContainer = container;
        if (DataContainer == null) return;
        DataContainer.Name.text = Name;
        if (Type == DataType.File)
        {
            DataContainer.DataType.text = "Format: ";
            DataContainer.DataType.text += (Format == "") ? "unknown" : Format;
            DataContainer.DataInfoText = NameWithFormat;
        }
        else
        {
            DataContainer.DataInfoText = Path;
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

        DataContainer.DataPath = Path;
        DataContainer.DataTextIcon.gameObject.SetActive(true);
        DataContainer.DataTextIcon.text = IconChar;
        DataContainer.Thumbnail.enabled = false;
    }*/

    public override void Setup3DObject()
    {
        /*if (Object3D == null)
        {
            Object3D = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/DataBrowser/ResourceObject"));
            DataBrowserResource resource = Object3D.GetComponent<DataBrowserResource>();
            resource.Init(this);            
            resource.ChangeIcon(IconChar);
            resource.InfoTextBox.text = NameWithFormat;
        }*/
    }

    public void Clear()
    {
        EmptyFolders.Clear();
        NonEmptyFolders.Clear();
        FileFormatMap.Clear();
        ElapsedTimeSinceLastActualization = 0;
    }

    //============================================ resources2City-Kram folgt ==================================================

    //public bool IsShortcutTarget { get { return BelongsToCity.LinkMap.ContainsValue(this); } }

    //private Grid _grid;
    //private Grid _startGrid;
    //public Grid Grid
    //{
    //    get
    //    {
    //        return _grid;
    //    }
    //    set
    //    {
    //        _grid = value;
    //        _startGrid = new Grid(_grid.Width, _grid.Height, 0, 0);
    //    }
    //}

    //public void ResetOriginalGridSize()
    //{
    //    _grid = new Grid(_startGrid.Width, _startGrid.Height, _startGrid.XPos, _startGrid.ZPos);
    //}

    //public string GetShortcutTarget()
    //{
    //    IWshRuntimeLibrary.WshShell wsh = new IWshRuntimeLibrary.WshShell();
    //    IWshRuntimeLibrary.IWshShortcut sc = (IWshRuntimeLibrary.IWshShortcut)wsh.CreateShortcut(Path);
    //    return sc.TargetPath;
    //}

    // sorts the files and empty folders in this folder with the choosen sorting-type of the city
    //public void SortAllData()
    //{

    //    // sort empty folders
    //    VRDataComparer comp = (BelongsToCity != null) ? new VRDataComparer(BelongsToCity.FileSorting) : new VRDataComparer(CityScript.Sorting.Alphabetic);
    //    EmptyFolderList.Sort(comp);


    //    // sort all files;
    //    Dictionary<string, List<VRResourceData>> newDict = new Dictionary<string, List<VRResourceData>>();
    //    List<string> formats = new List<string>(FileFormatMap.Keys);

    //    for (int f = 0; f < formats.Count; f++)
    //    {
    //        if (formats[f].Equals("links")) continue;
    //        FileFormatMap[formats[f]].Sort(comp);
    //    }

    //    BuildFileGroups(formats);
    //}

    //private void BuildFileGroups(List<string> formats)
    //{
    //    FolderGroupCount = 0;
    //    FileGroups = new Dictionary<string, List<List<VRResourceData>>>();
    //    List<VRResourceData> files; List<List<VRResourceData>> groups;
    //    VRDataComparer comp = new VRDataComparer(CityScript.Sorting.Size);
    //    bool linksHandled = false;
    //    for (int f = 0; f < formats.Count; f++)
    //    {
    //        if (formats[f].Equals("links")) continue;
    //        files = FileFormatMap[formats[f]];
    //        FileGroups.Add(formats[f], new List<List<VRResourceData>>());
    //        groups = FileGroups[formats[f]];
    //        groups.Add(new List<VRResourceData>());
    //        FolderGroupCount += 1;
    //        int grpLmt = CityScript.RESOURCE_FLOOR_LIMIT;//(ContainsShortcutLink) ? CityScript.FILE_GROUP_LIMIT - 3 : CityScript.FILE_GROUP_LIMIT;
    //        for (int i = 0; i < files.Count; i++)
    //        {
    //            if (groups[groups.Count - 1].Count == grpLmt)
    //            {
    //                groups.Add(new List<VRResourceData>() { files[i] });
    //                FolderGroupCount += 1;
    //                grpLmt = CityScript.RESOURCE_FLOOR_LIMIT;
    //            }
    //            else
    //                groups[groups.Count - 1].Add(files[i]);
    //            if (groups[groups.Count - 1].Count == grpLmt || i == files.Count - 1)
    //            {
    //                groups[groups.Count - 1].Sort(comp);
    //            }
    //        }

    //        if (!StolperwegeHelper.fileExplorer.GroupByFormat && ContainsShortcutLink && !linksHandled)
    //        {
    //            linksHandled = true;
    //            groups[0].InsertRange(0, FileFormatMap["links"]);
    //        }
    //    }

    //    EmptyFolders = new List<List<VRResourceData>>();
    //    foreach (VRResourceData folder in EmptyFolderList)
    //    {
    //        EmptyFolders.Add(new List<VRResourceData>() { folder });
    //        FolderGroupCount += 1;
    //    }
    //}

    //public IEnumerator Delete()
    //{
    //    bool success = false;
    //    if (Source == CityScript.SourceType.ResourceManager)
    //    {
    //        yield return StolperwegeHelper.resourceManagerInterface.DeleteResource(this, (bool res) => { success = res; });
    //    } else
    //    {            
    //        string msg;
    //        if (Type == DataType.File)
    //        {
    //            FileInfo file = new FileInfo(Path);
    //            try
    //            {
    //                file.Delete();
    //                msg = file.Name + DELETE_SUCCEED_MSG;
    //                success = true;
    //            }
    //            catch (Exception e)
    //            {
    //                if (e is UnauthorizedAccessException ||e is SecurityException) msg = UNAUTHORIZED_ACCES_MSG;
    //                else if (e is IOException) msg = FILE_IN_USE_MSG;
    //                else msg = file.Name + NOT_EXISTS_MSG;
    //            }
    //        }
    //        else
    //        {
    //            try
    //            {
    //                DirectoryInfo folder = new DirectoryInfo(Path);
    //                msg = folder.Name + DELETE_SUCCEED_MSG;
    //                folder.Delete(true);
    //                success = true;
    //            }
    //            catch (Exception e)
    //            {
    //                if (e is SecurityException || e is UnauthorizedAccessException) msg = UNAUTHORIZED_ACCES_MSG;
    //                else if (e is IOException) msg = FOLDER_IN_USE_MSG + ".";
    //                else msg = FOLDER_NOT_FOUND;
    //            }
    //        }
    //        StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(msg, 3, MessageBoxScript.PriorityType.High));
    //    }
    //    if (success && BelongsToCity != null)
    //    {
    //        if (BelongsToCity.Layout == CityScript.LayoutType.CustomGrid) VisualDelete();
    //        BelongsToCity.HasChanges = true;
    //    }
    //}

    //public IEnumerator Copy(string targetPath, bool recalculateCity=false)
    //{
    //    HashSet<string> copyFailures = new HashSet<string>();

    //    if (Source == CityScript.SourceType.ResourceManager)
    //    {
    //        bool success = false;
    //        if (StolperwegeHelper.fileExplorer.ActualCityScript.Source == CityScript.SourceType.ResourceManager)
    //            yield return StolperwegeHelper.resourceManagerInterface.CopyResource(this, targetPath, (bool res) => { success = res; });

    //        else
    //            yield return StolperwegeHelper.resourceManagerInterface.DownloadResource(this, targetPath, (bool res) => { success = res; });
    //        if (!success) yield break;
    //    }
    //    else
    //    {

    //        if (StolperwegeHelper.fileExplorer.ActualCityScript.Source == CityScript.SourceType.ResourceManager)
    //        {
    //            bool success = false;
    //            yield return StolperwegeHelper.resourceManagerInterface.UploadResource(this, targetPath, (bool res) => { success = res; });
    //            if (!success) yield break;

    //        } else
    //        {
    //            if (Type == DataType.File)
    //            {
    //                FileInfo file = new FileInfo(Path);
    //                DirectoryInfo targetDir = new DirectoryInfo(targetPath);
    //                if (!file.Exists)
    //                {
    //                    StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(NameWithFormat + NOT_EXISTS_MSG, 3, MessageBoxScript.PriorityType.High));
    //                    yield break;
    //                }
    //                if (!targetDir.Exists)
    //                {
    //                    StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(Name + FOLDER_NOT_FOUND, 3, MessageBoxScript.PriorityType.High));
    //                    yield break;
    //                }
    //                // CODE CHANGED
    //                if (!CopyFile(file, targetPath))
    //                    copyFailures.Add(file.FullName);
    //            }
    //            else
    //            {
    //                DirectoryInfo dir = new DirectoryInfo(Path);
    //                DirectoryInfo targetDir = new DirectoryInfo(targetPath + "\\" + Name);
    //                if (!dir.Exists)
    //                {
    //                    StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(Name + NOT_EXISTS_MSG, 3, MessageBoxScript.PriorityType.High));
    //                    yield break;
    //                }
    //                if (!targetDir.Exists)
    //                    targetDir.Create();

    //                CopyDir(dir, targetDir, copyFailures);
    //            }
    //        }
    //    }

    //    if (BelongsToCity == null) yield break;
    //    if (BelongsToCity.Layout == CityScript.LayoutType.CustomGrid)
    //        yield return VisualCopy(targetPath, recalculateCity, copyFailures, new MonoBehaviour(), true);

    //    BelongsToCity.HasChanges = true;

    //}

    //public IEnumerator Move(MonoBehaviour mb, string targetPath, bool recalculateCity=false)
    //{
    //    HashSet<string> moveFailures = new HashSet<string>();

    //    if (Source == CityScript.SourceType.ResourceManager)
    //    {
    //        bool success = false;
    //        if (StolperwegeHelper.fileExplorer.ActualCityScript.Source == CityScript.SourceType.ResourceManager)
    //        {
    //            yield return StolperwegeHelper.resourceManagerInterface.MoveResource(this, targetPath, (bool res) => { success = res; });
    //            if (!success) yield break;
    //        }
    //        else
    //        {
    //            yield return StolperwegeHelper.resourceManagerInterface.DownloadResource(this, targetPath, (bool res) => { success = res; });
    //            if (!success) yield break;
    //            mb.StartCoroutine(Delete());
    //        }
    //    }
    //    else
    //    {
    //        if (StolperwegeHelper.fileExplorer.ActualCityScript.Source == CityScript.SourceType.ResourceManager)
    //        {
    //            bool success = false;
    //            yield return StolperwegeHelper.resourceManagerInterface.UploadResource(this, targetPath, (bool res) => { success = res; });
    //            if (!success) yield break;
    //            mb.StartCoroutine(Delete());
    //        } else
    //        {
    //            if (Type == DataType.File)
    //            {
    //                FileInfo file = new FileInfo(Path);
    //                DirectoryInfo targetDir = new DirectoryInfo(targetPath);
    //                if (!file.Exists)
    //                {
    //                    string fileName = Name;
    //                    if (Format != "") fileName += "." + Format;
    //                    StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(fileName + NOT_EXISTS_MSG, 3, MessageBoxScript.PriorityType.High));
    //                    yield break;
    //                }
    //                if (!targetDir.Exists)
    //                {
    //                    string folderName = Name;
    //                    StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(folderName + FOLDER_NOT_FOUND, 3, MessageBoxScript.PriorityType.High));
    //                    yield break;
    //                }
    //                // CHANGED CODE
    //                if (!MoveFile(file, targetPath))
    //                    moveFailures.Add(file.FullName);
    //            }
    //            else
    //            {
    //                DirectoryInfo dir = new DirectoryInfo(Path);
    //                DirectoryInfo targetDir = new DirectoryInfo(targetPath);
    //                if (!dir.Exists)
    //                {
    //                    string folderName = Name;
    //                    StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(folderName + NOT_EXISTS_MSG, 3, MessageBoxScript.PriorityType.High));
    //                    yield break;
    //                }
    //                if (!targetDir.Exists)
    //                {
    //                    string folderName = Name;
    //                    StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message(folderName + FOLDER_NOT_FOUND, 3, MessageBoxScript.PriorityType.High));
    //                    yield break;
    //                }

    //                if (!MoveDir(dir, targetPath))
    //                    moveFailures.Add(dir.FullName);
    //            }
    //        }
    //    }

    //    if (BelongsToCity == null) yield break;
    //    if (BelongsToCity.Layout == CityScript.LayoutType.CustomGrid)
    //    {
    //        yield return VisualCopy(targetPath, recalculateCity, moveFailures, new MonoBehaviour());
    //        VisualDelete(moveFailures);
    //    }

    //    BelongsToCity.HasChanges = true;
    //}

    //public IEnumerator VisualCopy(string targetPath, bool recalculateCity, HashSet<string> copyFailures, MonoBehaviour mb,
    //                              bool copy = false, GameObject targetDistrict=null)
    //{

    //    // check first if the city existing or the copy of the whole path eventually failed
    //    HashSet<CityScript> cities = new HashSet<CityScript>(StolperwegeHelper.fileExplorer.PathCityMap.Values);
    //    if (!cities.Contains(BelongsToCity) || copyFailures.Contains(Path)) yield break;


    //    CityScript city = StolperwegeHelper.fileExplorer.ActualCity.GetComponent<CityScript>();
    //    GameObject district = targetDistrict;

    //    char pS = (city.Source == CityScript.SourceType.ResourceManager) ? '/' : '\\';

    //    if (district == null)
    //        yield return city.GetDistrictScriptOfPath(targetPath, (GameObject res) => { district = res; });

    //    VRResourceData districtData = district.GetComponent<DistrictScript>().Folder;

    //    if (Type == DataType.File)
    //    {
    //        string id = ID;
    //        if (copy)
    //        {
    //            city.IncreaseNoOfFiles();
    //            id = city.DataCount.ToString();
    //        }
    //        VRResourceData copiedFile = new VRResourceData(Name, targetPath + "\\" + Name + "." + Format, districtData,
    //                                       Format, Size, city.DataCount.ToString(), DateTime.Now, DateTime.Now, city, city.Source);

    //        districtData.AddFile(copiedFile);

    //        if (recalculateCity)
    //        {
    //            city.AllDistricts.Remove(district);
    //            district.GetComponent<CityElementScript>().Destroy();

    //            GameObject newDistrict = null;
    //            yield return mb.StartCoroutine(city.CreateGridDistrict(districtData, (GameObject res) => { district = res; }));
    //            newDistrict.transform.parent = city.transform;
    //            newDistrict.transform.position = city.transform.position;
    //        }

    //    }
    //    else
    //    {

    //        // create VRData for the copied folder, increase the datacount only if the file is copied
    //        string id = ID;
    //        if (copy)
    //        {
    //            city.IncreaseNoOfFolders();
    //            id = city.DataCount.ToString();
    //        }
    //        VRResourceData copiedFolder = new VRResourceData(Name, targetPath + pS + Name,
    //                                         districtData, id, DateTime.Now, DateTime.Now, city);

    //        // copy each file group
    //        foreach (string format in FileGroups.Keys)
    //        {
    //            List<List<VRResourceData>> fileGroups = FileGroups[format];
    //            List<List<VRResourceData>> copiedGroups = new List<List<VRResourceData>>();
    //            foreach (List<VRResourceData> fileGroup in fileGroups)
    //            {
    //                List<VRResourceData> copiedGroup = new List<VRResourceData>();
    //                foreach (VRResourceData file in fileGroup)
    //                {
    //                    // check if the file couldn't be copied
    //                    if (copyFailures.Contains(file.Path)) continue;

    //                    id = file.ID;
    //                    if (copy)
    //                    {
    //                        city.IncreaseNoOfFiles();
    //                        id = city.DataCount.ToString();
    //                    }
    //                    VRResourceData copiedFile = new VRResourceData(file.Name, copiedFolder.Path + pS + file.Name + "." + file.Format,
    //                                                   copiedFolder, file.Format, file.Size, id, DateTime.Now, DateTime.Now, city, city.Source);
    //                    copiedGroup.Add(copiedFile);
    //                }
    //                copiedGroups.Add(copiedGroup);
    //            }
    //            copiedFolder.FileGroups.Add(format, copiedGroups);
    //        }

    //        // copy the empty folders
    //        List<List<VRResourceData>> copiedEmptyFolders = new List<List<VRResourceData>>();
    //        foreach (List<VRResourceData> emptyFolders in EmptyFolders)
    //        {
    //            VRResourceData emptyFolder = emptyFolders[0];

    //            // check if the file couldn't be copied
    //            if (copyFailures.Contains(emptyFolder.Path)) continue;

    //            id = emptyFolder.ID;
    //            if (copy)
    //            {
    //                city.IncreaseNoOfFolders();
    //                id = city.DataCount.ToString();
    //            }
    //            VRResourceData copiedEmptyFolder = new VRResourceData(emptyFolder.Name, copiedFolder.Path + pS + emptyFolder.Name,
    //                                                  copiedFolder, id, DateTime.Now, DateTime.Now, city);
    //            copiedEmptyFolders.Add(new List<VRResourceData>() { copiedEmptyFolder });
    //        }
    //        copiedFolder.EmptyFolders = copiedEmptyFolders;


    //        // create new District
    //        int depth = copiedFolder.Path.Split(pS).Length - 1;
    //        if (city.DepthMap.ContainsKey(depth)) city.DepthMap[depth].Add(copiedFolder);
    //        else
    //        {
    //            city.DepthMap.Add(depth, new List<VRResourceData>() { copiedFolder });
    //            city.ActualBFSLevel += 1;
    //        }
    //        city.FolderPathMap.Add(copiedFolder.Path, copiedFolder);
    //        GameObject newDistrict = null;
    //        yield return mb.StartCoroutine(city.CreateGridDistrict(copiedFolder, (GameObject res) => { district = res; }));
    //        newDistrict.transform.parent = city.transform;
    //        newDistrict.transform.position = city.transform.position;

    //        // copy recursive all non-empty folders
    //        foreach (VRResourceData data in NonEmptyFolders)
    //            yield return data.VisualCopy(copiedFolder.Path, false, copyFailures, mb, copy, newDistrict);

    //        if (recalculateCity) city.RecalcCityGrid();

    //    }

    //}

    //public void VisualDelete(HashSet<string> deleteFailures=null, bool recalculateCity=false)
    //{
    //    // check first if the city existing
    //    HashSet<CityScript> cities = new HashSet<CityScript>(StolperwegeHelper.fileExplorer.PathCityMap.Values);
    //    if (!cities.Contains(BelongsToCity)) return;

    //    if (Type == DataType.File && !deleteFailures.Contains(Path))
    //        if (Object3D.GetComponent<CityElementScript>() != null)
    //            Object3D.GetComponent<CityElementScript>().DeleteElement();
    //    else
    //    {

    //        int depth = Path.Split('\\').Length - 1;

    //        // remove references
    //        if (deleteFailures.Count == 0)
    //        {
    //            BelongsToCity.DepthMap[depth].Remove(this);
    //                BelongsToCity.FolderPathMap.Remove(Path);
    //        }

    //        foreach (List<VRResourceData> emptyFolders in EmptyFolders)
    //            if (!deleteFailures.Contains(emptyFolders[0].Path))
    //                    BelongsToCity.FolderPathMap.Remove(emptyFolders[0].Path);

    //        foreach (VRResourceData folder in NonEmptyFolders)
    //        {
    //            if (!deleteFailures.Contains(folder.Path))
    //            {
    //                Debug.Log("underfolder removed from depthmap: " + BelongsToCity.DepthMap[depth + 1].Remove(folder));
    //                Debug.Log("underfolder removed from pathmap: " + BelongsToCity.FolderPathMap.Remove(folder.Path));
    //            }
    //        }

    //        //city.PathMap[ParentPath].Grid.ReduceSurface(Grid);
    //        if (deleteFailures.Count == 0)
    //            Object3D.GetComponent<CityElementScript>().DeleteElement();
    //    }
    //}

    //private bool _loading = false;
    //public IEnumerator LoadImageTexture()
    //{
    //    if (_loading) yield break;

    //    _loading = true;

    //    while (!StolperwegeHelper.cache.CacheLoaded) yield return null;

    //    string pathToUse = StolperwegeHelper.cache.CachedImages.ContainsKey(Path) ? StolperwegeHelper.cache.CachedImages[Path].FullCachedPath : Path;

    //    WWW www = new WWW("file://" + pathToUse);
    //    yield return www;
    //    ImageTexture = www.texture;

    //    if (pathToUse.Equals(Path))
    //    {
    //        if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\" + CacheManager.IMAGE_CACHE_FOLDER))
    //            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + CacheManager.IMAGE_CACHE_FOLDER);

    //        float aspectRatio = (float)ImageTexture.width / ImageTexture.height;

    //        if (ImageTexture.height > 512)
    //            TextureScaler.scale(ImageTexture, (int)(aspectRatio * 512), 512);

    //        string matPath = CacheManager.IMAGE_CACHE_FOLDER + 
    //                         Path.Replace('\\', '_').Replace('.', '_').Replace(":", "") + ".png"; ;

    //        FileStream stream = File.Create(matPath);
    //        byte[] bytes = ImageTexture.EncodeToPNG();
    //        stream.Write(bytes, 0, bytes.Length);
    //        stream.Close();
    //        UnityEngine.Object.Destroy(ImageTexture);

    //        ImageFileInfo info = new ImageFileInfo(matPath, LastChange, aspectRatio);
    //        StolperwegeHelper.cache.CachedImages.Add(Path, info);

    //        www = new WWW("file://" + matPath);
    //        yield return www;
    //        ImageTexture = www.texture;

    //        while (StolperwegeHelper.cache.IsSavingImages)
    //            yield return null;

    //        StolperwegeHelper.cache.CacheImages();

    //    }
    //    ImageLoaded = true;
    //}

    //public VRResourceData GetLinkedFile()
    //{

    //    if (!IsShortcutLink || BelongsToCity == null) return null;


    //    if (!BelongsToCity.LinkMap.ContainsKey(this))
    //    {Debug.Log("Verlinkte Datei konnte nicht gefunden werden.");
    //        StolperwegeHelper.messageBox.AddMessage(new MessageBoxScript.Message("Verlinkte Datei konnte nicht gefunden werden."));
    //        return null;
    //    }


    //    return BelongsToCity.LinkMap[this];
    //}

    //private void OnDestroy()
    //{
    //    if (CachingThread != null && CachingThread.IsAlive)
    //        CachingThread.Abort();
    //}

    //public void Clear()
    //{
    //    EmptyFolderList.Clear();
    //    NonEmptyFolders.Clear();
    //    FileFormatMap.Clear();
    //    ElapsedTimeSinceLastActualization = 0;
    //}

}
