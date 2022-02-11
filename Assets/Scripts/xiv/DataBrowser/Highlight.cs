using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Button", 30)]
    public class Highlight : Button
    {
        private Image Outline = null;
        private DataBrowser Browser = null;

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
            //Check if the GameObject is being highlighted
            if (IsHighlighted() == true)
            {
                Outline.material = Browser.GoetheOn;
            }
            else
            {
                Outline.material = Browser.GoetheOff;
            }
        }
    }
}