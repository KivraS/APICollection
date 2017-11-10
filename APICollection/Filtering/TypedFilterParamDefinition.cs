using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LeanStack.Filtering
{
    /// <summary>
    /// Defines a predicate responsible for filtering the results based on a value provided for the specific filter key.
    /// </summary>
    /// <typeparam name="TEntity">Type of object being filtered</typeparam>
    /// <typeparam name="T">Type of input parameter to parse from query string</typeparam>
    public class TypedFilterParamDefinition<TEntity, T> : FilterParamDefinition<TEntity>
    {
        private Func<T, Expression<Func<TEntity, bool>>> _filterExpression;
        /// <summary>
        /// Instantiates the filter expression with the provided filter value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override Expression<Func<TEntity, bool>> getFilterExpression(string value)
        {
            var tempVal = this.ConvertValue(value);
            return _filterExpression.Invoke((T)tempVal);
        }
        /// <summary>
        /// Created and initializes the definition 
        /// </summary>
        /// <param name="filterExpression">Predicate for this filter</param>
        /// <param name="hasSpecificValues">Set true if this particular filter accepts specific values that can be populated Currently supports only enums</param>
        public TypedFilterParamDefinition(Func<T, Expression<Func<TEntity, bool>>> filterExpression, bool hasSpecificValues = false)
        {
            _filterExpression = filterExpression;
            this.hasSpecificValues = hasSpecificValues;
        }
        public override Type GetFilterPropertyType()
        {
            return typeof(T);
        }
    }
}
