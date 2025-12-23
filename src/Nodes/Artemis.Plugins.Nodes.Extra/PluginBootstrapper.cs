using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Plugins.Nodes.Extra.MathNodes;

namespace Artemis.Plugins.Nodes.MathExtra
{
    public class MathExtraPluginBootstrapper : PluginBootstrapper
    {
        private readonly INodeService _nodeService;
        public MathExtraPluginBootstrapper(INodeService nodeService)
        {
            _nodeService = nodeService;
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            _nodeService.RegisterNode<DivideNumericsNode>();
            _nodeService.RegisterNode<FullLerpNode>();
            _nodeService.RegisterNode<MultiplyNode>();
            _nodeService.RegisterNode<PercentageOfNode>();
            _nodeService.RegisterNode<AbsNumericNode>();
        }
    }
}
