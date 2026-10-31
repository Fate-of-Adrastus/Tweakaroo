using System;

namespace Tweakaroo;

[AttributeUsage(AttributeTargets.Field)]
public class TweakSliderAttribute(object lowerBound, object upperBound) : Attribute {
    internal object LowerBound = lowerBound;
    internal object UpperBound = upperBound;
}
