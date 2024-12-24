﻿namespace Capstone_360s.Interfaces.IService
{
    public interface IAccessData<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(object id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(object id);
        Task Remove(T entity);
    }
}
