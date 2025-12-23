using Artemis.Core;
using System;

namespace Artemis.Plugins.Nodes.Extra.MathNodes
{
    [Node("Abs", "Return absolute value", "Mathematics", InputType = typeof(double), OutputType = typeof(double))]
    public class AbsNumericNode : Node
    {
        #region Properties & Fields

        public InputPin<double> Input { get; }
        public OutputPin<double> Output { get; }

        #endregion

        #region Constructors

        public AbsNumericNode()
        {
            Input = CreateInputPin<double>();
            Output = CreateOutputPin<double>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Evaluate()
        {
            Output.Value = Math.Abs(Input.Value);
        }

        #endregion
    }
}
