using Microsoft.EntityFrameworkCore;

namespace webapi.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
       : base(options)
        {
        }

        //public DbSet<TodoItem> TodoItems { get; set; } = null!;
        public DbSet<TodoItem> TodoItems { get; set; }
        //public DbSet<TodoItem> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TodoItem>(entity =>
            {
                entity.ToTable("items");
                entity.HasKey(e => e.Name); // Указываем что Name - первичный ключ
                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired(); // Обязательное поле
            });
        }
    }
}
