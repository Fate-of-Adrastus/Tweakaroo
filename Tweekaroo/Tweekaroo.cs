using Quintessential;

namespace Tweekaroo;

internal class Tweekaroo : QuintessentialMod, IDataMod {
    public static Tweekaroo Instance { get; }
    public override string ModId => "tweekaroo";

    public override void Load() { }

    public override void LoadContent() { }
    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
	public override void Unload() { }
}
