# Tweakaroo
Tweak & tune static constants in game for ease of development.
Tweakaroo is a mod for editing constants in game so you don't have to eyeball them.

The in game menu can be opened by with **F7** or the configured keybind.

## Attributes

## Tweak
Tweak a static field of the following types:
* `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `sbyte`, `byte`, `float`, `double`, `bool`, `string`.
* `Vector2`, `Vector3`, `Index2`, `HexIndex`, `Color`
* any Enum type
This can be `public`/`internal`/`private` and even `readonly`

You can set the name in the menu with the optional argument.

All other attributes require this one.

## TweakPriority

Provide a priority value for the tweak.
Higher values are placed above others.
The default value is 0.

## TweakSlider
Display the given field with a slider.

The optional argument configures whether a the field can also be set via a text input.

Supports:
* With 2 ints + optional args: (`10`)
   * `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `sbyte`, `byte`
* With 2 floats + optional args: (`10f`)
   * `float`
* With 2 doubles + optional args: (`10d`)
   * `double`
* With 4 ints + optional args:
   * `Index2`, `HexIndex`
* With 4 floats + optional args:
   * `Vector2`
* With 6 floats + optional args:
   * `Vector3`

First ***n*** lover bounds are provided, then ***n*** upper bounds giving a total of ***2n*** numeric inputs.

`Color` uses sliders by default, this cannot be changed.

## TweakSliderStepsize
Set the stepsize for the specific slider. ( Needs the [TweakSlider](#TweakSlider) attribute. )

Accepts ***n*** of the same type of arguments as the slider.
Arguments can be null to skip setting a step size for the given coordinate.

## Example

```c#
[Tweak("Test Int")]
[TweakSlider(0, 20, true)]
private static readonly int TestVal;

[Tweak("Alpha")]
[TweakSlider(0f, 1f)]
[TweakSliderStepsize(0.02f)]
internal static readonly float Alpha = 0.72f;

[Tweak("String-parsed Float")]
private static readonly float StringParsedF = 3.1415f;

[Tweak("String-parsed Sbyte")]
private static readonly sbyte StringParsedSb = -32;

[Tweak("Bool Test")]
internal static readonly bool A = true;

[Tweak("String from the test!")]
private static readonly string s = "When life gives you lemons, ...";

[Tweak]
[TweakPriority(1)]
private static readonly BoundsPosition Alignment;

[Tweak]
[TweakSlider(0f, -100f, 200f, 100f)]
[TweakSliderStepsize(1f, 1f)]
private static readonly Vector2 offset;

[Tweak]
[TweakSlider(0f, 0f, 0f, 100f, 100f, 100f, true)]
[TweakSliderStepsize(0.5f, null, 4f)]
private static readonly Vector3 Vec3D;

[Tweak]
[TweakSlider(0, 0, 5, 5)]
private static readonly HexIndex Hex = new(4, 5);

[Tweak]
[TweakSlider(0, 20, 5, 40)]
[TweakSliderStepsize(null, 4)]
private static readonly Index2 Index2;

[Tweak]
private static readonly HexIndex HexParsed = new(-2, -2);

[Tweak]
private static readonly Color Color = Color.White;
```
