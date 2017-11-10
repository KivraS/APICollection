using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LeanStack.Filtering
{

    /// <summary>
    /// Used when dynamic filtering is enabled.
    /// <pre>Searches source class properties for a matching property name with the filter key, and filters results comparing this property</pre>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class DynamicFilterParamDefinition<TEntity> : FilterParamDefinition<TEntity>
    {
        private PropertyInfo propInfo { get; set; }
        private Boolean compareStringsUsingContains { get; set; }

        public override Type GetFilterPropertyType()
        {
            return propInfo.PropertyType;
        }
        /// <summary>
        /// Creates a filter definition based on a class property info that will compare its value with.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="compareStringsUsingContains"></param>
        public DynamicFilterParamDefinition(PropertyInfo info, bool compareStringsUsingContains)
        {
            propInfo = info;
            this.compareStringsUsingContains = compareStringsUsingContains;
        }
        public override Expression<Func<TEntity, bool>> getFilterExpression(string value)
        {
            var tempVal = this.ConvertValue(value);
            Expression<Func<TEntity, bool>> compareExpression = null;
            ParameterExpression pe = Expression.Parameter(typeof(TEntity));
            Expression left = Expression.Property(pe, propInfo);
            Expression right = Expression.Constant(tempVal, this.GetFilterPropertyType());
            //Compare using left.Contains(right);
            if (compareStringsUsingContains && this.propInfo.PropertyType == typeof(String))
            {
                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var containsExpr = Expression.Call(left, method, right);
                compareExpression = Expression.Lambda<Func<TEntity, bool>>(containsExpr, pe);
            }
            else//Simple == compare
                compareExpression = Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(left, right), new[] { pe });
            return compareExpression;
        }
    }
}
