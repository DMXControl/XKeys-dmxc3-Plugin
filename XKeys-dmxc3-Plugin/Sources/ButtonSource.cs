using LumosLIB.Kernel;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using XKeysSharp;
using XKeysSharp.Devices;

namespace XKeys_dmxc3_Plugin.Sources
{
    public class ButtonSource : AbstractInputSource
    {
        public readonly string SerialNumber;
        public readonly uint ButtonID;
        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Bool;

        public override object Min => false;

        public override object Max => true;
        public ButtonSource(in string serialNumber,in  uint buttonID) : base(getID(serialNumber, buttonID), getDisplayName(buttonID), getCategory(serialNumber), false)
        {
            SerialNumber = serialNumber;
            ButtonID = buttonID;
            var button = getButton(SerialNumber, ButtonID);
            if (button != null)
                button.PropertyChanged += Button_PropertyChanged;
        }

        private void Button_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case nameof(IButton.ButtonState):
                        CurrentValue = getButton(SerialNumber, ButtonID)?.ButtonState == EButtonState.Down;
                        break;
                }
            }
            catch (Exception ex)
            {
                XKeysPlugin.Log.ErrorOrDebug(ex);
            }
        }
        private static IDevice? getDevice(string serialNumber)
        {
            return XKeysPlugin.Devices.FirstOrDefault(d => d.SerialNumber?.Equals(serialNumber) ?? false);
        }
        private static IButton? getButton(string serialNumber, uint buttonID)
        {
            var device = getDevice(serialNumber) as IDeviceWithButtons<IButton>;
            return device?.Buttons?.FirstOrDefault(b => b.Number.Equals(buttonID));
        }

        private static string getID(string serialNumber, uint buttonID)
        {
            return $"XKeys-{serialNumber}-Button:{buttonID}";
        }
        private static string getDisplayName(uint buttonID)
        {
            return $"Button: {buttonID}";
        }

        private static ParameterCategory getCategory(string serialNumber)
        {
            var device = getDevice(serialNumber);
            return ParameterCategoryTools.FromNames("XKeys", device?.Name, serialNumber);
        }
    }
}