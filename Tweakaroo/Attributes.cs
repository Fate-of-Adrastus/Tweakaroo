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

    public TweakSliderAttribute(object lowerBound, object upperBound) {
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }

    public TweakSliderAttribute(object lowerBoundX, object lowerBoundY, object upperBoundX, object upperBoundY) {
        LowerBound = new object[] { lowerBoundX, lowerBoundY };
        UpperBound = new object[] { upperBoundX, upperBoundY };
    }

    public TweakSliderAttribute(object lowerBoundX, object lowerBoundY, object lowerBoundZ, object upperBoundX, object upperBoundY, object upperBoundZ) {
        LowerBound = new object[] { lowerBoundX, lowerBoundY, lowerBoundZ };
        UpperBound = new object[] { upperBoundX, upperBoundY, upperBoundZ };
    }
}
