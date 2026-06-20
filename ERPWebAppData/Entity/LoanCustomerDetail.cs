using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Entity
{
    public class LoanCustomerDetail
    {
        [Key]
        public int Id { get; set; }

        public int LoanId { get; set; }

        public string CustomerAadhaarNo { get; set; } = string.Empty;
        public string CustomerMobileNo { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string? CustomerCity { get; set; }
        public string? CustomerState { get; set; }
        public string? CustomerPinCode { get; set; }

        public string? GuarantorName { get; set; }
        public string? GuarantorAadhaarNo { get; set; }
        public string? GuarantorMobileNo { get; set; }
        public string? GuarantorAddress { get; set; }
        public string? GuarantorRelationship { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime F_Created_Date_Time { get; set; }
        public DateTime? F_Updated_Date_Time { get; set; }
    }
}
