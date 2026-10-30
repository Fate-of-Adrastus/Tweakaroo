using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tweakaroo;

internal class TweakHandler {
    private record InvalidTweakError(FieldInfo field, string message);
    private static List<Type> AllowedTypes = [
        typeof(bool),
    ];


    private static IEnumerable<InvalidTweakError> ValidateTweaks(IEnumerable<FieldInfo> tweaks) {
        return tweaks.Transform<FieldInfo, InvalidTweakError>(field => {
            if (!field.IsStatic)
                return new InvalidTweakError(field, "Non static fields cannot be tweaked!");
            if (AllowedTypes.Contains(field.FieldType))
                return new InvalidTweakError(field, "Field has an untweakable type: " + field.FieldType);
            return MaybeHelper.empty;
        });
    }

    private static IEnumerable<FieldInfo> GetAllFieldsForAttribute(Type attributeType) {
        return Assembly.GetExecutingAssembly().GetTypes().SelectMany(type => {
            return type.GetFields().Where(field => field.GetCustomAttributes(attributeType, false).Length > 0);
        });
    }
}
