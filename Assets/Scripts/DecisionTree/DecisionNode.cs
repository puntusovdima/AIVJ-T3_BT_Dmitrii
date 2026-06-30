using System;

namespace DecisionTree
{
    /// <summary>
    /// Base class for all nodes in the Decision Tree.
    /// </summary>
    /// <typeparam name="T">The type of the agent being evaluated.</typeparam>
    public abstract class DecisionNode<T>
    {
        /// <summary>
        /// Evaluates the node and returns the chosen action/state name.
        /// </summary>
        /// <param name="agent">The agent to evaluate.</param>
        /// <returns>The string identifier of the chosen action/state.</returns>
        public abstract string Evaluate(T agent);
    }

    /// <summary>
    /// Leaf node of the Decision Tree. Represents a concrete action or state name to execute.
    /// </summary>
    /// <typeparam name="T">The type of the agent.</typeparam>
    public class ActionNode<T> : DecisionNode<T>
    {
        private readonly string actionName;

        public ActionNode(string actionName)
        {
            this.actionName = actionName;
        }

        public override string Evaluate(T agent)
        {
            return actionName;
        }
    }

    /// <summary>
    /// Binary branch node that evaluates a boolean condition on the agent and routes accordingly.
    /// </summary>
    /// <typeparam name="T">The type of the agent.</typeparam>
    public class DecisionQueryNode<T> : DecisionNode<T>
    {
        private readonly Func<T, bool> condition;
        private readonly DecisionNode<T> trueNode;
        private readonly DecisionNode<T> falseNode;

        public DecisionQueryNode(Func<T, bool> condition, DecisionNode<T> trueNode, DecisionNode<T> falseNode)
        {
            this.condition = condition ?? throw new ArgumentNullException(nameof(condition));
            this.trueNode = trueNode ?? throw new ArgumentNullException(nameof(trueNode));
            this.falseNode = falseNode ?? throw new ArgumentNullException(nameof(falseNode));
        }

        public override string Evaluate(T agent)
        {
            // Evaluate the binary query condition
            bool result = condition(agent);
            
            // Route to the appropriate child branch
            return result ? trueNode.Evaluate(agent) : falseNode.Evaluate(agent);
        }
    }
}
