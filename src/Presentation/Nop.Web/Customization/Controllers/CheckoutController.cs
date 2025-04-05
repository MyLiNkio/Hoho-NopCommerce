using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services.Payments;
using Nop.Web.Components;
using Nop.Web.Models.Checkout;
using Nop.Web.Customization.CustomExtentions;
using Nop.Web.Customization.Nop.Libraries.Nop.Core.Domain.Catalog;
using Nop.Plugin.Customization.CertificatesManager.Services;
using Microsoft.AspNetCore.Http;
using Nop.Core.Http.Extensions;

namespace Nop.Web.Controllers
{
    public partial class CheckoutController : BasePublicController
    {
        #region Methods (hoho. Custom one page checkout)

        [HttpGet]
        public virtual async Task<IActionResult> HohoOnePageCheckout()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            await RemoveMulticertificatesIfAny(customer, store);
            await RemovePackageIfAny(customer, store);

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            var model = await _checkoutModelFactory.HohoPrepareOnePageCheckoutModelAsync(cart);
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> HohoOpcLoadCustomerInfo(HohoOnePageCheckoutModel model)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            await RemoveMulticertificatesIfAny(customer, store);
            await RemovePackageIfAny(customer, store);

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                throw new Exception("The cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            if (model.CustomerInfo == null)
                model.CustomerInfo = new CheckoutCustomerModel();

            if (customer.BillingAddressId != null)
            {
                var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);
                if (billingAddress == null)
                    throw new Exception($"Didn't find correspond address in database: CustomerId: {customer.Id}, BillingAddresId: {customer.BillingAddressId}");

                model.CustomerInfo.FirstName = billingAddress.FirstName;
                model.CustomerInfo.LastName = billingAddress.LastName;
                model.CustomerInfo.PhoneNumber = billingAddress.PhoneNumber;
                model.CustomerInfo.Email = billingAddress.Email;
            }
            else
            {
                model.CustomerInfo.FirstName = customer.FirstName;
                model.CustomerInfo.LastName = customer.LastName;
                model.CustomerInfo.PhoneNumber = customer.Phone;
                model.CustomerInfo.Email = customer.Email;
            }

            //Save Package type firstly as shipping options depend on product type - shippable or no (isShippingEnabled)
            await SavePackaging(model, customer, store);

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "customer-info",
                    html = await RenderPartialViewWithPrefixToStringAsync("HohoOpcCustomerInfo", model.CustomerInfo, nameof(model.CustomerInfo)),
                    summary_html = await RenderViewComponentToStringAsync(typeof(CheckoutOrderTotalsViewComponent)),
                }
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> HohoOpcLoadDeliveryDetails(HohoOnePageCheckoutModel model)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");

            //validate model
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    warning = new { message = await _localizationService.GetResourceAsync("CheckoutPanel.CustomerInfo.Warning") },
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "customer-info",
                        html = await RenderPartialViewWithPrefixToStringAsync("HohoOpcCustomerInfo", model.CustomerInfo, nameof(model.CustomerInfo)),
                        summary_html = await RenderViewComponentToStringAsync(typeof(CheckoutOrderTotalsViewComponent)),
                    },
                });
            }

            //Update customer info
            if (await _customerService.IsGuestAsync(customer))
            {
                customer.FirstName = model.CustomerInfo.FirstName;
                customer.LastName = model.CustomerInfo.LastName;
                customer.Phone = model.CustomerInfo.PhoneNumber;
                customer.Email = model.CustomerInfo.Email;

                await _customerService.UpdateCustomerAsync(customer);
            }

            if (customer.BillingAddressId == null)
            {
                var billingAddress = new Address();
                billingAddress.FirstName = customer.FirstName;
                billingAddress.LastName = customer.LastName;
                billingAddress.PhoneNumber = customer.Phone;
                billingAddress.Email = customer.Email;
                billingAddress.CreatedOnUtc = DateTime.UtcNow;
                await _addressService.InsertAddressAsync(billingAddress);

                customer.BillingAddressId = billingAddress.Id;
                await _customerService.UpdateCustomerAsync(customer);
                await _customerService.InsertCustomerAddressAsync(customer, billingAddress);
            }
            else
            {
                var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);
                if (billingAddress == null)
                    throw new Exception($"Didn't find correspond address in database: CustomerId: {customer.Id}, BillingAddresId: {customer.BillingAddressId}");

                billingAddress.FirstName = model.CustomerInfo.FirstName;
                billingAddress.LastName = model.CustomerInfo.LastName;
                billingAddress.PhoneNumber = model.CustomerInfo.PhoneNumber;
                billingAddress.Email = model.CustomerInfo.Email;

                await _addressService.UpdateAddressAsync(billingAddress);
            }

            model.ShippingMethods = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));
            model.PaymentMethods = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, 0);

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "delivery-details",
                    //html = await RenderPartialViewWithPrefixToStringAsync("HohoOpcDeliveryDetails", model, nameof(model.CustomerInfo))
                    html = await RenderPartialViewToStringAsync("HohoOpcDeliveryDetails", model),
                    summary_html = await RenderViewComponentToStringAsync(typeof(CheckoutOrderTotalsViewComponent)),
                }
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> HohoOpcPayOrder(HohoOnePageCheckoutModel model, IFormCollection form)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var store = await _storeContext.GetCurrentStoreAsync();
                await RemoveMulticertificatesIfAny(customer, store);

                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync(customer))
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));

                //validate model
                if (!ModelState.IsValid)
                {
                    model.ShippingMethods = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));
                    model.PaymentMethods = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, 0);
                    ViewData.Add("IsValidModelState", false);

                    return Json(new
                    {
                        warning = new { message = await _localizationService.GetResourceAsync("CheckoutPanel.CustomerInfo.Warning") },
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "delivery-details",
                            //html = await RenderPartialViewWithPrefixToStringAsync("HohoOpcDeliveryDetails", model, nameof(model.CustomerInfo))
                            html = await RenderPartialViewToStringAsync("HohoOpcDeliveryDetails", model),
                            summary_html = await RenderViewComponentToStringAsync(typeof(CheckoutOrderTotalsViewComponent)),
                        }
                    });
                }

                var billingAddress = await SaveBillingAddressDetails(model, customer);
                await SaveMulticertificate(model, customer, store, cart, billingAddress);
                //IMPORTANT: to update a cart variable as SaveMulticertificate() added one more product to the cart
                cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                //remove shiping details
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, store.Id);

                if (model.CheckoutDetails.UseElectronicCertificate)
                {
                    //remove packaging prouct record
                    await _genericAttributeService.SaveAttributeAsync<string>(customer, NopCustomerDefaultsExtended.CheckoutPackageCartItem, null, store.Id);
                }
                else
                {
                    await SaveShippingMethod(model, form, customer, store, cart);
                    await SaveShippingAddress(model, customer, billingAddress);
                }

                var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, 0);
                //Currently we have only Unipay payment method. So just adding it by default choose step.
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName,
                    store.Id);

                //place order
                var processPaymentRequest = await HttpContext.Session.GetAsync<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    processPaymentRequest = new ProcessPaymentRequest();

                    //!!!this is new from Nopcommerce in 4.80 but currently it doesn't fit to custom logic.
                    //Check whether payment workflow is required
                    //if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                    //{
                    //    throw new Exception("Payment information is not entered");
                    //}                    
                }

                await _paymentService.GenerateOrderGuidAsync(processPaymentRequest);
                processPaymentRequest.StoreId = store.Id;
                processPaymentRequest.CustomerId = customer.Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
                await HttpContext.Session.SetAsync("OrderPaymentInfo", processPaymentRequest);
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);

                if (placeOrderResult.Success)
                {
                    await HttpContext.Session.SetAsync<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };

                    var paymentMethod = await _paymentPluginManager
                        .LoadPluginBySystemNameAsync(placeOrderResult.PlacedOrder.PaymentMethodSystemName, customer, store.Id);
                    if (paymentMethod == null)
                        //payment method could be null if order total is 0
                        //success
                        return Json(new { success = 1 });

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                        //redirect
                        return Json(new
                        {
                            redirect = $"{_webHelper.GetStoreLocation()}checkout/OpcCompleteRedirectionPayment"
                        });
                    }

                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);
                    //success
                    return Json(new { success = 1 });
                }


                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "confirm-order",
                        html = await RenderPartialViewToStringAsync("OpcConfirmOrder", model),
                        summary_html = await RenderViewComponentToStringAsync(typeof(CheckoutOrderTotalsViewComponent)),
                    },
                    goto_section = "confirm_order"
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }




        private async Task SavePackaging(HohoOnePageCheckoutModel model, Customer customer, Store store)
        {
            if (!model.CheckoutDetails.UseElectronicCertificate && model.CheckoutDetails.BoxProductId.HasValue)
            {
                var packageProduct = await _productService.GetProductByIdAsync(model.CheckoutDetails.BoxProductId.Value);
                if (packageProduct == null)
                    throw new Exception("Can't find packaging product");

                var warnings = await _shoppingCartService.AddToCartAsync(customer, packageProduct, ShoppingCartType.ShoppingCart, store.Id);

                if (warnings.Any())
                    throw new Exception($"CheckoutController.cs -> HohoOpcLoadCustomerInfo: {warnings.ToArray()}");
            }
        }

        private async Task SaveShippingAddress(HohoOnePageCheckoutModel model, Customer customer, Address billingAddress)
        {
            if (!model.CheckoutDetails.PickupInStore)
            {
                Address shippingAddress = new Address();
                if (model.CheckoutDetails.SendToCustomer)
                    shippingAddress = billingAddress;
                else if (model.CheckoutDetails.SendToRecipient)
                {
                    //delivery address
                    shippingAddress.FirstName = model.RecipientInfo.FirstName;
                    shippingAddress.LastName = model.RecipientInfo.LastName;
                    shippingAddress.PhoneNumber = model.RecipientInfo.PhoneNumber;
                    shippingAddress.Email = model.RecipientInfo.Email;
                    shippingAddress.City = model.RecipientInfo.City;
                    shippingAddress.Address1 = model.RecipientInfo.Address;

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                        shippingAddress.FirstName, shippingAddress.LastName, shippingAddress.PhoneNumber,
                        shippingAddress.Email, shippingAddress.FaxNumber, shippingAddress.Company,
                        shippingAddress.Address1, shippingAddress.Address2, shippingAddress.City,
                        shippingAddress.County, shippingAddress.StateProvinceId, shippingAddress.ZipPostalCode,
                        shippingAddress.CountryId, null);

                    if (address == null)
                    {
                        //address is not found. let's create a new one
                        address = shippingAddress;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        await _addressService.InsertAddressAsync(address);
                        await _customerService.InsertCustomerAddressAsync(customer, address);
                    }
                    else
                    {
                        shippingAddress = address;
                    }
                }
                customer.ShippingAddressId = shippingAddress.Id;
                await _customerService.UpdateCustomerAsync(customer);
            }
        }

        private async Task SaveMulticertificate(HohoOnePageCheckoutModel model, Customer customer, Store store, IList<ShoppingCartItem> cart, Address billingAddress)
        {
            var attributes = new MulticertificateAttributes();
            attributes.SenderFirstName = billingAddress.FirstName;
            attributes.SenderLastName = billingAddress.LastName;
            attributes.SenderEmail = billingAddress.Email;

            attributes.RecipientFirstName = attributes.SenderFirstName;
            attributes.RecipientLastName = attributes.SenderLastName;
            attributes.RecipientEmail = attributes.SenderEmail;

            if (model.CheckoutDetails.SendToRecipient)
            {
                attributes.RecipientFirstName = model.RecipientInfo.FirstName;
                attributes.RecipientLastName = model.RecipientInfo.LastName;

                if (model.CheckoutDetails.UseElectronicCertificate)
                    attributes.RecipientEmail = model.RecipientInfo.Email;
            }

            //attributes.SendAnonymously = false;
            //var minutesToAdd = 0;
            //attributes.SendAtUTC = DateTime.UtcNow.AddMinutes(minutesToAdd);
            //attributes.Message = "Happy Birthday! May this special day bring you everything you desire and more. Here's to health, success, and happiness in the year ahead. Enjoy your day to the fullest, and may all your dreams come true. Cheers to you on this wonderful occasion!";

            await AddMultiCertificateToCart(attributes, model.CheckoutDetails.UseElectronicCertificate, customer, store.Id, cart);
        }

        private async Task<Address> SaveBillingAddressDetails(HohoOnePageCheckoutModel model, Customer customer)
        {
            if (customer.BillingAddressId == null)
                throw new Exception($"CheckoutController.HohoOpcPayOrder: Customer {customer.Id} doesn't have billing address");

            var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);
            if (billingAddress == null)
                throw new Exception($"Didn't find correspond address in database: CustomerId: {customer.Id}, BillingAddresId: {customer.BillingAddressId}");

            if (model.CheckoutDetails.SendToCustomer && !model.CheckoutDetails.UseElectronicCertificate)
            {
                billingAddress.City = model.RecipientInfo.City;
                billingAddress.Address1 = model.RecipientInfo.Address;
                await _addressService.UpdateAddressAsync(billingAddress);
            }

            return billingAddress;
        }

        private async Task SaveShippingMethod(HohoOnePageCheckoutModel model, IFormCollection form, Customer customer, Store store, IList<ShoppingCartItem> cart)
        {
            //save shipping method
            if (model.CheckoutDetails.PickupInStore)
            {
                var pickupOption = await ParsePickupOptionAsync(cart, form);
                //saves shiping option and selected pickup point
                await SavePickupOptionAsync(pickupOption);
                return;
            }

            //parse selected shipping method
            var shippingoption = form["shippingoption"].ToString();//.Split(new[] { "___" }, StringSplitOptions.None);
            if (string.IsNullOrEmpty(shippingoption))
                throw new Exception("Selected shipping method can't be parsed");
            var splittedOption = shippingoption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedOption.Length != 2)
                throw new Exception("Selected shipping method can't be parsed");
            var selectedName = splittedOption[0];
            var shippingRateComputationMethodSystemName = splittedOption[1];

            //find it
            //performance optimization, try cache first
            var shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(customer,
                NopCustomerDefaults.OfferedShippingOptionsAttribute, store.Id);
            if (shippingOptions == null || !shippingOptions.Any())
            {
                //not found? let's load them using shipping service
                shippingOptions = (await _shippingService.GetShippingOptionsAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer),
                    customer, shippingRateComputationMethodSystemName, store.Id)).ShippingOptions.ToList();
            }
            else
            {
                //loaded cached results. let's filter result by a chosen shipping rate computation method
                shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            var shippingOption = shippingOptions
                .Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
            if (shippingOption == null)
                throw new Exception("Selected shipping method can't be loaded");

            //save
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption, store.Id);
        }

        private async Task RemoveMulticertificatesIfAny(Customer customer, Store store)
        {
            await _shoppingCartService.DeleteProductsWithSpecialProductTypeIfAnyAsync(customer, SpecialProductType.ElectronicCertificate, store.Id, _genericAttributeService, _productService);
            await _shoppingCartService.DeleteProductsWithSpecialProductTypeIfAnyAsync(customer, SpecialProductType.PhisicalCertificate, store.Id, _genericAttributeService, _productService);
        }

        private async Task RemovePackageIfAny(Customer customer, Store store)
        {
            await _shoppingCartService.DeleteProductsWithSpecialProductTypeIfAnyAsync(customer, SpecialProductType.PackagingBoxes, store.Id, _genericAttributeService, _productService);
        }

        private SpecialProductType GetCertificateType(bool useElectronicCertificate)
        {
            return useElectronicCertificate ? SpecialProductType.ElectronicCertificate : SpecialProductType.PhisicalCertificate;
        }

        private async Task<Product> GetCertificateProduct(bool useElectronicCertificate)
        {
            var certificates = await _productService.GetProductsBySpecialProductTypeAsync(GetCertificateType(useElectronicCertificate), _genericAttributeService);
            Product certificate = null;
            if (certificates != null)
            {
                certificate = certificates.FirstOrDefault();
                if (certificate != null)
                    return certificate;
            }

            throw new Exception("No certificate product found. Check if correspond product is added to the Product table and/or it properly bound to `SpecialProductType` in genericAttribute table.");
        }

        private async Task AddMultiCertificateToCart(MulticertificateAttributes attributes, bool useElectronicCertificate, Customer customer, int storeId, IList<ShoppingCartItem> cart)
        {
            var certificate = await GetCertificateProduct(useElectronicCertificate);

            var multicertificateAttributeService = HttpContext.RequestServices.GetService<IMulticertificateAttributeService>();
            var attributesXml = multicertificateAttributeService.SetMulticertificateAttributes(null, attributes);

            var maxPrice = decimal.Zero;
            foreach (var item in cart)
            {
                var specialProductType = await _productService.GetSpecialProductTypeByIdAsync(item.ProductId, _genericAttributeService, storeId);
                if (specialProductType != SpecialProductType.Undefined)
                    continue;

                //var product = await _productService.GetProductByIdAsync(item.ProductId);
                var (unitPrice, discountAmount, appliedDiscounts) = await _shoppingCartService.GetUnitPriceAsync(item, true);
                if (maxPrice < unitPrice)
                    maxPrice = unitPrice;
            }

            var warnings = await _shoppingCartService.AddToCartAsync(customer, certificate, ShoppingCartType.ShoppingCart, storeId, attributesXml: attributesXml, customerEnteredPrice: maxPrice);

            if (warnings.Any())
                throw new Exception($"AddMultiCertificateToCart: Can't add MultiCartificate to the cart: {string.Join(',', warnings)}");
        }

        protected virtual async Task<string> RenderPartialViewWithPrefixToStringAsync(string viewName, object model, string prefix)
        {
            var newViewData = new ViewDataDictionary(ViewData);
            newViewData.TemplateInfo.HtmlFieldPrefix = prefix;

            //get Razor view engine
            var razorViewEngine = EngineContext.Current.Resolve<IRazorViewEngine>();

            //create action context
            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor, ModelState);

            //set view name as action name in case if not passed
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            //set model
            newViewData.Model = model;

            //try to get a view by the name
            var viewResult = razorViewEngine.FindView(actionContext, viewName, false);
            if (viewResult.View == null)
            {
                //or try to get a view by the path
                viewResult = razorViewEngine.GetView(null, viewName, false);
                if (viewResult.View == null)
                    throw new ArgumentNullException($"{viewName} view was not found");
            }
            await using var stringWriter = new StringWriter();
            var viewContext = new ViewContext(actionContext, viewResult.View, newViewData, TempData, stringWriter, new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);
            return stringWriter.GetStringBuilder().ToString();
        }
        #endregion
    }
}
