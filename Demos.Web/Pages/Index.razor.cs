using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MLEM.Formatting;
using MLEM.Misc;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace Demos.Web.Pages;

public partial class Index {

    private GameImpl game;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
            await this.JsRuntime.InvokeAsync<object>("initRenderJS", DotNetObjectReference.Create(this));
    }

    [JSInvokable]
    public void TickDotNet() {
        if (this.game == null) {
            MlemPlatform.Current = new MlemPlatform.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);
            this.game = new GameImpl();
            this.game.OnLoadContent += g => g.UiSystem.Add("WebDisclaimer", new Paragraph(Anchor.BottomCenter, 1, "The Web version of MLEM's demos is still in development.\nFor the best experience, please try their Desktop and Android versions instead.") {
                PositionOffset = new Vector2(0, 1),
                Alignment = TextAlignment.Center,
                TextScale = 0.075F
            });
            this.game.Run();
        }
        this.game.Tick();
    }

}
