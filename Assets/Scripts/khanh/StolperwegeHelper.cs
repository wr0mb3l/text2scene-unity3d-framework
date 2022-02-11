using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using UnityEngine.Networking;

public class StolperwegeHelper 
{

    /// <summary>
    /// The private folder of the actual user of the system
    /// </summary>
    public static string UserFolder { get { return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); } }

    /// <summary>
    /// Stores the assigned collective name for each treated file-format
    /// </summary>
    public static Dictionary<string, string> FileExtensionTypeMap;

    /// <summary>
    /// Stores the assigned FontAwesome-Icon-Hex for each treated file-format
    /// </summary>
    public static Dictionary<string, char> FORMAT_ICON_MAP = new Dictionary<string, char>()
    {
        {"file", '\xf15b' }, {"zip", '\xf1c6' }, {"rar", '\xf1c6' }, {"mp3", '\xf1c7' }, {"wav", '\xf1c7' }, {"m4a", '\xf1c7' }, {"wma", '\xf1c7' },
        {"cs", '\xf1c9' }, {"java", '\xf1c9' }, {"xml", '\xf1c9' }, {"xmi", '\xf1c9' }, {"css", '\xf1c9' }, {"html", '\xf1c9' }, {"php", '\xf1c9' },
        { "js", '\xf1c9' }, {"application/bson", '\xf1c9' }, {"xls", '\xf1c3' }, {"xlsx", '\xf1c3' }, {"jpg", '\xf1c5' }, {"jpeg", '\xf1c5' },
        { "png", '\xf1c5' }, {"bmp", '\xf1c5' }, {"gif", '\xf1c5' }, {"pdf", '\xf1c1' }, {"ppt", '\xf1c4' }, {"pptx", '\xf1c5' }, {"mp4", '\xf1c8' },
        { "avi", '\xf1c8' }, {"wmv", '\xf1c8' }, {"mov", '\xf1c8' }, {"ogg", '\xf1c8' }, {"vob", '\xf1c8' }, {"flv", '\xf1c8' }, {"doc", '\xf1c2'},
        { "docx", '\xf1c2'}, {"txt", '\xf15c'}, {"lnk", '\xf0c1'}, { "fnt", '\xf031'}, {"ttf", '\xf031'}, {"url", '\xf57d'}, {"folder", '\xf07b'},
        { "drive", '\xf0a0'}
    };

    public static HashSet<string> SUPPORTED_TEXT_FORMATS = new HashSet<string> { "txt", "docx", "doc", "pdf", "odt", };
    public static HashSet<string> SUPPORTED_PICTURE_FORMATS = new HashSet<string> { "jpg", "jpeg", "png" };
    public static HashSet<string> TEXT_FORMATS = new HashSet<string> { "txt", "docx", "pdf", "odt", "html", "cs", "js", "doc", "xml", "xmi", "application/bson", "java", "css", "php" };
    public static HashSet<string> PICTURE_FORMATS = new HashSet<string> { "jpg", "jpeg", "png", "bmp", "gif" };
    public static HashSet<string> AUDIO_FORMATS = new HashSet<string> { "mp3", "wav", "m4a", "wma" };
    public static HashSet<string> VIDEO_FORMATS = new HashSet<string> { "mov", "wmv", "avi", "mp4", "vob", "flv", "ogg" };

    public static string FOLDER = "Folder";
    public static string TEXT = "Text";
    public static string PICTURE = "Picture";
    public static string AUDIO = "Audio";
    public static string VIDEO = "Video";
    public static string OTHER = "Other";

    public static int ConnectionTimeOut = 20;

    private static Material _fAW;
    public static Material FontAwesomeWhite
    {
        get
        {
            if (_fAW == null)
                _fAW = (Material)UnityEngine.Object.Instantiate(Resources.Load("FileExplorer/Font/FontAwesomeSolid_white"));
            return _fAW;
        }
    }

    private static Material _fAGWO;
    public static Material FontAwesomeGreenWithOutline
    {
        get
        {
            if (_fAGWO == null)
                _fAGWO = (Material)UnityEngine.Object.Instantiate(Resources.Load("FileExplorer/Font/FontAwesomeSolid_green_w_outline"));
            return _fAGWO;
        }
    }

    private static Material _emptyMat;
    public static Material ThumbnailMaterial
    {
        get
        {
            if (_emptyMat == null)
                _emptyMat = (Material)UnityEngine.Object.Instantiate(Resources.Load("Materials/UI/ThumbnailMaterial"));
            return _emptyMat;
        }
    }

    //public static SceneController SceneController;

    //public static VRWriter VRWriter;

    //public static AvatarController User;
    public static GameObject CenterEyeAnchor;
    //public static SimpleGazeCursor Gaze;
    //public static StatusBoxScript StatusBox;

    public static GameObject RightShoulder;
    public static GameObject LeftShoulder;

    public static GameObject RightUpperElbow;
    public static GameObject LeftUpperElbow;

    public static GameObject RightUpperArm;
    public static GameObject LeftUpperArm;

    public static GameObject RightLowerElbow;
    public static GameObject LeftLowerElbow;

    public static GameObject RightHand;
    //public static HandAnimator RightHandAnim;
    public static GameObject LeftHand;
    //public static HandAnimator LeftHandAnim;

    //public static SmartWatchController Smartwatch;

    //public static RadialMenuController RadialMenu;

    //public static PointFinger RightFinger;
    //public static PointFinger LeftFinger;

    //public static DragFinger RightFist;
    //public static DragFinger LeftFist;

    //public static InventoryController Inventory;
    //public static WordRecognizer WordRecognizer;
    public static DictationRecognizer DictationRecognizer;

    public static bool BlockInteractiveObjClick = false;

    //Goethe-Uni Farbsystem http://www.muk.uni-frankfurt.de/52945514/farben
    public class GUCOLOR
    {
        public static Color GOETHEBLAU = new Color(0f / 255f, 97f / 255f, 143f / 255f);
        public static Color LICHTBLAU = new Color(72f / 255f, 169f / 255f, 218f / 255f);
        public static Color HELLGRAU = new Color(248f / 255f, 246f / 255f, 245f / 255f);
        public static Color SANDGRAU = new Color(228f / 255f, 169f / 255f, 218f / 255f);
        public static Color DUNKELGRAU = new Color(77f / 255f, 75f / 255f, 70f / 255f);
        public static Color PURPLE = new Color(134f / 255f, 0f / 255f, 71f / 255f);
        public static Color EMOROT = new Color(179f / 255f, 6f / 255f, 44f / 255f); //B3062C
        public static Color SENFGELB = new Color(227f / 255f, 186f / 255f, 15f / 255f);
        public static Color GRUEN = new Color(115f / 255f, 124f / 255f, 69f / 255f); //737C45
        public static Color MAGENTA = new Color(173f / 255f, 59f / 255f, 118f / 255f);
        public static Color ORANGE = new Color(201f / 255f, 98f / 255f, 21f / 255f);
        public static Color SONNENGELB = new Color(247f / 255f, 217f / 255f, 38f / 255f);
        public static Color HELLESGRUEN = new Color(165f / 255f, 171f / 255f, 82f / 255f);
    }

    public static Color DEFAULT_GRAY = new Color(100f / 255f, 100f / 255f, 100f / 255f);
    public static Color DEFAULT_ORANGE = new Color(1, 0.5f, 0);
    public static Color goetheBlauTrans = new Color(0f / 255f, 97f / 255f, 143f / 255f, 160f / 255f);

    public static GameObject PointerPath;
    public static GameObject PointerSphere;

    public static GameObject pointerLeap;
    public static GameObject pointerLeap2;
    public static GameObject pointerLeap3;
    public static GameObject pointerLeapSphere;


    public static GameObject leftLeapHand;

    public static void InitFileExtensionTypeMap()
    {
        FileExtensionTypeMap = new Dictionary<string, string>();
        foreach (string type in TEXT_FORMATS)
            FileExtensionTypeMap.Add(type, TEXT);
        foreach (string type in PICTURE_FORMATS)
            FileExtensionTypeMap.Add(type, PICTURE);
        foreach (string type in AUDIO_FORMATS)
            FileExtensionTypeMap.Add(type, AUDIO);
        foreach (string type in VIDEO_FORMATS)
            FileExtensionTypeMap.Add(type, VIDEO);
    }

    public static string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

    /*public static float GetDistanceToPlayer(GameObject from)
    {
        return (User.transform.position - from.transform.position).magnitude;
    }

    public static float GetDistanceToPlayer(Vector3 position)
    {
        return (User.transform.position - position).magnitude;
    }

    public static float GetDistance2DToPlayer(GameObject from)
    {
        return (new Vector2(User.transform.position.x, User.transform.position.z) -
                new Vector2(from.transform.position.x, from.transform.position.z)).magnitude;
    }*/

    public static void ChangeBlendMode(Material material, string blendMode)
    {
        switch (blendMode)
        {
            case "Opaque":
                material.SetFloat("_Mode", 0f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case "Fade":
                material.SetFloat("_Mode", 2f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case "Transparent":
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }

    public static GameObject findParentWithTag(Transform t, string tag)
    {
        if(t.parent != null)
        {
            if (t.parent.tag == tag)
            {
                return t.parent.gameObject;
            }
            else
                return findParentWithTag(t.parent, tag);
        }        
        return null; // Could not find a parent with given tag.
    }

    public static GameObject findObjectWithTagInParent(string cTag, Transform parent)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.tag.Equals(cTag)) return child.gameObject;
        }
        return null;
    }

    public static GameObject findObjectInParent(string name, Transform parent)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>();
        foreach(Transform child in children)
        {
            if (child.name.Equals(name)) return child.gameObject;
        }
        return null;
    }

    /// <summary>Converts rgb 0-255 to Unity Color.</summary>
    /// <param name="r">Red (0-255)</param>
    /// <param name="g">Green (0-255)</param>
    /// <param name="b">Blue (0-255)</param>
    public static Color ConvertRGBtoColor(int r, int g, int b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }

    /// <summary>Converts hex String to Unity Color.</summary>
    /// <param name="hex"> String of hexadecimal color code (eg. '#0f0f0f', '0f0f0f')</param>
    public static Color ConvertHexToColor(string hex)
    {
        if (hex.Length > 7 || hex.Length < 6)
        {
            Debug.LogError("Invalid Hex Color String: " + hex);
            return Color.white;
        }
        if (hex.Length == 7) hex = hex.Substring(0, 1);
        return ConvertRGBtoColor(int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber), int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber), int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
    }

    /*public static bool IsLeftHandPointing()
    {
        if (LeftHand == null) return false;
        return LeftHandAnim.IsPointing;
    }

    public static bool IsRightHandPointing()
    {
        if (RightHand == null) return false;
        return RightHandAnim.IsPointing;
    }*/

    /*public static bool IsAnyHandPointing()
    {
        return IsLeftHandPointing() || IsRightHandPointing();
    }

    public delegate void TasksOnBlackscreen();
    public static IEnumerator FadeOutTeleportationTo(Vector3 position, TasksOnBlackscreen DoTasks = null)
    {
        if (User == null || CenterEyeAnchor == null) yield break;
        ///yield return CenterEyeAnchor.GetComponent<OVRScreenFade>().FadeOut();
        DoTasks?.Invoke();
        User.transform.position = position;
        //yield return CenterEyeAnchor.GetComponent<OVRScreenFade>().FadeIn();
        yield break;
    }*/

    public static Material outlineMaterial = Resources.Load("materials/OutlineMaterialBlue", typeof(Material)) as Material;

    public static void outline(Renderer renderer, bool activate)
    {

        if (renderer == null) return;

        Material[] mats = renderer.materials;
        Material[] newMats;

        if (activate)
        {
            newMats = new Material[mats.Length + 1];

            for (int i = 0; i < mats.Length; i++)
                newMats[i] = mats[i];

            newMats[mats.Length] = outlineMaterial;
            renderer.materials = newMats;
            return;
        }

        newMats = new Material[mats.Length - 1];
        int j = 0;
        for (int i = 0; i < mats.Length; i++)
            if (mats[i].shader == outlineMaterial.shader)
                j = 1;
            else
                newMats[i - j] = mats[i];

        renderer.materials = newMats;
    }

    public static Bounds GetBoundsOfChilds(GameObject gameObject)
    {
        Bounds b = new Bounds();

        foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>())
            b.Encapsulate(renderer.bounds);

        return b;
    }

    /*public static void LogMsgOnStatusBox(string msg, bool autoClose)
    {
        Debug.Log(msg);
        StatusBox.StartCoroutine(StatusBox.SetInfoText(msg, autoClose));
    }*/

    public static void PlaceInFrontOfUser(Transform _toPositionate, float _dist, bool _lookAt=true)
    {
        _toPositionate.position = CenterEyeAnchor.transform.position + CenterEyeAnchor.transform.forward * _dist;
        if (_lookAt) _toPositionate.LookAt(CenterEyeAnchor.transform.position);
    }

    public static Vector3 RoundVector3(Vector3 vec, int decPoints)
    {
        vec.x = (int)(vec.x * Mathf.Pow(10, decPoints)) / Mathf.Pow(10, decPoints);
        vec.y = (int)(vec.y * Mathf.Pow(10, decPoints)) / Mathf.Pow(10, decPoints);
        vec.z = (int)(vec.z * Mathf.Pow(10, decPoints)) / Mathf.Pow(10, decPoints);
        return vec;
    }

    public static Vector4 RoundVector4(Vector4 vec, int decPoints)
    {
        vec.x = (int)(vec.x * Mathf.Pow(10, decPoints)) / Mathf.Pow(10, decPoints);
        vec.y = (int)(vec.y * Mathf.Pow(10, decPoints)) / Mathf.Pow(10, decPoints);
        vec.z = (int)(vec.z * Mathf.Pow(10, decPoints)) / Mathf.Pow(10, decPoints);
        vec.w = (int)(vec.w * Mathf.Pow(10, decPoints)) / Mathf.Pow(10, decPoints);
        return vec;
    }

    public static void WriteToFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public static int GetVisibleCharacters(TextMeshPro textMesh, string text)
    {
        // Store original settings
        TextOverflowModes originalOverflowMode = textMesh.overflowMode;
        bool wrapping = textMesh.enableWordWrapping;
        string originalText = textMesh.text;

        // Change settings to determine the visible char-count
        textMesh.overflowMode = TextOverflowModes.Overflow;
        textMesh.enableWordWrapping = true;
        textMesh.text = text;
        textMesh.ForceMeshUpdate();
        int res = textMesh.textInfo.lineInfo[0].characterCount;

        // undone changes
        textMesh.overflowMode = originalOverflowMode;
        textMesh.enableWordWrapping = wrapping;
        textMesh.text = originalText;

        return res;
    }

    public static IEnumerable<Type> GetTypesDerivingFrom<T>()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.BaseType == typeof(T) && !type.IsAbstract);
    }

    public static UnityWebRequest CreateWebRequest(string url, WWWForm form=null)
    {
        UnityWebRequest webRequest;
        if (form == null)
            webRequest = new UnityWebRequest(url);
        else webRequest = UnityWebRequest.Post(url, form);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        return webRequest;
    }

    public static UnityWebRequest CreateTextureWebRequest(string url)
    {
        UnityWebRequest webRequest;
        webRequest = UnityWebRequestTexture.GetTexture(url);
        webRequest.downloadHandler = new DownloadHandlerTexture();
        return webRequest;
    }

    //public static UnityWebRequest CreateFileWebRequest(string url)
    //{
    //    UnityWebRequest webRequest = new UnityWebRequest(url);
    //    webRequest.downloadHandler = new DownloadHandlerBuffer();
    //    return webRequest;
    //}

    public static string ColorToHex(Color color)
    {
        int red = (int)(color.r * 255);
        int green = (int)(color.g * 255);
        int blue = (int)(color.b * 255);
        string redHex = red.ToString("X");
        if (redHex.Length == 1) redHex = "0" + redHex;
        string greenHex = green.ToString("X");
        if (greenHex.Length == 1) greenHex = "0" + greenHex;
        string blueHex = blue.ToString("X");
        if (blueHex.Length == 1) blueHex = "0" + blueHex;
        string hexColor = "#" + redHex + greenHex + blueHex;
        return hexColor;
    }

}
