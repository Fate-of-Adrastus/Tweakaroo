using System;
using System.Collections.Generic;
using static MonoMod.InlineRT.MonoModRule;

namespace Tweakaroo;

internal class DropDownScreen : IScreen {

    readonly Vector2 DropDownPosition;
    readonly string[] Options;
    readonly Action<int> OnSelect;
    bool canExit = false;
    public DropDownScreen(Vector2 dropDownPosition, string[] options, Action<int> onSelect) {
        DropDownPosition = dropDownPosition;
        Options = options;
        OnSelect = onSelect;
    }

    public void OnOpenOrClose(bool isOpening) { }
    public bool PreventLowerScreenUpdates() { return false; }
    public void RenderFrame(float deltaTime) {

        float height = Options.Length * 25f + 24f;
        Vector2 size = new Vector2(287f, 25f);
        Vector2 pos = DropDownPosition + new Vector2(9f, -height - 6f);
        Bounds2 bounds = Bounds2.WithSize(pos + new Vector2(-10f, 3f), new Vector2(size.X + 21f, height + 3f));
        TextureRenderer.RenderColor(Color.Black.WithAlpha(0.9f), bounds.Expanded(-4f, -4f, -5f, -1f));
        TextureRenderer.Render9Slice(Assets.textures.UI.dropdown_list, Color.White, bounds);
        pos = DropDownPosition + new Vector2(0f, -38f);
        for (int i = 0; i < Options.Length; i++) {
            var option = Options[i];
            Bounds2 bounds2 = Bounds2.WithSize(pos, size + new Vector2(0f, -1f));
            if (bounds2.Contains(InputManager.MousePos())) {
                TextureRenderer.Render(Assets.textures.UI.dropdown_list_hover, bounds2.Min + new Vector2(-2f, -10f));
                if (InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
                    OnSelect(i);
                    Assets.sounds.click_button.PlaySound(1f);
                }
            }
            TextureRenderer.RenderText(option, pos + new Vector2(152f, 7f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, false, true);
            pos.Y -= size.Y;
        }
        if (canExit && InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
            TweakRenderer.PopOverlayScreen();
        }
        canExit = true;
    }
    public void Reset() { }
}
