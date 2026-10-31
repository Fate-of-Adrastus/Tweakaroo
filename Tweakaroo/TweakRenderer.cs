using Quintessential;
using System;

namespace Tweakaroo;

internal static class TweakRenderer {

    public static float RenderNumeric(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;

        if (tweak.UpperBound != null && tweak.LowerBound != null) {

            object currentValue = tweak.Getter.Invoke(null, []);
            bool changed = false;

            var bounds = TextureRenderer.RenderText(string.Format("{0}: {1}", tweak.Name, TypeSafeString(currentValue, tweak)), pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            bounds = TextureRenderer.RenderText(TypeSafeString(tweak.LowerBound, tweak), pos, Assets.fonts.cormorant_15, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            var backStepPosition = pos + new Vector2(bounds.Width, float.Round(-Assets.textures.UI.volume.GetHeight()/ 2.5f));
            pos += new Vector2(bounds.Width, float.Round(-Assets.textures.UI.volume.GetHeight()));
            float updated = SliderBar(ProgressInRange(currentValue, tweak), backStepPosition, ref changed, false, true);
            backStepPosition += new Vector2(310f, float.Round(Assets.textures.UI.volume.GetHeight()/ 2.5f));
            TextureRenderer.RenderText(TypeSafeString(tweak.UpperBound, tweak), backStepPosition, Assets.fonts.cormorant_15, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, false, true);

            if (changed) {
                ValueFromProgress(updated, tweak);
                tweak.Setter.Invoke(null, [updatedValue]);
            }
        }
        return pos.Y-origY;
    }
    private static object updatedValue;




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
    private static string TypeSafeString(object obj, Tweak tweak) {
        return tweak.TweakType switch {
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
    private static float ProgressInRange(object obj, Tweak tweak) {
        return tweak.TweakType switch {
            TweakType.Short => ((short)obj - (short)tweak.LowerBound) / (float)((short)tweak.UpperBound - (short)tweak.LowerBound),
            TweakType.UShort => ((ushort)obj - (ushort)tweak.LowerBound) / (float)((ushort)tweak.UpperBound - (ushort)tweak.LowerBound),
            TweakType.Int => ((int)obj - (int)tweak.LowerBound) / (float)((int)tweak.UpperBound - (int)tweak.LowerBound),
            TweakType.UInt => ((uint)obj - (uint)tweak.LowerBound) / (float)((uint)tweak.UpperBound - (uint)tweak.LowerBound),
            TweakType.Long => ((long)obj - (long)tweak.LowerBound) / (float)((long)tweak.UpperBound - (long)tweak.LowerBound),
            TweakType.ULong => ((ulong)obj - (ulong)tweak.LowerBound) / (float)((ulong)tweak.UpperBound - (ulong)tweak.LowerBound),
            TweakType.SByte => ((sbyte)obj - (sbyte)tweak.LowerBound) / (float)((sbyte)tweak.UpperBound - (sbyte)tweak.LowerBound),
            TweakType.Byte => ((byte)obj - (byte)tweak.LowerBound) / (float)((byte)tweak.UpperBound - (byte)tweak.LowerBound),
            TweakType.Float => ((float)obj - (float)tweak.LowerBound) / (float)((float)tweak.UpperBound - (float)tweak.LowerBound),
            TweakType.Double => (float)(((double)obj - (double)tweak.LowerBound) / ((double)tweak.UpperBound - (double)tweak.LowerBound)),
            TweakType.String => throw new InvalidOperationException(),
            TweakType.Boolean => throw new InvalidOperationException(),
            TweakType.EnumValue => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException()
        };
    }
    private static void ValueFromProgress(float progress, Tweak tweak) {
        switch (tweak.TweakType) {
            case TweakType.Short:
                updatedValue = (short)(((short)tweak.UpperBound - (short)tweak.LowerBound) * progress) + (short)tweak.LowerBound;
                break;
            case TweakType.UShort:
                updatedValue = (ushort)(((ushort)tweak.UpperBound - (ushort)tweak.LowerBound) * progress) + (ushort)tweak.LowerBound;
                break;
            case TweakType.Int:
                updatedValue = (int)(((int)tweak.UpperBound - (int)tweak.LowerBound) * progress) + (int)tweak.LowerBound;
                break;
            case TweakType.UInt:
                updatedValue = (uint)(((uint)tweak.UpperBound - (uint)tweak.LowerBound) * progress) + (uint)tweak.LowerBound;
                break;
            case TweakType.Long:
                updatedValue = (long)(((long)tweak.UpperBound - (long)tweak.LowerBound) * progress) + (long)tweak.LowerBound;
                break;
            case TweakType.ULong:
                updatedValue = (ulong)(((ulong)tweak.UpperBound - (ulong)tweak.LowerBound) * progress) + (ulong)tweak.LowerBound;
                break;
            case TweakType.SByte:
                updatedValue = (sbyte)(((sbyte)tweak.UpperBound - (sbyte)tweak.LowerBound) * progress) + (sbyte)tweak.LowerBound;
                break;
            case TweakType.Byte:
                updatedValue = (byte)(((byte)tweak.UpperBound - (byte)tweak.LowerBound) * progress) + (byte)tweak.LowerBound;
                break;
            case TweakType.Float:
                updatedValue = (float)(((float)tweak.UpperBound - (float)tweak.LowerBound) * progress) + (float)tweak.LowerBound;
                break;
            case TweakType.Double:
                updatedValue = (double)(((double)tweak.UpperBound - (double)tweak.LowerBound) * progress) + (double)tweak.LowerBound;
                break;
            case TweakType.Boolean:
            case TweakType.EnumValue:
            case TweakType.String:
            default:
                throw new InvalidOperationException();
        }
    }
}
