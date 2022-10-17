using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TTPRateUsCanvas : MonoBehaviour {


    public RectTransform mainPanelRectTransform;

    private System.Action _goToStoreAction;
    private System.Action _neverAction;
    private System.Action _laterAction;
    private System.Action _closeAction;


    private void Start()
    {
        Debug.Log("TTPRateUsCanvas::Start: screen orientation is " + Screen.orientation);
        if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            mainPanelRectTransform.anchorMin = new Vector2(0.15f, 0.3f);
            mainPanelRectTransform.anchorMax = new Vector2(0.85f, 0.7f);
        }
        else
        {
            mainPanelRectTransform.anchorMin = new Vector2(0.35f, 0.2f);
            mainPanelRectTransform.anchorMax = new Vector2(0.65f, 0.8f);
        }
        mainPanelRectTransform.offsetMin = new Vector2(0, 0);
        mainPanelRectTransform.offsetMax = new Vector2(0, 0);
    }

    public void Init(System.Action goToStoreAction,
        System.Action neverAction,
        System.Action laterAction,
        System.Action closeAction,
        string iconExtension)
    {
        _goToStoreAction = goToStoreAction;
        _neverAction = neverAction;
        _laterAction = laterAction;
        _closeAction = closeAction;

        foreach (Image iconImage in GetComponentsInChildren<Image>())
        {
            if (iconImage.name == "GameIconImage")
            {
                byte[] imageData = Tabtale.TTPlugins.TTPUtils.ReadDataFromStreamingAssets("ttp/rateus/game_icon."+iconExtension);
                if (imageData != null)
                {
                    Texture2D texture = new Texture2D(1, 1);
                    texture.LoadImage(imageData);
                    Vector2 pivot = new Vector2(0.5f, 0.5f);
                    Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), pivot, 100.0f);
                    iconImage.sprite = sprite;
                }
                break;
            }
        }

        foreach(Text text in GetComponentsInChildren<Text>())
        {
            if(text.name == "TitleText")
            {
                text.text = "Enjoying " + Application.productName + "?";
            }
        }
    }

	public void OnClickGoToStore()
    {
        Debug.Log("TTPRateUsCanvas::OnClickGoToStore");
        _goToStoreAction.Invoke();
        OnClickClose();
    }

    public void OnClickNever()
    {
        Debug.Log("TTPRateUsCanvas::OnClickNever");
        _neverAction.Invoke();
        OnClickClose();
    }

    public void OnClickLater()
    {
        Debug.Log("TTPRateUsCanvas::OnClickLater");

        _laterAction.Invoke();
        OnClickClose();
    }

    public void OnClickClose()
    {
        Debug.Log("TTPRateUsCanvas::OnClickClose");
        _closeAction.Invoke();
        Destroy(this.gameObject);
    }

}
