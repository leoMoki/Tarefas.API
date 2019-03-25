using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Tarefas.API.Mongo
{
    public interface IDatabase<T> where T : class, new()
    {
        bool Delete(Expression<Func<T, bool>> expression);
        void DeleteAll();
        T Single(Expression<Func<T, bool>> expression);
        System.Linq.IQueryable<T> Query { get; set; }
        System.Linq.IQueryable<T> All(int page, int pageSize);
        System.Linq.IQueryable<T> All(System.Linq.Expressions.Expression<Func<T, bool>> expression, int page, int pageSize);
        bool Add(T item);
        int Add(IEnumerable<T> items);
        bool UpdateMany(string filterField, string filterValue, string updateField, dynamic updateValue);
        UpdateResult UpdateMany(System.Linq.Expressions.Expression<Func<T, bool>> expression, UpdateDefinition<T> updateDefinition);

    }
}
