namespace WebApp.Model.Transaction
{
    public class LoanCustomerDetailRequestModel
    {
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
    }
}
