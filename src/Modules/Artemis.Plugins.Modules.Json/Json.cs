using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Plugins.Modules.Json.Controllers;
using Artemis.Plugins.Modules.Json.DataModels;
using System.Collections.Generic;
using Artemis.Plugins.Modules.Json.Services.JsonDataModelServices;

namespace Artemis.Plugins.Modules.Json
{
    public class Json : Module<JsonDataModel>
    {
        private readonly IWebServerService _webServerService;
        private readonly JsonDataModelServices _jsonDataModelServices;
        private WebApiControllerRegistration? _controllerRegistration;

        public Json(JsonDataModelServices jsonDataModelServices, IWebServerService webServerService)
        {
            _jsonDataModelServices = jsonDataModelServices;
            _webServerService = webServerService;
        }

        public override List<IModuleActivationRequirement> ActivationRequirements => null;

        public override void Enable()
        {
            _jsonDataModelServices.Initialize(DataModel);
            _jsonDataModelServices.LoadFromRepository();

            // Provide a path string for the controller
            _controllerRegistration = _webServerService.AddController<JsonController>(this, "/json");
        }

        public override void Disable()
        {
            if (_controllerRegistration != null)
            {
                _webServerService.RemoveController(_controllerRegistration);
                _controllerRegistration = null;
            }
        }

        public override void Update(double deltaTime) { }
    }
}
