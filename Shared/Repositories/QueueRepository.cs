using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.DAL;
using Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Shared.Repositories;

public class QueueRepository : IQueueRepository
{
    private readonly IConfiguration _configuration;

    public QueueRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public async Task<ClientQueueEntity> GetMessageFromQlientQueue()
    {
        await using QueueDbContext context = GetContext();

        var item = await context.ClientQueue
            .Where(q => q.QueueStatus == QueueStatus.New)
            .OrderBy(q => q.Created)
            .FirstOrDefaultAsync();

        if (item == null) return null;

        var sql = $"update ClientQueue set QueueStatus = {(int)QueueStatus.Processed}, StatusDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}' where Id = '{item.Id}'";
        var result = await context.Database.ExecuteSqlRawAsync(sql);
        return item;
    }


    public async Task<ServerQueueEntity> GetMessageFromServerQueueByCorrelationId(Guid correlationId)
    {
        await using QueueDbContext context = GetContext();

        var item = await context.ServerQueue
            .Where(q => q.CorrelationId == correlationId)
            .FirstOrDefaultAsync();

        if (item == null) return null;

        var sql = $"update ServerQueue set QueueStatus = {(int)QueueStatus.Processed}, StatusDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}' where Id = '{item.Id}'";
        var result = await context.Database.ExecuteSqlRawAsync(sql);
        return item;
    }


    public async Task<int> AddClientQueueItemAsync(ClientQueueEntity entity)
    {
        var _context = GetContext();
        _context.ClientQueue.Add(entity);
        return await _context.SaveChangesAsync();
    }


    public async Task<int> AddServerQueueItemAsync(ServerQueueEntity entity)
    {
        var _context = GetContext();
        _context.ServerQueue.Add(entity);

        return await _context.SaveChangesAsync();
    }

    private QueueDbContext GetContext()
    {
        return new QueueDbContext(_configuration.GetConnectionString("QueueDbConnection"));
    }
}

