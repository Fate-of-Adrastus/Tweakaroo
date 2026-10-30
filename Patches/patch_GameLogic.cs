using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System.Linq;

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

    public void tweakaroo_RenderTweakarooOverlay() {

    }
}