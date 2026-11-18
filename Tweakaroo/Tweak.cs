using Mono.Cecil;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Tweakaroo;

internal class Tweak {

    public FieldInfo Field;
    public TweakType TweakType;

    public string Name;

    private Func<object, object> Getter;
    private DynamicMethod Setter;
    public MethodInfo PresetGetter;
    public MethodInfo PresetSetter;

    public object UpperBound;
    public object LowerBound;
    public object[] StepSizes;
    public bool AllowTextEdit;

    public string[] DropdownStrings;
    public MethodInfo DropdownSource;

    public int DrawPriority;
    public bool[] CurrentlyEdited;
    public Tweak(FieldInfo field, TweakType type, string name, Func<object, object> getter, DynamicMethod setter, int drawPriority) {
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
            TweakType.Color => [false, false, false, false],
            _ => [false]
        };
        StepSizes = type switch {
            TweakType.Vector2 => [null, null],
            TweakType.Vector3 => [null, null, null],
            TweakType.Index2 => [null, null],
            TweakType.HexIndex => [null, null],
            TweakType.Color => [null, null, null, null],
            _ => [null]
        };
    }

    public object GetValue() {
        if (PresetGetter is not null)
            return PresetGetter.Invoke(null, []);
        else
            return Getter.Invoke(null);
    }
    public void SetValue(object value) {
        if (PresetSetter is not null)
            PresetSetter.Invoke(null, [value]);
        else
            Setter.Invoke(null, [value]);
    }
}
