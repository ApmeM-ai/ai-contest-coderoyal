using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;

namespace AiCup22
{
    public class PrintAction : IAction<AIState>
        {
            private string action;

            public PrintAction(string action)
            {
                this.action = action;
            }

            public void Execute(AIState context)
            {
                context.debug?.AddPlacedText(
                    context.currentUnit.Position,
                    action,
                    new Vec2(0, 0),
                    1,
                    new Debugging.Color(1, 0, 0, 1));
            }
        }
 }