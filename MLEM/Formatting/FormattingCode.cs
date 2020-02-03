using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Animations;
using MLEM.Font;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Formatting {
    public class FormattingCode {

        public readonly Type CodeType;
        public readonly Color Color;
        public readonly TextStyle Style;
        public readonly SpriteAnimation Icon;
        public readonly int StartIndex;

        protected FormattingCode(int startIndex, Type type) {
            this.CodeType = type;
            this.StartIndex = startIndex;
        }

        public FormattingCode(int startIndex, Color color) : this(startIndex, Type.Color) {
            this.Color = color;
        }

        public FormattingCode(int startIndex, TextStyle style) : this(startIndex, Type.Style) {
            this.Style = style;
        }

        public FormattingCode(int startIndex, TextureRegion icon) :
            this(startIndex, new SpriteAnimation(0, icon)) {
        }

        public FormattingCode(int startIndex, SpriteAnimation icon) : this(startIndex, Type.Icon) {
            this.Icon = icon;
        }

        public virtual string GetReplacementString(IGenericFont font) {
            return this.CodeType == Type.Icon ? TextFormatting.GetOneEmString(font) : string.Empty;
        }

        public virtual void Update(GameTime time) {
            if (this.CodeType == Type.Icon)
                this.Icon.Update(time);
        }

        public virtual void Reset() {
            if (this.CodeType == Type.Icon)
                this.Icon.Restart();
        }

        public virtual void ModifyFormatting(ref FormatState state) {
            switch (this.CodeType) {
                case Type.Color:
                    state.Color = this.Color;
                    break;
                case Type.Style:
                    switch (this.Style) {
                        case TextStyle.Regular:
                            state.DrawBehavior = FormatState.DefaultDrawBehavior;
                            state.Font = state.RegularFont;
                            break;
                        case TextStyle.Bold:
                            state.Font = state.BoldFont;
                            break;
                        case TextStyle.Italic:
                            state.Font = state.ItalicFont;
                            break;
                        case TextStyle.Shadow:
                            var formatState = state;
                            state.PrependBehavior((s, batch, totalText, index, charSt, position, scale, layerDepth) => {
                                var lastColor = s.Color;
                                s.Color = s.Settings.DropShadowColor;
                                // Call the last behavior to draw the shadow
                                formatState.DrawBehavior(s, batch, totalText, index, charSt, position + s.Settings.DropShadowOffset * scale, scale, layerDepth);
                                s.Color = lastColor;
                            });
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case Type.Icon:
                    state.PrependBehavior((s, batch, totalText, index, charSt, position, scale, layerDepth) => {
                        if (index == this.StartIndex)
                            batch.Draw(this.Icon.CurrentRegion, new RectangleF(position, new Vector2(s.RegularFont.LineHeight * scale)), s.Color, 0, Vector2.Zero, SpriteEffects.None, layerDepth);
                    });
                    break;
            }
        }

        public enum Type {

            Color,
            Style,
            Icon,
            Animation

        }

    }

    public enum TextStyle {

        Regular,
        Bold,
        Italic,
        Shadow

    }
}