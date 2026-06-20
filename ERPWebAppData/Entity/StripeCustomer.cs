using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Data.Entity
{
    public class StripeCustomer : BaseEntity
    {
        public string CustomerId { get; set; } = string.Empty;

        [ForeignKey(nameof(User))] 
        public string UserId { get; set; } = string.Empty;

        public virtual User User { get; set; }
    }
}
