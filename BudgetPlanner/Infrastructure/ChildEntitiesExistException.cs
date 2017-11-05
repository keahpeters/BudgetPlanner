using System;

namespace BudgetPlanner.Infrastructure
{
    public class ChildEntitiesExistException : Exception
    {
        public ChildEntitiesExistException(string message)
            : base(message)
        {
        }
    }
}