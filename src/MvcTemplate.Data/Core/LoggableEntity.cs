using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvcTemplate.Objects;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace MvcTemplate.Data
{
    public class LoggableEntity
    {
        public String Name { get; }
        public String Action { get; }
        public Func<Int64> Id { get; }
        public Boolean IsModified { get; }
        private static String IdName { get; }
        private LoggableProperty[] Properties { get; }

        static LoggableEntity()
        {
            IdName = typeof(BaseModel).GetProperties().Single(property => property.IsDefined(typeof(KeyAttribute), false)).Name;
        }

        public LoggableEntity(EntityEntry<BaseModel> entry)
        {
            Type type = entry.Entity.GetType();
            PropertyValues values =
                entry.State == EntityState.Modified || entry.State == EntityState.Deleted
                    ? entry.GetDatabaseValues()
                    : entry.CurrentValues;

            Properties = values
                .Properties
                .Where(property => property.Name != IdName)
                .Select(property => new LoggableProperty(entry.Property(property.Name), values[property]))
                .Where(property => entry.State != EntityState.Modified || property.IsModified)
                .ToArray();

            Name = type.AssemblyQualifiedName?.StartsWith("Castle.Proxies") == true ? type.BaseType!.Name : type.Name;
            IsModified = Properties.Length > 0;
            Action = entry.State.ToString();
            Id = () => entry.Entity.Id;
        }

        public override String ToString()
        {
            StringBuilder changes = new StringBuilder();

            foreach (LoggableProperty property in Properties)
                changes.Append(property).Append('\n');

            return changes.ToString();
        }
    }
}
