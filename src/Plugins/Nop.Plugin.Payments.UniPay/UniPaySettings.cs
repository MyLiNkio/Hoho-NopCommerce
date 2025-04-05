using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.UniPay
{
  public class UniPaySettings : ISettings
  {
    public string MerchantId { get; set; }

    public string SecretKey { get; set; }

    public string Language { get; set; } = "GE";

    public string Currency { get; set; } = "GEL";
  }
}
