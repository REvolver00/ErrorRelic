using System;
using ErrorRelics.ErrorRelicsCode.Relics;

namespace ErrorRelics.ErrorRelicsCode.Visuals;

internal static class ErrorGlitchName
{
    private static readonly char[] Glyphs =
        ("龘靐齉鱻麤龗爨饢驫"
       + "あいうえおかきくけこさしすせそたちつてとなにぬねの"
       + "アイウエオカキクケコサシスセソタチツテトナニヌネノ"
       + "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩαβγδεζηθικλμνξοπρστυφχψω"
       + "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЭЮЯабвгдежзийклмнопрстуфхцчшщэюя"
       + "가나다라마바사아자차카타파하"
       + "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
       + "▀▄█▌▐░▒▓╔╗╚╝╠╣╦╩╬═║←↑→↓↔↕↖↗↘↙∑∏√∞±×÷≈≠≤≥◆◇○●□■△▲▽▼").ToCharArray();

    public static string For(ErrorRandomTestRelic relic)
    {
        var rng = new Random(StableHash(
            relic.VisualSourceIconPath + "|" +
            relic.GeneratedHookId + "|" +
            relic.GeneratedEffectId + "|title"));
        int length = rng.Next(3, 7);
        Span<char> chars = stackalloc char[length];
        for (int i = 0; i < length; i++)
            chars[i] = Glyphs[rng.Next(Glyphs.Length)];
        return new string(chars);
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            uint h = 2166136261u;
            foreach (char c in value) { h ^= c; h *= 16777619u; }
            return (int)h;
        }
    }
}
