using System;
using Nop.Services.Orders;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents default values related to customers data
    /// </summary>
    public static class NopCustomerDefaultsExtended
    {
        /// <summary>
        /// Gets a system name of 'CheckoutPackage' customer role
        /// </summary>
        public static string CheckoutPackageCartItem => "CheckoutPackageCartItem";
    }
}
