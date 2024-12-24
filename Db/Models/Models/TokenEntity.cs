using System.ComponentModel.DataAnnotations;

namespace Db.Models;

public class TokenEntity
{
    [Key] public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public bool IsRefreshToken { get; set; }

    public DateTime ExpirationDate { get; set; }
}