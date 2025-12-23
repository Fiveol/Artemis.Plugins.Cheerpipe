using Artemis.Core;

namespace Artemis.Plugins.Nodes.Extra
{
    public class ExtraNodesPluginBootstrapper : PluginBootstrapper
    {
        public override void OnPluginEnabled(Plugin plugin)
        {
            // No manual registration needed anymore.
            // Nodes with [Node] attribute will be discovered automatically.
        }
    }
}
