namespace Capstone_360s.Interfaces.IService
{
    public interface IRoleManager
    {
        public Task<IEnumerable<string>> GetRoles(Guid microsoftId);
    }
}