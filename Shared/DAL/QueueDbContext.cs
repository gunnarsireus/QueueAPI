using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Shared.DAL;

public class QueueDbContext : DbContext
{
    private readonly string _connectionString;

    public QueueDbContext()
    {
    }
    public QueueDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public QueueDbContext(DbContextOptions<QueueDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_connectionString != null)
        {
            base.OnConfiguring(optionsBuilder.UseSqlServer(_connectionString));
        }
        else
        {
            base.OnConfiguring(optionsBuilder);
        }
    }

    public virtual DbSet<ServerQueueEntity> ServerQueue { get; set; }
    public virtual DbSet<ClientQueueEntity> ClientQueue { get; set; }
}
