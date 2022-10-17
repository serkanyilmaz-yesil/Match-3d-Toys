//
//  TTPUnityViewController.h
//  Unity-iPhone
//
//  Created by TabTale on 31/10/2018.
//

#import "UnityAppController.h"
#import <Foundation/Foundation.h>
#import <TT_Plugins_Core/TTPIappLifeCycleMgr.h>

NS_ASSUME_NONNULL_BEGIN

@interface TTPUnityAppController : UnityAppController

@property (nonatomic, strong) id<TTPIappLifeCycleMgr> appLifeCycleMgr;

@end

NS_ASSUME_NONNULL_END
