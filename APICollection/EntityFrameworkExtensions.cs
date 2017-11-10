using System;
using System.Linq;
using System.Linq.Expressions;
using LeanStack;

namespace LeanStack
{
    public static class EntityFrameworkExtensions
    {
        /// <summary>
        /// Supports sorting by a property passed in String format, retrieved using reflection.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="orderByProperty"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderByProperty,
                          bool desc)
        {
            string command = desc ? "OrderByDescending" : "OrderBy";
            var type = typeof(TEntity);
            var property = type.GetProperty(orderByProperty);
            if (property == null)
                throw new ArgumentException($"Class {typeof(TEntity).Name} does not contain a property with the name {orderByProperty}");
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
                                          source.Expression, Expression.Quote(orderByExpression));
            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }
        /// <summary>
        ///Unboxes if boxed the expression value type parameter and sorts queryables by passed expression.
        /// <para> Used when the expression has a value type parameter stored as object and gets boxed by the compiler.Because Linq to entities does not support unboxing.</para>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="expression"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> UnboxOrderByExpression<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity, object>> expression,bool desc)
        {
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                //Reconstucts the expression keeping only the Member 
                var property = (expression.Body as UnaryExpression).Operand;
                string command = desc ? "OrderByDescending" : "OrderBy";
                var orderByExpression = Expression.Lambda(property, expression.Parameters);
                var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { typeof(TEntity), property.Type },
                                              source.Expression, Expression.Quote(orderByExpression));
                return source.Provider.CreateQuery<TEntity>(resultExpression);
            }
            return desc?source.OrderByDescending(expression):source.OrderBy(expression);
        }



    }
}