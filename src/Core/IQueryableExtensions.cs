using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable OperatorIsCanBeUsed

namespace EF.EntityFrameworkCore.DynamicWhere;

[PublicAPI]
// ReSharper disable once InconsistentNaming
public static class IQueryableExtensions
{
    private static MemberInfo ExtractMemberInfoFromPredicate(Expression body)
    {
        if (body is BinaryExpression expression)
        {
            return ((MemberExpression)expression.Left).Member;
        }

        if (body is UnaryExpression unaryExpression)
        {
            return ((MemberExpression)unaryExpression.Operand).Member;
        }

        var extractedMemberInfo = ((MemberExpression)body).Member;

        return extractedMemberInfo;
    }

    /// <summary>
    ///     SQL Engine optimized version of
    ///     <see
    ///         cref="ConditionalWhere{T}(System.Linq.IQueryable{T},System.Func{bool},System.Linq.Expressions.Expression{System.Func{T,bool}})" />
    ///     .
    ///     <br />
    ///     This will always return back the
    ///     <param name="condition" />
    ///     as an expression, which should help the SQL Engine determine query plans.
    /// </summary>
    /// <param name="source">The <see cref="IQueryable{T}" /> source.</param>
    /// <param name="condition">Truthiness lambda</param>
    /// <param name="predicate">
    ///     Expression which will be applied if <paramref name="condition" /> results in <c>true</c>
    /// </param>
    /// <typeparam name="T">Type used in the <see cref="IQueryable{T}" /></typeparam>
    public static IQueryable<T> PreConditionalWhere<T>(
        this IQueryable<T> source,
        Func<bool> condition,
        Expression<Func<T, bool>> predicate)
    {
        return PreConditionalWhere(source, condition(), predicate);
    }

    /// <summary>
    ///     SQL Engine optimized version of
    ///     <see
    ///         cref="ConditionalWhere{T}(System.Linq.IQueryable{T},System.Func{bool},System.Linq.Expressions.Expression{System.Func{T,bool}})" />.
    ///     <br />
    ///     This will always return back the
    ///     <param name="condition" />
    ///     as an expression, which should help the SQL Engine determine query plans.
    /// </summary>
    /// <param name="source">The <see cref="IQueryable{T}" /> source.</param>
    /// <param name="condition">Truthiness condition</param>
    /// <param name="predicate">
    ///     Expression which will be applied if <paramref name="condition" /> results in <c>true</c>
    /// </param>
    /// <typeparam name="T">Type used in the <see cref="IQueryable{T}" /></typeparam>
    public static IQueryable<T> PreConditionalWhere<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var truthyExpression = Expression.Constant(true);
        var falsyExpression = Expression.Constant(false);

        var extractedMemberInfo = ExtractMemberInfoFromPredicate(predicate.Body);
        var memberExp = Expression.MakeMemberAccess(parameter, extractedMemberInfo);
        var predicateLogicalExpression = Expression.Equal(memberExp,
            predicate.Body.GetType() == typeof(UnaryExpression) ? falsyExpression : truthyExpression);

        var test1 = Expression.Constant(condition);

        var preconditionExpression = Expression.Equal(test1, falsyExpression);

        // Where(true) if our precondition is false, apply predicate otherwise.
        var equalOr = Expression.Or(preconditionExpression, predicateLogicalExpression);

        var lambda = Expression.Lambda<Func<T, bool>>(equalOr, parameter);

        return source.Where(lambda);
    }


    /// <summary>
    ///     Interop for
    ///     <see
    ///         cref="ConditionalWhere{T}(System.Linq.IQueryable{T},System.Func{bool},System.Linq.Expressions.Expression{System.Func{T,bool}})" />
    ///     .
    ///     Works the same way as if we make an if statement that checks for the truthiness of <paramref name="condition" />
    ///     and if it's <c>true</c>, it will apply Where with <paramref name="predicate" />.
    /// </summary>
    /// <param name="source">The <see cref="IQueryable{T}" /> source.</param>
    /// <param name="condition">Truthiness lambda</param>
    /// <param name="predicate">
    ///     Expression which will be applied if <paramref name="condition" /> results in <c>true</c>
    /// </param>
    /// <typeparam name="T">Type used in the <see cref="IQueryable{T}" /></typeparam>
    /// <returns></returns>
    public static IQueryable<T> ConditionalWhere<T>(
        this IQueryable<T> source,
        Func<bool> condition,
        Expression<Func<T, bool>> predicate)
    {
        return source.ConditionalWhere(condition(), predicate);
    }

    /// <summary>
    ///     Works the same way as if we make an if statement that checks for the truthiness of <paramref name="condition" />
    ///     and if it's <c>true</c>, it will apply Where of <paramref name="predicate" />.
    /// </summary>
    /// <param name="source">The <see cref="IQueryable{T}" /> source.</param>
    /// <param name="condition">Truthiness lambda</param>
    /// <param name="predicate">
    ///     Expression which will be applied if <paramref name="condition" /> results in <c>true</c>
    /// </param>
    /// <typeparam name="T">Type used in the <see cref="IQueryable{T}" /></typeparam>
    /// <returns></returns>
    public static IQueryable<T> ConditionalWhere<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
}
