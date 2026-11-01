using MonoMod;
using Tweakaroo;

[MonoModPatch("GameLogic/ScreenUpdater")]
public class patch_ScreenUpdater {
    [MonoModIgnore] public patch_GameLogic gameLogic;
    [MonoModIgnore] public float lastDeltaTime;

    [MonoModIgnore]
    internal extern void orig_UpdateScreen(IScreen screen, bool hasInputAccess, Maybe<Transition> transition);

    internal void UpdateScreen(IScreen screen, bool hasInputAccess, Maybe<Transition> transition) {
        bool wasDisabled = false;
        if (hasInputAccess && gameLogic.tweakaroo_ShouldRenderOverlay && ( InputManager.MousePos().X < gameLogic.tweakaroo_MenuWidth || TweakRenderer.overlayScreens.Size() > 0)) {
            InputManager.DisableInput();
            wasDisabled = true;
        }
        orig_UpdateScreen(screen, hasInputAccess, transition);
        if (wasDisabled) {
            InputManager.EnableInput();
        }
    }
}
