//
//  TTPCrashTool.mm
//  Unity-iPhone
//
//  Created by TabTale on 10/01/2019.
//

#import "TTPUnityServiceManager.h"

    extern "C" {
        
        void ttpCrashToolAddBreadCrumb(const char* crumb){
            TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
            id<TTPIcrashTool> crashTool = [serviceManager get:@protocol(TTPIcrashTool)];
            if (crashTool != nil){
                [crashTool addBreadCrumb:[[NSString alloc] initWithUTF8String:crumb]];
            }
        }
        
        void ttpCrashToolClearAllBreadCrumbs(){
            TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
            id<TTPIcrashTool> crashTool = [serviceManager get:@protocol(TTPIcrashTool)];
            if (crashTool != nil){
                [crashTool clearAllBreadCrumbs];
            }
        }
    }

