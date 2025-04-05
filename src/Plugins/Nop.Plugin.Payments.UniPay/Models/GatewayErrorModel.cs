namespace Nop.Plugin.Payments.UniPay.Models
{
  public class GatewayErrorModel
  {
    public int ErrorCode { get; set; }

    public string ErrorCodeName { get; set; }

    public string ErrorCodeDescription { get; set; }
  }
}
