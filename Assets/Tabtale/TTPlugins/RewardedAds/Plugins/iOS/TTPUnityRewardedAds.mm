//
//  TTPUnityRewardedAds.m
//  Unity-iPhone
//
//  Created by Tabtale on 18/12/2018.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"
#import <TT_Plugins_Core/TTPIrewardedads.h>

extern "C" {
    
    bool ttpRewardedAdsShow(const char * location)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIrewardedAds> rewardedAdsService = [serviceManager get:@protocol(TTPIrewardedAds)];
        if(rewardedAdsService != nil){
            return [rewardedAdsService show:[[NSString alloc] initWithUTF8String:location]];
        }
        return false;
    }
    
    bool ttpRewardedAdsIsReady()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIrewardedAds> rewardedAdsService = [serviceManager get:@protocol(TTPIrewardedAds)];
        if(rewardedAdsService != nil){
            return [rewardedAdsService isReady];
        }
        return false;
    }
    
}
