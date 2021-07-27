using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace ShellyToHomie {
    public class Shelly1PmHomieProducer {
        private HostDevice _hostDevice;
        public PahoHostDeviceConnection Broker { get; } = new();

        public Shelly1PmClient ShellyClient { get; private set; }

        private HostChoiceProperty _relayStateProperty;
        private HostChoiceProperty _inputStateProperty;
        private HostChoiceProperty _relayControlProperty;

        private HostNumberProperty _actualPowerConsumptionProperty;
        private HostNumberProperty _energyUsedProperty;
        private HostNumberProperty _internalTemperatureProperty;

        public bool IsInitialized { get; private set; }

        public void Initialize(Shelly1PmClient shellyClient, string homieDeviceId, string homieDeviceFriendlyName, string mqttBrokerIpAddress) {
            ShellyClient = shellyClient;

            _hostDevice = DeviceFactory.CreateHostDevice(homieDeviceId, homieDeviceFriendlyName);

            _hostDevice.UpdateNodeInfo("basic", "Basic", "no-type");
            _relayStateProperty = _hostDevice.CreateHostChoiceProperty(PropertyType.State, "basic", "actual-relay-state", "Actual relay state", new[] { "ON", "OFF" }, ShellyClient.OutputState ? "ON" : "OFF");
            _inputStateProperty = _hostDevice.CreateHostChoiceProperty(PropertyType.State, "basic", "actual-input-state", "Actual input state", new[] { "ON", "OFF" }, ShellyClient.InputState ? "ON" : "OFF");
            _relayControlProperty = _hostDevice.CreateHostChoiceProperty(PropertyType.Command, "basic", "relay-control", "Manual relay control", new[] { "ON", "OFF" });

            _hostDevice.UpdateNodeInfo("advanced", "Advanced", "no-type");
            _actualPowerConsumptionProperty = _hostDevice.CreateHostNumberProperty(PropertyType.State, "advanced", "actual-power-consumption", "Actual power consumption", (float)ShellyClient.PowerInW, "W");
            _internalTemperatureProperty = _hostDevice.CreateHostNumberProperty(PropertyType.State, "advanced", "internal-temperature", "Internal temperature", (float)shellyClient.TemperatureInC, "°C");
            _energyUsedProperty = _hostDevice.CreateHostNumberProperty(PropertyType.State, "advanced", "energy-used", "Energy used", (float)shellyClient.EnergyInKwh, "kWh");

            _relayControlProperty.PropertyChanged += (sender, args) => {
                if (_relayControlProperty.Value == "ON") ShellyClient.EnableRelay();
                if (_relayControlProperty.Value == "OFF") ShellyClient.DisableRelay();
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
            _relayStateProperty.Value = ShellyClient.OutputState ? "ON" : "OFF";
            _inputStateProperty.Value = ShellyClient.InputState ? "ON" : "OFF";
            _actualPowerConsumptionProperty.Value = (float)ShellyClient.PowerInW;
            _internalTemperatureProperty.Value = (float)ShellyClient.TemperatureInC;
            _energyUsedProperty.Value = (float)ShellyClient.EnergyInKwh;
        }
    }
}