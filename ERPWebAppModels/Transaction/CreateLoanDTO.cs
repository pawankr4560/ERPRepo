namespace WebApp.Model.Transaction
{
    public class CreateLoanDTO
    {
        public string LoanNumber { get; set; }
        public List<CustomerDropdownDto> CustomerList { get; set; }
    }
  
    public class CustomerDropdownDto
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
    }
}
