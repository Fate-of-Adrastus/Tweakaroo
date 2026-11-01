using SDL2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tweakaroo;

internal class StringInputScreen : IScreen {
    readonly Bounds2 bounds;
    string value;
    Action<string> onConfirm;

    float cursorBlinkTimer;
    static float cursorBlinkPeriod = 1.2f;
    public StringInputScreen(Bounds2 bounds, string value, Action<string> onConfirm) {
        this.bounds = bounds;
        this.value = value;
        this.onConfirm = onConfirm;
    }

    public void OnOpenOrClose(bool isOpening) { }
    public bool PreventLowerScreenUpdates() => false;
    public void RenderFrame(float deltaTime) {

        TextureRenderer.RenderMasked(Assets.textures.window.overlay, Color.White, bounds.Min, bounds);
        Vector2 vector = bounds.Center.Rounded();
        vector.Y -= 34f;

        TextureRenderer.Render9Slice(Assets.textures.UI.dropdown_list, Color.White, Bounds2.WithSize(vector + new Vector2(-265f, 24f), new Vector2(532f, 48f)));
        Bounds2 bounds2 = TextureRenderer.RenderText((this.value.Length == 0) ? " " : this.value, vector + new Vector2(0f, 43f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
        this.cursorBlinkTimer = (this.cursorBlinkTimer + deltaTime) % cursorBlinkPeriod;
        if (this.cursorBlinkTimer < cursorBlinkPeriod / 2f) {
            TextureRenderer.RenderColor(UIConsts.lightTextColor, Bounds2.WithSize(bounds2.BottomRight + new Vector2(2f, 1f), new Vector2(2f, 22f)));
        }
        char c = InputManager.GetCharByInput();
        if (c != '\0') {
            this.value = (this.value + c.ToString()).RemoveStyleFormat();
            this.cursorBlinkTimer = 0f;
        }
        if (InputManager.IsKeyTyped(SDL.SDLKey.SDLK_BACKSPACE) && this.value.Length > 0) {
            if (InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl)) {
                this.value = this.value.TrimEnd([]);
                this.value = this.value[..(this.value.LastIndexOf(' ') + 1)];
            } else {
                this.value = this.value[..^1];
            }
            this.cursorBlinkTimer = 0f;
        }

        bool isValid = this.value.Length > 0;
        bool flag2 = isValid && InputManager.IsEnterPressed();
        if (UIUtils.TextButton("Set", vector + new Vector2(15f, -50f)).RenderAndCheckIfPressed(isValid, true) || flag2) {
            this.onConfirm(value);
            TweakRenderer.PopOverlayScreen();
            Assets.sounds.click_button.PlaySound(1f);
        }

        int num = -265;
        if (UIUtils.TextButton("Cancel", vector + new Vector2((float)num, -50f)).RenderAndCheckIfPressed(true, true) || InputManager.IsKeyPressed(SDL.SDLKey.SDLK_ESCAPE)) {
            TweakRenderer.PopOverlayScreen();
            Assets.sounds.click_button.PlaySound(1f);
        }
        if (!this.bounds.Contains(InputManager.MousePos()) && InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
            TweakRenderer.PopOverlayScreen();
            Assets.sounds.click_button.PlaySound(1f);
        }
    }
    public void Reset() { }
}
