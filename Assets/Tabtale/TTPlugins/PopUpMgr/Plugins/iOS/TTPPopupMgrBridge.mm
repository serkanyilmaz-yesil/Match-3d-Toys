//
//  TTPUnityInterstitials.m
//  Unity-iPhone
//
//  Created by Tabtale on 17/12/2018.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"
#import <TT_Plugins_Core/TTPIpopupMgr.h>//Check
#import <TT_Plugins_Core/TTPsourceType.h>

extern "C" {

    NSString* ttpPopupMgrConvertToNSString(const char* string)
    {
        if(string == NULL)
            return nil;
        else
            return [NSString stringWithUTF8String:string];
    }
    
    // Connect to Game Center
    TTPsourceType ttpPopupMgrConvertToSourceType(const char* string)
    {
        NSString* nsStringSourceType = ttpPopupMgrConvertToNSString(string);

        if ([nsStringSourceType caseInsensitiveCompare:@"rateus"] == NSOrderedSame){
            return TTP_RATEUS_TYPE;
        }
        else if ([nsStringSourceType caseInsensitiveCompare:@"interstitial"] == NSOrderedSame){
            return TTP_INTERSTITIAL;
        }
        else if ([nsStringSourceType caseInsensitiveCompare:@"stand"] == NSOrderedSame){
            return TTP_STAND;
        }
        else if ([nsStringSourceType caseInsensitiveCompare:@"RV"] == NSOrderedSame){
            return TTP_RV;
        }
     
        return TTP_NONE;
    }

    bool ttpPopupMgrShouldShow(const char* source)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIpopupMgr> popupMgr = [serviceManager get:@protocol(TTPIpopupMgr)];
        if(popupMgr != nil){
            return [popupMgr shouldShow:ttpPopupMgrConvertToSourceType(source)];
        }
        
        return true;
    }
   
    void  ttpPopupMgrOnShown(const char* source)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIpopupMgr> popupMgr = [serviceManager get:@protocol(TTPIpopupMgr)];
        if(popupMgr != nil){
            [popupMgr onShow:ttpPopupMgrConvertToSourceType(source)];
        }
    }


    
}
