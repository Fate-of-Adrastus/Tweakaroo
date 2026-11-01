using System;
using System.Linq;
using System.Reflection;

namespace Tweakaroo;

internal static class TweakRenderer {
    public static RenderTarget OverlayTarget;
    public static Stack<IScreen> overlayScreens = new();
    private static object updatedValue;
    public static void PushOverlayScreen(IScreen screen) { 
        overlayScreens.Push(screen);
        screen.OnOpenOrClose(true);
    }
    public static void PopOverlayScreen() {
        var screen = overlayScreens.Pop();
        screen.OnOpenOrClose(false);
    }

    public static Bounds2 RenderTweaks(float offset) {
        Vector2 position = new(40f, offset - 30f);
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
                case TweakType.EnumValue:
                    delta = RenderEnum(position, tweak);
                    break;
                case TweakType.String:
                    delta = RenderString(position, tweak);
                    break;
                case TweakType.Vector2:
                    delta = RenderVector2(position, tweak);
                    break;
                case TweakType.Vector3:
                    delta = RenderVector3(position, tweak);
                    break;
                case TweakType.Index2:
                    delta = RenderIndex2(position, tweak);
                    break;
                case TweakType.HexIndex:
                    delta = RenderHexIndex(position, tweak);
                    break;
                case TweakType.Color:
                    delta = RenderColor(position, tweak);
                    break;
                default:
                    throw new NotImplementedException();
            }

            position += new Vector2(0, delta.Y -10f);
            width = float.Max(width, delta.X);
        }

        return Bounds2.WithCorners(position, position + new Vector2(width - 30f, offset - position.Y));
    }
    public static Vector2 RenderNumeric(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;
        object currentValue = tweak.Getter.Invoke(null, []);
        float width = 0f;
        if (tweak.UpperBound != null && tweak.LowerBound != null) {

            var bounds = TextureRenderer.RenderText(string.Format("{0}: {1}", tweak.Name, TypeSafeString(currentValue, tweak.TweakType)), pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, tweak.UpperBound, tweak.LowerBound, currentValue, newValue => tweak.Setter.Invoke(null, [newValue]), tweak.TweakType, ref tweak.CurrentlyEdited[0]);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        } else {

            var bounds = TextureRenderer.RenderText(tweak.Name, pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = TextInputButton(pos, currentValue, newValue => TryParseFromString(newValue, tweak), newValue => { if (TryParseFromString(newValue, tweak)) tweak.Setter.Invoke(null, [updatedValue]); } , tweak.TweakType);
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
        bounds = TextInputButton(pos, currentValue, newValue => TryParseFromString(newValue, tweak), newValue => { if (TryParseFromString(newValue, tweak)) tweak.Setter.Invoke(null, [updatedValue]); }, tweak.TweakType);
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
    public static Vector2 RenderEnum(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;
        float width = 0;

        object currentValue = tweak.Getter.Invoke(null, []);
        var bounds = TextureRenderer.RenderText(tweak.Name, pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
        pos += new Vector2(0, -bounds.Height);
        width = float.Max(width, bounds.Width);
        Vector2 vector = Assets.textures.UI.dropdown.size.ToVector2();
        bounds = Bounds2.WithSize(pos- new Vector2(0, vector.Y - 20f), vector);
        if (DropDownListButton(TypeSafeString(currentValue, tweak.TweakType), bounds.BottomLeft, true)) {

            var enumType = tweak.Field.FieldType;
            FieldInfo[] fields = [.. enumType.GetFields().Where(field => !field.Name.Equals("value__"))];
            string[] names = [.. fields.Select(field => field.Name)];
            PushOverlayScreen(new DropDownScreen(pos - new Vector2(0, vector.Y / 2), names, (selected) => {
                tweak.Setter.Invoke(null, [ fields[selected].GetRawConstantValue() ]);
            }));
        }
        pos += new Vector2(0, -bounds.Height);
        width = float.Max(width, bounds.Width);

        return new Vector2(width, pos.Y-origY);
    }
    public static Vector2 RenderVector2(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;
        Vector2 currentValue = (Vector2)tweak.Getter.Invoke(null, []);
        float width = 0f;
        if (tweak.UpperBound != null && tweak.LowerBound != null) {
            object[] upperBound = (object[])tweak.UpperBound;
            object[] lowerBound = (object[])tweak.LowerBound;

            var bounds = TextureRenderer.RenderText(string.Format("{0}: {1}", tweak.Name, TypeSafeString(currentValue, tweak.TweakType)), pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, upperBound[0], lowerBound[0], currentValue.X, newValue => tweak.Setter.Invoke(null, [new Vector2((float)newValue, currentValue.Y)]), TweakType.Float, ref tweak.CurrentlyEdited[0]);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, upperBound[1], lowerBound[1], currentValue.Y, newValue => tweak.Setter.Invoke(null, [new Vector2(currentValue.X, (float)newValue)]), TweakType.Float, ref tweak.CurrentlyEdited[1]);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        } else {

            var bounds = TextureRenderer.RenderText(tweak.Name, pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = TextInputButton(pos, currentValue, newValue => TryParseFromString(newValue, tweak), newValue => { if (TryParseFromString(newValue, tweak)) tweak.Setter.Invoke(null, [updatedValue]); }, tweak.TweakType);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        }
        return new Vector2(width, pos.Y - origY);
    }
    public static Vector2 RenderVector3(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;
        Vector3 currentValue = (Vector3)tweak.Getter.Invoke(null, []);
        float width = 0f;
        if (tweak.UpperBound != null && tweak.LowerBound != null) {
            object[] upperBound = (object[])tweak.UpperBound;
            object[] lowerBound = (object[])tweak.LowerBound;

            var bounds = TextureRenderer.RenderText(string.Format("{0}: {1}", tweak.Name, TypeSafeString(currentValue, tweak.TweakType)), pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, upperBound[0], lowerBound[0], currentValue.X, newValue => tweak.Setter.Invoke(null, [new Vector3((float)newValue, currentValue.Y, currentValue.Z)]), TweakType.Float, ref tweak.CurrentlyEdited[0]);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, upperBound[1], lowerBound[1], currentValue.Y, newValue => tweak.Setter.Invoke(null, [new Vector3(currentValue.X, (float)newValue, currentValue.Z)]), TweakType.Float, ref tweak.CurrentlyEdited[1]);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, upperBound[2], lowerBound[2], currentValue.Z, newValue => tweak.Setter.Invoke(null, [new Vector3(currentValue.X, currentValue.Y, (float)newValue)]), TweakType.Float, ref tweak.CurrentlyEdited[2]);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        } else {

            var bounds = TextureRenderer.RenderText(tweak.Name, pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = TextInputButton(pos, currentValue, newValue => TryParseFromString(newValue, tweak), newValue => { if (TryParseFromString(newValue, tweak)) tweak.Setter.Invoke(null, [updatedValue]); }, tweak.TweakType);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        }
        return new Vector2(width, pos.Y - origY);
    }
    public static Vector2 RenderIndex2(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;
        Index2 currentValue = (Index2)tweak.Getter.Invoke(null, []);
        float width = 0f;
        if (tweak.UpperBound != null && tweak.LowerBound != null) {
            object[] upperBound = (object[])tweak.UpperBound;
            object[] lowerBound = (object[])tweak.LowerBound;

            var bounds = TextureRenderer.RenderText(string.Format("{0}: {1}", tweak.Name, TypeSafeString(currentValue, tweak.TweakType)), pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, upperBound[0], lowerBound[0], currentValue.X, newValue => tweak.Setter.Invoke(null, [new Index2((int)newValue, currentValue.Y)]), TweakType.Int, ref tweak.CurrentlyEdited[0]);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, upperBound[1], lowerBound[1], currentValue.Y, newValue => tweak.Setter.Invoke(null, [new Index2(currentValue.X, (int)newValue)]), TweakType.Int, ref tweak.CurrentlyEdited[1]);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        } else {

            var bounds = TextureRenderer.RenderText(tweak.Name, pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = TextInputButton(pos, currentValue, newValue => TryParseFromString(newValue, tweak), newValue => { if (TryParseFromString(newValue, tweak)) tweak.Setter.Invoke(null, [updatedValue]); }, tweak.TweakType);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        }
        return new Vector2(width, pos.Y - origY);
    }
    public static Vector2 RenderHexIndex(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;
        HexIndex currentValue = (HexIndex)tweak.Getter.Invoke(null, []);
        float width = 0f;
        if (tweak.UpperBound != null && tweak.LowerBound != null) {
            object[] upperBound = (object[])tweak.UpperBound;
            object[] lowerBound = (object[])tweak.LowerBound;

            var bounds = TextureRenderer.RenderText(string.Format("{0}: {1}", tweak.Name, TypeSafeString(currentValue, tweak.TweakType)), pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, upperBound[0], lowerBound[0], currentValue.Q, newValue => tweak.Setter.Invoke(null, [new HexIndex((int)newValue, currentValue.R)]), TweakType.Int, ref tweak.CurrentlyEdited[0]);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = SliderBar(pos, upperBound[1], lowerBound[1], currentValue.R, newValue => tweak.Setter.Invoke(null, [new HexIndex(currentValue.Q, (int)newValue)]), TweakType.Int, ref tweak.CurrentlyEdited[1]);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        } else {

            var bounds = TextureRenderer.RenderText(tweak.Name, pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
            pos += new Vector2(0, -bounds.Height);
            width = float.Max(width, bounds.Width);
            bounds = TextInputButton(pos, currentValue, newValue => TryParseFromString(newValue, tweak), newValue => { if (TryParseFromString(newValue, tweak)) tweak.Setter.Invoke(null, [updatedValue]); }, tweak.TweakType);
            pos += new Vector2(0, bounds.Height);
            width = float.Max(width, bounds.Width);
        }
        return new Vector2(width, pos.Y - origY);
    }
    public static Vector2 RenderColor(Vector2 pos, Tweak tweak) {
        float origY = pos.Y;
        Color currentValue = (Color)tweak.Getter.Invoke(null, []);
        float width = 0f;

        var bounds = TextureRenderer.RenderText(string.Format("{0}: {1}", tweak.Name, TypeSafeString(currentValue, tweak.TweakType)), pos, Assets.fonts.cormorant_18, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, true, true);
        pos += new Vector2(0, -bounds.Height);
        width = float.Max(width, bounds.Width);
        bounds = SliderBar(pos, 1f, 0f, currentValue.R, newValue => tweak.Setter.Invoke(null, [new Color((float)newValue, currentValue.G, currentValue.B, currentValue.A)]), TweakType.Float, ref tweak.CurrentlyEdited[0]);
        pos += new Vector2(0, bounds.Height);
        width = float.Max(width, bounds.Width);
        bounds = SliderBar(pos, 1f, 0f, currentValue.G, newValue => tweak.Setter.Invoke(null, [new Color(currentValue.R, (float)newValue, currentValue.B, currentValue.A)]), TweakType.Float, ref tweak.CurrentlyEdited[1]);
        pos += new Vector2(0, bounds.Height);
        width = float.Max(width, bounds.Width);
        bounds = SliderBar(pos, 1f, 0f, currentValue.B, newValue => tweak.Setter.Invoke(null, [new Color(currentValue.R, currentValue.G, (float)newValue, currentValue.A)]), TweakType.Float, ref tweak.CurrentlyEdited[2]);
        pos += new Vector2(0, bounds.Height);
        width = float.Max(width, bounds.Width);
        bounds = SliderBar(pos, 1f, 0f, currentValue.A, newValue => tweak.Setter.Invoke(null, [new Color(currentValue.R, currentValue.G, currentValue.B, (float)newValue)]), TweakType.Float, ref tweak.CurrentlyEdited[3]);
        pos += new Vector2(0, bounds.Height);
        width = float.Max(width, bounds.Width);

        return new Vector2(width, pos.Y - origY);
    }


    private static Bounds2 TextInputButton(Vector2 pos, object currentValue, Predicate<string> checkValid, Action<string> updateValue, TweakType type) {
        string current = TypeSafeString(currentValue, type);
        Bounds2 textBounds = TextureRenderer.RenderText(Utils.FormatBold(StringExtension.TranslateToUpper(current), true), Vector2.Zero, Assets.fonts.crimson_13, Color.White, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, -1, Color.White, null, int.MaxValue, calculateBounds: true, doRender: false);
        float width = 310f;
        bool isTooLong = textBounds.Width > width;
        Bounds2 bounds = Bounds2.WithSize(pos, new Vector2(width + 20, 46));

        if (UIUtils.TextButton(isTooLong ? "..." : current, pos - new Vector2(0, bounds.Height / 2f), (int)float.Round(width), (int)float.Round(bounds.Height)).RenderAndCheckIfPressed(true, true)) {
            PushOverlayScreen(new StringInputScreen(Bounds2.WithSize(Vector2.Zero, InputManager.screenSize), current, updateValue, checkValid));
            Assets.sounds.click_button.PlaySound(1f);
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
    private static bool DropDownListButton(string text, Vector2 pos, bool canHoverAndClick) {
        Vector2 vector = Assets.textures.UI.dropdown.size.ToVector2();
        Bounds2 bounds = Bounds2.WithSize(pos, vector);
        TextureRenderer.RenderText(text, bounds.Center + new Vector2(0f, -5f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default, null, int.MaxValue, false, true);
        if (bounds.Contains(InputManager.MousePos()) && canHoverAndClick) {
            TextureRenderer.Render(Assets.textures.UI.dropdown_hover, pos);
            if (InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
                return true;
            }
        } else {
            TextureRenderer.Render(Assets.textures.UI.dropdown, pos);
        }
        return false;
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
            TweakType.Vector2 => ((Vector2)obj).ToString().Trim('(', ')'),
            TweakType.Vector3 => ((Vector3)obj).ToString().Trim('(', ')'),
            TweakType.Index2 => ((Index2)obj).ToString().Trim('(', ')'),
            TweakType.HexIndex => ((HexIndex)obj).Q.ToString() + ", " + ((HexIndex)obj).R.ToString(),
            TweakType.Color => ((Color)obj).Packed().r.ToString() + ", " + ((Color)obj).Packed().g.ToString()
                + ", " + ((Color)obj).Packed().b.ToString() + ", " + ((Color)obj).Packed().a.ToString(),
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
            case TweakType.Vector2:
                success = false;
                var parts = str.Split(", ");
                if (parts.Length == 2 && float.TryParse(parts[0], out var x) && float.TryParse(parts[1], out var y)) {
                    success = true;
                    updatedValue = new Vector2(x, y);
                }
                break;
            case TweakType.Vector3:
                success = false;
                parts = str.Split(", ");
                if (parts.Length == 3 && float.TryParse(parts[0], out x) && float.TryParse(parts[1], out y) && float.TryParse(parts[2], out var z)) {
                    success = true;
                    updatedValue = new Vector3(x, y, z);
                }
                break;
            case TweakType.Index2:
            case TweakType.HexIndex:
                success = false;
                parts = str.Split(", ");
                if (parts.Length == 2 && int.TryParse(parts[0], out var iX) && int.TryParse(parts[1], out var iY)) {
                    success = true;
                    if (tweak.TweakType == TweakType.Index2)
                        updatedValue = new Index2(iX, iY);
                    else
                        updatedValue = new HexIndex(iX, iY);
                }
                break;
            case TweakType.Color:
                throw new InvalidOperationException();
            default:
                throw new NotImplementedException();
        }
        return success;
    }
}
