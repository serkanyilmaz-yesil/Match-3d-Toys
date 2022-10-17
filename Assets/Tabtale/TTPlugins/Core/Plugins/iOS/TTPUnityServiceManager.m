//
//  TTPUnityServiceManager.m
//  Unity-iPhone
//
//  Created by Tabtale on 21/11/2018.
//

#import "TTPUnityServiceManager.h"

extern void UnitySendMessage(const char *, const char *, const char *);

static TTPServiceManager *instance;

@implementation TTPUnityServiceManager

+ (TTPServiceManager*)sharedInstance
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        instance = [[TTPServiceManager alloc] init];
    });
    return instance;
}

- (void)unitySendMessage:(const char *)method message:(const char *)message
{
    UnitySendMessage("TTPluginsGameObject", method, message);
}

@end
