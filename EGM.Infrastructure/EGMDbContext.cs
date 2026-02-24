using EGM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGM.Infrastructure;

public class EGMDbContext : DbContext
{
    public EGMDbContext(DbContextOptions<EGMDbContext> options) : base(options)
    {
        
    }

    //Kullanıcılar için Dbset
    public DbSet<User> Users {get; set;} = null!;
}
