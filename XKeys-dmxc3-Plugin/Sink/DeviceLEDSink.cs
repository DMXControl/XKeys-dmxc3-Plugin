using LumosLIB.Kernel;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using XKeysSharp;
using XKeysSharp.Devices;

namespace XKeys_dmxc3_Plugin.Sink
{
    public class DeviceLEDSink : AbstractInputSink
    {
        public readonly string SerialNumber;
        public readonly string ColorOfLED;

        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Bool;
        public override EWellKnownInputType[] AdditionallySupportedIOTypes => new EWellKnownInputType[] { EWellKnownInputType.String };
        public override object Min => false;
        public override object Max => true;

        public DeviceLEDSink(in string serialNumber, in string colorOfLED) :
            base(getID(serialNumber, colorOfLED), getDisplayName(colorOfLED), getCategory(serialNumber))

        {
            SerialNumber = serialNumber;
            ColorOfLED = colorOfLED;
        }

        private static string getID(string serialNumber, string colorOfLED)
        {
            return $"XKeys-{serialNumber}-Set{colorOfLED}LED";
        }

        private static string getDisplayName(string colorOfLED)
        {
            return $"{colorOfLED} LED";
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
                if (newValue is string str)
                    newValue = (ELEDState)Enum.Parse(typeof(ELEDState), str);
                else if (newValue is bool b)
                    newValue = b ? ELEDState.ON : ELEDState.OFF;
            }
            catch (Exception ex)
            {
                XKeysPlugin.Log.ErrorOrDebug(ex);
            }

            try
            {
                if (newValue is ELEDState ledState)
                {
                    var device = getDevice(SerialNumber);
                    switch (ColorOfLED)
                    {
                        case "Green":
                            if (device is IDeviceWithGreenLED greenLED)
                                greenLED.SetGreenLEDState(ledState);
                            return true;
                        case "Red":
                            if (device is IDeviceWithRedLED redLED)
                                redLED.SetRedLEDState(ledState);
                            return true;
                    }
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