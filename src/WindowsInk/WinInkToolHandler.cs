using System;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace VoiDPlugins.WindowsInk
{
    [PluginName("Windows Ink Config")]
    public class WinInkToolHandler : ITool
    {
        [BooleanProperty("Enforce click", "Enforce a click behavior after press the pen button.")]
        [DefaultPropertyValue(true)]
        public static bool EnforceClick { get; set; } = true;
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        public bool Initialize()
        {
            return true;
        }
    }
}