using Quintessential;
using SDL2;
using System;

namespace Tweakaroo;

internal class StringInputScreen : IScreen {
    readonly Bounds2 bounds;
    string beforeCursor;
    string afterCursor = "";
    Action<string> onConfirm;
    Predicate<string> checkValid;

    float cursorBlinkTimer;
    static float cursorBlinkPeriod = 1.2f;
    public StringInputScreen(Bounds2 bounds, string value, Action<string> onConfirm, Predicate<string> checkValid) {
        this.bounds = bounds;
        this.beforeCursor = value;
        this.onConfirm = onConfirm;
        this.checkValid = checkValid;
    }

    public void OnOpenOrClose(bool isOpening) { }
    public bool PreventLowerScreenUpdates() => false;
    public void RenderFrame(float deltaTime) {

        TextureRenderer.RenderRepeating(UI.BackgroundLarger, Color.White.WithAlpha(0.72f), bounds.Min, bounds.Size);
        Vector2 vector = bounds.Center.Rounded();
        vector.Y -= 34f;

        TextureRenderer.Render9Slice(Assets.textures.UI.dropdown_list, Color.White, Bounds2.WithSize(vector + new Vector2(-265f, 24f), new Vector2(532f, 48f)));
        Bounds2 bounds2 = TextureRenderer.RenderText((beforeCursor + afterCursor).Length == 0 ? " " : beforeCursor + afterCursor, vector + new Vector2(0f, 43f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
        float afterTextWidth = afterCursor.Length == 0 ? 0f : TextureRenderer.RenderText(afterCursor, Vector2.Zero, Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, false).Width;
        cursorBlinkTimer = (cursorBlinkTimer + deltaTime) % cursorBlinkPeriod;
        if (cursorBlinkTimer < cursorBlinkPeriod / 2f) {
            TextureRenderer.RenderColor(UIConsts.lightTextColor, Bounds2.WithSize(bounds2.BottomRight + new Vector2(1f -afterTextWidth, 1f), new Vector2(2f, 22f)));
        }
        char c = InputManager.GetCharByInput();
        if (c != '\0') {
            beforeCursor = (beforeCursor + c.ToString()).RemoveStyleFormat();
            cursorBlinkTimer = 0f;
        }
        if (InputManager.IsKeyTyped(SDL.SDLKey.SDLK_BACKSPACE) && beforeCursor.Length > 0) {
            if (InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl)) {
                beforeCursor = beforeCursor.TrimEnd([]);
                beforeCursor = beforeCursor[..(beforeCursor.LastIndexOf(' ') + 1)];
            } else {
                beforeCursor = beforeCursor[..^1];
            }
            cursorBlinkTimer = 0f;
        }
        if (InputManager.IsKeyTyped(SDL.SDLKey.SDLK_LEFT) && beforeCursor.Length > 0) {
            if (InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl)) {
                while (beforeCursor.Length > 0 && beforeCursor[^1] == ' ') {
                    afterCursor = beforeCursor[^1] + afterCursor;
                    beforeCursor = beforeCursor[..^1];
                }
                if (beforeCursor.Length > 0) {
                    afterCursor = beforeCursor[(beforeCursor.LastIndexOf(' ') + 1)..] + afterCursor;
                    beforeCursor = beforeCursor[..(beforeCursor.LastIndexOf(' ') + 1)];
                }
            } else {
                afterCursor = beforeCursor[^1] + afterCursor;
                beforeCursor = beforeCursor[..^1];
            }
            cursorBlinkTimer = 0f;
        }
        if (InputManager.IsKeyTyped(SDL.SDLKey.SDLK_RIGHT) && afterCursor.Length > 0) {
            if (InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl)) {
                while (afterCursor.Length > 0 && afterCursor[0] == ' ') {
                    beforeCursor += afterCursor[0];
                    afterCursor = afterCursor[1..];
                }
                if (afterCursor.Length > 0) {
                    beforeCursor += (afterCursor + " ")[..((afterCursor + " ").FirstIndexOf(' ').GetValue() + 1)][..^1];
                    afterCursor = afterCursor.Contains(' ') ? afterCursor[afterCursor.FirstIndexOf(' ').GetValue()..] : "";
                }
            } else {
                beforeCursor += afterCursor[0];
                afterCursor = afterCursor[1..];
            }
            cursorBlinkTimer = 0f;
        }
        if (InputManager.IsKeyTyped(SDL.SDLKey.SDLK_HOME) && afterCursor.Length > 0) {
            afterCursor = beforeCursor + afterCursor;
            beforeCursor = "";
            cursorBlinkTimer = 0f;
        }

        bool isValid = checkValid(beforeCursor + afterCursor);
        bool flag2 = isValid && InputManager.IsEnterPressed();
        if (UIUtils.TextButton("Set", vector + new Vector2(15f, -50f)).RenderAndCheckIfPressed(isValid, true) || flag2) {
            onConfirm(beforeCursor + afterCursor);
            TweakRenderer.PopOverlayScreen();
            Assets.sounds.click_button.PlaySound(1f);
        }

        int num = -265;
        if (UIUtils.TextButton("Cancel", vector + new Vector2(num, -50f)).RenderAndCheckIfPressed(true, true) || InputManager.IsKeyPressed(SDL.SDLKey.SDLK_ESCAPE)) {
            TweakRenderer.PopOverlayScreen();
            Assets.sounds.click_button.PlaySound(1f);
        }
        if (!bounds.Contains(InputManager.MousePos()) && InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
            TweakRenderer.PopOverlayScreen();
            Assets.sounds.click_button.PlaySound(1f);
        }
    }
    public void Reset() { }
}
