using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTPRateUsNotConnectedCanvas : MonoBehaviour {

    public RectTransform mainPanelRectTransform;

    // Use this for initialization
    void Start () {
        Debug.Log("TTPRateUsCanvas::Start: screen orientation is " + Screen.orientation);
        if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            mainPanelRectTransform.anchorMin = new Vector2(0.1f, 0.3f);
            mainPanelRectTransform.anchorMax = new Vector2(0.9f, 0.7f);
        }
        else
        {
            mainPanelRectTransform.anchorMin = new Vector2(0.2f, 0.2f);
            mainPanelRectTransform.anchorMax = new Vector2(0.8f, 0.8f);
        }
        mainPanelRectTransform.offsetMin = new Vector2(0, 0);
        mainPanelRectTransform.offsetMax = new Vector2(0, 0);
    }

    public void OnClickClose()
    {
        Destroy(this.gameObject);
    }
}
