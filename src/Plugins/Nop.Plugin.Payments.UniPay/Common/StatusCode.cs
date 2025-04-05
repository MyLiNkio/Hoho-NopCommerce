
namespace Nop.Plugin.Payments.UniPay.Common
{
    public enum StatusCode
    {
        PROCESS = 1,        //Payment is under process
        HOLD = 2,           //Payment was hold
        SUCCESS = 3,        //Payment is successful
        REFUNDED = 5,       //Payment was refunded
        FAILED = 13,        //Payment failed due to some errors
        PARTIAL_REFUNDED = 19,//was initiated a partial refund
        INCOMPLETE_BANK = 22,//Payment timeout
        INCOMPLETE = 23,    //Payment is incomplete
        CREATED = 1000,     //Payment was created
        PROCESSING = 1001,  //Payment is processing and waiting to get update status
    }
}
