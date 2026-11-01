using MonoMod;

[MonoModPatch("GameLogic/ScreenUpdater")]
public class patch_ScreenUpdater {
    [MonoModIgnore]
    public patch_GameLogic gameLogic;

    [MonoModIgnore]
    internal extern void orig_UpdateScreen(IScreen screen, bool hasInputAccess, Maybe<Transition> transition);

    internal void UpdateScreen(IScreen screen, bool hasInputAccess, Maybe<Transition> transition) {
        bool wasDisabled = false;
        if (hasInputAccess && gameLogic.tweakaroo_ShouldRenderOverlay && InputManager.MousePos().X < gameLogic.tweakaroo_MenuWidth) {
            InputManager.DisableInput();
            wasDisabled = true;
        }
        orig_UpdateScreen(screen, hasInputAccess, transition);
        if (wasDisabled) {
            InputManager.EnableInput();
        }
    }
}
