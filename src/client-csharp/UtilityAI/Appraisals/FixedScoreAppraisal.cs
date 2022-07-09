namespace BrainAI.AI.UtilityAI.Considerations
{
    using BrainAI.AI.UtilityAI.Actions;
    using BrainAI.AI.UtilityAI.Considerations.Appraisals;

    /// <summary>
    /// always returns a fixed score. Serves double duty as a default Consideration.
    /// </summary>
    public class FixedScoreAppraisal<T> : IAppraisal<T>
    {
        public float Score;

        public IAction<T> Action { get; set; }

        public FixedScoreAppraisal( float score = 1 )
        {
            this.Score = score;
        }

        public float GetScore( T context )
        {
            return this.Score;
        }
    }
}

