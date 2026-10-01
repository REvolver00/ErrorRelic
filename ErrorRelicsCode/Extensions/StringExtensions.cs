using Godot;

namespace ErrorRelics.ErrorRelicsCode.Extensions;

// Utilities for Godot resource paths. res:// is URI-like and must use '/'.
public static class StringExtensions
{
    private static string ResJoin(params string[] parts)
    {
        if (parts.Length == 0) return string.Empty;
        string result = parts[0].Replace('\\', '/').TrimEnd('/');
        for (int i = 1; i < parts.Length; i++)
        {
            string part = parts[i].Replace('\\', '/').Trim('/');
            if (part.Length != 0) result += "/" + part;
        }
        return result;
    }

    public static string ImagePath(this string path)
        => ResJoin(MainFile.ResPath, "images", path);

    public static string CardImagePath(this string path)
    {
        path = ResJoin(MainFile.ResPath, "images", "card_portraits", path);
        if (ResourceLoader.Exists(path)) return path;
        MainFile.Logger.Info("Could not find card image path: " + path);
        return ResJoin(MainFile.ResPath, "images", "card_portraits", "card.png");
    }

    public static string BigCardImagePath(this string path)
    {
        path = ResJoin(MainFile.ResPath, "images", "card_portraits", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        MainFile.Logger.Info("Could not find big card image path: " + path);
        return ResJoin(MainFile.ResPath, "images", "card_portraits", "big", "card.png");
    }

    public static string PowerImagePath(this string path)
    {
        path = ResJoin(MainFile.ResPath, "images", "powers", path);
        if (ResourceLoader.Exists(path)) return path;
        MainFile.Logger.Info("Could not find power image path: " + path);
        return ResJoin(MainFile.ResPath, "images", "powers", "power.png");
    }

    public static string BigPowerImagePath(this string path)
    {
        path = ResJoin(MainFile.ResPath, "images", "powers", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        MainFile.Logger.Info("Could not find big power image path: " + path);
        return ResJoin(MainFile.ResPath, "images", "powers", "big", "power.png");
    }

    public static string RelicImagePath(this string path)
    {
        path = ResJoin(MainFile.ResPath, "images", "relics", path);
        if (ResourceLoader.Exists(path)) return path;
        MainFile.Logger.Info("Could not find relic image path: " + path);
        return ResJoin(MainFile.ResPath, "images", "relics", "relic.png");
    }

    public static string BigRelicImagePath(this string path)
    {
        path = ResJoin(MainFile.ResPath, "images", "relics", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        MainFile.Logger.Info("Could not find big relic image path: " + path);
        return ResJoin(MainFile.ResPath, "images", "relics", "big", "relic.png");
    }

    public static string CharacterUiPath(this string path)
        => ResJoin(MainFile.ResPath, "images", "charui", path);
}
