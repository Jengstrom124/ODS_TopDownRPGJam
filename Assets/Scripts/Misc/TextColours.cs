using UnityEngine;

public enum TextColour
{
    Green,
    Yellow,
    Red,
    Pink,
    Aqua
}
public static class TextColours
{
    public static string stopColour = "</color>";
    public static string green = "<color=#00E700>";
    public static string yellow = "<color=#FFE134>";
    public static string red = "<color=#FF0000>";
    public static string pink = "<color=#FF42DE>";
    public static string aqua = "<color=#34FFFF>";

    public static string GetColour(TextColour colour_)
    {
        if (colour_ == TextColour.Green) return green;
        else if (colour_ == TextColour.Yellow) return yellow;
        else if (colour_ == TextColour.Red) return red;
        else if (colour_ == TextColour.Pink) return pink;
        else if (colour_ == TextColour.Aqua) return aqua;
        else return yellow;
    }
}
