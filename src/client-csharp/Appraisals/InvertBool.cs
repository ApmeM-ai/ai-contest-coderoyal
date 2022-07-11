using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class InvertBool : IAppraisal<AIState>
        {
            private readonly IAppraisal<AIState> boolAppraisal;

            public InvertBool(IAppraisal<AIState> boolAppraisal)
            {
                this.boolAppraisal = boolAppraisal;
            }
            public float GetScore(AIState context)
            {
                return 1 - this.boolAppraisal.GetScore(context);
            }
        }
 }