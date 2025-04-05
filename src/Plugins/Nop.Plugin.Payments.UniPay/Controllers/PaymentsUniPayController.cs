using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.UniPay.Common;
using Nop.Plugin.Payments.UniPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Serilog;
using Serilog.Core;

namespace Nop.Plugin.Payments.UniPay.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PaymentsUniPayController : BasePluginController
    {
        private readonly Dictionary<int, string> _errorCodeMap = new Dictionary<int, string>()
        {
            {0, "OK" },
            {403, "HTTP_AUTORIZATION_MERCHANT_ID_WRONG" },
            {402, "HTTP_AUTORIZATION_MERCHANT_NOT_DEFINED" },
            {401, "HTTP_AUTORIZATION_HASH_WRONG" },
            {101, "ERROR_MERCHANT_IS_DISABLED" },
            {102, "ERROR_MERCHANT_ID_NOT_DEFINED" },
            {103, "ERROR_MERCHANT_ORDER_ID_NOT_DEFINED" },
            {104, "ERROR_ORDER_PRICE_NOT_DEFINED" },
            {105, "ERROR_ORDER_CURRENCY_NOT_DEFINED" },
            {106, "ERROR_ORDER_CURRENCY_BAT_FORMAT" },
            {107, "ERROR_LANGUAGE_BAD_FORMAT" },
            {108, "ERROR_MIN_AMOUNT" },
            {109, "ERROR_MAX_AMOUNT" },
            {110, "ERROR_HASH" },
            {111, "ERROR_BAD_FORMAT_OF_BACKLINKS" },
            {112, "ERROR_BAD_FORMAT_OF_LOGO" },
            {113, "ERROR_BAD_OF_ITEM_IN_LIST" },
            {116, "ERROR_CARD_NOT_FOUND" },
            {117, "ERROR_CHECKOUT_DEACTIVATED" },
            {118, "ERROR_CHECKOUT_DOMAIN_NOT_RESOLVED" },
            {119, "ERROR_PLATFORM_NOT_EXISTS" },
            {120, "ERROR_CALLBACK_IS_EMPTY" },
            {121, "ERROR_REQUEST_DATA_IS_EMPTY" },
            {122, "ERROR_REASON_IS_EMPTY" },
            {123, "ERROR_ORDER_NOT_FOUND" },
            {124, "ERROR_CARD_TOKEN_IS_EMPTY" },
            {125, "ERROR_MERCHANT_IS_REJECTED" },
            {126, "ERROR_CURRENCY_IS_OUT_OF_DATE" },
            {127, "ERROR_CURRENCY_IS_NOT_CONFIGURED" },
            {128, "ERROR_PAYMENT_TYPE_IS_NOT_CORRECT" },
            {129, "ERROR_PAYMENT_CHANNEL_NOT_FOUND" },
            {130, "ERROR_RECURRING_OPTION_TURNED_OFF" },
            {140, "INSUFFICIENT_FUNDS" },
            {141, "AMOUNT_LIMIT_EXCEEDED" },
            {142, "FREQUENCY_LIMIT_EXCEEDED" },
            {143, "CARD_NOR_EFFECTIVE" },
            {144, "CARD_EXPIRED" },
            {145, "CARD_LOST" },
            {146, "CARD_STOLEN" },
            {147, "CARD_RESTRICTED" },
            {148, "DECLINED_BY_ISSUER" },
            {149, "BANK_SYSTEM_ERROR" },
            {150, "UNKNOWN" },
            {151, "AUTHENTICATION_FAILED" },
            {152, "OFFER_TIMEOUT" },
        };

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly UniPaySettings _settings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly INopFileProvider _nopFileProvider;
        private readonly ICustomerService _customerService;
        //private readonly Logger _logger;
        private readonly Nop.Services.Logging.ILogger _logger;

        public PaymentsUniPayController(
          ILocalizationService localizationService,
          INotificationService notificationService,
          IPermissionService permissionService,
          ISettingService settingService,
          IStoreContext storeContext,
          IOrderService orderService,
          IWorkContext workContext,
          UniPaySettings liqPaySettings,
          IOrderProcessingService orderProcessingService,
          IPaymentPluginManager paymentPluginManager,
          INopFileProvider nopFileProvider,
          ICustomerService customerService,
          Nop.Services.Logging.ILogger logger)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _orderService = orderService;
            _workContext = workContext;
            _settings = liqPaySettings;
            _orderProcessingService = orderProcessingService;
            _paymentPluginManager = paymentPluginManager;
            _nopFileProvider = nopFileProvider;
            _customerService = customerService;
            //_logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(_nopFileProvider.MapPath(Common.Constants.LogFilePath)).CreateLogger();
            _logger = logger;
        }

        [AuthorizeAdmin(false)]
        [Area("Admin")]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS))
                return AccessDeniedView();
            int scopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            UniPaySettings uniPaySettings = _settingService.LoadSetting<UniPaySettings>(scopeConfiguration);
            ConfigurationModel model = new ConfigurationModel(default, uniPaySettings.MerchantId, default, uniPaySettings.SecretKey, default, uniPaySettings.Language, default, uniPaySettings.Currency, default, (await _storeContext.GetCurrentStoreAsync()).Url.TrimEnd('/') + Url.RouteUrl("UniPay.Handler"));
            if (scopeConfiguration > 0)
            {
                model = new ConfigurationModel(default, uniPaySettings.MerchantId, default, uniPaySettings.SecretKey, default, uniPaySettings.Language, default, uniPaySettings.Currency, default, (await _storeContext.GetCurrentStoreAsync()).Url.TrimEnd('/') + Url.RouteUrl("UniPay.Handler"))
                {
                    MerchantId_OverrideForStore = (_settingService.SettingExists(uniPaySettings, x => x.MerchantId, scopeConfiguration) ? 1 : 0) != 0,
                    SecretKey_OverrideForStore = (_settingService.SettingExists(uniPaySettings, x => x.SecretKey, scopeConfiguration) ? 1 : 0) != 0,
                    Language_OverrideForStore = (_settingService.SettingExists(uniPaySettings, x => x.Language, scopeConfiguration) ? 1 : 0) != 0,
                    Currency_OverrideForStore = (_settingService.SettingExists(uniPaySettings, x => x.Currency, scopeConfiguration) ? 1 : 0) != 0,
                };
            }
            else
            {
                model = new ConfigurationModel(default, uniPaySettings.MerchantId, default, uniPaySettings.SecretKey, default, uniPaySettings.Language, default, uniPaySettings.Currency, default, (await _storeContext.GetCurrentStoreAsync()).Url.TrimEnd('/') + Url.RouteUrl("UniPay.Handler"));
            }
            return PluginView(model: model);
        }

        [AuthorizeAdmin(false)]
        [Area("Admin")]
        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS))
                return AccessDeniedView();
            int scopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            UniPaySettings uniPaySettings = _settingService.LoadSetting<UniPaySettings>(scopeConfiguration);
            uniPaySettings.MerchantId = model.MerchantId;
            uniPaySettings.SecretKey = model.SecretKey;
            uniPaySettings.Language = model.Language;
            uniPaySettings.Currency = model.Currency;
            await _settingService.SaveSettingOverridablePerStoreAsync(uniPaySettings, x => x.MerchantId, (model.MerchantId_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(uniPaySettings, x => x.SecretKey, (model.SecretKey_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(uniPaySettings, x => x.Language, (model.Language_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(uniPaySettings, x => x.Currency, (model.Currency_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"), true);
            return await Configure();
        }

        public async Task<IActionResult> GatewayError(int errorCode)
        {
            string str = "UNKNOWN";
            if (_errorCodeMap.ContainsKey(errorCode))
                str = _errorCodeMap[errorCode];
            string resource = await _localizationService.GetResourceAsync("Plugins.Payments.UniPay.GatewayError." + str + ".Description");
            return PluginView(model: new GatewayErrorModel()
            {
                ErrorCode = errorCode,
                ErrorCodeName = str,
                ErrorCodeDescription = resource
            });
        }

        public async Task<IActionResult> PostProcessPayment(int orderId)
        {
            _logger.Information($"Trying to process OrderId={orderId}; Customer: {(await _workContext.GetCurrentCustomerAsync()).Id}");
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
            {
                _logger.Information($"OrderId={orderId} not found for Customer: {(await _workContext.GetCurrentCustomerAsync()).Id}");
                Response.StatusCode = 404;
                return NotFound();
            }

            string successRedirectUrl = (await _storeContext.GetCurrentStoreAsync()).Url.TrimEnd('/') + Url.RouteUrl("OrderDetails", new { orderId = orderId });
            string cancellRedirectUrl = (await _storeContext.GetCurrentStoreAsync()).Url.TrimEnd('/') + Url.RouteUrl("UniPay.PaymentCanceled", new { orderId = orderId });
            string callbackUrl = (await _storeContext.GetCurrentStoreAsync()).Url.TrimEnd('/') + Url.RouteUrl("UniPay.Handler");

            string merchantUser = "GUEST";
            //ToDo make customer automaticaly registered when he does payment
            if (await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync(), true))
                merchantUser = (await _workContext.GetCurrentCustomerAsync()).Id.ToString();

            var requestParams = new OrderedDictionary {
                {"MerchantID", _settings.MerchantId},
                {"MerchantUser", merchantUser},
                {"MerchantOrderID", orderId.ToString()},
                {"OrderPrice", order.OrderTotal.ToString(CultureInfo.InvariantCulture)},
                {"OrderCurrency", _settings.Currency},
                {"SuccessRedirectUrl", Convert.ToBase64String(Encoding.ASCII.GetBytes(successRedirectUrl))},
                {"CancelRedirectUrl", Convert.ToBase64String(Encoding.ASCII.GetBytes(cancellRedirectUrl))},
                {"CallBackUrl", Convert.ToBase64String(Encoding.ASCII.GetBytes(callbackUrl))},
                {"Language", _settings.Language},
                {"OrderName", string.Format("order{0} {1}", orderId, (await _storeContext.GetCurrentStoreAsync()).Name)},
                { "OrderDescription", string.Format("order{0} {1}", orderId, (await _storeContext.GetCurrentStoreAsync()).Name) },
            };
            requestParams["Hash"] = UniPayHelper.CalculateParamsHash(requestParams, _settings.SecretKey);
            var requestJson = JsonConvert.SerializeObject(requestParams);

            _logger.Information($"OrderId={orderId}: CALL: {Common.Constants.ApiUrl}");
            _logger.Information($"OrderId={orderId}: REQUEST: {requestJson}");

            var client = new HttpClient();
            HttpResponseMessage httpResponseMessage = await client.PostAsync(Common.Constants.ApiUrl, new StringContent(requestJson, Encoding.UTF8, "application/json"));

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var message = $"OrderId={orderId}: Request is failed. Status code {(int)httpResponseMessage.StatusCode}";
                _logger.Information(message);
                SendOrderNote(order, message);

                return RedirectToRoute("OrderId={orderId}: Plugin.Payments.UniPay.GatewayError", new
                {
                    errorCode = (int)httpResponseMessage.StatusCode
                });
            }

            var httpResponseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
            _logger.Information($"OrderId={orderId}: RESPONSE: {httpResponseContent}");

            CreateOrderResponse result = null;
            try
            {
                result = JsonConvert.DeserializeObject<CreateOrderResponse>(httpResponseContent);
            }
            catch (Exception ex)
            {
                _logger.Error("OrderId={orderId}: Can't parse API response", ex);
                return RedirectToRoute("OrderId={orderId}: Plugin.Payments.UniPay.GatewayError", new
                {
                    errorCode = 500
                });
            }

            SendOrderNote(order, httpResponseContent);

            if (result.ErrorCode != "0")
            {
                _logger.Error($"OrderId={orderId}: API response with error: {result.ErrorCode}");
                return RedirectToRoute("OrderId={orderId}: Plugin.Payments.UniPay.GatewayError", new
                {
                    errorCode = int.Parse(result.ErrorCode)
                });
            }


            order.CaptureTransactionId = result.Data.UnipayOrderHashID;
            await _orderService.UpdateOrderAsync(order);
            return Redirect(result.Data.Checkout);
        }

        public async Task<IActionResult> PaymentCanceled(int orderId)
        {
            _logger.Information($"CANCELLED: User has canceled the payment for orderId={orderId}");
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    _logger.Information($"CANCELLED: OrderId={orderId} wasn't found");
                    Response.StatusCode = 404;
                    return NotFound();
                }

                return RedirectToRoute("OrderDetails", new { orderId = orderId });
            }
            catch (Exception ex)
            {
                _logger.Error($"CANCELLED: Plugin.Payments.UniPay.PaymentCanceled: OrderId={orderId}. Can't process: {ex}");
            }
            return Ok();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken(Order = 2000)]
        public async Task<IActionResult> Handler(string merchantOrderID, string status, string hash, string amount, string unipayOrderID, string errorCode, string errorMessage, string reason)
        {
            try
            {
                _logger.Information($"CALLBACK: merchantOrderID={merchantOrderID}&status={status}&hash={hash}&amount={amount}&unipayOrderID={unipayOrderID}&errorCode={errorCode}&errorMessage={errorMessage}&reason={reason}");

                if (!(await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.UniPay", null, 0) is UniPayPlugin uniPayPlugin) || !_paymentPluginManager.IsPluginActive(uniPayPlugin))
                    throw new NopException("UniPay module cannot be loaded");

                string hashCheck = UniPayHelper.ToSHA256(unipayOrderID + "|" + merchantOrderID + "|" + status + "|" + _settings.SecretKey);
                if (hash != hashCheck)
                    throw new Exception("Hash check not passed");

                if (!Enum.TryParse<StatusCode>(status, out var statusResult))
                    throw new Exception("Server has sent unknown status: {status}");

                Order order = await _orderService.GetOrderByIdAsync(int.Parse(merchantOrderID));
                SendOrderNote(order, $"Unipay: Callback Status {statusResult}");

                if (order == null)
                    throw new Exception($"The Order not found.");

                if (statusResult == Common.StatusCode.SUCCESS)
                {
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);
                    _logger.Information($"CALLBACK: OrderId={merchantOrderID} marked as Paid");
                }
                else if (statusResult == Common.StatusCode.FAILED || statusResult == Common.StatusCode.INCOMPLETE || statusResult == Common.StatusCode.INCOMPLETE_BANK)
                {
                    await _orderProcessingService.SetOrderStatusToIncomplete(order);
                    _logger.Information($"CALLBACK: OrderId={merchantOrderID} marked as Incomplete. StatusResuslt={statusResult}");
                }
                else
                {
                    _logger.Information($"CALLBACK: OrderId={merchantOrderID} didn't change OrderStatus because got unexpected payment statusResult. StatusResuslt={statusResult}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"CALLBACK: Plugin.Payments.UniPay.Handler: OrderId={merchantOrderID}. Can't process: {ex}");
            }

            return Ok();
        }

        [AuthorizeAdmin(false)]
        [Area("Admin")]
        public IActionResult GetLog()
        {
            return File(new FileStream(_nopFileProvider.MapPath(Common.Constants.LogFilePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite), "text/plain", Common.Constants.LogFileName);
        }

        protected virtual IActionResult PluginView(string viewName = null, object model = null)
        {
            ControllerContext controllerContext = ControllerContext;
            var assembly = ControllerContext.ActionDescriptor.ControllerTypeInfo.Assembly;
            //int startIndex = assembly.CodeBase.IndexOf(".Plugin.") + 8;
            //int num = assembly.CodeBase.LastIndexOf(".dll");
            //string str = assembly.CodeBase.Substring(startIndex, num - startIndex);

            string assemblyPath = assembly.Location;

            int startIndex = assemblyPath.IndexOf(".Plugin.") + 8;
            int num = assemblyPath.LastIndexOf(".dll");
            string str = assemblyPath.Substring(startIndex, num - startIndex);

            if (string.IsNullOrEmpty(viewName))
                viewName = controllerContext.ActionDescriptor.ActionName;
            return View("~/Plugins/" + str + "/Views/" + controllerContext.ActionDescriptor.ControllerName + "/" + viewName + ".cshtml", model);
        }

        private async void SendOrderNote(Order order, string note)
        {
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                DownloadId = 0,
            });
        }
    }
}
