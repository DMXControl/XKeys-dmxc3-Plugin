using LumosLIB.Kernel;
using LumosLIB.Tools;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using XKeysSharp.Devices;
 
namespace XKeys_dmxc3_Plugin.Sink
{
    public class DeviceBlueBacklightLEDIntensitySink : AbstractInputSink
    {
        public readonly string SerialNumber;

        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Numeric;
        public override object Min => 0.0;
        public override object Max => 1.0;

        public DeviceBlueBacklightLEDIntensitySink(in string serialNumber) :
            base(getID(serialNumber), getDisplayName(), getCategory(serialNumber))

        {
            SerialNumber = serialNumber;
        }

        private static string getID(string serialNumber)
        {
            return $"XKeys-{serialNumber}-Set-BackgroundLEDIntensity";
        }

        private static string getDisplayName()
        {
            return $"Blue LEDs Intensity";
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
                if (LumosTools.TryConvertToDouble(newValue, out var value))
                {

                    var device = getDevice(SerialNumber);
                    if (device is IDeviceWithBlueBacklightLEDs blueBacklight)
                        blueBacklight.SetBacklightIntensity((byte)LumosTools.RuleOfThree(value, 1, 0, byte.MaxValue, byte.MinValue, LumosTools.ERuleOfThreeBehavior.TRIM));
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