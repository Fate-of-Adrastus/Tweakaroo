using System.Reflection;
using System.Reflection.Emit;

namespace Tweakaroo;

internal class Tweak {

    public FieldInfo Field;
    public TweakType TweakType;

    public string Name;

    public DynamicMethod Getter;
    public DynamicMethod Setter;

    public object UpperBound;
    public object LowerBound;

    public int DrawPriority;
    public bool[] CurrentlyEdited;
    public Tweak(FieldInfo field, TweakType type, string name, DynamicMethod getter, DynamicMethod setter, int drawPriority) {
        Field = field;
        TweakType = type;
        Name = name;
        Getter = getter;
        Setter = setter;
        DrawPriority = drawPriority;
        CurrentlyEdited = type switch {
            TweakType.Vector2 => [false, false],
            TweakType.Vector3 => [false, false, false],
            TweakType.Index2 => [false, false],
            TweakType.HexIndex => [false, false],
            _ => [false]
        };
    }
}
