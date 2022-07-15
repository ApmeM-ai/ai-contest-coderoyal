using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class AnyBool : IAppraisal<AIState>
    {
        private readonly IAppraisal<AIState>[] boolAppraisal;

        public AnyBool(params IAppraisal<AIState>[] boolAppraisal)
        {
            this.boolAppraisal = boolAppraisal;
        }
        public float GetScore(AIState context)
        {
            foreach (var a in boolAppraisal)
            {
                if (a.GetScore(context) > 0)
                {
                    return 1;
                }
            }
            return 0;
        }
    }
}