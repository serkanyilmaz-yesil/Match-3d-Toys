//
//  TTPUnityGameProgression.m
//  Unity-iPhone
//
//  Created by Tabtale on 15/01/2019.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"

extern "C" {
    
    void ttpPopUpMgrSetLevel(int level)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIpopupMgr> popUpMgr = [serviceManager get:@protocol(TTPIpopupMgr)];
        if (popUpMgr != nil) {
            [popUpMgr setLevel:level];
        }
    }
    
}
