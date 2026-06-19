using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.Utilities;
using Entities;
using Entities.Entities;
using Entities.Entities.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Data.DbContext
{
    public partial class AppDbContext
        : IdentityDbContext<
            ApplicationUser,
            ApplicationRole,
            int,
            ApplicationUserClaim,
            ApplicationUserRole,
            ApplicationUserLogin,
            ApplicationRoleClaim,
            ApplicationUserToken
        >
    {
        public AppDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<City> Cities { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("");
        //    base.OnConfiguring(optionsBuilder);

        //}




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //دریافت اسمبلی لایه انتیتی
            //با گرفتن تایپ آو یک کلاس از لایه انتیتی اسمبلی آن بدست میاد
            var entityAssembly = typeof(IEntity).Assembly;

            //رجیستر کردن تمام کلاس های که از اینترفیس ای اینتیتی ارث بری کرده اند به عنوان جدول
            // modelBuilder.RegisterAllEntities<IEntity>(entityAssembly);


            //modelBuilder.ApplyConfiguration(new PostConfiguration());بجای این از  دستورات زیر استفاده میکنیم

            //modelBuilder.RegisterEntityTypeConfiguration(entityAssembly);

            //باحذف یک رکورد که فرزند دارد، عمل حذف انجام نمی شود
            modelBuilder.AddRestrictDeleteBehaviorConvention();

            //سکونشیال کردن Guid==> کارایی بالاتری نسبت به نوع معمولی داره
            modelBuilder.AddSequentialGuidForIdConvention();

            modelBuilder.AddPluralizingTableNameConvention();

            modelBuilder.ApplyConfigurationsFromAssembly(entityAssembly);
        }

        public override int SaveChanges()
        {
            _cleanString();
            AddAuitInfo();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            _cleanString();
            AddAuitInfo();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            _cleanString();
            // AddAuitInfo();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _cleanString();
            AddAuitInfo();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void _cleanString()
        {
            var changedEntities = ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);
            foreach (var item in changedEntities)
            {
                if (item.Entity == null)
                    continue;

                var properties = item
                    .Entity.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(string));

                foreach (var property in properties)
                {
                    var propName = property.Name;

                    var val = (string)property.GetValue(item.Entity, null);

                    if (val.HasValue())
                    {
                        var newVal = val.Fa2En().FixPersianChars();
                        if (newVal == val)
                            continue;
                        property.SetValue(item.Entity, newVal, null);
                    }
                }
            }
        }

        private void AddAuitInfo()
        {
            IEnumerable<EntityEntry> entries = ChangeTracker
                .Entries()
                .Where(x => /*(x.Entity is BaseEntity<int> || x.Entity is BaseEntity<Guid>) &&*/
                    (x.State == EntityState.Added || x.State == EntityState.Modified)
                );

            foreach (EntityEntry entry in entries)
            {
                var properties = entry.Entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var item in properties)
                {
                    //if (item.PropertyType==typeof(Guid))
                    //{
                    //    ((BaseEntity<Guid>)entry.Entity).CreatedDate = DateTimeOffset.Now;
                    //    continue;

                    //}
                    //else
                    //{
                    //    ((BaseEntity<int>)entry.Entity).CreatedDate = DateTimeOffset.Now;
                    //    continue;
                    //}

                    if (entry.State == EntityState.Added)
                    {
                        if (item.Name == "CreatedDate" || item.Name == "ModifiedDate")
                        {
                            item.SetValue(entry.Entity, DateTimeOffset.Now);
                        }
                    }
                    else
                    {
                        if (item.Name == "ModifiedDate")
                        {
                            item.SetValue(entry.Entity, DateTimeOffset.Now);
                        }
                    }
                }
                //if (entry.State == EntityState.Added)
                //{

                //    ((BaseEntity<object>)entry.Entity).CreatedDate = DateTimeOffset.Now;

                //    ((BaseEntity<object>)entry.Entity).ModifiedDate = DateTimeOffset.Now;

                //}
                //else
                //{
                //    ((BaseEntity<object>)entry.Entity).ModifiedDate = DateTimeOffset.Now;

                //}
            }
        }
    }
}
