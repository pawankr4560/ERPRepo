using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Entity
{
    public class UserDetails
    {
        [Key]
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public long Mobile { get; set; }

        public string Address { get; set; } = string.Empty;
    }
}
