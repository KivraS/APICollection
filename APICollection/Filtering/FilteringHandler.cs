using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web;

namespace LeanStack.Filtering
{
    /// <summary>
    /// Filters a specific class using pre-configured expressions for keys or either comparing it's properties with the specified query key values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FilteringHandler<T>
    {
        public NameValueCollection _queryValues { get; set; }
        public Dictionary<String, FilterParamDefinition<T>> definitions { get; set; }
        /// <summary>
        /// Used when dynamic filtering is enabled. If true, string source contains value comparison will be used instead of equality
        /// </summary>
        public Boolean compareStringsUsingContains { get; set; } = true;
        /// <summary>
        /// Configured with dictionary of associated predicates to execute for each found key on the query values.
        /// <pre>If dynamic filtering is enabled, it just search the class properties for a matching key and just compares using equality comparer. 
        /// In case of dynamic filtering, filter definitions are not required.
        /// </pre>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterDefinitions"></param>
        /// <param name="dynamicfiltering"></param>
        public FilteringHandler(NameValueCollection values, Dictionary<String, FilterParamDefinition<T>> filterDefinitions,bool dynamicfiltering=false)
        {
            definitions = filterDefinitions ?? new Dictionary<string, FilterParamDefinition<T>>();
            if (dynamicfiltering)
                this.EnableDynamicFiltering();
            _queryValues = values;
        }
        //Reads all properties of the class in order to find specified keys to compare
        private Dictionary<String, FilterParamDefinition<T>> GetDynamicFilterParams()
        {
            return typeof(T).GetProperties().Where(p => p.GetGetMethod().IsVirtual == false)
                .ToDictionary(p => p.Name, p => new DynamicFilterParamDefinition<T>(p,compareStringsUsingContains) as FilterParamDefinition<T>);
        }

        private void EnableDynamicFiltering()
        {
            if (definitions == null || definitions.Count==0)
                definitions = this.GetDynamicFilterParams();
            else
            {
                var dynamicDefinitions = this.GetDynamicFilterParams();
                foreach (var item in dynamicDefinitions)
                {
                    definitions.Add(item.Key, item.Value);
                }
            }
        }
        /// <summary>
        ///  Lists all filter keys this handler accepts
        /// </summary>
        /// <returns></returns>
        public SupportedFilterProperty[] GetSupportedFilteringProperties()
        {
            return this.definitions.Select(d =>new SupportedFilterProperty{
               Key= d.Key,
               Type = d.Value.GetTypeAsString(),
               SupportedValues = d.Value.GetSupportedValues()
            }).ToArray();
        }
        //Applies all filters for found keys on the IQueryable.
        public IQueryable<T> Filter(IQueryable<T> entities)
        {
            foreach (var item in _queryValues.AllKeys)
            {
                FilterParamDefinition<T> def;
                if (definitions.TryGetValue(item,out def))
                {
                    var values = _queryValues.GetValues(item);
                    Expression<Func<T,bool>> temp;
                    if (values.Length == 1)
                        temp = def.getFilterExpression(values.Single());
                    //If multiple values for the same key found, the multiple value handling method is called.
                    else
                        temp = def.getFilterExpression(values);
                    entities = entities.Where(temp);
                }
            }
            return entities;
        }
    }





}