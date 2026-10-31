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
    public int TestVal;

    [Tweak("Alpha")]
    [TweakSlider(0f, 1f)]
    public static float Alpha = 0.6f;

    public bool tweakaroo_ShouldRenderOverlay;
    public void tweakaroo_RenderTweakarooOverlay() {
        if (InputManager.IsKeyPressed(SDL2.SDL.SDLKey.SDLK_F7))
            tweakaroo_ShouldRenderOverlay = !tweakaroo_ShouldRenderOverlay;
        if (!tweakaroo_ShouldRenderOverlay) return;


        Vector2 screenSize = InputManager.screenSize;
        float height = float.Min(screenSize.Y, 1040f / 4f);

        TextureRenderer.RenderRepeating(Quintessential.UI.BackgroundLarger, Color.White.WithAlpha(Alpha), new Vector2(0, screenSize.Y - height), new Vector2(screenSize.X, height));

        Vector2 position = new Vector2(20f, screenSize.Y - 30f);

        foreach (var tweak in TweakHandler.Tweaks) {
            float deltaY = 0f;
            switch (tweak.TweakType) {


                case TweakType.Short:
                case TweakType.UShort:
                case TweakType.Int:
                case TweakType.UInt:
                case TweakType.Long:
                case TweakType.ULong:
                case TweakType.SByte:
                case TweakType.Byte:
                case TweakType.Float:
                case TweakType.Double:
                    deltaY = TweakRenderer.RenderNumeric(position, tweak);
                    break;
                case TweakType.Boolean:
                case TweakType.EnumValue:
                case TweakType.String:
                    break;
                default:
                    throw new NotImplementedException();
            }

            position += new Vector2(0, deltaY -10f);
        }

    }
}