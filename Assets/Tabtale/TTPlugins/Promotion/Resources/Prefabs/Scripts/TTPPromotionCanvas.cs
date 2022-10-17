#if TTP_PROMOTION
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tabtale.TTPlugins;

public class TTPPromotionCanvas : MonoBehaviour {

    private TTPPromotion.PromotionDelegate _promotionDelegate;
    private TTPPromotion.PromotionDelegate Delegate
    {
        get
        {
            if (_promotionDelegate == null)
            {
                GameObject ttpGo = GameObject.Find("TTPluginsGameObject");
                if (ttpGo != null)
                {
                    _promotionDelegate = ttpGo.GetComponent<TTPPromotion.PromotionDelegate>();
                }
            }
            return _promotionDelegate;
        }
    }

    private void OnEnable()
    {
        Debug.Log("TTPPromotionCanvas::OnEnable");
    }

    public void OnClick()
    {
        Debug.Log("TTPPromotionCanvas::OnClick");
        if (Delegate != null)
        {
            Delegate.OnPromotionClosed("");
        }
        gameObject.SetActive(false);
    }
}
#endif