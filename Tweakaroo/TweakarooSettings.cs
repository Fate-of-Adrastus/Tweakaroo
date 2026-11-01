using Quintessential;
using Quintessential.Settings;

namespace Tweakaroo;

public class TweakarooSettings {

    public static TweakarooSettings Instance => Tweakaroo.Instance.Settings as TweakarooSettings;

    [SettingsLabelKey("tweakaroo.settings.overlay")]
    public Keybinding ToggleOverlay = new("F7");
}
