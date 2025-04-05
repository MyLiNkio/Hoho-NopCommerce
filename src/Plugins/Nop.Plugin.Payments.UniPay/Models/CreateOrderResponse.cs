
namespace Nop.Plugin.Payments.UniPay.Models
{
  public class CreateOrderResponse
  {
    public string ErrorCode { get; set; }

    public string Message { get; set; }

    public Data Data { get; set; }
  }
}
