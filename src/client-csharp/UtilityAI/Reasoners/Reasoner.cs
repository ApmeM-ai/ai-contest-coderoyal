namespace BrainAI.AI.UtilityAI.Reasoners
{
    using System.Collections.Generic;

    using BrainAI.AI.UtilityAI.Actions;
    using BrainAI.AI.UtilityAI.Considerations.Appraisals;

    /// <summary>
    /// the root of UtilityAI.
    /// </summary>
    public abstract partial class Reasoner<T>
    {
        protected class Consideration
        {
            public IAppraisal<T> Appraisal { get; set; }
            public IAction<T> Action { get; set; }
        }

        protected readonly List<Consideration> Considerations = new List<Consideration>();

        public abstract IAction<T> SelectBestAction(T context);

        public Reasoner<T> Add(IAppraisal<T> appraisal, params IAction<T>[] actions)
        {
            IAction<T> action;
            if (actions.Length == 0)
            {
                action = new NoAction<T>();
            }
            else if (actions.Length == 1)
            {
                action = actions[0];
            }
            else
            {
                var newAction = new CompositeAction<T>();
                newAction.Actions.AddRange(actions);
                action = newAction;
            }

            this.Considerations.Add(new Consideration
            {
                Appraisal = appraisal,
                Action = action
            });

            return this;
        }
    }
}

