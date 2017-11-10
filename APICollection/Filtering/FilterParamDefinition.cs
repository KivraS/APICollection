using LeanStack.Filtering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LeanStack.Filtering
{

    public abstract class FilterParamDefinition<TEntity>
    {
        public abstract Expression<Func<TEntity, bool>> getFilterExpression(string value);
        /// <summary>
        /// True means that this object has, can populate and accepts specific values.
        /// </summary>
        public bool hasSpecificValues { get; set; }
        /// <summary>
        /// Converts the string value from the query to the specific type of the filter accepted parameter.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Object ConvertValue(String value)
        {
            var type = this.GetFilterPropertyType();
            if (type == typeof(DateTime))
                return DateTime.Parse(value, null, DateTimeStyles.RoundtripKind);
            if (type.IsEnum)
                return Enum.Parse(type, value);
            else
            {
                var converter = TypeDescriptor.GetConverter(type);
                return converter.ConvertFrom(value);
            }
        }
        public String GetTypeAsString()
        {
            var type = GetFilterPropertyType();
            //Prevents returning the fully qualified namespace of the enum. Enum generic type is only needed.
            if (type.IsEnum)
                return typeof(Enum).ToString();
            else
                return type.ToString();
        }
        /// <summary>
        /// Returns if applicable, all possible values this filter can accept
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<String, String>> GetSupportedValues()
        {
            if (this.hasSpecificValues)
            {
                if (!this.GetFilterPropertyType().IsEnum)
                    return null;
                else
                    return Enum.GetNames(this.GetFilterPropertyType())
                        .Select(e => new KeyValuePair<String, String>(e, e)).ToArray();
            }
            else
                return null;
        }
        /// <summary>
        /// Returns the type of the parameter this filter accepts.
        /// </summary>
        /// <returns></returns>
        public abstract Type GetFilterPropertyType();
        /// <summary>
        /// In case of multiple values for the same key found. The filter performs OR type filtering on the values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public virtual Expression<Func<TEntity, bool>> getFilterExpression(IEnumerable<String> values)
        {
            using (var enumerator = values.GetEnumerator())
            {
                enumerator.MoveNext();
                var expr = this.getFilterExpression(enumerator.Current);
                var parameters = expr.Parameters;
                var body = expr.Body;
                //Replace parameter references on all expressions with those found on the first item just to keep a common reference on all bodies.
                //Since on this type of delegate there is only 1 parameter we just keep the one at index 0.
                ParameterReplacer replacer = new ParameterReplacer(parameters[0]);
                //Merges the bodies of all filter expressions into one body with multiple OR operators.
                while (enumerator.MoveNext())
                {
                    var nextExpr = this.getFilterExpression(enumerator.Current);
                    body = Expression.OrElse(body,replacer.Replace(nextExpr.Body,nextExpr.Parameters[0]));
                }
                return Expression.Lambda<Func<TEntity, bool>>(body, parameters);
            }
        }
    }
}
