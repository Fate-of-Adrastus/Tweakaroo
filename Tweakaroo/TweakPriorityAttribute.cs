using System;

namespace Tweakaroo;

[AttributeUsage(AttributeTargets.Field)]
public class TweakPriorityAttribute(int priority) : Attribute {
    public readonly int Priority = priority;
}
