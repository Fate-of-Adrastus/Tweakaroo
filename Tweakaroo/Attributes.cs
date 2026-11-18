using System;

namespace Tweakaroo;

[AttributeUsage(AttributeTargets.Field)]
public class TweakAttribute(string aliase = "") : Attribute {
    internal string Aliase = aliase;
}

[AttributeUsage(AttributeTargets.Field)]
public class TweakPriorityAttribute(int priority) : Attribute {
    public readonly int Priority = priority;
}

[AttributeUsage(AttributeTargets.Field)]
public class TweakSliderAttribute : Attribute {
    internal object LowerBound;
    internal object UpperBound;
    internal bool AllowTextEdit;

    public TweakSliderAttribute(object lowerBound, object upperBound, bool allowTextEdit = false) {
        LowerBound = lowerBound;
        UpperBound = upperBound;
        AllowTextEdit = allowTextEdit;
    }

    public TweakSliderAttribute(object lowerBoundX, object lowerBoundY, object upperBoundX, object upperBoundY, bool allowTextEdit = false) {
        LowerBound = new object[] { lowerBoundX, lowerBoundY };
        UpperBound = new object[] { upperBoundX, upperBoundY };
        AllowTextEdit = allowTextEdit;
    }

    public TweakSliderAttribute(object lowerBoundX, object lowerBoundY, object lowerBoundZ, object upperBoundX, object upperBoundY, object upperBoundZ, bool allowTextEdit = false) {
        LowerBound = new object[] { lowerBoundX, lowerBoundY, lowerBoundZ };
        UpperBound = new object[] { upperBoundX, upperBoundY, upperBoundZ };
        AllowTextEdit = allowTextEdit;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class TweakSliderStepsize : Attribute {
    internal object[] stepSizes;

    public TweakSliderStepsize(object stepSize) {
        stepSizes = [stepSize];
    }
    public TweakSliderStepsize(object stepSizeX, object stepSizeY) {
        stepSizes = [stepSizeX, stepSizeY];
    }
    public TweakSliderStepsize(object stepSizeX, object stepSizeY, object stepSizeZ) {
        stepSizes = [stepSizeX, stepSizeY, stepSizeZ];
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class TweakSource : Attribute {
    internal string setterName, getterName;

    public TweakSource(string setterName, string getterName) {
        this.setterName = setterName;
        this.getterName = getterName;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class TweakDropdownString : Attribute {
    internal string[] optionNames;
    internal string sourceMethod;

    public TweakDropdownString(params string[] optionNames) {
        this.optionNames = optionNames;
    }

    public TweakDropdownString(bool unusedBool, string sourceMethod) {
        this.sourceMethod = sourceMethod;
    }
}
