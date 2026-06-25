using YouTubeMusicAPI.Json;

namespace YouTubeMusicAPI.Utils;

internal static class JElementExtensions
{
    extension(JElement element)
    {
        public JElement GetRuns() => element.Get("runs");

        public JElement GetFirstRun() => element.GetRuns().GetAt(0);

        public JElement GetText() => element.Get("text");

        public JElement GetMultiSelectMenu() => element.Get("musicMultiSelectMenuRenderer");

        public JElement GetMultiSelectMenuItem() => element.Get("musicMultiSelectMenuItemRenderer");
    }
}
