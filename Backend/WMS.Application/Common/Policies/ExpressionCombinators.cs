using System.Linq.Expressions;

namespace WMS.Application.Common.Policies
{
    /// <summary>
    /// Combines two predicate expressions over the same entity type into one, with proper
    /// parameter unification so the result still translates to SQL. Used where a hardcoded
    /// RBAC-style rule and a PBAC row filter need to grant access additively (either one being
    /// true is enough), rather than one replacing the other.
    /// </summary>
    public static class ExpressionCombinators
    {
        public static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>>? right)
        {
            if (right == null) return left;

            var parameter = Expression.Parameter(typeof(T), "x");
            var leftBody = new ParameterReplacer(left.Parameters[0], parameter).Visit(left.Body);
            var rightBody = new ParameterReplacer(right.Parameters[0], parameter).Visit(right.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(leftBody, rightBody!), parameter);
        }

        private class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam = oldParam;
            private readonly ParameterExpression _newParam = newParam;

            protected override Expression VisitParameter(ParameterExpression node) =>
                node == _oldParam ? _newParam : base.VisitParameter(node);
        }
    }
}
