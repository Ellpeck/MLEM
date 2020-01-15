using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Textures;

namespace MLEM.Ui.Style {
    public class UntexturedStyle : UiStyle {

        public UntexturedStyle(SpriteBatch batch) {
            this.SelectionIndicator = batch.GenerateTexture(Color.Transparent, Color.Red);
            this.ButtonTexture = batch.GenerateTexture(Color.CadetBlue);
            this.ButtonHoveredColor = Color.LightGray;
            this.ButtonDisabledColor = Color.Gray;
            this.PanelTexture = batch.GenerateTexture(Color.Gray);
            this.TextFieldTexture = batch.GenerateTexture(Color.MediumBlue);
            this.TextFieldHoveredColor = Color.LightGray;
            this.ScrollBarBackground = batch.GenerateTexture(Color.LightBlue);
            this.ScrollBarScrollerTexture = batch.GenerateTexture(Color.Blue);
            this.CheckboxTexture = batch.GenerateTexture(Color.LightBlue);
            this.CheckboxHoveredColor = Color.LightGray;
            this.CheckboxCheckmark = batch.GenerateTexture(Color.Blue).Region;
            this.RadioTexture = batch.GenerateTexture(Color.AliceBlue);
            this.RadioHoveredColor = Color.LightGray;
            this.RadioCheckmark = batch.GenerateTexture(Color.CornflowerBlue).Region;
            this.TooltipBackground = batch.GenerateTexture(Color.Black * 0.65F, Color.Black * 0.65F);
            this.TooltipOffset = new Vector2(2, 3);
            this.ProgressBarTexture = batch.GenerateTexture(Color.RoyalBlue);
            this.ProgressBarColor = Color.White;
            this.ProgressBarProgressPadding = new Vector2(1);
            this.ProgressBarProgressColor = Color.Red;
            this.Font = new EmptyFont();
        }

        private class EmptyFont : IGenericFont {

            public float LineHeight => 1;

            public Vector2 MeasureString(string text) {
                return Vector2.One;
            }

            public Vector2 MeasureString(StringBuilder text) {
                return Vector2.One;
            }

            public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color) {
            }

            public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            }

            public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            }

            public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color) {
            }

            public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            }

            public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            }

            public void DrawCenteredString(SpriteBatch batch, string text, Vector2 position, float scale, Color color, bool horizontal = true, bool vertical = false, float addedScale = 0) {
            }

            public string SplitString(string text, float width, float scale) {
                return text;
            }

            public string TruncateString(string text, float width, float scale, bool fromBack = false) {
                return text;
            }

        }

    }
}