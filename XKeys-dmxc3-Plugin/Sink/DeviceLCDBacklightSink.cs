using LumosLIB.Kernel;
using LumosLIB.Tools;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using XKeysSharp.Devices;

namespace XKeys_dmxc3_Plugin.Sink
{
    public class DeviceLCDBacklightSink : AbstractInputSink
    {
        public readonly string SerialNumber;

        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Bool;
        public override object Min => false;
        public override object Max => true;

        internal static Dictionary<string, bool> backlightCache = new Dictionary<string, bool>();

        public DeviceLCDBacklightSink(in string serialNumber) :
            base(getID(serialNumber), getDisplayName(), getCategory(serialNumber))

        {
            SerialNumber = serialNumber;
        }

        private static string getID(string serialNumber)
        {
            return $"XKeys-{serialNumber}-Set-LCDBacklight";
        }

        private static string getDisplayName()
        {
            return $"LCD Backlight";
        }

        private static ParameterCategory getCategory(string serialNumber)
        {
            var device = getDevice(serialNumber);
            return ParameterCategoryTools.FromNames("XKeys", device?.Name, serialNumber);
        }
        private static IDevice? getDevice(string serialNumber)
        {
            return XKeysPlugin.Devices.FirstOrDefault(d => d.SerialNumber?.Equals(serialNumber) ?? false);
        }

        public override bool UpdateValue(object newValue)
        {
            try
            {
                if (LumosTools.TryConvertToBool(newValue, out var b))
                {
                    var device = getDevice(SerialNumber);
                    if (device is IDeviceWithLCD deviceLCD)
                        deviceLCD.SetLCDBottomText(DeviceLCDTextSink.bottomRowCache[SerialNumber] ?? string.Empty, b);
                }
            }
            catch (Exception ex)
            {
                XKeysPlugin.Log.ErrorOrDebug(ex);
            }

            return false;
        }
    }
}