using Quintessential;
using Quintessential.Settings;
using System;

namespace Tweakaroo;

internal class Tweakaroo : QuintessentialMod, IDataMod {
    public static Tweakaroo Instance { get; }
    public override string ModId => "tweakaroo";
    public override Type SettingsType => typeof(TweakarooSettings);

    public override void Load() {
        TweakHandler.CollectTweaks();
    }

    public override void LoadContent() { }
    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
	public override void Unload() { }
}
public class TweakarooSettings {

    public static TweakarooSettings Instance => Tweakaroo.Instance.Settings as TweakarooSettings;

    [SettingsLabelKey("tweakaroo.settings.overlay")]
    public Keybinding ToggleOverlay = new("F7");
}
