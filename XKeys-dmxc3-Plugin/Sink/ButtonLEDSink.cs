using LumosLIB.Kernel;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using org.dmxc.lumos.Kernel.PropertyType;
using XKeysSharp;
using XKeysSharp.Devices;

namespace XKeys_dmxc3_Plugin.Sink
{
    public class ButtonLEDSink : AbstractInputSink
    {
        public readonly string SerialNumber;
        public readonly string ColorOfLED;
        public readonly uint ButtonID;

        public override EWellKnownInputType AutoGraphIOType
        {
            get
            {
                if ("RGB".Equals(ColorOfLED))
                    return EWellKnownInputType.Color;
                return EWellKnownInputType.Bool;
            }
        }
        public override EWellKnownInputType[] AdditionallySupportedIOTypes => new EWellKnownInputType[] { EWellKnownInputType.String };
        public override object Min
        {
            get
            {
                if ("RGB".Equals(ColorOfLED))
                    return LumosColor.Black;
                return false;
            }
        }
        public override object Max
        {
            get
            {
                if ("RGB".Equals(ColorOfLED))
                    return LumosColor.White;
                return true;
            }
        }

        public ButtonLEDSink(in string serialNumber, in uint buttonID, in string colorOfLED) :
            base(getID(serialNumber, colorOfLED, buttonID), getDisplayName(colorOfLED, buttonID), getCategory(serialNumber))

        {
            SerialNumber = serialNumber;
            ColorOfLED = colorOfLED;
            ButtonID = buttonID;
        }

        private static string getID(string serialNumber, string colorOfLED, in uint buttonID)
        {
            return $"XKeys-{serialNumber}-Button{buttonID}-Set{colorOfLED}LED";
        }

        private static string getDisplayName(string colorOfLED, in uint buttonID)
        {
            return $"Button {buttonID} {colorOfLED} LED";
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
        private static IButton? getButton(string serialNumber, uint buttonID)
        {
            var device = getDevice(serialNumber) as IDeviceWithButtons<IButton>;
            return device?.Buttons?.FirstOrDefault(b => b.Number.Equals(buttonID));
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
                    var button = getButton(SerialNumber,ButtonID);
                    switch (ColorOfLED)
                    {
                        case "Blue":
                            if (button is IButtonWithBlueLED blueLED)
                                blueLED.SetBlueLEDState(ledState);
                            return true;
                        case "Red":
                            if (button is IButtonWithRedLED redLED)
                                redLED.SetRedLEDState(ledState);
                            return true;

                        case "RGB":
                            if (button is IButtonWithRGBLED rgbLED) ;
                                //rgbLED.SetRedLEDState(ledState);
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