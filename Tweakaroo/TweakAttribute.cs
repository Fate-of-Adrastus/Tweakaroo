using System;

namespace Tweakaroo;

[AttributeUsage(AttributeTargets.Field)]
public class TweakAttribute(string aliase = "") : Attribute {
    internal string Aliase = aliase;
}
