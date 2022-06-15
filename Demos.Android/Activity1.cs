using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MLEM.Extensions;
using MLEM.Misc;

namespace Demos.Android {
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.UserLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class Activity1 : AndroidGameActivity {

        private GameImpl game;
        private View view;

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);
            // render under notches
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                this.Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;

            MlemPlatform.Current = new MlemPlatform.Mobile(KeyboardInput.Show, l => this.StartActivity(new Intent(Intent.ActionView, Uri.Parse(l))));
            this.game = new GameImpl();
            // reset MlemGame width and height to use device's aspect ratio
            this.game.GraphicsDeviceManager.ResetWidthAndHeight(this.game.Window);
            // disable mouse handling for android to make emulator behavior more coherent
            this.game.OnLoadContent += game => game.InputHandler.HandleMouse = false;
            // set the game to fullscreen to cause the status bar to be hidden
            this.game.GraphicsDeviceManager.IsFullScreen = true;
            this.view = this.game.Services.GetService(typeof(View)) as View;
            this.SetContentView(this.view);
            this.game.Run();
        }

        public override void OnWindowFocusChanged(bool hasFocus) {
            base.OnWindowFocusChanged(hasFocus);
            // hide the status bar
            if (hasFocus)
                this.Window.DecorView.SystemUiVisibility = (StatusBarVisibility) (SystemUiFlags.ImmersiveSticky | SystemUiFlags.LayoutStable | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutFullscreen | SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen);
        }

    }
}