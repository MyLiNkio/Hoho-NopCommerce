using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order service interface
    /// </summary>
    public partial interface IOrderTotalCalculationServiceOverridden
    {
        /// <summary>
        /// Gets payment subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the applied discount amount. Applied discounts. Sub total (without discount). Sub total (with discount). Tax rates (of order sub total)
        /// </returns>
        Task<(decimal discountAmountInclTax, decimal discountAmountExclTax, List<Discount> appliedDiscounts, decimal subTotalWithoutDiscountInclTax, decimal subTotalWithoutDiscountExclTax, decimal subTotalWithDiscountInclTax, decimal subTotalWithDiscountExclTax, SortedDictionary<decimal, decimal> taxRates)> GetPaymentSubTotalsAsync(IList<ShoppingCartItem> cart);


        /// <summary>
        /// Gets payment subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the applied discount amount. Applied discounts. Sub total (without discount). Sub total (with discount). Tax rates (of order sub total)
        /// </returns>
        Task<(decimal discountAmount, List<Discount> appliedDiscounts, decimal subTotalWithoutDiscount, decimal subTotalWithDiscount, SortedDictionary<decimal, decimal> taxRates)> GetPaymentSubTotalsAsync(IList<ShoppingCartItem> cart, bool includingTax);


        /// <summary>
        /// Gets shopping cart total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="useRewardPoints">A value indicating reward points should be used; null to detect current choice of the customer</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shopping cart total;Null if shopping cart total couldn't be calculated now. Applied gift cards. Applied discount amount. Applied discounts. Reward points to redeem. Reward points amount in primary store currency to redeem
        /// </returns>
        Task<(decimal? shoppingCartTotal, decimal discountAmount, List<Discount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards, int redeemedRewardPoints, decimal redeemedRewardPointsAmount)> GetPaymentTotalAsync(IList<ShoppingCartItem> cart, bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true);
    }
}
