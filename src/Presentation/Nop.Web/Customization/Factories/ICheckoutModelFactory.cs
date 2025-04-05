using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Payments;
using Nop.Web.Models.Checkout;

namespace Nop.Web.Factories
{
    public partial interface ICheckoutModelFactory
    {
        /// <summary>
        /// Hoho. Prepare one page checkout model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the one page checkout model
        /// </returns>
        Task<HohoOnePageCheckoutModel> HohoPrepareOnePageCheckoutModelAsync(IList<ShoppingCartItem> cart);
    }
}
