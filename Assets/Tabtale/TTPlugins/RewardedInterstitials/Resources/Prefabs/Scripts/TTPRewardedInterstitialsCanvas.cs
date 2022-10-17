#if TTP_REWARDED_INTERSTITIALS && TTP_CORE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tabtale.TTPlugins;

public class TTPRewardedInterstitialsCanvas : MonoBehaviour {

    private TTPRewardedInterstitials.RewardedInterstitialsDelegate _rewardedInterDelegate;
    private TTPRewardedInterstitials.RewardedInterstitialsDelegate Delegate
    {
        get
        {
            if (_rewardedInterDelegate == null)
            {
                GameObject ttpGo = GameObject.Find("TTPluginsGameObject");
                if (ttpGo != null)
                {
                    _rewardedInterDelegate = ttpGo.GetComponent<TTPRewardedInterstitials.RewardedInterstitialsDelegate>();
                }
            }
            return _rewardedInterDelegate;
        }
    }

    private void OnEnable()
    {
        if(Delegate != null)
        {
            Delegate.OnRewardedInterstitialsShown("");
        }
    }

    public void ClickReward(bool shouldReward)
    {
        if (Delegate != null)
        {
            if (shouldReward)
            {
                Delegate.OnRewardedInterstitialsClosed("{\"shouldReward\":true}");
            }
            else
            {
                Delegate.OnRewardedInterstitialsClosed("{\"shouldReward\":false}");
            }
        }
        gameObject.SetActive(false);
    }
}
#endif