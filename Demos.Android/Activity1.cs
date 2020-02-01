using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MLEM.Input;

namespace Demos.Android {
    [Activity(Label = "Demos.Android"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.FullUser
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity {

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);
            var g = new GameImpl();
            g.OnLoadContent += game => {
                // disable mouse handling for android to make emulator behavior more coherent
                game.InputHandler.HandleMouse = false;
                // enable android text input style
                game.InputHandler.TextInputStyle = new TextInputStyle.Mobile();
            };
            this.SetContentView((View) g.Services.GetService(typeof(View)));
            g.Run();
        }

    }
}