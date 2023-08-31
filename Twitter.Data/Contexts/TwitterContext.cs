using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Twitter.Data.Entities;

namespace Twitter.Data.Contexts
{
	public class TwitterContext : DbContext
	{
		public TwitterContext()
		{
		}

		public TwitterContext(DbContextOptions options) : base(options) { }
		public DbSet<User> Users { get; set; }
		public DbSet<Tweet> Tweets { get; set; }
		public DbSet<Follow> Follows { get; set; }
		public DbSet<Interaction> Interactions { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(@"Server=localhost;Database=Twitter;trusted_connection=true;TrustServerCertificate=true;");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>()
				.HasMany(x => x.Interactions)
				.WithOne(x => x.User)
				.OnDelete(DeleteBehavior.ClientSetNull);
		}


	}
}
