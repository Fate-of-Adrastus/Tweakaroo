using Quintessential;
using SDL2;
using System;

namespace Tweakaroo;

internal class StringInputScreen : IScreen {
    readonly Bounds2 bounds;
    string beforeCursor = "";
    string selectionBefore = "";
    string selectionAfter = "";
    string afterCursor = "";
    readonly Action<string> onConfirm;
    readonly Predicate<string> checkValid;

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

        Bounds2 fullBounds = TextureRenderer.RenderText((beforeCursor + selectionBefore + selectionAfter + afterCursor).Length == 0 ? " " : beforeCursor + selectionBefore + selectionAfter + afterCursor, vector + new Vector2(0f, 43f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, false);
        float beforeSelection =        beforeCursor.Length == 0 ? 0f :
            TextureRenderer.RenderText(beforeCursor, vector + new Vector2(0f, 43f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, false).Width;
        float beforeCursorW =         (beforeCursor + selectionBefore).Length == 0 ? 0f :
            TextureRenderer.RenderText(beforeCursor + selectionBefore, vector + new Vector2(0f, 43f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, false).Width;
        float selection =             (beforeCursor + selectionBefore + selectionAfter).Length == 0 ? 0f :
            TextureRenderer.RenderText(beforeCursor + selectionBefore + selectionAfter, vector + new Vector2(0f, 43f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, false).Width;
        
        Bounds2 selectionBounds = Bounds2.WithCorners(fullBounds.TopLeft + new Vector2(beforeSelection, 0f), fullBounds.BottomLeft + new Vector2(selection, 0f));
        TextureRenderer.RenderColor(Color.FromInts(75, 146, 191, 136), selectionBounds);

        TextureRenderer.RenderText((beforeCursor + selectionBefore + selectionAfter + afterCursor).Length == 0 ? " " : beforeCursor + selectionBefore + selectionAfter + afterCursor, vector + new Vector2(0f, 43f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, false, true);
        
        cursorBlinkTimer = (cursorBlinkTimer + deltaTime) % cursorBlinkPeriod;
        if (cursorBlinkTimer < cursorBlinkPeriod / 2f) {
            TextureRenderer.RenderColor(UIConsts.lightTextColor, Bounds2.WithSize(fullBounds.BottomLeft + new Vector2(1f + beforeCursorW, 1f), new Vector2(2f, 22f)));
        }

        char c = InputManager.GetCharByInput();
        if (c != '\0') {
            beforeCursor = (beforeCursor + c.ToString()).RemoveStyleFormat();
            cursorBlinkTimer = 0f;
        }
        if (InputManager.IsKeyTyped(SDL.SDLKey.SDLK_BACKSPACE) && (beforeCursor + selectionBefore + selectionAfter).Length > 0) {
            if (selectionAfter.Length > 0) {
                selectionAfter = "";
                if (selectionBefore.Length > 0) selectionBefore = "";
            } else if (selectionBefore.Length > 0) {
                selectionBefore = "";

            }
            else if (InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl)) {
                beforeCursor = beforeCursor.TrimEnd([]);
                beforeCursor = beforeCursor[..(beforeCursor.LastIndexOf(' ') + 1)];
            } else {
                beforeCursor = beforeCursor[..^1];
            }
            cursorBlinkTimer = 0f;
        }
        if (InputManager.IsKeyTyped(SDL.SDLKey.SDLK_DELETE) && (selectionBefore + selectionAfter + afterCursor).Length > 0) {
            if (selectionAfter.Length > 0) {
                selectionAfter = "";
                if (selectionBefore.Length > 0) selectionBefore = "";
            } else if (selectionBefore.Length > 0) {
                selectionBefore = "";

            }
            else if (InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl)) {
                while (afterCursor.Length > 0 && afterCursor[0] == ' ') {
                    afterCursor = afterCursor[1..];
                }
                if (afterCursor.Length > 0) {
                    afterCursor = afterCursor.Contains(' ') ? afterCursor[afterCursor.FirstIndexOf(' ').GetValue()..] : "";
                }
            } else {
                afterCursor = afterCursor[1..];
            }
            cursorBlinkTimer = 0f;
        }
        if (InputManager.IsKeyTyped(SDL.SDLKey.SDLK_LEFT) && (beforeCursor + selectionBefore + selectionAfter).Length > 0) {
            if (InputManager.IsModifierKeyHeld(ModifierKeyType.Shift)) {
                if (selectionBefore.Length == 0 && beforeCursor.Length > 0) {
                    MoveLeft(ref beforeCursor, ref selectionAfter);
                } else if (selectionBefore.Length > 0) {
                    if (selectionAfter.Length != 0) {
                        afterCursor = selectionAfter + afterCursor;
                        selectionAfter = "";
                    }
                    MoveLeft(ref selectionBefore, ref afterCursor);
                }
            }
            else if (selectionBefore.Length == 0) {
                if (selectionAfter.Length == 0) {
                    MoveLeft(ref beforeCursor, ref afterCursor);
                } else {
                    afterCursor = selectionAfter + afterCursor;
                    selectionAfter = "";
                }
            } else {
                afterCursor = selectionBefore + selectionAfter + afterCursor;
                selectionBefore = "";
                selectionAfter = "";
            }
            cursorBlinkTimer = 0f;
        }
        if (InputManager.IsKeyTyped(SDL.SDLKey.SDLK_RIGHT) && (selectionBefore + selectionAfter + afterCursor).Length > 0) {
            if (InputManager.IsModifierKeyHeld(ModifierKeyType.Shift)) {
                if (selectionAfter.Length == 0 && afterCursor.Length > 0) {
                    MoveRight(ref selectionBefore, ref afterCursor);
                } else if (selectionAfter.Length > 0) {
                    if (selectionBefore.Length != 0) {
                        beforeCursor += selectionBefore;
                        selectionBefore = "";
                    }
                    MoveRight(ref beforeCursor, ref selectionAfter);
                }
            }
            else if (selectionBefore.Length == 0) {
                if (selectionAfter.Length == 0) {
                    MoveRight(ref beforeCursor, ref afterCursor);
                } else {
                    beforeCursor += selectionAfter;
                    selectionAfter = "";
                }
            } else {
                beforeCursor += selectionBefore + selectionAfter;
                selectionBefore = "";
                selectionAfter = "";
            }
            cursorBlinkTimer = 0f;
        }
        if (InputManager.IsKeyTyped(SDL.SDLKey.SDLK_HOME)) {
            afterCursor = beforeCursor + selectionBefore + selectionAfter + afterCursor;
            beforeCursor = "";
            selectionBefore = "";
            selectionAfter = "";
            cursorBlinkTimer = 0f;
        }

        bool isValid = checkValid(beforeCursor + selectionBefore + selectionAfter + afterCursor);
        bool flag2 = isValid && InputManager.IsEnterPressed();
        if (UIUtils.TextButton("Set", vector + new Vector2(15f, -50f)).RenderAndCheckIfPressed(isValid, true) || flag2) {
            onConfirm(beforeCursor + selectionBefore + selectionAfter + afterCursor);
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

    private void MoveLeft(ref string left, ref string right) {
        if (InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl)) {

            while (left.Length > 0 && left[^1] == ' ') {
                right = left[^1] + right;
                left = left[..^1];
            }
            if (left.Length > 0) {
                right = left[(left.LastIndexOf(' ') + 1)..] + right;
                left = left[..(left.LastIndexOf(' ') + 1)];
            }
        } else {
            right = left[^1] + right;
            left = left[..^1];
        }
    }
    private void MoveRight(ref string left, ref string right) {
        if (InputManager.IsModifierKeyHeld(ModifierKeyType.Ctrl)) {
            while (right.Length > 0 && right[0] == ' ') {
                left += right[0];
                right = right[1..];
            }
            if (right.Length > 0) {
                left += (right + " ")[..((right + " ").FirstIndexOf(' ').GetValue() + 1)][..^1];
                right = right.Contains(' ') ? right[right.FirstIndexOf(' ').GetValue()..] : "";
            }
        } else {
            left += right[0];
            right = right[1..];
        }
    }
}
