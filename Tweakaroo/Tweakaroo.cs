using Quintessential;

namespace Tweakaroo;

internal class Tweakaroo : QuintessentialMod, IDataMod {
    public static Tweakaroo Instance { get; }
    public override string ModId => "tweakaroo";

    public override void Load() {
        TweakHandler.CollectTweaks();
    }

    public override void LoadContent() { }
    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
	public override void Unload() { }
}
