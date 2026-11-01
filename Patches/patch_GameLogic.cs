using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Tweakaroo;

internal class patch_GameLogic {

    [MonoModILInject("GameLoop")]
    static void PatchRenderOverlay(MethodDefinition method, CustomAttribute attribute) {

        ILCursor cursor = new(new ILContext(method));

        cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("GameLogic/ScreenUpdater", "updateFunc"));
        cursor.GotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Br);
        cursor.Goto(cursor.Prev.Operand as Instruction, MoveType.After);
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld("GameLogic", "backgroundFadeAmount"));

        cursor.EmitLdarg0();
        cursor.Index--;
        cursor.EmitCall(method.DeclaringType.Methods.First(m => m.Name.Equals("tweakaroo_RenderTweakarooOverlay")));
    }

    [Tweak("Test Int")]
    [TweakSlider(0, 20)]
    public static int TestVal;

    [Tweak("Alpha")]
    [TweakSlider(0f, 1f)]
    public static float Alpha = 0.6f;

    [Tweak("String-parsed Float")]
    public static float StringParsedF = 3.1415f;

    [Tweak("String-parsed Sbyte")]
    public static sbyte StringParsedSb = -32;
    [Tweak("Bool Test")]
    public static bool A = true;
    [Tweak("String from the test!")]
    public static string s = "When life gives you a lemons, make lemonade.";

    public bool tweakaroo_ShouldRenderOverlay;
    public void tweakaroo_RenderTweakarooOverlay() {
        if (InputManager.IsKeyPressed(SDL2.SDL.SDLKey.SDLK_F7))
            tweakaroo_ShouldRenderOverlay = !tweakaroo_ShouldRenderOverlay;
        if (!tweakaroo_ShouldRenderOverlay) return;


        float height = float.Min(InputManager.screenSize.Y, 1040f / 4f);

        TextureRenderer.RenderRepeating(Quintessential.UI.BackgroundLarger, Color.White.WithAlpha(Alpha), new Vector2(0, InputManager.screenSize.Y - height), new Vector2(InputManager.screenSize.X, height));

        TweakRenderer.RenderTweaks();

    }
}