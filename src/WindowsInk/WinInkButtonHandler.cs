using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using VoiDPlugins.Library.VMulti;
using VoiDPlugins.Library.VMulti.Device;
using VoiDPlugins.Library.VoiD;
using static VoiDPlugins.OutputMode.WindowsInkConstants;

namespace VoiDPlugins.OutputMode
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
            "Eraser (Toggle)",
            "Eraser (Hold)"
        ];

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string? Button { get; set; }

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
                Log.WriteNotify("WinInk",
                          "Windows Ink bindings are being used without an active Windows Ink output mode.",
                          LogLevel.Error);
            }
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (_instance == null)
                return;

            var eraserState = _sharedStore.Get<bool>(ERASER_STATE);

            // Pen Behavior
            // Enforce a click behavior when either the Pen Tip or Pen Button is pressed.
            // This is particularly useful for the Pen Button, enabling right-click functionality while hovering.
            // The default Windows Ink behavior is unintuitive:
            //   - First, the user must press the Pen Button.
            //   - Then, press the Pen Tip.
            // This sequence is unacceptable in certain 3D applications where precise and immediate input is required.

            if (Button != null && Button.Contains("Pen"))
            {
                _sharedStore.Set(TIP_PRESSED, true);
                _instance.EnableButtonBit((int)(eraserState ? WindowsInkButtonFlags.Eraser : WindowsInkButtonFlags.Press));
            }

            switch (Button)
            {
                case "Pen Button":
                    _instance.EnableButtonBit((int)WindowsInkButtonFlags.Barrel);
                    break;

                case "Eraser (Toggle)":
                    _sharedStore.Set(MANUAL_ERASER, !eraserState);
                    EraserStateTransition(_sharedStore, _instance, !eraserState);
                    break;

                case "Eraser (Hold)":
                    _sharedStore.Set(MANUAL_ERASER, true);
                    EraserStateTransition(_sharedStore, _instance, true);
                    break;
            }
            _instance.Write();
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (_instance == null)
                return;

            // Refer to the comment in the Press() method for details on enforcing consistent behavior
            // between Pen Tip and Pen Button inputs.
            if (Button != null && Button.Contains("Pen"))
            {
                _sharedStore.Set(TIP_PRESSED, false);
                _instance.DisableButtonBit((int)(WindowsInkButtonFlags.Press | WindowsInkButtonFlags.Eraser));
            }

            switch (Button)
            {
                case "Pen Button":
                    _instance.DisableButtonBit((int)WindowsInkButtonFlags.Barrel);
                    break;

                case "Eraser (Hold)":
                    _sharedStore.Set(MANUAL_ERASER, false);
                    EraserStateTransition(_sharedStore, _instance, false);
                    break;
            }
            _instance.Write();
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
    }
}