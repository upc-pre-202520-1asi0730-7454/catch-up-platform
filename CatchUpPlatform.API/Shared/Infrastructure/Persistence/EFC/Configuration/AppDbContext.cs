using CatchUpPlatform.API.News.Domain.Model.Aggregates;
using CatchUpPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CatchUpPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // News Bounded Context Object-Relational Mapping Configurations
        builder.Entity<FavoriteSource>().HasKey(f => f.Id);
        builder.Entity<FavoriteSource>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<FavoriteSource>().Property(f => f.NewsApiKey).IsRequired().HasMaxLength(100);
        builder.Entity<FavoriteSource>().Property(f => f.SourceId).IsRequired().HasMaxLength(100);
        builder.Entity<FavoriteSource>().HasIndex(f => new { f.NewsApiKey, f.SourceId }).IsUnique();
        
        
        
        
        // Apply snake_case naming convention to database objects
        builder.UseSnakeCaseNamingConvention();
    }
}