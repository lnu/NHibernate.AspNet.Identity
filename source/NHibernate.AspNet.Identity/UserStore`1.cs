using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using NHibernate.AspNet.Identity.Properties;
using NHibernate.Linq;

namespace NHibernate.AspNet.Identity
{
    /// <summary>
    /// Implements IUserStore using NHibernate where TUser is the entity type of the user being stored
    /// </summary>
    /// <typeparam name="TUser"/>
    public class UserStore<TUser> : IUserLoginStore<TUser>, IUserClaimStore<TUser>, IUserRoleStore<TUser>, IUserPasswordStore<TUser>, IUserSecurityStampStore<TUser>, IQueryableUserStore<TUser>, IUserStore<TUser>, IUserLockoutStore<TUser, string>, IUserEmailStore<TUser>, IUserPhoneNumberStore<TUser>, IUserTwoFactorStore<TUser, string>, IDisposable where TUser : IdentityUser
    {
        private bool _disposed;

        /// <summary>
        /// If true then disposing this object will also dispose (close) the session. False means that external code is responsible for disposing the session.
        /// </summary>
        public bool ShouldDisposeSession { get; set; }

        public ISession Context { get; private set; }

        public UserStore(ISession context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.ShouldDisposeSession = true;
            this.Context = context;
        }

        public virtual Task<TUser> FindByIdAsync(string userId)
        {
            this.ThrowIfDisposed();
            return this.GetUserAggregateAsync((TUser u) => u.Id.Equals(userId));
        }

        public virtual async Task<TUser> FindByNameAsync(string userName)
        {
            this.ThrowIfDisposed();
            return await this.GetUserAggregateAsync((TUser u) => u.UserName.ToUpper() == userName.ToUpper());
        }

        public virtual async Task CreateAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            this.Context.Save(user);
            await this.Context.FlushAsync();
        }

        public virtual async Task DeleteAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            this.Context.Delete(user);
            await this.Context.FlushAsync();
        }

        public virtual async Task UpdateAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            this.Context.Update(user);
            await this.Context.FlushAsync();
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.Context != null && this.ShouldDisposeSession)
            {
                this.Context.Dispose();
            }
            this._disposed = true;
            this.Context = null;
        }

        public virtual Task<TUser> FindAsync(UserLoginInfo login)
        {
            this.ThrowIfDisposed();
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            var query = from u in this.Context.Query<TUser>()
                        from l in u.Logins
                        where l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey
                        select u;

            return Task.FromResult(query.SingleOrDefault());
        }

        public virtual Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            user.Logins.Add(new IdentityUserLogin()
            {
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider
            });

            this.Context.SaveOrUpdate(user);
            return Task.FromResult(0);
        }

        public virtual Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            var info = user.Logins.SingleOrDefault(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
            if (info != null)
            {
                user.Logins.Remove(info);
                this.Context.Update(user);
            }

            return Task.FromResult(0);
        }

        public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            IList<UserLoginInfo> result = new List<UserLoginInfo>();
            foreach (IdentityUserLogin identityUserLogin in user.Logins)
            {
                result.Add(new UserLoginInfo(identityUserLogin.LoginProvider, identityUserLogin.ProviderKey));
            }

            return Task.FromResult(result);
        }

        public virtual Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            IList<Claim> result = new List<Claim>();
            foreach (IdentityUserClaim identityUserClaim in user.Claims)
            {
                result.Add(new Claim(identityUserClaim.ClaimType, identityUserClaim.ClaimValue));
            }

            return Task.FromResult(result);
        }

        public virtual Task AddClaimAsync(TUser user, Claim claim)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            user.Claims.Add(new IdentityUserClaim()
            {
                User = user,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            });

            return Task.FromResult(0);
        }

        public virtual Task RemoveClaimAsync(TUser user, Claim claim)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            foreach (IdentityUserClaim identityUserClaim in user.Claims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList())
            {
                user.Claims.Remove(identityUserClaim);
            }

            return Task.FromResult(0);
        }

        public virtual Task AddToRoleAsync(TUser user, string role)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException(Resources.ValueCannotBeNullOrEmpty, "role");
            }

            IdentityRole identityRole = this.Context.Query<IdentityRole>().SingleOrDefault(r => r.Name.ToUpper() == role.ToUpper());
            if (identityRole == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.RoleNotFound, new object[1] { role }));
            }
            user.Roles.Add(identityRole);

            return Task.FromResult(0);

        }

        public virtual Task RemoveFromRoleAsync(TUser user, string role)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException(Resources.ValueCannotBeNullOrEmpty, "role");
            }

            IdentityRole identityUserRole = user.Roles.FirstOrDefault(r => r.Name.ToUpper() == role.ToUpper());
            if (identityUserRole != null)
            {
                user.Roles.Remove(identityUserRole);
            }

            return Task.FromResult(0);
        }

        public virtual Task<IList<string>> GetRolesAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            else
            {
                return Task.FromResult<IList<string>>(user.Roles.Select(u => u.Name).ToList());
            }
        }

        public virtual Task<bool> IsInRoleAsync(TUser user, string role)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException(Resources.ValueCannotBeNullOrEmpty, "role");
            }
            else
            {
                return Task.FromResult(user.Roles.Any(r => r.Name.ToUpper() == role.ToUpper()));
            }
        }

        public virtual Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetPasswordHashAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            else
            {
                return Task.FromResult(user.PasswordHash);
            }
        }

        public virtual Task SetSecurityStampAsync(TUser user, string stamp)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetSecurityStampAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            else
            {
                return Task.FromResult(user.SecurityStamp);
            }
        }

        public virtual Task<bool> HasPasswordAsync(TUser user)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public IQueryable<TUser> Users
        {
            get
            {
                this.ThrowIfDisposed();
                return this.Context.Query<TUser>();
            }
        }

        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.AccessFailedCount);
        }

        public virtual Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.LockoutEnabled);
        }

        public virtual Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            DateTimeOffset dateTimeOffset;
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (user.LockoutEndDateUtc.HasValue)
            {
                DateTime? lockoutEndDateUtc = user.LockoutEndDateUtc;
                dateTimeOffset = new DateTimeOffset(DateTime.SpecifyKind(lockoutEndDateUtc.Value, DateTimeKind.Utc));
            }
            else
            {
                dateTimeOffset = new DateTimeOffset();
            }
            return Task.FromResult(dateTimeOffset);
        }

        public virtual Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount = user.AccessFailedCount + 1;
            return Task.FromResult(user.AccessFailedCount);
        }

        public virtual Task ResetAccessFailedCountAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public virtual Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            DateTime? nullable;
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (lockoutEnd == DateTimeOffset.MinValue)
            {
                nullable = null;
            }
            else
            {
                nullable = new DateTime?(lockoutEnd.UtcDateTime);
            }
            user.LockoutEndDateUtc = nullable;
            return Task.FromResult(0);
        }

        public virtual Task<TUser> FindByEmailAsync(string email)
        {
            this.ThrowIfDisposed();
            return this.GetUserAggregateAsync((TUser u) => u.Email.ToUpper() == email.ToUpper());
        }

        public virtual Task<string> GetEmailAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.Email);
        }

        public virtual Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.EmailConfirmed);
        }

        public virtual Task SetEmailAsync(TUser user, string email)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.Email = email;
            return Task.FromResult(0);
        }

        public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetPhoneNumberAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.PhoneNumber);
        }

        public virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        private Task<TUser> GetUserAggregateAsync(Expression<Func<TUser, bool>> filter)
        {
            return Task.Run(() =>
            {
                // no cartesian product, batch call. Don't know if it's really needed: should we eager load or let lazy loading do its stuff?
                var query = this.Context.Query<TUser>().Where(filter);
                query.Fetch(p => p.Roles).ToFuture();
                query.Fetch(p => p.Claims).ToFuture();
                query.Fetch(p => p.Logins).ToFuture();
                return query.ToFuture().FirstOrDefault();
            });
        }
    }
}
