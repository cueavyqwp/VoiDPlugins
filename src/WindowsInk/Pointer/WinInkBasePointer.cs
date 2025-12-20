using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using VoiDPlugins.Library.VMulti;
using VoiDPlugins.Library.VMulti.Device;
using VoiDPlugins.Library.VoiD;
using static VoiDPlugins.WindowsInk.WindowsInkConstants;

namespace VoiDPlugins.WindowsInk
{
    public unsafe abstract class WinInkBasePointer : IPressureHandler, ITiltHandler, IEraserHandler, ISynchronousPointer, IPenActionHandler
    {
        private readonly Vector2 _conversionFactor;
        private readonly int _pressureConv;
        private readonly IVirtualScreen _screen;
        protected ThinOSPointer _osPointer;
        protected DigitizerInputReport* RawPointer { get; }
        protected VMultiInstance<DigitizerInputReport> Instance { get; }
        protected SharedStore SharedStore { get; }

        public WinInkBasePointer(string name, TabletReference tabletReference, IVirtualScreen screen)
        {
            _screen = screen;
            _osPointer = new ThinOSPointer(_screen);
            _conversionFactor = new Vector2(32767, 32767) / new Vector2(screen.Width, screen.Height);
            SharedStore = SharedStore.GetStore(tabletReference, STORE_KEY);
            Instance = SharedStore.GetOrUpdate(INSTANCE, createInstance, out var updated);
            RawPointer = Instance.Pointer;

            if (updated)
            {
                SharedStore.SetOrAdd(POINTER, this);
                SharedStore.SetOrAdd(ERASER_STATE, false);
                SharedStore.SetOrAdd(MANUAL_ERASER, false);
                SharedStore.SetOrAdd(TIP_PRESSED, false);
            }

            if (Instance.Extended)
            {
                Log.Write(name, "Using extended VMulti digitizer");
                _pressureConv = 16383;
            }
            else
            {
                _pressureConv = 8191;
            }

            VMultiInstance<DigitizerInputReport> createInstance()
            {
                return new VMultiInstance<DigitizerInputReport>(name, extended =>
                {
                    if (extended)
                        return DigitizerInputReport.Extended();
                    else
                        return DigitizerInputReport.Normal();
                });
            }
        }

        public void SetEraser(bool isEraser)
        {
            if (!SharedStore.Get<bool>(MANUAL_ERASER))
            {
                WindowsInkButtonHandler.EraserStateTransition(SharedStore, Instance, isEraser);
            }
        }

        public void SetPressure(float percentage)
        {
            RawPointer->Pressure = (ushort)(percentage * _pressureConv);
        }

        public void SetTilt(Vector2 tilt)
        {
            RawPointer->XTilt = (byte)tilt.X;
            RawPointer->YTilt = (byte)tilt.Y;
        }

        public void Reset()
        {
            SyncOSCursor();
        }

        public void Flush()
        {
            if (!SharedStore.Get<bool>(TIP_PRESSED))
                SetPressure(0);
            Instance.Write();
        }

        protected Vector2 Convert(Vector2 pos)
        {
            return pos * _conversionFactor;
        }

        private void SyncOSCursor()
        {
            _osPointer.SetPosition(ThinOSPointer.GetCursorPos());
        }
        public void Activate(PenAction action)
        {
            Instance.EnableButtonBit(GetFlag(action));
        }

        public void Deactivate(PenAction action)
        {
            Instance.DisableButtonBit(GetFlag(action));

        }

        private static int GetFlag(PenAction action) => action switch
        {
            PenAction.Tip => (int)WindowsInkButtonFlags.Press,
            PenAction.Eraser => (int)WindowsInkButtonFlags.Eraser,
            _ => (int)WindowsInkButtonFlags.Barrel,
        };
    }
}