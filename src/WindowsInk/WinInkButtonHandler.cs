using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using VoiDPlugins.Library.VMulti;
using VoiDPlugins.Library.VMulti.Device;
using VoiDPlugins.Library.VoiD;
using static VoiDPlugins.WindowsInk.WindowsInkConstants;

namespace VoiDPlugins.WindowsInk
{
    [PluginName("Windows Ink")]
    public unsafe partial class WindowsInkButtonHandler : IStateBinding
    {
        private VMultiInstance _instance = null!;
        private SharedStore _sharedStore = null!;

        public static string[] ValidButtons { get; } =
        [
            "Pen Tip",
            "Pen Button",
            "Eraser"
        ];

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public required string Button { get; set; }

        [TabletReference]
        public TabletReference Reference { set => Initialize(value); }

        private void Initialize(TabletReference tabletReference)
        {
            try
            {
                _sharedStore = SharedStore.GetStore(tabletReference, STORE_KEY);
                _instance = _sharedStore.Get<VMultiInstance>(INSTANCE);
            }
            catch
            {
                Log.WriteNotify("WinInk", "Windows Ink bindings are being used without an active Windows Ink output mode.", LogLevel.Error);
            }
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (_instance == null)
                return;
            SetAction(Button, true);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (_instance == null)
                return;
            SetAction(Button, false);
        }
        public void Activate(PenAction action)
        {
            SetAction(action, true);
        }
        public void Deactivate(PenAction action)
        {
            SetAction(action, false);
        }
        private void SetAction(PenAction action, bool isActive = true)
        {
            SetAction(_sharedStore, _instance, action, isActive);
        }
        public static void SetAction(SharedStore store, VMultiInstance instance, PenAction action, bool isActive = true)
        {
            bool eraserState = store.Get<bool>(ERASER_STATE);
            bool update = true;
            int flags = 0;
            switch (action)
            {
                case PenAction.Tip:
                    store.Set(TIP_PRESSED, isActive);
                    flags = (int)(eraserState ? WindowsInkButtonFlags.Eraser : WindowsInkButtonFlags.Press);
                    break;
                case PenAction.Eraser:
                    if (!WinInkToolHandler.ToggleEraser)
                    {
                        store.Set(MANUAL_ERASER, isActive);
                        EraserStateTransition(store, instance, isActive);
                    }
                    else if (isActive)
                    {
                        store.Set(MANUAL_ERASER, !eraserState);
                        EraserStateTransition(store, instance, !eraserState);
                    }
                    update = false;
                    break;
                default:
                    if (WinInkToolHandler.EnforceClick)
                    {
                        store.Set(TIP_PRESSED, isActive);
                        flags = (int)((eraserState ? WindowsInkButtonFlags.Eraser : WindowsInkButtonFlags.Press) | WindowsInkButtonFlags.Barrel);
                    }
                    else
                        flags = (int)WindowsInkButtonFlags.Barrel;
                    break;
            }
            if (update)
            {
                if (isActive)
                    instance.EnableButtonBit(flags);
                else
                    instance.DisableButtonBit(flags);
            }
            instance.Write();
        }
        public void SetAction(string button, bool isActive = true)
        {
            SetAction(button switch
            {
                "Pen Tip" => PenAction.Tip,
                "Eraser" => PenAction.Eraser,
                _ => PenAction.BarrelButton1,
            }, isActive);
        }
        internal static void EraserStateTransition(SharedStore store, VMultiInstance instance, bool isEraser)
        {
            var eraserState = store.Get<bool>(ERASER_STATE);
            if (eraserState != isEraser)
            {
                store.Set(ERASER_STATE, isEraser);
                eraserState = isEraser;
                var report = (DigitizerInputReport*)instance.Header;
                var buttons = report->Header.Buttons;
                var pressure = report->Pressure;

                // Send In-Range but no tips
                instance.DisableButtonBit((int)(WindowsInkButtonFlags.Press | WindowsInkButtonFlags.Eraser));
                report->Pressure = 0;
                instance.Write();

                // Send Out-Of-Range
                report->Header.Buttons = 0;
                instance.Write();

                // Send In-Range but no tips
                instance.EnableButtonBit((int)WindowsInkButtonFlags.InRange);
                if (eraserState)
                    instance.EnableButtonBit((int)WindowsInkButtonFlags.Invert);

                instance.Write();

                // Set Proper Report
                if (VMultiInstance.HasBit(buttons, (int)(WindowsInkButtonFlags.Press | WindowsInkButtonFlags.Eraser)))
                    instance.EnableButtonBit((int)(eraserState ? WindowsInkButtonFlags.Eraser : WindowsInkButtonFlags.Press));
                report->Pressure = pressure;
            }
        }
        public override string ToString() => $"Windows Ink: {Button}";
    }
}