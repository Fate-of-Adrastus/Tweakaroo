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
    public bool CurrentlyEdited = false;
    public Tweak(FieldInfo field, TweakType type, string name, DynamicMethod getter, DynamicMethod setter, int drawPriority) {
        Field = field;
        TweakType = type;
        Name = name;
        Getter = getter;
        Setter = setter;
        DrawPriority = drawPriority;
    }
}
