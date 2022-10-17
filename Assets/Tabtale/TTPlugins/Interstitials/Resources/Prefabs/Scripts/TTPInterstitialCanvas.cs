#if TTP_INTERSTITIALS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tabtale.TTPlugins;

public class TTPInterstitialCanvas : MonoBehaviour {

    private TTPInterstitials.InterstitialsDelegate _interstitialsDelegate;
    private TTPInterstitials.InterstitialsDelegate Delegate
    {
        get
        {
            if (_interstitialsDelegate == null)
            {
                GameObject ttpGo = GameObject.Find("TTPluginsGameObject");
                if (ttpGo != null)
                {
                    _interstitialsDelegate = ttpGo.GetComponent<TTPInterstitials.InterstitialsDelegate>();
                }
            }
            return _interstitialsDelegate;
        }
    }

    private void OnEnable()
    {
        if(Delegate != null)
        {
            Delegate.OnInterstitialShown("");
        }
    }

    public void OnClick()
    {
       if(Delegate != null)
        {
            Delegate.OnInterstitialClosed("");
        }
        gameObject.SetActive(false);
    }
}
#endif