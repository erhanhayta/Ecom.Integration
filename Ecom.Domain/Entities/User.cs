namespace Ecom.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = default!;    // benzersiz
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = "Admin";         // Admin, Operator vs.
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAtUtc { get; set; }
    }
}
