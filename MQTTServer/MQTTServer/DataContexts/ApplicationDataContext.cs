using Microsoft.EntityFrameworkCore;
using MQTTServer.Models;

namespace MQTTServer.DataContexts;

public sealed class ApplicationDataContext : DbContext
{
    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Device> Devices { get; set; } = null!;
}