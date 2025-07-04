using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;
using ProductApp.Models;
using System.Data;

namespace ProductApp.Identity
{
    public class DapperRoleStore : IRoleStore<ApplicationRole>, IQueryableRoleStore<ApplicationRole>
    {
        private readonly IDbConnection _db;

        public DapperRoleStore(IDbConnection db) => _db = db;

        // IQueryableRoleStore implementation
        public IQueryable<ApplicationRole> Roles
        {
            get
            {
                try
                {
                    var roles = _db.Query<ApplicationRole>("SELECT * FROM AspNetRoles").ToList();
                    return roles.AsQueryable();
                }
                catch
                {
                    return new List<ApplicationRole>().AsQueryable();
                }
            }
        }

        public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(role.Id))
                    role.Id = Guid.NewGuid().ToString();

                if (string.IsNullOrEmpty(role.ConcurrencyStamp))
                    role.ConcurrencyStamp = Guid.NewGuid().ToString();

                var sql = @"INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                            VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp)";

                await _db.ExecuteAsync(sql, role);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Failed to create role: {ex.Message}" });
            }
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            try
            {
                await _db.ExecuteAsync("DELETE FROM AspNetUserRoles WHERE RoleId = @Id", new { role.Id });
               
                await _db.ExecuteAsync("DELETE FROM AspNetRoles WHERE Id = @Id", new { role.Id });
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Failed to delete role: {ex.Message}" });
            }
        }

        public async Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            try
            {
                return await _db.QuerySingleOrDefaultAsync<ApplicationRole>(
                    "SELECT * FROM AspNetRoles WHERE Id = @Id", new { Id = roleId });
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            try
            {
                return await _db.QuerySingleOrDefaultAsync<ApplicationRole>(
                    "SELECT * FROM AspNetRoles WHERE NormalizedName = @NormalizedName",
                    new { NormalizedName = normalizedRoleName });
            }
            catch
            {
                return null;
            }
        }

        public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
            => Task.FromResult(role.NormalizedName);

        public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
            => Task.FromResult(role.Id);

        public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
            => Task.FromResult(role.Name);

        public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            try
            {
                var sql = @"UPDATE AspNetRoles SET 
                    Name = @Name, NormalizedName = @NormalizedName, ConcurrencyStamp = @ConcurrencyStamp 
                    WHERE Id = @Id";

                await _db.ExecuteAsync(sql, role);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Failed to update role: {ex.Message}" });
            }
        }

        public void Dispose() { }
    }
}