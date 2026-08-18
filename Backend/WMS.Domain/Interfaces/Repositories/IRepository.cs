using System.Linq.Expressions;
using WMS.Domain.Common.Models;

namespace WMS.Domain.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> where);
        Task<T?> GetFirstOrDefaultWithFiltersIgnoredAsync(Expression<Func<T, bool>> where);
        Task<TOut?> GetFirstOrDefaultAsync<TOut>(Expression<Func<T, TOut>> select, Expression<Func<T, bool>> where);
        Task<T?> IncludeAndGetFirstOrDefaultAsync<TOut>(
            Expression<Func<T, bool>> where,
            params Expression<Func<T, object>>[] includes);
        Task<List<T>> IncludeAndGetAllAsync(
            Expression<Func<T, bool>> where,
            params Expression<Func<T, object>>[] includes);
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> where);
        Task<List<TOut>> GetAllAsync<TOut>(Expression<Func<T, TOut>> select, Expression<Func<T, bool>> where);
        Task<List<TOut>> GetAllAsync<TOut, TObj>
            (Expression<Func<T, TOut>> select, Expression<Func<T, bool>> where, Expression<Func<T, TObj>> orderBy, bool? isAsc = true);

        Task<PaginationDTO<TOut>> GetPaginatedList<TOut>(
            Expression<Func<T, TOut>> select,
            PaginationDTO<TOut> paginationDTO,
            Func<IQueryable<T>, IQueryable<T>>? where = null) where TOut : class;

        Task<PaginationDTO<TOut, TFilter>> GetPaginatedList<TOut, TFilter>(
            Expression<Func<T, TOut>> select,
            PaginationDTO<TOut, TFilter> paginationDTO,
            Func<IQueryable<T>, IQueryable<T>>? where = null) where TOut : class where TFilter : class;
    }
}
