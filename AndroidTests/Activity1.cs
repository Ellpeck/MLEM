using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace AndroidTests {
    [Activity(Label = "AndroidTests"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.UserLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity {

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);
            var g = new GameImpl();
            this.SetContentView((View) g.Services.GetService(typeof(View)));
            g.Run();
        }

    }
}