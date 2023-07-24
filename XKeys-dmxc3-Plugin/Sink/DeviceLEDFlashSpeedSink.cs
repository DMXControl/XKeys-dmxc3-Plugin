using LumosLIB.Kernel;
using LumosLIB.Tools;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using XKeysSharp.Devices;

namespace XKeys_dmxc3_Plugin.Sink
{
    public class DeviceLEDFlashSpeedSink : AbstractInputSink
    {
        public readonly string SerialNumber;

        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Numeric;
        public override object Min => 0.0;
        public override object Max => 1.0;

        public DeviceLEDFlashSpeedSink(in string serialNumber) :
            base(getID(serialNumber), getDisplayName(), getCategory(serialNumber))

        {
            SerialNumber = serialNumber;
        }

        private static string getID(string serialNumber)
        {
            return $"XKeys-{serialNumber}-Set-LEDFlashSpeed";
        }

        private static string getDisplayName()
        {
            return $"LED Flash Speed";
        }

        private static ParameterCategory getCategory(string serialNumber)
        {
            var device = getDevice(serialNumber);
            return ParameterCategoryTools.FromNames("XKeys", device?.Name, serialNumber);
        }
        private static IDeviceWithFlashingLEDs? getDevice(string serialNumber)
        {
            return XKeysPlugin.Devices.FirstOrDefault(d => d.SerialNumber?.Equals(serialNumber) ?? false) as IDeviceWithFlashingLEDs;
        }

        public override bool UpdateValue(object newValue)
        {
            try
            {
                if (LumosTools.TryConvertToDouble(newValue, out var value))
                {
                    var device = getDevice(SerialNumber);
                    byte b = (byte)LumosTools.RuleOfThree(value, 1, 0, byte.MaxValue, byte.MinValue, LumosTools.ERuleOfThreeBehavior.TRIM);
                    device?.SetFlashFrequency((byte)(byte.MaxValue - b));
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