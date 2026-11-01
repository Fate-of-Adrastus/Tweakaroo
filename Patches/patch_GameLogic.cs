using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using Quintessential;
using System.Collections.Generic;
using System.Linq;
using Tweakaroo;
using static MonoMod.InlineRT.MonoModRule;

public class patch_GameLogic {

    [MonoModILInject("GameLoop")]
    static void PatchRenderOverlay(MethodDefinition method, CustomAttribute attribute) {

        ILCursor cursor = new(new ILContext(method));

        cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("GameLogic/ScreenUpdater", "updateFunc"));
        cursor.GotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Br);
        cursor.Goto(cursor.Prev.Operand as Instruction, MoveType.After);
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld("GameLogic", "backgroundFadeAmount"));

        cursor.EmitLdarg0();
        cursor.Index--;
        cursor.EmitLdloc0();
        cursor.EmitCall(method.DeclaringType.Methods.First(m => m.Name.Equals("tweakaroo_RenderTweakarooOverlay")));
    }

    [MonoModIgnore]
    private RenderTarget gameRenderTarget;

    [Tweak("Test Int")]
    [TweakSlider(0, 20)]
    public static int TestVal;

    [Tweak("Alpha")]
    [TweakSlider(0f, 1f)]
    public static float Alpha = 0.72f;

    [Tweak("String-parsed Float")]
    public static float StringParsedF = 3.1415f;

    [Tweak("String-parsed Sbyte")]
    public static sbyte StringParsedSb = -32;
    [Tweak("Bool Test")]
    public static bool A = true;
    [Tweak("String from the test!")]
    public static string s = "When life gives you lemons, ...";
    [Tweak]
    [TweakPriority(1)]
    public static BoundsPosition Alignment;

    internal bool tweakaroo_ShouldRenderOverlay;
    private static readonly Scrollbar tweakaroo_Scrollbar = new();
    internal float tweakaroo_MenuWidth;
    public void tweakaroo_RenderTweakarooOverlay(patch_ScreenUpdater updater) {
        if (TweakarooSettings.Instance.ToggleOverlay.Pressed())
            tweakaroo_ShouldRenderOverlay = !tweakaroo_ShouldRenderOverlay;
        if (!tweakaroo_ShouldRenderOverlay) return;

        TweakRenderer.OverlayTarget ??= Renderer.CreateRenderTarget(InputManager.screenSize.CeilingToInt().X, InputManager.screenSize.CeilingToInt().Y);
        using (RenderTargetStackManager.Push(gameRenderTarget)) {

            Bounds2 bounds = Bounds2.Empty;
            Vector2 offset = new Vector2(30f, 0);
            using (RenderTargetStackManager.Push(TweakRenderer.OverlayTarget)) {
                RenderTargetStackManager.Clear(Color.Transparent);
                bool disableInputForMainOverlay = TweakRenderer.overlayScreens.Size() > 0;

                if (disableInputForMainOverlay)
                    InputManager.DisableInput();
                bounds = TweakRenderer.RenderTweaks(offset);
                if (disableInputForMainOverlay)
                    InputManager.EnableInput();
            }
            bounds = Bounds2.WithSize( 0, 0f, bounds.Width + 80f, InputManager.screenSize.Y);
            tweakaroo_MenuWidth = bounds.Min.X + bounds.Width;

            using (tweakaroo_Scrollbar.RenderScrollbar(bounds.Min, bounds.Size.CeilingToInt(), 0, -bounds.Width)) {
                RenderTargetStackManager.Clear(Color.Transparent);
                TextureRenderer.RenderRepeating(UI.BackgroundLarger, Color.White.WithAlpha(Alpha), bounds.Min + offset, bounds.Size);
                TextureRenderer.Render(TweakRenderer.OverlayTarget.renderedTexture, Color.White, Vector2.Zero);
            }

            List<IScreen> screens = TweakRenderer.overlayScreens.stack;
            for (int i = 0; i < screens.Count; i++) {
                bool skip = false;
                for (int j = i + 1; j < screens.Count; j++) {
                    if (screens[j].PreventLowerScreenUpdates()) {
                        skip = true;
                        break;
                    }
                }
                if (!skip) {
                    screens[i].RenderFrame(updater.lastDeltaTime);
                }
            }
        }
    }
}
