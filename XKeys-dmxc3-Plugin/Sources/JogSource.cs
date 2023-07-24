using LumosLIB.Kernel;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using XKeysSharp.Devices;

namespace XKeys_dmxc3_Plugin.Sources
{
    public class JogSource : AbstractInputSource
    {
        public readonly string SerialNumber;
        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Beat;
        public override object Min => long.MinValue;
        public override object Max => long.MaxValue;

        public JogSource(in string serialNumber) : base(getID(serialNumber), getDisplayName(), getCategory(serialNumber), false)
        {
            SerialNumber = serialNumber;
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
                    case nameof(IDeviceWithJogShuttle.Jog):
                        CurrentValue = getDevice(SerialNumber)?.Jog;
                        break;
                }
            }
            catch (Exception ex)
            {
                XKeysPlugin.Log.ErrorOrDebug(ex);
            }
        }
        private static IDeviceWithJogShuttle? getDevice(string serialNumber)
        {
            return XKeysPlugin.Devices.FirstOrDefault(d => d.SerialNumber?.Equals(serialNumber) ?? false) as IDeviceWithJogShuttle;
        }

        private static string getID(string serialNumber)
        {
            return $"XKeys-{serialNumber}-Jog";
        }
        private static string getDisplayName()
        {
            return $"Jog";
        }

        private static ParameterCategory getCategory(string serialNumber)
        {
            var device = getDevice(serialNumber);
            return ParameterCategoryTools.FromNames("XKeys", device?.Name, serialNumber);
        }
    }
}