using System;

namespace Tweakaroo;

internal static class TweakRenderer {
    public static RenderTarget OverlayTarget;
    public static Stack<IScreen> overlayScreens;
    private static object updatedValue;

    public static Bounds2 RenderTweaks(Vector2 offset) {
        Vector2 position = new Vector2(10f, InputManager.screenSize.Y - 30f) + offset;
        float width = 0f;

        foreach (var tweak in TweakHandler.Tweaks) {
            Vector2 delta = Vector2.Zero;
            switch (tweak.TweakType) {


                case TweakType.Short:
                case TweakType.UShort:
                case TweakType.Int:
                case TweakType.UInt:
                case TweakType.Long:
                case TweakType.ULong:
                case TweakType.SByte:
                case TweakType.Byte:
                case TweakType.Float:
                case TweakType.Double:
                    delta = RenderNumeric(position, tweak);
                    break;
                case TweakType.Boolean:
                    delta = RenderBoolean(position, tweak);
                    break;
                case TweakType.EnumValue: // TODO droppdown list
                    break;
                case TweakType.String:
                    delta = RenderString(position, tweak);
                    break;
                default:
                    throw new NotImplementedException();
            }

            position += new Vector2(0, delta.Y -10f);
            width = float.Max(width, delta.X);
        }

        return Bounds2.WithCorners(position, new Vector2(width + 10f, InputManager.screenSize.Y));
    }
    public static Vector2 RenderNumeric(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;
        object currentValue = tweak.Getter.Invoke(null, []);
        float width = 0f;
        if (tweak.UpperBound != null && tweak.LowerBound != null) {

            var bounds = TextureRenderer.RenderText(string.Format("{0}: {1}", tweak.Name, TypeSafeString(currentValue, tweak.TweakType)), pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, tweak.UpperBound, tweak.LowerBound, currentValue, newValue => tweak.Setter.Invoke(null, [newValue]), tweak.TweakType, ref tweak.CurrentlyEdited);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        } else {

            var bounds = TextureRenderer.RenderText(tweak.Name, pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = TextButton(pos, currentValue, newValue => { if (TryParseFromString(newValue, tweak)) tweak.Setter.Invoke(null, [updatedValue]); } , tweak.TweakType, ref tweak.CurrentlyEdited);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        }
        return new Vector2(width, pos.Y-origY);
    }

    public static Vector2 RenderString(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;
        object currentValue = tweak.Getter.Invoke(null, []);
        float width = 0;

        var bounds = TextureRenderer.RenderText(tweak.Name, pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
        pos += new Vector2(0, -bounds.Height);
        width = float.Max(width, bounds.Width);
        bounds = TextButton(pos, currentValue, newValue => { if (TryParseFromString(newValue, tweak)) tweak.Setter.Invoke(null, [updatedValue]); }, tweak.TweakType, ref tweak.CurrentlyEdited);
        pos += new Vector2(0, bounds.Height);
        width = float.Max(width, bounds.Width);

        return new Vector2(width, pos.Y-origY);
    }

    public static Vector2 RenderBoolean(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;
        float width = 0;

        bool currentValue = (bool)tweak.Getter.Invoke(null, []);
        var bounds = TextureRenderer.RenderText(tweak.Name, pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
        pos += new Vector2(0, -bounds.Height);
        width = float.Max(width, bounds.Width);
        bounds = Bounds2.WithSize(pos, new Vector2(36f, -37f));
        if (currentValue != ToggleButtons(currentValue, pos + new Vector2(0, -10f), true, false)) {
            tweak.Setter.Invoke(null, [!currentValue]);
        }
        pos += new Vector2(0, bounds.Height);
        width = float.Max(width, bounds.Width);

        return new Vector2(width, pos.Y-origY);
    }


    private static Bounds2 TextButton(Vector2 pos, object currentValue, Action<string> updateValue, TweakType type, ref bool edited) {
        string current = TypeSafeString(currentValue, type);
        Bounds2 textBounds = TextureRenderer.RenderText(Utils.FormatBold(StringExtension.TranslateToUpper(current), true), Vector2.Zero, Assets.fonts.crimson_13, Color.White, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, -1, Color.White, null, int.MaxValue, calculateBounds: true, doRender: false);
        float width = 310f;
        bool isTooLong = textBounds.Width > width;
        Bounds2 bounds = Bounds2.WithSize(pos, new Vector2(width + 20, 46));

        if (UIUtils.TextButton(isTooLong ? "..." : current, pos - new Vector2(0, bounds.Height / 2f), (int)float.Round(width), (int)float.Round(bounds.Height)).RenderAndCheckIfPressed(true, true)) {
            GameLogic.instance.PushScreen(MessageBoxScreen.TextFieldBox(Bounds2.WithSize(Vector2.Zero, InputManager.screenSize), false, "", current, Translations.Translate("Set"), updateValue));
            Assets.sounds.click_button.PlaySound(1f);
            edited = true;
        }
        bounds = Bounds2.WithSize(pos.X, pos.Y, width, -bounds.Height);
        return bounds;
    }
    private static Bounds2 SliderBar(Vector2 pos, object upperBound, object lowerBound, object currentValue, Action<object> updateValue, TweakType type, ref bool edited) {
        var origPos = pos;
        var bounds = TextureRenderer.RenderText(TypeSafeString(lowerBound, type), pos, Assets.fonts.cormorant_15, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
        var backStepPosition = pos + new Vector2(bounds.Width, float.Round(-Assets.textures.UI.volume.GetHeight()/ 2.5f));
        pos += new Vector2(bounds.Width, float.Round(-Assets.textures.UI.volume.GetHeight()));
        float updated = SliderBar(ProgressInRange(currentValue, upperBound, lowerBound, type), backStepPosition, ref edited, false, true);
        backStepPosition += new Vector2(310f, float.Round(Assets.textures.UI.volume.GetHeight()/ 2.5f));
        pos += new Vector2(310f, 0f);
        bounds = TextureRenderer.RenderText(TypeSafeString(upperBound, type), backStepPosition, Assets.fonts.cormorant_15, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);

        if (edited) {
            ValueFromProgress(updated, upperBound, lowerBound, type);
            updateValue(updatedValue);
        }
        return Bounds2.WithSize(origPos, pos - origPos + new Vector2(bounds.Width, 0));
    }
    private static float SliderBar(float value, Vector2 pos, ref bool valueChanged, bool disableClick, bool canHoverAndClick) {
        pos = pos.Rounded();
        Vector2 vector = pos + new Vector2(11f, 12f);
        float num = 286f * value;
        float num2 = value;
        Bounds2 bounds = Bounds2.WithSize(pos, new Vector2(308f, 33f));
        TextureRenderer.Render(Assets.textures.UI.volume, pos);
        if (num > 5f) {
            Bounds2.WithSize(vector, new Vector2(num, 10f));
            TextureRenderer.Render9Slice(Assets.textures.UI.volume_bar, Color.White, vector, new Vector2(num, 10f));
        }
        if ((bounds.Contains(InputManager.MousePos()) && canHoverAndClick) | valueChanged) {
            TextureRenderer.Render(Assets.textures.UI.volume_hover, pos);
        }
        if (bounds.Contains(InputManager.MousePos()) && InputManager.IsClickPressed(MouseButtonType.LeftClick) && !disableClick && canHoverAndClick) {
            valueChanged = true;
        }
        if (valueChanged && !InputManager.IsClickHeld(MouseButtonType.LeftClick)) {
            valueChanged = false;
            if (num2 < 0.017482517f) {
                num2 = 0f;
            }
        }
        if (valueChanged) {
            num2 = Utils.Clamp0to1((InputManager.MousePos().X - vector.X) / 286f);
        }
        return num2;
    }
    private static bool ToggleButtons(bool currentValue, Vector2 pos, bool canHoverAndClick, bool disableOn) {
        pos = pos.Rounded();
        Bounds2 bounds = Bounds2.WithSize(pos, new Vector2(36f, 37f));
        if (!disableOn && (bounds.Contains(InputManager.MousePos())) && canHoverAndClick) {
            TextureRenderer.Render(Assets.textures.UI.checkbox_hover, bounds.Min);
            if (InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
                currentValue = !currentValue;
            }
        } else {
            TextureRenderer.Render(Assets.textures.UI.checkbox, Color.White.WithAlpha(disableOn ? 0.3f : 1f), bounds.Min);
        }
        if (currentValue) {
            TextureRenderer.Render(Assets.textures.UI.checkbox_fill, bounds.Min);
        }
        return currentValue;
    }
    private static string TypeSafeString(object obj, TweakType type) {
        return type switch {
            TweakType.Short => ((short)obj).ToString(),
            TweakType.UShort => ((ushort)obj).ToString(),
            TweakType.Int => ((int)obj).ToString(),
            TweakType.UInt => ((uint)obj).ToString(),
            TweakType.Long => ((long)obj).ToString(),
            TweakType.ULong => ((ulong)obj).ToString(),
            TweakType.SByte => ((sbyte)obj).ToString(),
            TweakType.Byte => ((byte)obj).ToString(),
            TweakType.Float => ((float)obj).ToString(),
            TweakType.Double => ((double)obj).ToString(),
            TweakType.String => ((string)obj).ToString(),
            TweakType.Boolean => ((bool)obj) ? "True" : "False",
            TweakType.EnumValue => ((Enum)obj).ToString(),
            _ => throw new NotImplementedException()
        };
    }
    private static float ProgressInRange(object obj, object upperBound, object lowerBound, TweakType type) {
        return type switch {
            TweakType.Short => ((short)obj - (short)lowerBound) / (float)((short)upperBound - (short)lowerBound),
            TweakType.UShort => ((ushort)obj - (ushort)lowerBound) / (float)((ushort)upperBound - (ushort)lowerBound),
            TweakType.Int => ((int)obj - (int)lowerBound) / (float)((int)upperBound - (int)lowerBound),
            TweakType.UInt => ((uint)obj - (uint)lowerBound) / (float)((uint)upperBound - (uint)lowerBound),
            TweakType.Long => ((long)obj - (long)lowerBound) / (float)((long)upperBound - (long)lowerBound),
            TweakType.ULong => ((ulong)obj - (ulong)lowerBound) / (float)((ulong)upperBound - (ulong)lowerBound),
            TweakType.SByte => ((sbyte)obj - (sbyte)lowerBound) / (float)((sbyte)upperBound - (sbyte)lowerBound),
            TweakType.Byte => ((byte)obj - (byte)lowerBound) / (float)((byte)upperBound - (byte)lowerBound),
            TweakType.Float => ((float)obj - (float)lowerBound) / (float)((float)upperBound - (float)lowerBound),
            TweakType.Double => (float)(((double)obj - (double)lowerBound) / ((double)upperBound - (double)lowerBound)),
            TweakType.String => throw new InvalidOperationException(),
            TweakType.Boolean => throw new InvalidOperationException(),
            TweakType.EnumValue => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException()
        };
    }
    private static void ValueFromProgress(float progress, object upperBound, object lowerBound, TweakType type) {
        updatedValue =type switch {
            TweakType.Short => (short)(((short)upperBound - (short)lowerBound) * progress) + (short)lowerBound,
            TweakType.UShort => (ushort)(((ushort)upperBound - (ushort)lowerBound) * progress) + (ushort)lowerBound,
            TweakType.Int => (int)(((int)upperBound - (int)lowerBound) * progress) + (int)lowerBound,
            TweakType.UInt => (uint)(((uint)upperBound - (uint)lowerBound) * progress) + (uint)lowerBound,
            TweakType.Long => (long)(((long)upperBound - (long)lowerBound) * progress) + (long)lowerBound,
            TweakType.ULong => (ulong)(((ulong)upperBound - (ulong)lowerBound) * progress) + (ulong)lowerBound,
            TweakType.SByte => (sbyte)(((sbyte)upperBound - (sbyte)lowerBound) * progress) + (sbyte)lowerBound,
            TweakType.Byte => (byte)(((byte)upperBound - (byte)lowerBound) * progress) + (byte)lowerBound,
            TweakType.Float => (float)(((float)upperBound - (float)lowerBound) * progress) + (float)lowerBound,
            TweakType.Double => (object)((double)(((double)upperBound - (double)lowerBound) * progress) + (double)lowerBound),
            _ => throw new InvalidOperationException(),
        };
    }
    private static bool TryParseFromString(string str, Tweak tweak) {
        bool success;
        switch (tweak.TweakType) {
            case TweakType.Short:
                success = short.TryParse(str, out var shortValue);
                updatedValue = shortValue;
                break;
            case TweakType.UShort:
                success = ushort.TryParse(str, out var ushortValue);
                updatedValue = ushortValue;
                break;
            case TweakType.Int:
                success = int.TryParse(str, out var intValue);
                updatedValue = intValue;
                break;
            case TweakType.UInt:
                success = uint.TryParse(str, out var uintValue);
                updatedValue = uintValue;
                break;
            case TweakType.Long:
                success = long.TryParse(str, out var longValue);
                updatedValue = longValue;
                break;
            case TweakType.ULong:
                success = ulong.TryParse(str, out var ulongValue);
                updatedValue = ulongValue;
                break;
            case TweakType.SByte:
                success = sbyte.TryParse(str, out var sbyteValue);
                updatedValue = sbyteValue;
                break;
            case TweakType.Byte:
                success = byte.TryParse(str, out var byteValue);
                updatedValue = byteValue;
                break;
            case TweakType.Float:
                success = float.TryParse(str, out var floatValue);
                updatedValue = floatValue;
                break;
            case TweakType.Double:
                success = double.TryParse(str, out var doubleValue);
                updatedValue = doubleValue;
                break;
            case TweakType.Boolean:
                success = bool.TryParse(str, out var boolValue);
                updatedValue = boolValue;
                break;
            case TweakType.EnumValue:
                success = Enum.TryParse(tweak.Field.FieldType, str, out var enumValue);
                updatedValue = enumValue;
                break;
            case TweakType.String:
                success = true;
                updatedValue = str;
                break;
            default:
                throw new InvalidOperationException();
        }
        return success;
    }
}
