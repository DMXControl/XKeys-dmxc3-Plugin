using LumosLIB.Kernel;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using XKeysSharp.Devices;

namespace XKeys_dmxc3_Plugin.Sink
{
    public class DeviceLCDTextSink : AbstractInputSink
    {
        public readonly string SerialNumber;
        public readonly byte Row;

        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.String;
        public override object Min => string.Empty;
        public override object Max => string.Empty;

        internal static Dictionary<string, string> topRowCache = new Dictionary<string, string>();
        internal static Dictionary<string, string> bottomRowCache = new Dictionary<string, string>();

        public DeviceLCDTextSink(in string serialNumber, in byte row) :
            base(getID(serialNumber, row), getDisplayName(row), getCategory(serialNumber))

        {
            SerialNumber = serialNumber;
            Row = row;
        }

        private static string getID(string serialNumber, byte row)
        {
            return $"XKeys-{serialNumber}-Set-Row-{row}-LCDText";
        }

        private static string getDisplayName(byte row)
        {
            switch (row)
            {
                case 0:
                    return $"LCD 1st Row Text";
                case 1:
                    return $"LCD 2nd Row Text";
                case 2:
                    return $"LCD 3rd Row Text";
                default:
                    return $"LCD {row + 1}th Row Text";
            }
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
                string str = string.Empty;

                if (newValue is string strg)
                    str = strg;
                else if (newValue != null)
                    str = newValue.ToString();

                if (str == null)
                    return false;
                var device = getDevice(SerialNumber);
                if (device is IDeviceWithLCD deviceLCD)
                    switch (Row)
                    {
                        case 0:
                            deviceLCD.SetLCDTopText(str, true);
                            topRowCache[SerialNumber] = str;
                            return true;
                        case 1:
                            deviceLCD.SetLCDBottomText(str, true);
                            bottomRowCache[SerialNumber] = str;
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