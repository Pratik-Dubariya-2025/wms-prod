using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WMS.Domain.Models;
using WMS.Domain.Common.Models;
using WMS.Domain.Interfaces.Repositories;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
    {
        public DbSet<T> DbSet = context.Set<T>();

        public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);
        public async Task AddRangeAsync(IEnumerable<T> entities) => await DbSet.AddRangeAsync(entities);
        public void Update(T entity) => DbSet.Update(entity);
        public void UpdateRange(IEnumerable<T> entities) => DbSet.UpdateRange(entities);
        public void Remove(T entity) => DbSet.Remove(entity);
        public void RemoveRange(IEnumerable<T> entities) => DbSet.RemoveRange(entities);
        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> where) =>
            await DbSet.FirstOrDefaultAsync(where);
        public async Task<T?> GetFirstOrDefaultWithFiltersIgnoredAsync(Expression<Func<T, bool>> where) =>
            await DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(where);
        public async Task<TOut?> GetFirstOrDefaultAsync<TOut>(Expression<Func<T, TOut>> select, Expression<Func<T, bool>> where) =>
            await DbSet.Where(where).Select(select).FirstOrDefaultAsync();
        public async Task<T?> IncludeAndGetFirstOrDefaultAsync<TOut>(
            Expression<Func<T, bool>> where,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = DbSet.Where(where);
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync();
        }
        public async Task<List<T>> IncludeAndGetAllAsync(
            Expression<Func<T, bool>> where,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = DbSet.Where(where);
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }
        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> where) => await DbSet.Where(where).ToListAsync();
        public async Task<List<TOut>> GetAllAsync<TOut>(Expression<Func<T, TOut>> select, Expression<Func<T, bool>> where) =>
            await DbSet.Where(where).Select(select).ToListAsync();
        public async Task<List<TOut>> GetAllAsync<TOut, TObj>
            (Expression<Func<T, TOut>> select, Expression<Func<T, bool>> where, Expression<Func<T, TObj>> orderBy, bool? isAsc = true) =>
                isAsc == true ? await DbSet.OrderBy(orderBy).Where(where).Select(select).ToListAsync() :
                    await DbSet.OrderByDescending(orderBy).Where(where).Select(select).ToListAsync();

        public async Task<PaginationDTO<TOut>> GetPaginatedList<TOut>(
            Expression<Func<T, TOut>> select,
            PaginationDTO<TOut> paginationDTO,
            Func<IQueryable<T>, IQueryable<T>>? where = null) where TOut : class
        {
            IQueryable<T> query = DbSet;

            if (where is not null)
                query = where(query);

            paginationDTO.PageNumber = paginationDTO.PageNumber == 0 ? 1 : paginationDTO.PageNumber;
            paginationDTO.ItemsPerPage = paginationDTO.ItemsPerPage == 0 ? 5 : paginationDTO.ItemsPerPage;
            paginationDTO.TotalRecords = await query.CountAsync();
            paginationDTO.TotalPages = (int)Math.Ceiling((double)paginationDTO.TotalRecords / paginationDTO.ItemsPerPage);
            int skip = (paginationDTO.PageNumber - 1) * paginationDTO.ItemsPerPage;
            paginationDTO.Records = await query.Skip(skip).Take(paginationDTO.ItemsPerPage).Select(select).ToListAsync();
            paginationDTO.RecordFrom = skip + 1;
            paginationDTO.RecordTo = skip + paginationDTO.ItemsPerPage;
            if (paginationDTO.RecordTo > paginationDTO.TotalRecords)
                paginationDTO.RecordTo = paginationDTO.TotalRecords;

            return paginationDTO;
        }

        public async Task<PaginationDTO<TOut, TFilter>> GetPaginatedList<TOut, TFilter>(
            Expression<Func<T, TOut>> select,
            PaginationDTO<TOut, TFilter> paginationDTO,
            Func<IQueryable<T>, IQueryable<T>>? where = null) where TOut : class where TFilter : class
        {
            IQueryable<T> query = DbSet;

            if (where is not null)
                query = where(query);

            paginationDTO.PageNumber = paginationDTO.PageNumber == 0 ? 1 : paginationDTO.PageNumber;
            paginationDTO.ItemsPerPage = paginationDTO.ItemsPerPage == 0 ? 5 : paginationDTO.ItemsPerPage;
            paginationDTO.TotalRecords = await query.CountAsync();
            paginationDTO.TotalPages = (int)Math.Ceiling((double)paginationDTO.TotalRecords / paginationDTO.ItemsPerPage);
            int skip = (paginationDTO.PageNumber - 1) * paginationDTO.ItemsPerPage;
            paginationDTO.Records = await query.Skip(skip).Take(paginationDTO.ItemsPerPage).Select(select).ToListAsync();
            paginationDTO.RecordFrom = skip + 1;
            paginationDTO.RecordTo = skip + paginationDTO.ItemsPerPage;
            if (paginationDTO.RecordTo > paginationDTO.TotalRecords)
                paginationDTO.RecordTo = paginationDTO.TotalRecords;

            return paginationDTO;
        }
    }
}
