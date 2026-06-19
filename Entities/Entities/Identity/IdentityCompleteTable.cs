using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class ApplicationUser : IdentityUser<int>, IEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string ProfileImage { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset RegisterDate { get; set; }
        public DateTimeOffset? ForgotDate { get; set; }
        public DateTimeOffset? ConfirmDate { get; set; }
        public DateTimeOffset? LastLoginDate { get; set; }

        public virtual ICollection<ApplicationUserClaim> UserClaims { get; set; }
        public virtual ICollection<ApplicationUserLogin> UserLogins { get; set; }
        public virtual ICollection<ApplicationUserToken> UserTokens { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }

    public class ApplicationRole : IdentityRole<int>
    {
        public string Title { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; }
    }

    public class ApplicationUserRole : IdentityUserRole<int>
    {
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }

    public class ApplicationUserToken : IdentityUserToken<int>
    {
        public virtual ApplicationUser User { get; set; }
    }

    public class ApplicationRoleClaim : IdentityRoleClaim<int>
    {
        public virtual ApplicationRole Role { get; set; }
    }

    public class ApplicationUserLogin : IdentityUserLogin<int>
    {
        public virtual ApplicationUser User { get; set; }
    }

    public class ApplicationUserClaim : IdentityUserClaim<int>
    {

        public virtual ApplicationUser User { get; set; }
    }





    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError ConcurrencyFailure()
        {
            return new IdentityError
            {
                Code = nameof(ConcurrencyFailure),
                Description = "رکورد جاری پیشتر ویرایش شده‌است و تغییرات شما آن‌را بازنویسی خواهد کرد."
            };
        }

        public override IdentityError DefaultError()
        {
            return new IdentityError
            {
                Code = nameof(DefaultError),
                Description = "خطایی رخ داده‌است."
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateEmail),
                Description = string.Format("ایمیل '{0}' هم اکنون مورد استفاده است.", email)
            };
        }

        public override IdentityError DuplicateRoleName(string role)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateRoleName),
                Description = string.Format("نقش '{0}' هم اکنون مورد استفاده‌است.", role)
            };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = string.Format("نام کاربری '{0}' هم اکنون مورد استفاده‌است.", userName)
            };
        }

        public override IdentityError InvalidEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(InvalidEmail),
                Description = string.Format("ایمیل '{0}' معتبر نیست.", email)
            };
        }

        public override IdentityError InvalidRoleName(string role)
        {
            return new IdentityError
            {
                Code = nameof(InvalidRoleName),
                Description = string.Format("نقش '{0}' معتبر نیست.", role)
            };
        }

        public override IdentityError InvalidToken()
        {
            return new IdentityError
            {
                Code = nameof(InvalidToken),
                Description = "توکن غیر معتبر."
            };
        }

        public override IdentityError InvalidUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = string.Format("نام کاربری '{0}' معتبر نیست و تنها می‌تواند حاوی حروف و یا ارقام باشد.", userName)
            };
        }

        public override IdentityError LoginAlreadyAssociated()
        {
            return new IdentityError
            {
                Code = nameof(LoginAlreadyAssociated),
                Description = "این کاربر پیشتر اضافه شده‌است."
            };
        }

        public override IdentityError PasswordMismatch()
        {
            return new IdentityError
            {
                Code = nameof(PasswordMismatch),
                Description = "کلمه‌ی عبور نامعتبر."
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "کلمه‌ی عبور باید حداقل دارای یک رقم بین 0 تا 9 باشد."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresLower),
                Description = "کلمه‌ی عبور باید حداقل دارای یک حرف کوچک انگلیسی باشد."
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "کلمه‌ی عبور باید حداقل دارای یک حرف خارج از حروف الفبای انگلیسی و همچنین اعداد باشد."
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresUpper),
                Description = "کلمه‌ی عبور باید حداقل دارای یک حرف بزرگ انگلیسی باشد."
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = nameof(PasswordTooShort),
                Description = string.Format("کلمه‌ی عبور باید حداقل {0} حرف باشد.", length)
            };
        }

        public override IdentityError UserAlreadyHasPassword()
        {
            return new IdentityError
            {
                Code = nameof(UserAlreadyHasPassword),
                Description = "کلمه‌ی عبور کاربر پیشتر تنظیم شده‌است."
            };
        }

        public override IdentityError UserAlreadyInRole(string role)
        {
            return new IdentityError
            {
                Code = nameof(UserAlreadyInRole),
                Description = string.Format("کاربر هم اکنون دارای نقش '{0}' است.", role)
            };
        }

        public override IdentityError UserLockoutNotEnabled()
        {
            return new IdentityError
            {
                Code = nameof(UserLockoutNotEnabled),
                Description = "قفل شدن اکانت برای این کاربر تنظیم نشده‌است."
            };
        }

        public override IdentityError UserNotInRole(string role)
        {
            return new IdentityError
            {
                Code = nameof(UserNotInRole),
                Description = "کاربر دارای نقش '{0}' نیست."
            };
        }

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresUniqueChars),
                Description = "کلمه‌ی عبور بايد حداقل داراى {0} حرف متفاوت باشد."
            };
        }

        public override IdentityError RecoveryCodeRedemptionFailed()
        {
            return new IdentityError
            {
                Code = nameof(RecoveryCodeRedemptionFailed),
                Description = "بازيابى با شكست مواجه شد."
            };
        }
    }
    public class ApplicationUserClaimsConfiguration : IEntityTypeConfiguration<ApplicationUserClaim>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserClaim> b)
        {
            // Maps to the AspNetUserClaims table
            b.ToTable("UserClaims");
            //b.Property(c => c.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            // Primary key
            b.HasKey(uc => uc.Id);

            b.Property(e => e.ClaimType).HasColumnName("CType");
            b.Property(e => e.ClaimValue).HasColumnName("CValue");
        }
    }

    public class ApplicationUserTokensConfiguration : IEntityTypeConfiguration<ApplicationUserToken>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserToken> b)
        {
            // Maps to the AspNetUserTokens table
            b.ToTable("UserTokens");

            // Composite primary key consisting of the UserId, LoginProvider and Name
            b.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            // Limit the size of the composite key columns due to common DB restrictions
            b.Property(t => t.LoginProvider).HasMaxLength(128);
            b.Property(t => t.Name).HasMaxLength(128);
        }
    }

    public class ApplicationRoleClaimsConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
    {
        public void Configure(EntityTypeBuilder<ApplicationRoleClaim> b)
        {
            // Primary key
            b.HasKey(rc => rc.Id);
            //b.Property(c => c.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            // Maps to the AspNetRoleClaims table
            b.ToTable("RoleClaims");
        }
    }
}
