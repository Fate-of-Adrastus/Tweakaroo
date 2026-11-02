using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Tweakaroo;

internal static class TweakHandler {
    private record InvalidTweakError(FieldInfo Field, string Message);
    private static readonly Dictionary<Type, TweakType> AllowedTypes = new() {
        { typeof(short), TweakType.Short },
        { typeof(ushort), TweakType.UShort },
        { typeof(int), TweakType.Int },
        { typeof(uint), TweakType.UInt },
        { typeof(long), TweakType.Long },
        { typeof(ulong), TweakType.ULong },
        { typeof(sbyte), TweakType.SByte },
        { typeof(byte), TweakType.Byte },
        { typeof(float), TweakType.Float },
        { typeof(double), TweakType.Double },
        { typeof(bool), TweakType.Boolean },
        { typeof(Enum), TweakType.EnumValue },
        { typeof(string), TweakType.String },
        { typeof(Vector2), TweakType.Vector2 },
        { typeof(Vector3), TweakType.Vector3 },
        { typeof(Index2), TweakType.Index2 },
        { typeof(HexIndex), TweakType.HexIndex },
        { typeof(Color), TweakType.Color }
    };
    public static List<Tweak> Tweaks = [];


    public static void CollectTweaks() {
        var tweaks = GetAllFieldsForAttribute(typeof(TweakAttribute));

        var errors = ValidateTweaks(tweaks);
        if (errors.Any()) {
            var grouped = errors.GroupBy(error => error.Field);
            string message = string.Join("\n", grouped.Select(grouping =>
                grouping.Key.DeclaringType.FullName + "." +
                grouping.Key.Name + "\n  " +
                string.Join("\n  ", grouping.Select(error => error.Message))
            ));
            throw new Exception("Found errors in tweakables!\n" + message);
        }

        foreach (var tweak in tweaks) {
            var (getter, setter) = GetSetterAndGetter(tweak);

            TweakType type;
            if (typeof(Enum).IsAssignableFrom(tweak.FieldType))
                type = TweakType.EnumValue;
            else
                type = AllowedTypes[tweak.FieldType];

            int priority = 0;
            if (tweak.GetCustomAttributes<TweakPriorityAttribute>().Any())
                priority = tweak.GetCustomAttributes<TweakPriorityAttribute>().First().Priority;

            string name = tweak.GetCustomAttributes<TweakAttribute>().First().Aliase;
            if (string.IsNullOrEmpty(name)) name = tweak.Name;

            Tweaks.Add(new Tweak(tweak, type, name, getter, setter, priority));

            if (tweak.GetCustomAttributes<TweakSliderAttribute>().Any()) {
                var sliderData = tweak.GetCustomAttributes<TweakSliderAttribute>().First();
                Tweaks[^1].LowerBound = sliderData.LowerBound;
                Tweaks[^1].UpperBound = sliderData.UpperBound;
                Tweaks[^1].AllowTextEdit = sliderData.AllowTextEdit;

                if (tweak.GetCustomAttributes<TweakSliderStepsize>().Any())
                    Tweaks[^1].StepSizes = tweak.GetCustomAttributes<TweakSliderStepsize>().First().stepSizes;
            }
        }

        Tweaks = [.. Tweaks.OrderByDescending(tw => tw.DrawPriority)];
    }


    private static IEnumerable<InvalidTweakError> ValidateTweaks(IEnumerable<FieldInfo> tweaks) {
        return tweaks.SelectMany(field => {
            List<InvalidTweakError> errors = [];
            if (!field.IsStatic)
                errors.Add( new InvalidTweakError(field, "Non static fields cannot be tweaked!"));
            if (!AllowedTypes.ContainsKey(field.FieldType) && !typeof(Enum).IsAssignableFrom(field.FieldType) )
                errors.Add( new InvalidTweakError(field, "Field has an untweakable type: " + field.FieldType));
            if (field.GetCustomAttributes<TweakSliderAttribute>().Any()) {
                var sliderData = field.GetCustomAttributes<TweakSliderAttribute>().First();
                if (field.FieldType == typeof(Vector2)) {
                    if (!typeof(object[]).IsAssignableFrom(sliderData.UpperBound.GetType()) ||
                        !typeof(object[]).IsAssignableFrom(sliderData.LowerBound.GetType()) ||
                        ((object[])sliderData.UpperBound).Length != 2 ||
                        ((object[])sliderData.LowerBound).Length != 2 ||
                        !((object[])sliderData.UpperBound).All(obj => typeof(float).IsAssignableFrom(obj.GetType())) ||
                        !((object[])sliderData.LowerBound).All(obj => typeof(float).IsAssignableFrom(obj.GetType()))
                    ) {
                        errors.Add(new InvalidTweakError(field, "Field beforeCursor is incompatible with provided slider bounds.\n  Did you forget to add the 'f' after the numeric beforeCursor to make it a float/double?\n  You must provide 4 float values for a Vector2 slider."));
                    }
                } else if (field.FieldType == typeof(Vector3)) {
                    if (!typeof(object[]).IsAssignableFrom(sliderData.UpperBound.GetType()) ||
                        !typeof(object[]).IsAssignableFrom(sliderData.LowerBound.GetType()) ||
                        ((object[])sliderData.UpperBound).Length != 3 ||
                        ((object[])sliderData.LowerBound).Length != 3 ||
                        !((object[])sliderData.UpperBound).All(obj => typeof(float).IsAssignableFrom(obj.GetType())) ||
                        !((object[])sliderData.LowerBound).All(obj => typeof(float).IsAssignableFrom(obj.GetType()))
                    ) {
                        errors.Add(new InvalidTweakError(field, "Field value is incompatible with provided slider bounds.\n  Did you forget to add the 'f' after the numeric value to make it a float/double?\n  You must provide 6 float values for a Vector2 slider."));
                    }
                } else if (field.FieldType == typeof(Index2) || field.FieldType == typeof(HexIndex)) {
                    if (!typeof(object[]).IsAssignableFrom(sliderData.UpperBound.GetType()) ||
                        !typeof(object[]).IsAssignableFrom(sliderData.LowerBound.GetType()) ||
                        ((object[])sliderData.UpperBound).Length != 2 ||
                        ((object[])sliderData.LowerBound).Length != 2 ||
                        !((object[])sliderData.UpperBound).All(obj => typeof(int).IsAssignableFrom(obj.GetType())) ||
                        !((object[])sliderData.LowerBound).All(obj => typeof(int).IsAssignableFrom(obj.GetType()))
                    ) {
                        errors.Add(new InvalidTweakError(field, "Field value is incompatible with provided slider bounds.\n  You must provide 4 integer values for an Index2 or HexIndex slider."));
                    }
                } else if (!field.FieldType.IsAssignableFrom(sliderData.UpperBound.GetType()) ||
                    !field.FieldType.IsAssignableFrom(sliderData.LowerBound.GetType())
                ) {
                    errors.Add(new InvalidTweakError(field, "Field value is incompatible with provided slider bounds.\n  Did you forget to add the 'f/d' after the numeric value to make it a float/double?"));
                }

                if (field.GetCustomAttributes<TweakSliderStepsize>().Any()) {
                    var sliderData2 = field.GetCustomAttributes<TweakSliderStepsize>().First();
                    if (field.FieldType == typeof(Vector2)) {
                        if (sliderData2.stepSizes.Length != 2 ||
                            !sliderData2.stepSizes.All(obj => obj == null || typeof(float).IsAssignableFrom(obj.GetType()))
                        ) {
                            errors.Add(new InvalidTweakError(field, "Field value is incompatible with provided slider step sizes.\n  Did you forget to add the 'f' after the numeric value to make it a float/double?\n  You must provide 2 float values for a Vector2 slider."));
                        }
                    } else if (field.FieldType == typeof(Vector3)) {
                        if (sliderData2.stepSizes.Length != 3 ||
                            !sliderData2.stepSizes.All(obj => obj == null || typeof(float).IsAssignableFrom(obj.GetType()))
                        ) {
                            errors.Add(new InvalidTweakError(field, "Field value is incompatible with provided slider step sizes.\n  Did you forget to add the 'f' after the numeric value to make it a float/double?\n  You must provide 3 float values for a Vector3 slider."));
                        }
                    } else if (field.FieldType == typeof(Index2) || field.FieldType == typeof(HexIndex)) {
                        if (sliderData2.stepSizes.Length != 2 ||
                            !sliderData2.stepSizes.All(obj => obj == null || typeof(int).IsAssignableFrom(obj.GetType()))
                        ) {
                            errors.Add(new InvalidTweakError(field, "Field value is incompatible with provided slider step sizes.\n  You must provide 2 integer values for an Index2 or HexIndex slider."));
                        }
                    } else if (sliderData2.stepSizes.Length != 1 ||
                            !sliderData2.stepSizes.All(obj => obj == null || field.FieldType.IsAssignableFrom(obj.GetType()))
                    ) {
                        errors.Add(new InvalidTweakError(field, "Field value is incompatible with provided slider step sizes.\n  Did you forget to add the 'f/d' after the numeric value to make it a float/double?"));
                    }
                }
            }
            return errors;
        });
    }

    private static IEnumerable<FieldInfo> GetAllFieldsForAttribute(Type attributeType) {
        return Assembly.GetExecutingAssembly().GetTypes().SelectMany(type => {
            return type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(field => field.GetCustomAttributes(attributeType, false).Length > 0);
        });
    }

    private static (Func<object, object> getter, DynamicMethod setter) GetSetterAndGetter(FieldInfo field) {
        // Using Dynamic Methods so we can set InitOnly fields too.
        var setter = new DynamicMethod(
            name: "set_" + field.Name,
            returnType: null,
            parameterTypes: [field.FieldType],
            restrictedSkipVisibility: true
        );
        var IL = setter.GetILGenerator();

        IL.Emit(OpCodes.Ldarg, 0);
        IL.Emit(OpCodes.Stsfld, field);
        IL.Emit(OpCodes.Ret);

        // readonly fields only return the actual value if we invoke them this way.
        // Is this safe?
        // No.
        object getter(object self) {
            var getter = new DynamicMethod(
                name: "get_" + field.Name,
                returnType: field.FieldType,
                parameterTypes: [],
                restrictedSkipVisibility: true
            );
            IL = getter.GetILGenerator();

            IL.Emit(OpCodes.Ldsfld, field);
            IL.Emit(OpCodes.Ret);

            return getter.Invoke(self, []);
        }

        return (getter, setter);
    }
}
