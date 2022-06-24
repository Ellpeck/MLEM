using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MLEM.Formatting.Codes;

namespace MLEM.Misc {
    /// <summary>
    /// MlemPlatform is a wrapper around some of MonoGame's platform-dependent behavior to allow for MLEM to stay platform-independent.
    /// See <see cref="DesktopGl{T}"/>, <see cref="Mobile"/> and <see cref="None"/> for information on the specific platforms.
    /// The MLEM demos' main classes also make use of this functionality: https://github.com/Ellpeck/MLEM/blob/main/Demos.DesktopGL/Program.cs#L8 and https://github.com/Ellpeck/MLEM/blob/main/Demos.Android/Activity1.cs#L33.
    /// </summary>
    public abstract class MlemPlatform {

        /// <summary>
        /// The current MLEM platform
        /// Set this value before starting your game if you want to use platform-dependent MLEM features.
        /// </summary>
        public static MlemPlatform Current;

        /// <summary>
        /// Opens the on-screen keyboard if one is required by the platform.
        /// Note that, if no on-screen keyboard is required, a null string should be returned.
        /// </summary>
        /// <param name="title">Title of the dialog box.</param>
        /// <param name="description">Description of the dialog box.</param>
        /// <param name="defaultText">Default text displayed in the input area.</param>
        /// <param name="usePasswordMode">If password mode is enabled, the characters entered are not displayed.</param>
        /// <returns>Text entered by the player. Null if back was used.</returns>
        public abstract Task<string> OpenOnScreenKeyboard(string title, string description, string defaultText, bool usePasswordMode);

        /// <summary>
        /// Adds a text input listener to this platform, if supported.
        /// The supplied listener will be called whenever a character is input.
        /// </summary>
        /// <param name="window">The game's window</param>
        /// <param name="callback">The callback that should be called whenever a character is pressed</param>
        public abstract void AddTextInputListener(GameWindow window, TextInputCallback callback);

        /// <summary>
        /// A method that should be executed to open a link in the browser or a file explorer.
        /// This method is currently used only by MLEM.Ui's implementation of the <see cref="LinkCode"/> formatting code.
        /// </summary>
        public abstract void OpenLinkOrFile(string link);

        /// <summary>
        /// Ensures that <see cref="Current"/> is set to a valid <see cref="MlemPlatform"/> value by throwing an <see cref="InvalidOperationException"/> exception if <see cref="Current"/> is null.
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="Current"/> is null</exception>
        public static void EnsureExists() {
            if (MlemPlatform.Current == null)
                throw new InvalidOperationException("MlemPlatform was not initialized. For more information, see the MlemPlatform class or https://mlem.ellpeck.de/api/MLEM.Misc.MlemPlatform");
        }

        /// <summary>
        /// A delegate method that can be used for <see cref="MlemPlatform.AddTextInputListener"/>
        /// </summary>
        /// <param name="sender">The object that sent the event. The <see cref="GameWindow"/> or <see cref="MlemPlatform"/> used in most cases.</param>
        /// <param name="key">The key that was pressed. Note that this is always <see cref="Keys.None"/> on FNA.</param>
        /// <param name="character">The character that corresponds to that key.</param>
        public delegate void TextInputCallback(object sender, Keys key, char character);

        /// <summary>
        /// The MLEM DesktopGL platform.
        /// This platform uses the built-in MonoGame TextInput event, which makes this listener work with any keyboard localization natively.
        /// This platform is initialized as follows:
        /// <code>
        /// MlemPlatform.Current = new MlemPlatform.DesktopGl&lt;TextInputEventArgs&gt;((w, c) => w.TextInput += c);
        /// </code>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class DesktopGl<T> : MlemPlatform {

            private FieldInfo key;
            private FieldInfo character;
            private readonly Action<GameWindow, EventHandler<T>> addListener;

            /// <summary>
            /// Creates a new DesktopGL-based platform.
            /// See <see cref="MlemPlatform.DesktopGl{T}"/> class documentation for more detailed information.
            /// </summary>
            /// <param name="addListener">The function that is used to add a text input listener.</param>
            public DesktopGl(Action<GameWindow, EventHandler<T>> addListener) {
                this.addListener = addListener;
            }

            /// <inheritdoc />
            public override Task<string> OpenOnScreenKeyboard(string title, string description, string defaultText, bool usePasswordMode) {
                return Task.FromResult<string>(null);
            }

            /// <inheritdoc />
            public override void AddTextInputListener(GameWindow window, TextInputCallback callback) {
                this.addListener(window, (sender, args) => {
                    if (this.key == null)
                        this.key = args.GetType().GetField("Key");
                    if (this.character == null)
                        this.character = args.GetType().GetField("Character");
                    callback.Invoke(sender, (Keys) this.key.GetValue(args), (char) this.character.GetValue(args));
                });
            }

            /// <inheritdoc />
            public override void OpenLinkOrFile(string link) {
                Process.Start(new ProcessStartInfo(link) {UseShellExecute = true});
            }

        }

        /// <summary>
        /// The MLEM Desktop platform for FNA.
        /// This platform uses the built-in FNA TextInputEXT event, which makes this listener work with any keyboard localization natively.
        /// This platform is initialized as follows:
        /// <code>
        /// MlemPlatform.Current = new MlemPlatform.DesktopFna(a => TextInputEXT.TextInput += a);
        /// </code>
        /// </summary>
        public class DesktopFna : MlemPlatform {

            private readonly Action<Action<char>> addListener;

            /// <summary>
            /// Creates a new Desktop for FNA platform.
            /// See <see cref="MlemPlatform.DesktopFna"/> class documentation for more detailed information.
            /// </summary>
            /// <param name="addListener">The function that is used to add a text input listener.</param>
            public DesktopFna(Action<Action<char>> addListener) {
                this.addListener = addListener;
            }

            /// <inheritdoc />
            public override Task<string> OpenOnScreenKeyboard(string title, string description, string defaultText, bool usePasswordMode) {
                return Task.FromResult<string>(null);
            }

            /// <inheritdoc />
            public override void AddTextInputListener(GameWindow window, TextInputCallback callback) {
                this.addListener(c => callback(this, Keys.None, c));
            }

            /// <inheritdoc />
            public override void OpenLinkOrFile(string link) {
                Process.Start(new ProcessStartInfo(link) {UseShellExecute = true});
            }

        }

        /// <summary>
        /// The MLEM platform for mobile platforms as well as consoles.
        /// This platform opens an on-screen keyboard using the <see cref="Microsoft.Xna.Framework.Input"/> <c>KeyboardInput</c>  class on mobile devices.
        /// Additionally, it starts a new activity whenever <see cref="OpenLinkOrFile"/> is called.
        /// This listener is initialized as follows in the game's <c>Activity</c> class:
        /// <code>
        /// MlemPlatform.Current = new MlemPlatform.Mobile(KeyboardInput.Show, l =&gt; this.StartActivity(new Intent(Intent.ActionView, Uri.Parse(l))));
        /// </code>
        /// </summary>
        public class Mobile : MlemPlatform {

            private readonly OpenOnScreenKeyboardDelegate openOnScreenKeyboard;
            private readonly Action<string> openLink;

            /// <summary>
            /// Creates a new mobile- and console-based platform.
            /// See <see cref="MlemPlatform.Mobile"/> class documentation for more detailed information.
            /// </summary>
            /// <param name="openOnScreenKeyboard">The function that is used to display the on-screen keyboard</param>
            /// <param name="openLink">The action that is invoked to open a link in the browser, which is used for <see cref="LinkCode"/></param>
            public Mobile(OpenOnScreenKeyboardDelegate openOnScreenKeyboard, Action<string> openLink = null) {
                this.openOnScreenKeyboard = openOnScreenKeyboard;
                this.openLink = openLink;
            }

            /// <inheritdoc />
            public override Task<string> OpenOnScreenKeyboard(string title, string description, string defaultText, bool usePasswordMode) {
                return this.openOnScreenKeyboard(title, description, defaultText, usePasswordMode);
            }

            /// <inheritdoc />
            public override void AddTextInputListener(GameWindow window, TextInputCallback callback) {}

            /// <inheritdoc />
            public override void OpenLinkOrFile(string link) {
                this.openLink?.Invoke(link);
            }

            /// <summary>
            /// A delegate method used for <see cref="Mobile.OpenOnScreenKeyboard"/>
            /// </summary>
            /// <param name="title">Title of the dialog box.</param>
            /// <param name="description">Description of the dialog box.</param>
            /// <param name="defaultText">Default text displayed in the input area.</param>
            /// <param name="usePasswordMode">If password mode is enabled, the characters entered are not displayed.</param>
            /// <returns>Text entered by the player. Null if back was used.</returns>
            public delegate Task<string> OpenOnScreenKeyboardDelegate(string title, string description, string defaultText, bool usePasswordMode);

        }

        /// <summary>
        /// A MLEM platform implementation that does nothing.
        /// This can be used if no platform-dependent code is required for the game.
        /// </summary>
        public class None : MlemPlatform {

            /// <inheritdoc />
            public override Task<string> OpenOnScreenKeyboard(string title, string description, string defaultText, bool usePasswordMode) {
                return Task.FromResult<string>(null);
            }

            /// <inheritdoc />
            public override void AddTextInputListener(GameWindow window, TextInputCallback callback) {}

            /// <inheritdoc />
            public override void OpenLinkOrFile(string link) {}

        }

    }
}
