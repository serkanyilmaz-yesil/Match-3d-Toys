//
//  TTPUnityViewController.m
//  Unity-iPhone
//
//  Created by TabTale on 31/10/2018.
//

#import "TTPUnityViewController.h"

extern UIViewController* UnityGetGLViewController();

@implementation TTPUnityViewController

-(UIViewController *) get
{
    return UnityGetGLViewController();
}


@end
