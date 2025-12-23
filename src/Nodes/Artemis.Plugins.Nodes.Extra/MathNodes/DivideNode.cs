using Artemis.Core;
using System;

namespace Artemis.Plugins.Nodes.Extra.MathNodes
{
    [Node("Divide", "Divide the connected numeric values.", "Mathematics", InputType = typeof(double), OutputType = typeof(double))]
    public class DivideNumericsNode : Node
    {
        #region Properties & Fields

        public InputPin<double> A { get; }
        public InputPin<double> B { get; }

        public OutputPin<double> Result { get; }

        #endregion

        #region Constructors

        public DivideNumericsNode()
        {
            A = CreateInputPin<double>("A");
            B = CreateInputPin<double>("B");

            Result = CreateOutputPin<double>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            // Guard against divide-by-zero
            if (B.Value == 0)
                Result.Value = double.NaN;
            else
                Result.Value = A.Value / B.Value;
        }

        #endregion
    }
}
