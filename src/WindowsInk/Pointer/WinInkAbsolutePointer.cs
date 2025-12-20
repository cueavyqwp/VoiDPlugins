using System.Numerics;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace VoiDPlugins.WindowsInk
{
    public unsafe class WinInkAbsolutePointer(TabletReference tabletReference, IVirtualScreen screen) : WinInkBasePointer("Windows Ink", tabletReference, screen), IAbsolutePointer
    {
        private Vector2 _prev;

        public void SetPosition(Vector2 pos)
        {
            if (pos == _prev)
                return;

            SetInternalPosition(pos);
            Instance.EnableButtonBit((int)WindowsInkButtonFlags.InRange);
            pos = Convert(pos);
            RawPointer->X = (ushort)pos.X;
            RawPointer->Y = (ushort)pos.Y;
            _prev = pos;
        }
    }
}