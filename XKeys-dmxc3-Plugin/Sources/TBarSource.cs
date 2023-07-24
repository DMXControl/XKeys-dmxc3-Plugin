using LumosLIB.Kernel;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using XKeysSharp.Devices;

namespace XKeys_dmxc3_Plugin.Sources
{
    public class TBarSource : AbstractInputSource
    {
        public readonly string SerialNumber;
        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Numeric;
        public override object Min => 0.0;
        public override object Max => 1.0;

        public TBarSource(in string serialNumber) : base(getID(serialNumber), getDisplayName(), getCategory(serialNumber), false)
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
                    case nameof(IDeviceWithTBar.TBarPosition):
                        CurrentValue = getDevice(SerialNumber)?.TBarPosition;
                        break;
                }
            }
            catch (Exception ex)
            {
                XKeysPlugin.Log.ErrorOrDebug(ex);
            }
        }
        private static IDeviceWithTBar? getDevice(string serialNumber)
        {
            return XKeysPlugin.Devices.FirstOrDefault(d => d.SerialNumber?.Equals(serialNumber) ?? false) as IDeviceWithTBar;
        }

        private static string getID(string serialNumber)
        {
            return $"XKeys-{serialNumber}-TBar";
        }
        private static string getDisplayName()
        {
            return $"T-Bar";
        }

        private static ParameterCategory getCategory(string serialNumber)
        {
            var device = getDevice(serialNumber);
            return ParameterCategoryTools.FromNames("XKeys", device?.Name, serialNumber);
        }
    }
}