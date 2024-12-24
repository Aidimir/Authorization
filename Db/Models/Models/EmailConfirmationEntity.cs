using System.ComponentModel.DataAnnotations;

namespace Db.Models;

public class EmailConfirmationEntity
{
    [Key] public string Email { get; set; }
    public string VerificationCode { get; set; }
    public DateTime ExpirationTime { get; set; }
    public bool IsVerified { get; set; }
}