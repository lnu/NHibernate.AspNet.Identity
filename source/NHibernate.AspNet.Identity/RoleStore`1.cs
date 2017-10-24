using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using NHibernate.Linq;

namespace NHibernate.AspNet.Identity
{
    public class RoleStore<TRole> : IQueryableRoleStore<TRole>, IRoleStore<TRole>, IDisposable where TRole : IdentityRole
    {
        private bool _disposed;

        /// <summary>
        /// If true then disposing this object will also dispose (close) the session. False means that external code is responsible for disposing the session.
        /// </summary>
        public bool ShouldDisposeSession { get; set; }

        public ISession Context { get; private set; }

        public RoleStore(ISession context)
        {
            this.ShouldDisposeSession = true;
            this.Context = context ?? throw new ArgumentNullException("context");
        }

        public virtual Task<TRole> FindByIdAsync(string roleId)
        {
            this.ThrowIfDisposed();
            return this.Context.GetAsync<TRole>(roleId);
        }

        public virtual async Task<TRole> FindByNameAsync(string roleName)
        {
            this.ThrowIfDisposed();
            return await this.Context.Query<TRole>().FirstOrDefaultAsync(u => u.Name.ToUpper() == roleName.ToUpper());
        }

        public virtual async Task CreateAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException("role");
            this.Context.Save(role);
            await this.Context.FlushAsync();
        }

        public virtual async Task DeleteAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }
            this.Context.Delete(role);
            await this.Context.FlushAsync();
        }

        public virtual async Task UpdateAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException("role");
            this.Context.Update(role);
            await this.Context.FlushAsync();
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().Name);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this._disposed && this.ShouldDisposeSession)
                this.Context.Dispose();
            this._disposed = true;
            this.Context = null;
        }

        public IQueryable<TRole> Roles
        {
            get
            {
                this.ThrowIfDisposed();
                return this.Context.Query<TRole>();
            }
        }
    }
}
