#if TTP_REWARDED_ADS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tabtale.TTPlugins;

public class TTPRewardedAdsCanvas : MonoBehaviour {

    private TTPRewardedAds.RewardedAdsDelegate _rewardedAdsDelegate;
    private TTPRewardedAds.RewardedAdsDelegate Delegate
    {
        get
        {
            if (_rewardedAdsDelegate == null)
            {
                GameObject ttpGo = GameObject.Find("TTPluginsGameObject");
                if (ttpGo != null)
                {
                    _rewardedAdsDelegate = ttpGo.GetComponent<TTPRewardedAds.RewardedAdsDelegate>();
                }
            }
            return _rewardedAdsDelegate;
        }
    }

    private void OnEnable()
    {
        if(Delegate != null)
        {
            Delegate.OnRewardedAdsShown("");
        }
    }

    public void ClickReward(bool shouldReward)
    {
        if (Delegate != null)
        {
            if (shouldReward)
            {
                Delegate.OnRewardedAdsClosed("{\"shouldReward\":true}");
            }
            else
            {
                Delegate.OnRewardedAdsClosed("{\"shouldReward\":false}");
            }
        }
        gameObject.SetActive(false);
    }
}
#endif