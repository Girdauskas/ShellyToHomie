using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace ShellyToHomie {
    public class ShellyDimmerHomieProducer {
        private HostDevice _hostDevice;

        public ShellyDimmerClient ShellyClient { get; private set; }

        public PahoHostDeviceConnection Broker { get; } = new();

        private HostChoiceProperty _stateProperty;
        private HostNumberProperty _brightnessProperty;
        private HostChoiceProperty _onOffCommand;

        private HostNumberProperty _actualPowerConsumptionProperty;
        private HostNumberProperty _energyUsedProperty;
        private HostNumberProperty _internalTemperatureProperty;

        public bool IsInitialized { get; private set; }

        public void Initialize(ShellyDimmerClient shellyClient, string homieDeviceId, string homieDeviceFriendlyName, string mqttBrokerIpAddress) {
            ShellyClient = shellyClient;

            _hostDevice = DeviceFactory.CreateHostDevice(homieDeviceId, homieDeviceFriendlyName);

            _hostDevice.UpdateNodeInfo("basic", "Basic", "no-type");
            _stateProperty = _hostDevice.CreateHostChoiceProperty(PropertyType.State, "basic", "actual-state", "Actual state", new []{"ON", "OFF"}, ShellyClient.ActualState ? "ON" : "OFF");
            _brightnessProperty = _hostDevice.CreateHostNumberProperty(PropertyType.Parameter, "basic", "brightness", "Brightness", ShellyClient.Brightness, "%", 0);
            _onOffCommand = _hostDevice.CreateHostChoiceProperty(PropertyType.Command, "basic", "on-off-command", "Turn on off", new []{"ON", "OFF"});

            _hostDevice.UpdateNodeInfo("advanced", "Advanced", "no-type");
            _actualPowerConsumptionProperty = _hostDevice.CreateHostNumberProperty(PropertyType.State, "advanced", "actual-power-consumption", "Actual power consumption", ShellyClient.PowerInW, "W");
            _internalTemperatureProperty = _hostDevice.CreateHostNumberProperty(PropertyType.State, "advanced", "internal-temperature", "Internal temperature", shellyClient.TemperatureInC, "°C");
            _energyUsedProperty = _hostDevice.CreateHostNumberProperty(PropertyType.State, "advanced", "energy-used", "Energy used", shellyClient.EnergyInKwh, "kWh");

            _onOffCommand.PropertyChanged += (sender, args) => {
                if (_onOffCommand.Value == "ON") ShellyClient.TurnOn();
                if (_onOffCommand.Value == "OFF") ShellyClient.TurnOff();
            };

            _brightnessProperty.PropertyChanged += (sender, args) => {
                var value = _brightnessProperty.Value;

                if (value < 10) {
                    value = 10;
                    _brightnessProperty.Value = value;
                }

                if (value > 100) {
                    value = 100;
                    _brightnessProperty.Value = value;
                }

                ShellyClient.SetBrightness((int)value);
            };

            Broker.Initialize(mqttBrokerIpAddress);
            _hostDevice.Initialize(Broker);

            ShellyClient.PropertyChanged += (sender, args) => {
                RefreshAllProperties();
            };

            RefreshAllProperties();

            IsInitialized = true;
        }

        private void RefreshAllProperties() {
            _stateProperty.Value = ShellyClient.ActualState ? "ON" : "OFF";
            _brightnessProperty.Value = ShellyClient.Brightness;
            _actualPowerConsumptionProperty.Value = ShellyClient.PowerInW;
            _internalTemperatureProperty.Value = ShellyClient.TemperatureInC;
            _energyUsedProperty.Value = ShellyClient.EnergyInKwh;
        }
    }
}