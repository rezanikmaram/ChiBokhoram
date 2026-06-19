using Common.enumeration;
using Common.Utilities;
using Data.DbContext;
using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity
    {
        protected readonly AppDbContext DbContext;
        public DbSet<TEntity> Entities { get; }
        public virtual IQueryable<TEntity> Table => Entities;
        public virtual IQueryable<TEntity> TableNoTracking => ApplyStatusFilter(Entities.AsNoTracking());

        public Repository(AppDbContext dbContext)
        {
            DbContext = dbContext;
            Entities = DbContext.Set<TEntity>(); // City => Cities
        }

        #region Async Method
        public virtual async ValueTask<TEntity> GetByIdAsync(CancellationToken cancellationToken, params object[] ids)
        {
            var entity = await Entities.FindAsync(ids, cancellationToken);
            var statusProperty = typeof(TEntity).GetProperty("Status");
            if (statusProperty != null)
            {
                if (entity != null && (DataStatus)statusProperty.GetValue(entity) != DataStatus.Deleted)
                {
                    return entity;
                }
                return null;
            }
            return entity;
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            SetValueSystem(entity);
            await Entities.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            if (saveNow)
            {
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            foreach (var entity in entities)
            {
                SetValueSystem(entity);
            }

            await Entities.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            if (saveNow)
            {
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));

            SetUpdateValueSystem(entity);

            Entities.Update(entity);
            if (saveNow)
            {
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                SetUpdateValueSystem(entity);
            }

            Entities.UpdateRange(entities);
            if (saveNow)
            {
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));

            var statusProperty = typeof(TEntity).GetProperty("RecordStatus");
            if (statusProperty != null)
            {
                statusProperty.SetValue(entity, DataStatus.Deleted);
            }

            Entities.Update(entity);

            if (saveNow)
            {
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                // تغییر وضعیت به Deleted
                var statusProperty = typeof(TEntity).GetProperty("RecordStatus");
                if (statusProperty != null)
                {
                    statusProperty.SetValue(entity, DataStatus.Deleted);
                }
            }

            Entities.UpdateRange(entities);

            if (saveNow)
            {
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        #endregion

        #region Sync Methods
        public virtual TEntity GetById(params object[] ids)
        {
            var entity = Entities.Find(ids);
            var statusProperty = typeof(TEntity).GetProperty("Status");
            if (statusProperty != null)
            {
                if (entity != null && (DataStatus)statusProperty.GetValue(entity) != DataStatus.Deleted)
                {
                    return entity;
                }
                return null;
            }
            return entity;
        }

        public virtual void Add(TEntity entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));

            SetValueSystem(entity);

            Entities.Add(entity);
            if (saveNow)
            {
                DbContext.SaveChanges();
            }
        }

        public virtual void AddRange(IEnumerable<TEntity> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                SetValueSystem(entity);
            }

            Entities.AddRange(entities);
            if (saveNow)
            {
                DbContext.SaveChanges();
            }
        }

        public virtual void Update(TEntity entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));

            SetUpdateValueSystem(entity);

            Entities.Update(entity);
            if (saveNow)
            {
                DbContext.SaveChanges();
            }
        }

        public virtual void UpdateRange(IEnumerable<TEntity> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                SetUpdateValueSystem(entity);
            }

            Entities.UpdateRange(entities);
            if (saveNow)
            {
                DbContext.SaveChanges();
            }
        }

        public virtual void Delete(TEntity entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));

            var statusProperty = typeof(TEntity).GetProperty("RecordStatus");
            if (statusProperty != null)
            {
                statusProperty.SetValue(entity, DataStatus.Deleted);
            }

            Entities.Update(entity);

            if (saveNow)
            {
                DbContext.SaveChanges();
            }
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                // تغییر وضعیت به Deleted
                var statusProperty = typeof(TEntity).GetProperty("RecordStatus");
                if (statusProperty != null)
                {
                    statusProperty.SetValue(entity, DataStatus.Deleted);
                }
            }

            Entities.UpdateRange(entities);

            if (saveNow)
            {
                DbContext.SaveChanges();
            }
        }
        #endregion

        #region Attach & Detach
        public virtual void Detach(TEntity entity)
        {
            Assert.NotNull(entity, nameof(entity));
            var entry = DbContext.Entry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
            }
        }

        public virtual void Attach(TEntity entity)
        {
            Assert.NotNull(entity, nameof(entity));
            if (DbContext.Entry(entity).State == EntityState.Detached)
            {
                Entities.Attach(entity);
            }
        }
        #endregion

        #region Explicit Loading
        public virtual async Task LoadCollectionAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Attach(entity);

            var collection = DbContext.Entry(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
            {
                await collection.LoadAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public virtual void LoadCollection<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty)
            where TProperty : class
        {
            Attach(entity);
            var collection = DbContext.Entry(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
            {
                collection.Load();
            }
        }

        public virtual async Task LoadReferenceAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Attach(entity);
            var reference = DbContext.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
            {
                await reference.LoadAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public virtual void LoadReference<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty)
            where TProperty : class
        {
            Attach(entity);
            var reference = DbContext.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
            {
                reference.Load();
            }
        }
        #endregion

        #region Private Method
        private void SetValueSystem(TEntity entity)
        {
            var properties = entity.GetType().GetProperties();
            foreach (var property in properties)
            {

                if (property.Name.ToLower() == "CreatedDate")
                {
                    property.SetValue(entity, DateTimeOffset.Now);
                }
                if (property.Name.ToLower() == "ModifiedDate")
                {
                    property.SetValue(entity, DateTimeOffset.Now);
                }
            }
        }

        private void SetUpdateValueSystem(TEntity entity)
        {
            var type = entity.GetType();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (property.Name.ToLower() == "modifieddate")
                {
                    property.SetValue(entity, DateTimeOffset.Now);
                }
                if (property.Name.ToLower() == "recordstatus")
                {
                    property.SetValue(entity, DataStatus.Edited);
                }
            }
        }
        private IQueryable<TEntity> ApplyStatusFilter(IQueryable<TEntity> query)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "p");
            var property = Expression.Property(parameter, "RecordStatus");

            // شرط برای بررسی اینکه Status != StatusEnum.Deleted
            var deletedStatus = Expression.Constant(DataStatus.Deleted);
            var notEqual = Expression.NotEqual(property, deletedStatus);

            // ساختن lambda: p => p.Status != StatusEnum.Deleted
            var lambda = Expression.Lambda<Func<TEntity, bool>>(notEqual, parameter);

            return query.Where(lambda);
        }
        #endregion
    }
}
