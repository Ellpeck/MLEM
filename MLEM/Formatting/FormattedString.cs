using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace MLEM.Formatting {
    public class FormattedString {

        private string value;
        public string Value {
            get => this.value;
            set {
                var val = value ?? string.Empty;
                if (this.value != val) {
                    this.value = val;
                    this.isDirty = true;
                }
            }
        }
        private Dictionary<int, FormattingCode> codeLocations;
        private string textWithoutFormatting;
        private bool isDirty = true;

        public FormattedString(string s = null) {
            this.Value = s;
        }

        public static implicit operator string(FormattedString s) {
            return s.Value;
        }

        private void CheckDirtyState() {
            if (this.isDirty) {
                this.isDirty = false;
                this.codeLocations = this.Value.GetFormattingCodes();
                this.textWithoutFormatting = this.Value.RemoveFormatting();
            }
        }

        public void Draw(IGenericFont regularFont, SpriteBatch batch, Vector2 pos, Color color, float scale, IGenericFont boldFont = null, IGenericFont italicFont = null, float depth = 0, TimeSpan timeIntoAnimation = default) {
            this.CheckDirtyState();
            if (this.codeLocations.Count <= 0) {
                regularFont.DrawString(batch, this.textWithoutFormatting, pos, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            } else {
                regularFont.DrawFormattedString(batch, pos, this.textWithoutFormatting, this.codeLocations, color, scale, boldFont, italicFont, depth, timeIntoAnimation);
            }
        }

    }
}