

namespace VoucherManager.Models
{
    public enum VoucherStatus
    {
        Generated = 0, //Voucher just generated and can be used for printing or selling or other purposes.
        ReadyForPrinting = 10, //Voucher was prepared for printing or just sent to print center.
        DeliveredToTheShop = 20, //Voucher is delivered to the store
        OnBalanceInTheStore = 30, //Voucher is taken into the balance on the store
        Sold = 40, //Voucher is sold
        Expired = 50, //The voucher is expired
        Activated = 60, //The voucher was activated
        Redeemed = 70, //The voucher was redeemed after activation because service provider confirmed it
        PaidOut = 80, //The company has paid money to service provider for that voucher
        Canceled = 90, //The voucher was canceled because customer returned it or divided to other two vouchers
        Suspended = 100, //Due to un suspicient actions or other reason
        Rejected = 110, // A manufacturing defect or the voucher was damaged or othe reason.
    }
}
