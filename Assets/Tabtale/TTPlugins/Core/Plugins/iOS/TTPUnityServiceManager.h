//
//  TTPUnityServiceManager.h
//  Unity-iPhone
//
//  Created by Tabtale on 21/11/2018.
//

#import <Foundation/Foundation.h>
#import <TT_Plugins_Core/TTPluginsCore.h>
#import <TT_Plugins_Core/TTPIunityMessenger.h>

NS_ASSUME_NONNULL_BEGIN

@interface TTPUnityServiceManager : NSObject <TTPIunityMessenger>

+ (TTPServiceManager*)sharedInstance;


@end

NS_ASSUME_NONNULL_END
