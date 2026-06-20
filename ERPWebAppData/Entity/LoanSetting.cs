using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Entity
{
    public class LoanSetting
    {
        [Key]
        public int Id { get; set; }

        public bool InterestCalculationType { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
