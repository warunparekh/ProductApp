using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;
using ProductApp.Models;
using System.Data;

namespace ProductApp.Identity
{
    public class DapperUserStore :
        IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserRoleStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>,
        IQueryableUserStore<ApplicationUser>
    {
        private readonly IDbConnection _db;

        public DapperUserStore(IDbConnection db) => _db = db;

        // IQueryableUserStore implementation
        public IQueryable<ApplicationUser> Users
        {
            get
            {
                try
                {
                    var users = _db.Query<ApplicationUser>("SELECT * FROM AspNetUsers").ToList();
                    return users.AsQueryable();
                }
                catch
                {
                    return new List<ApplicationUser>().AsQueryable();
                }
            }
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Id))
                    user.Id = Guid.NewGuid().ToString();

                if (string.IsNullOrEmpty(user.SecurityStamp))
                    user.SecurityStamp = Guid.NewGuid().ToString();

                if (string.IsNullOrEmpty(user.ConcurrencyStamp))
                    user.ConcurrencyStamp = Guid.NewGuid().ToString();

                user.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                user.TwoFactorEnabled = user.TwoFactorEnabled;
                user.LockoutEnabled = user.LockoutEnabled;
                user.AccessFailedCount = user.AccessFailedCount;
                user.UserAddress = user.UserAddress ?? "";

                var sql = @"INSERT INTO AspNetUsers (
                    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
                    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, 
                    TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, UserAddress, IsAdmin
                ) VALUES (
                    @Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed, 
                    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed, 
                    @TwoFactorEnabled, @LockoutEnd, @LockoutEnabled, @AccessFailedCount, @UserAddress, @IsAdmin
                )";

                await _db.ExecuteAsync(sql, user);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Failed to create user: {ex.Message}" });
            }
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            try
            {
                await _db.ExecuteAsync("DELETE FROM AspNetUserRoles WHERE UserId = @Id", new { user.Id });
                
                await _db.ExecuteAsync("DELETE FROM AspNetUsers WHERE Id = @Id", new { user.Id });
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Failed to delete user: {ex.Message}" });
            }
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                return await _db.QuerySingleOrDefaultAsync<ApplicationUser>(
                    "SELECT * FROM AspNetUsers WHERE Id = @Id", new { Id = userId });
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            try
            {
                return await _db.QuerySingleOrDefaultAsync<ApplicationUser>(
                    "SELECT * FROM AspNetUsers WHERE NormalizedUserName = @NormalizedUserName",
                    new { NormalizedUserName = normalizedUserName });
            }
            catch
            {
                return null;
            }
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.NormalizedUserName);

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.Id);

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.UserName);

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            try
            {
                var sql = @"UPDATE AspNetUsers SET 
                    UserName = @UserName, NormalizedUserName = @NormalizedUserName, Email = @Email, 
                    NormalizedEmail = @NormalizedEmail, EmailConfirmed = @EmailConfirmed, PasswordHash = @PasswordHash, 
                    SecurityStamp = @SecurityStamp, ConcurrencyStamp = @ConcurrencyStamp, PhoneNumber = @PhoneNumber, 
                    PhoneNumberConfirmed = @PhoneNumberConfirmed, TwoFactorEnabled = @TwoFactorEnabled, 
                    LockoutEnd = @LockoutEnd, LockoutEnabled = @LockoutEnabled, AccessFailedCount = @AccessFailedCount, 
                    UserAddress = @UserAddress, IsAdmin = @IsAdmin 
                    WHERE Id = @Id";

                await _db.ExecuteAsync(sql, user);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Failed to update user: {ex.Message}" });
            }
        }

        // Password store
        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.PasswordHash);

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

        // IUserEmailStore implementation
        public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            try
            {
                return await _db.QuerySingleOrDefaultAsync<ApplicationUser>(
                    "SELECT * FROM AspNetUsers WHERE NormalizedEmail = @NormalizedEmail",
                    new { NormalizedEmail = normalizedEmail });
            }
            catch
            {
                return null;
            }
        }

        public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.Email);

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.EmailConfirmed);

        public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.NormalizedEmail);

        public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        // IUserRoleStore implementation
        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            try
            {
                var role = await _db.QuerySingleOrDefaultAsync<ApplicationRole>(
                    "SELECT * FROM AspNetRoles WHERE NormalizedName = @NormalizedName",
                    new { NormalizedName = roleName.ToUpper() });

                if (role != null)
                {
                    var existingUserRole = await _db.QuerySingleOrDefaultAsync<int>(
                        "SELECT COUNT(*) FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId",
                        new { UserId = user.Id, RoleId = role.Id });

                    if (existingUserRole == 0)
                    {
                        await _db.ExecuteAsync(
                            "INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)",
                            new { UserId = user.Id, RoleId = role.Id });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add user to role: {ex.Message}", ex);
            }
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            try
            {
                var role = await _db.QuerySingleOrDefaultAsync<ApplicationRole>(
                    "SELECT * FROM AspNetRoles WHERE NormalizedName = @NormalizedName",
                    new { NormalizedName = roleName.ToUpper() });

                if (role != null)
                {
                    await _db.ExecuteAsync(
                        "DELETE FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId",
                        new { UserId = user.Id, RoleId = role.Id });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove user from role: {ex.Message}", ex);
            }
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            try
            {
                var sql = @"SELECT r.Name FROM AspNetRoles r
                            INNER JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
                            WHERE ur.UserId = @UserId";
                var roles = await _db.QueryAsync<string>(sql, new { UserId = user.Id });
                return roles.ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            try
            {
                var sql = @"SELECT COUNT(*) FROM AspNetRoles r
                            INNER JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
                            WHERE ur.UserId = @UserId AND r.NormalizedName = @NormalizedName";
                var count = await _db.ExecuteScalarAsync<int>(sql, new { UserId = user.Id, NormalizedName = roleName.ToUpper() });
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            try
            {
                var sql = @"SELECT u.* FROM AspNetUsers u
                            INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
                            INNER JOIN AspNetRoles r ON r.Id = ur.RoleId
                            WHERE r.NormalizedName = @NormalizedName";
                var users = await _db.QueryAsync<ApplicationUser>(sql, new { NormalizedName = roleName.ToUpper() });
                return users.ToList();
            }
            catch
            {
                return new List<ApplicationUser>();
            }
        }

        public void Dispose() { }
    }
}