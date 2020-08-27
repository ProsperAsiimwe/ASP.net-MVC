using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using SteppingStone.Domain.Entities;

namespace SteppingStone.Domain.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Activity> Activities { get; set; }
                
        public DbSet<Parent> Parents { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<ClassLevel> ClassLevels { get; set; }

        public DbSet<StudentParent> StudentParents { get; set; }

        public DbSet<StudentEvent> StudentEvents { get; set; }

        public DbSet<Expense> Expenses { get; set; }

        public DbSet<Bank> Banks { get; set; }

        public DbSet<Term> Terms { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public override int SaveChanges()
        {
            try {
                return base.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex) {
                var error = GetEntityValidationErrors(ex);
                throw new System.Data.Entity.Validation.DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    error, ex
                    ); // Add the original exception as the innerException
            }
        }

        public override async System.Threading.Tasks.Task<int> SaveChangesAsync()
        {
            try {
                return await base.SaveChangesAsync();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex) {
                var error = GetEntityValidationErrors(ex);
                throw new System.Data.Entity.Validation.DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    error, ex
                    ); // Add the original exception as the innerException
            }
        }

        private string GetEntityValidationErrors(System.Data.Entity.Validation.DbEntityValidationException ex)
        {
            var sb = new System.Text.StringBuilder();

            foreach (var failure in ex.EntityValidationErrors) {
                sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                foreach (var error in failure.ValidationErrors) {
                    sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}