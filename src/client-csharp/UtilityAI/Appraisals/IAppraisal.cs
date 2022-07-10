namespace AiCup22.UtilityAI.Appraisals
{
    /// <summary>
    /// scorer for use with a Consideration
    /// </summary>
    public interface IAppraisal<T>
    {
        float GetScore(T context);
    }
}

