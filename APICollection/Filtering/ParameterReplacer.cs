using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LeanStack.Filtering
{
    /// <summary>
    /// Replaces the parameters of a provided expression with those provided on initialization
    /// <pre>Used when we need to combine multiple expression bodies keeping common parameter references.</pre>
    /// </summary>
    public class ParameterReplacer : ExpressionVisitor
    {
        private readonly Expression _newValue;
        private Expression _oldValue;

        public ParameterReplacer(Expression newValue)
        {
            _newValue = newValue;
        }
        public override Expression Visit(Expression node)
        {
            if (node == _oldValue)
                return _newValue;
            return base.Visit(node);
        }
        public Expression Replace(Expression body, Expression old)
        {
            this._oldValue = old;
            return this.Visit(body);
        }
    }
}
