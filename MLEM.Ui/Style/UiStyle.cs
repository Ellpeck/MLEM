using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Ui.Style {
    public class UiStyle : GenericDataHolder {

        public NinePatch SelectionIndicator;
        public NinePatch ButtonTexture;
        public NinePatch ButtonHoveredTexture;
        public Color ButtonHoveredColor;
        public NinePatch ButtonDisabledTexture;
        public Color ButtonDisabledColor;
        public NinePatch PanelTexture;
        public NinePatch TextFieldTexture;
        public NinePatch TextFieldHoveredTexture;
        public Color TextFieldHoveredColor;
        public NinePatch ScrollBarBackground;
        public NinePatch ScrollBarScrollerTexture;
        public NinePatch CheckboxTexture;
        public NinePatch CheckboxHoveredTexture;
        public Color CheckboxHoveredColor;
        public TextureRegion CheckboxCheckmark;
        public NinePatch RadioTexture;
        public NinePatch RadioHoveredTexture;
        public Color RadioHoveredColor;
        public TextureRegion RadioCheckmark;
        public NinePatch TooltipBackground;
        public Vector2 TooltipOffset;
        public NinePatch ProgressBarTexture;
        public Color ProgressBarColor;
        public Vector2 ProgressBarProgressPadding;
        public NinePatch ProgressBarProgressTexture;
        public Color ProgressBarProgressColor;
        public GenericFont Font;
        [Obsolete("Use the new GenericFont.Bold and GenericFont.Italic instead")]
        public GenericFont BoldFont;
        [Obsolete("Use the new GenericFont.Bold and GenericFont.Italic instead")]
        public GenericFont ItalicFont;
        [Obsolete("Use the new text formatting system in MLEM.Formatting instead")]
        public FormatSettings FormatSettings;
        public float TextScale = 1;
        public SoundEffect ActionSound;

    }
}