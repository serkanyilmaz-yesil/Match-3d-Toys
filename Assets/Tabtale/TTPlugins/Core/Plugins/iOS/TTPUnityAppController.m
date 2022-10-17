//
//  TTPUnityAppController.m
//  Unity-iPhone
//
//  Created by TabTale on 29/10/2018.
//

#import "TTPUnityAppController.h"
#import "TTPUnityViewController.h"
#import "TTPUnityServiceManager.h"

IMPL_APP_CONTROLLER_SUBCLASS(TTPUnityAppController)

@interface TTPUnityAppController()

@property Class psdkAppControllerCls;
@property (nonatomic, strong) id psdkAppController;

@end

@implementation TTPUnityAppController

- (void)shouldAttachRenderDelegate {
    
    Class cls = NSClassFromString(@"TTPUnityAppControllerExpansion");
    if (cls != nil){
        
        NSObject* obj = [cls alloc];
        if ([obj respondsToSelector:@selector(shouldAttachRenderDelegate)]){
            self.renderDelegate = [obj performSelector:@selector(shouldAttachRenderDelegate)];
        }
    }
}

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    
    [[TTPUnityServiceManager sharedInstance] didFinishLaunchingWithOptions:launchOptions application:application];
    
    
    BOOL success = [super application:application didFinishLaunchingWithOptions:launchOptions];
    [self callPsdkAppController:@"application:didFinishLaunchingWithOptions:" application:application launchOptions:launchOptions];
    
    Class cls = NSClassFromString(@"TTPUnityAppControllerExpansion");
    if (cls != nil){
        
        SEL selector = NSSelectorFromString(@"application:didFinishLaunchingWithOptions:");
        NSMethodSignature *signature = [cls instanceMethodSignatureForSelector:selector];
        
        if (signature != nil){
            
            NSObject* obj = [cls alloc];
            
            NSInvocation* inv = [NSInvocation invocationWithMethodSignature:signature];
            [inv setSelector:selector];
            [inv setTarget:obj];
            
            [inv setArgument:&application atIndex:2];
            [inv setArgument:&launchOptions atIndex:3];
            
            [inv invoke];
            [inv getReturnValue:&success];
        }
    }
    
    return success;
}

- (void)applicationWillResignActive:(UIApplication *)application {
    NSLog(@"TTPUnityAppController:applicationWillResignActive: ");
    [super applicationWillResignActive:application];
    if([[TTPUnityServiceManager sharedInstance] appLifeCycleMgr] != nil){
        [[[TTPUnityServiceManager sharedInstance] appLifeCycleMgr]  applicationWillResignActive];
    }
    
    [self callPsdkAppController:@"applicationWillResignActive:" application:application  launchOptions:nil];
    [self callByReflection:@"applicationWillResignActive:" application:application];
}

- (void)applicationDidEnterBackground:(UIApplication *)application {
    NSLog(@"TTPUnityAppController:applicationDidEnterBackground: ");
    [super applicationDidEnterBackground:application];
    if([[TTPUnityServiceManager sharedInstance] appLifeCycleMgr] != nil){
        [[[TTPUnityServiceManager sharedInstance] appLifeCycleMgr]  onPaused];
    }
    
    [self callPsdkAppController:@"applicationDidEnterBackground:" application:application  launchOptions:nil];
    [self callByReflection:@"applicationDidEnterBackground:" application:application];
}

- (void)applicationWillEnterForeground:(UIApplication *)application {
    NSLog(@"TTPUnityAppController:applicationWillEnterForeground: ");
    [super applicationWillEnterForeground:application];
    
    [self callPsdkAppController:@"applicationWillEnterForeground:" application:application  launchOptions:nil];
    [self callByReflection:@"applicationWillEnterForeground:" application:application];
}

- (void)applicationDidBecomeActive:(UIApplication *)application {
    NSLog(@"TTPUnityAppController:applicationDidBecomeActive: ");
    [super applicationDidBecomeActive:application];
    if([[TTPUnityServiceManager sharedInstance] appLifeCycleMgr]  != nil){
        [[[TTPUnityServiceManager sharedInstance] appLifeCycleMgr]  onResume];
    }
    
    [self callPsdkAppController:@"applicationDidBecomeActive:" application:application  launchOptions:nil];
    [self callByReflection:@"applicationDidBecomeActive:" application:application];
}

- (void)applicationWillTerminate:(UIApplication *)application {
    NSLog(@"TTPUnityAppController:applicationWillTerminate: ");
    [super applicationWillTerminate:application];
    
    [self callPsdkAppController:@"applicationWillTerminate:" application:application launchOptions:nil];
    [self callByReflection:@"applicationWillTerminate:" application:application];
}

- (id) psdkAppController
{
    if(_psdkAppController == nil){
        _psdkAppControllerCls = NSClassFromString(@"TTUPsdkUnityAppController");
        _psdkAppController = [[_psdkAppControllerCls alloc] init];
    }
    return _psdkAppController;
}

-(void) callPsdkAppController:(NSString*)method application:(UIApplication *)application launchOptions:(NSDictionary *)launchOptions
{
    if([self psdkAppController] != nil){
        SEL selector = NSSelectorFromString(method);
        NSMethodSignature *signature = [_psdkAppControllerCls instanceMethodSignatureForSelector:selector];
        if (signature == nil){
            NSLog(@"TTPUnityAppController::callPsdkAppController: method : %@ does not exist", method);
            return;
        }
        NSInvocation* inv = [NSInvocation invocationWithMethodSignature:signature];;
        [inv setSelector:selector];
        [inv setTarget:_psdkAppController];
        [inv setArgument:&application atIndex:2];
        if(launchOptions != nil){
            [inv setArgument:&launchOptions atIndex:3];
        }
        [inv invoke];
    }
}

-(void)callByReflection:(NSString*)method application:(UIApplication *)application
{
    Class cls = NSClassFromString(@"TTPUnityAppControllerExpansion");
    if (cls != nil){
        
        SEL selector = NSSelectorFromString(method);
        NSMethodSignature *signature = [cls instanceMethodSignatureForSelector:selector];
        
        if (signature != nil){
            
            NSObject* obj = [cls alloc];
            
            NSInvocation* inv = [NSInvocation invocationWithMethodSignature:signature];
            [inv setSelector:selector];
            [inv setTarget:obj];
            
            [inv setArgument:&application atIndex:2];
            [inv invoke];
        }
    }
}

@end
