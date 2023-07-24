using LumosLIB.Kernel.Log;
using Microsoft.Extensions.Logging;
using org.dmxc.lumos.Kernel.Input.v2;
using org.dmxc.lumos.Kernel.Plugin;
using System.Collections.ObjectModel;
using System.Linq;
using XKeys_dmxc3_Plugin.Plugin.Logging;
using XKeys_dmxc3_Plugin.Sink;
using XKeys_dmxc3_Plugin.Sources;
using XKeysSharp;
using XKeysSharp.Devices;
using XKeys = XKeysSharp.XKeysSharp;

namespace XKeys_dmxc3_Plugin
{
    public class XKeysPlugin : KernelPluginBase
    {
        internal static readonly ILumosLog Log = LumosLogger.getInstance(nameof(XKeysPlugin));
        internal static ObservableCollection<IDevice> Devices;
        public XKeysPlugin() : base("{a21635df-3aa1-46f9-9430-564dbae08314}", "XKeys-Plugin")
        {
            Tools.LoggerFactory = new LoggerFactory();
            Tools.LoggerFactory.AddProvider(new LumosLogWrapperProvider(nameof(XKeysPlugin)));
            Devices = Devices ?? new ObservableCollection<IDevice>();
            Devices.CollectionChanged += Devices_CollectionChanged;
        }

        protected override void initializePlugin()
        {
            Log.Info("Initialize");
        }
        protected override void shutdownPlugin()
        {
            Log.Info("Shutdown");
            deregisterInputAssignment();
            Devices.Clear();
        }
        protected override void startupPlugin()
        {
            Log.Info("Startup");
            XKeys.Instance.Connected += Instance_Connected;
            XKeys.Instance.Disconnected += Instance_Disconnected;
        }

        private void Instance_Connected(object? sender, IDevice e)
        {
            Devices.Add(e);
        }

        private void Instance_Disconnected(object? sender, IDevice e)
        {
            Devices.Remove(e);
        }

        private void Devices_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (IDevice device in e.NewItems!)
                    {
                        Log.Info($"Connect to {device.Name} SN:{device.SerialNumber}");
                        registerDeviceAtInputAssignment(device);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (IDevice device in e.OldItems!)
                    {
                        Log.Info($"Disconnect from {device.Name} SN:{device.SerialNumber}");
                        deregisterDeviceAtInputAssignment(device);
                    }
                    break;
            }
        }

        private void registerDeviceAtInputAssignment(IDevice device)
        {
            var im = InputManager.getInstance();

            //device implements multiple interfaces, so this is correct pgrote 12.06.2023
            if (device is IDeviceWithButtons<IButton> deviceWithButtons)
            {
                foreach (var button in deviceWithButtons.Buttons!)
                {
                    im.RegisterSource(new ButtonSource(device.SerialNumber!, button.Number));
                    if (button is IButtonWithBlueLED buttonBlueLED)
                        im.RegisterSink(new ButtonLEDSink(device.SerialNumber!, buttonBlueLED.Number, "Blue"));
                    if (button is IButtonWithRedLED buttonRedLED)
                        im.RegisterSink(new ButtonLEDSink(device.SerialNumber!, buttonRedLED.Number, "Red"));
                    if (button is IButtonWithRGBLED buttonRGBLED)
                        im.RegisterSink(new ButtonLEDSink(device.SerialNumber!, buttonRGBLED.Number, "RGB"));
                }
            }
            if (device is IDeviceWithGreenLED deviceWithGreenLED)
                im.RegisterSink(new DeviceLEDSink(device.SerialNumber!, "Green"));
            if (device is IDeviceWithRedLED deviceWithRedLED)
                im.RegisterSink(new DeviceLEDSink(device.SerialNumber!, "Red"));

            if (device is IDeviceWithBlueBacklightLEDs deviceWithBlueBacklightLEDs)
                im.RegisterSink(new DeviceBlueBacklightLEDIntensitySink(device.SerialNumber!));
            if (device is IDeviceWithBlueAndRedBacklightLEDs deviceWithBlueAndRedBacklightLEDs)
            {
                im.RegisterSink(new DeviceBlueAndRedBacklightLEDIntensitySink(device.SerialNumber!, "Blue"));
                im.RegisterSink(new DeviceBlueAndRedBacklightLEDIntensitySink(device.SerialNumber!, "Red"));
            }
            ///ToDo
            //if (device is IDeviceWithRGBBacklightLEDs deviceWithRGBBacklightLEDs)
            //    im.RegisterSink();
            if (device is IDeviceWithLCD deviceWithLCD)
            {
                im.RegisterSink(new DeviceLCDTextSink(device.SerialNumber!, 0));
                im.RegisterSink(new DeviceLCDTextSink(device.SerialNumber!, 1));
                im.RegisterSink(new DeviceLCDBacklightSink(device.SerialNumber!));
            }
            if (device is IDeviceWithJogShuttle deviceWithJogShuttle)
            {
                im.RegisterSource(new JogSource(device.SerialNumber!));
                im.RegisterSource(new ShuttleSource(device.SerialNumber!));
            }
            if (device is IDeviceWithJoystick deviceWithJoystick)
            {
                im.RegisterSource(new JoystickSource(device.SerialNumber!, "X"));
                im.RegisterSource(new JoystickSource(device.SerialNumber!, "Y"));
                im.RegisterSource(new JoystickSource(device.SerialNumber!, "Z"));
            }
            if (device is IDeviceWithTBar deviceWithTBar)
                im.RegisterSource(new TBarSource(device.SerialNumber!));

            im.RegisterSink(new DeviceLEDFlashSpeedSink(device.SerialNumber!));
        }
        private void deregisterDeviceAtInputAssignment(IDevice device)
        {
            var im = InputManager.getInstance();
            im.UnregisterSinks(im.Sinks.Where(sink => sink.ID.StartsWith("XKeys") && sink.ID.Contains(device.SerialNumber!)).ToList());
            im.UnregisterSources(im.Sources.Where(source => source.ID.StartsWith("XKeys") && source.ID.Contains(device.SerialNumber!)).ToList());
        }
        private void deregisterInputAssignment()
        {
            var im = InputManager.getInstance();
            im.UnregisterSinks(im.Sinks.Where(sink => sink.ID.StartsWith("XKeys")).ToList());
            im.UnregisterSources(im.Sources.Where(source => source.ID.StartsWith("XKeys")).ToList());
        }
    }
}