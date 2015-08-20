using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace EduChess
{
    [Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
    {
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }

        UIWindow window;
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            window.RootViewController = new HomeController();
            window.MakeKeyAndVisible();
			
            return true;
        }
    }
}

