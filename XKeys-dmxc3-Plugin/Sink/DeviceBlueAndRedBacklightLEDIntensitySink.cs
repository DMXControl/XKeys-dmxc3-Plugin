using LumosLIB.Kernel;
using LumosLIB.Tools;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using System.Linq;
using XKeysSharp.Devices;

namespace XKeys_dmxc3_Plugin.Sink
{
    public class DeviceBlueAndRedBacklightLEDIntensitySink : AbstractInputSink
    {
        public readonly string SerialNumber;
        public readonly string ColorOfLED;
        internal static Dictionary<string, byte> blueCache = new Dictionary<string, byte>();
        internal static Dictionary<string, byte> redCache = new Dictionary<string, byte>();

        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Numeric;
        public override object Min => 0.0;
        public override object Max => 1.0;

        public DeviceBlueAndRedBacklightLEDIntensitySink(in string serialNumber, in string colorOfLED) :
            base(getID(serialNumber, colorOfLED), getDisplayName(colorOfLED), getCategory(serialNumber))

        {
            SerialNumber = serialNumber;
            ColorOfLED = colorOfLED;
        }

        private static string getID(string serialNumber, string colorOfLED)
        {
            return $"XKeys-{serialNumber}-Set-{colorOfLED}BackgroundLEDIntensity";
        }

        private static string getDisplayName(string colorOfLED)
        {
            return $"{colorOfLED} LEDs Intensity";
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
                    if (!blueCache.ContainsKey(SerialNumber))
                        blueCache[SerialNumber] = 0;
                    if (!redCache.ContainsKey(SerialNumber))
                        redCache[SerialNumber] = 0;
                    switch (ColorOfLED)
                    {
                        case "Blue":
                            blueCache[SerialNumber] = (byte)LumosTools.RuleOfThree(value, 1, 0, byte.MaxValue, byte.MinValue, LumosTools.ERuleOfThreeBehavior.TRIM);
                            break;
                        case "Red":
                            redCache[SerialNumber] = (byte)LumosTools.RuleOfThree(value, 1, 0, byte.MaxValue, byte.MinValue, LumosTools.ERuleOfThreeBehavior.TRIM);
                            break;
                    }
                    var device = getDevice(SerialNumber);
                    if (device is IDeviceWithBlueAndRedBacklightLEDs backlight)
                        backlight.SetBacklightIntensity(blueCache[SerialNumber], redCache[SerialNumber]);
                    return true;
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