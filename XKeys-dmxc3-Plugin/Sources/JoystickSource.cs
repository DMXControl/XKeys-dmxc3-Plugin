using LumosLIB.Kernel;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using XKeysSharp.Devices;

namespace XKeys_dmxc3_Plugin.Sources
{
    public class JoystickSource : AbstractInputSource
    {
        public readonly string SerialNumber;
        public readonly string Part;
        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Numeric;
        public override object Min => 0.0;
        public override object Max => 1.0;

        public JoystickSource(in string serialNumber, in string part) : base(getID(serialNumber, part), getDisplayName(part), getCategory(serialNumber), false)
        {
            SerialNumber = serialNumber;
            Part = part;
            var device = getDevice(SerialNumber);
            if (device != null)
                device.PropertyChanged += Button_PropertyChanged;
        }

        private void Button_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case nameof(IDeviceWithJoystick.JoystickX):
                        if ("X".Equals(Part))
                            CurrentValue = getDevice(SerialNumber)?.JoystickX;
                        break;
                    case nameof(IDeviceWithJoystick.JoystickY):
                        if ("Y".Equals(Part))
                            CurrentValue = getDevice(SerialNumber)?.JoystickY;
                        break;
                    case nameof(IDeviceWithJoystick.JoystickZ):
                        if ("Z".Equals(Part))
                            CurrentValue = getDevice(SerialNumber)?.JoystickZ;
                        break;
                }
            }
            catch (Exception ex)
            {
                XKeysPlugin.Log.ErrorOrDebug(ex);
            }
        }
        private static IDeviceWithJoystick? getDevice(string serialNumber)
        {
            return XKeysPlugin.Devices.FirstOrDefault(d => d.SerialNumber?.Equals(serialNumber) ?? false) as IDeviceWithJoystick;
        }

        private static string getID(string serialNumber, string part)
        {
            return $"XKeys-{serialNumber}-Joystick-{part}";
        }
        private static string getDisplayName(string part)
        {
            return $"Joystick-{part}";
        }

        private static ParameterCategory getCategory(string serialNumber)
        {
            var device = getDevice(serialNumber);
            return ParameterCategoryTools.FromNames("XKeys", device?.Name, serialNumber);
        }
    }
}