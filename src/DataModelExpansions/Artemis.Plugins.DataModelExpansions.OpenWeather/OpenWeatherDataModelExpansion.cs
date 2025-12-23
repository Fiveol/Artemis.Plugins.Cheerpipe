using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.DataModelExpansions.OpenWeather.DataModels;
using Awesomio.Weather;   // WeatherClient and CurrentWeatherModel are here
using Serilog;

namespace Artemis.Plugins.DataModelExpansions.OpenWeather
{
    public class OpenWeather : Module<OpenWeatherDataModel>
    {
        private readonly PluginSetting<string> _apiKeySetting;
        private readonly PluginSetting<string> _citySetting;
        private readonly PluginSetting<string> _unitOfMeasurementSetting;
        private readonly ILogger _logger;

        public OpenWeather(PluginSettings pluginSettings, ILogger logger)
        {
            _logger = logger;
            _apiKeySetting = pluginSettings.GetSetting("ApiKey", string.Empty);
            _citySetting = pluginSettings.GetSetting("City", string.Empty);
            _unitOfMeasurementSetting = pluginSettings.GetSetting("Unit", Enum.GetNames(typeof(UnitsOfMeasurement)).FirstOrDefault());

            _apiKeySetting.PropertyChanged += _OpenWeatherSettingsChanged_PropertyChanged;
            _citySetting.PropertyChanged += _OpenWeatherSettingsChanged_PropertyChanged;
            _unitOfMeasurementSetting.PropertyChanged += _OpenWeatherSettingsChanged_PropertyChanged;
        }

        public override List<IModuleActivationRequirement> ActivationRequirements => null;

        private void _OpenWeatherSettingsChanged_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateWeatherData();
        }

        public override void Enable()
        {
            // TODO: Make frequency configurable 
            AddTimedUpdate(TimeSpan.FromMinutes(10), _ => UpdateWeatherData());
            UpdateWeatherData();
        }

        public override void Disable() { }

        public override void Update(double deltaTime) { }

        public void UpdateWeatherData()
        {
            try
            {
                if (string.IsNullOrEmpty(_citySetting.Value) || string.IsNullOrEmpty(_unitOfMeasurementSetting.Value))
                    return;

                string accessKey = _apiKeySetting.Value;
                var client = new WeatherClient(accessKey);

                // Correct type name, no extra namespace
                CurrentWeatherModel data = client
                    .GetCurrentWeatherAsync<CurrentWeatherModel>(_citySetting.Value, "en", _unitOfMeasurementSetting.Value)
                    .Result;

                // Weather Measurements
                DataModel.Weather = (WeatherConditions)Enum.Parse(typeof(WeatherConditions), data.Weather.FirstOrDefault()?.Main ?? "Unknown");
                DataModel.Temp = data.Main.Temp;
                DataModel.FeelsLike = data.Main.FeelsLike;
                DataModel.TempMin = data.Main.TempMin;
                DataModel.TempMax = data.Main.TempMax;
                DataModel.Pressure = data.Main.Pressure;
                DataModel.Humidity = data.Main.Humidity;

                // Visibility
                DataModel.Clouds = data.Clouds.All;
                DataModel.Visibility = data.Visibility;

                // Sunrise / Sunset
                DataModel.Sunrise = DateTimeOffset.FromUnixTimeSeconds(data.Sys.Sunrise).DateTime.ToLocalTime();
                DataModel.Sunset = DateTimeOffset.FromUnixTimeSeconds(data.Sys.Sunset).DateTime.ToLocalTime();

                // Wind
                DataModel.Wind.Speed = data.Wind.Speed;
                DataModel.Wind.Deg = data.Wind.Deg;
                DataModel.Wind.WindDirection = (WindDirectionCodes)Enum.Parse(typeof(WindDirectionCodes), data.Wind.WindDirection);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }
        }
    }
}
