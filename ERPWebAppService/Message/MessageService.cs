using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace WebApp.Service.Message
{
    public class MessageService : IMessageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public MessageService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<bool> SendLoanSmsAsync(string mobileNumber, string loanNo, string amount)
        {
            var client = _httpClientFactory.CreateClient("MSG91Client");
            var templateId = _configuration["MSG91:TemplateId"];

            // Ensure the mobile number does NOT contain a '+' sign
            // MSG91 expects format like "919876543210"
            string cleanMobile = mobileNumber.Replace("+", "").Trim();

            var payload = new Msg91FlowRequest
            {
                template_id = templateId,
                short_url = "0",
                recipients = new List<Recipient>
        {
            new Recipient
            {
                mobiles = cleanMobile,
                loanNo = loanNo,
                amount = amount
            }
        }
            };

            // Use CamelCase or exact naming depending on your Msg91FlowRequest definition
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("flow/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // CRITICAL CHECK: MSG91 can return HTTP 200 but include an error message in the body
                if (responseString.Contains("\"type\":\"error\"") || responseString.Contains("failure"))
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
public class Msg91FlowRequest
{
    public string template_id { get; set; }
    public string short_url { get; set; } = "0"; // "1" for On, "0" for Off
    public List<Recipient> recipients { get; set; }
}

public class Recipient
{
    public string mobiles { get; set; }

    // These keys must perfectly match the ##variable## names you used in the template
    public string loanNo { get; set; }
    public string amount { get; set; }
}
