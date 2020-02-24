using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MLEM.Misc;

namespace Demos.Android {
    [Activity(Label = "Demos.Android"
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
            TextInputWrapper.Current = new TextInputWrapper.Mobile();
            var g = new GameImpl();
            // disable mouse handling for android to make emulator behavior more coherent
            g.OnLoadContent += game => game.InputHandler.HandleMouse = false;
            this.SetContentView((View) g.Services.GetService(typeof(View)));
            g.Run();
        }

    }
}