namespace WebApp.Model.Transaction
{
    public class CreateLoanDTO
    {
        public string LoanNumber { get; set; }
        public List<CustomerDropdownDto> CustomerList { get; set; }
    }
  
    public class CustomerDropdownDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
