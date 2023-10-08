using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MQTTServer.Models;

public sealed class User
{
    [Key]
    [Required]
    public required string UserID { get; set; }
    
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string PasswordHash { get; set; }
    
    [Required]
    public required string Salt { get; set; }
    
    public List<string>? SubscribedTopics { get; set; }
    
    [ForeignKey(nameof(Device.DeviceID))]
    public List<string>? ConnectedDevicesID { get; set; }
}