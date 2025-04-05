/*
** nopCommerce one page checkout
*/



var CheckoutLoaderHelper = {
  form: null,
  actionName: "",
  priceDetails: null,
  previousSection: "",
  currentActiveSection: "",

  init: function (form_selector, price_details) {
    this.formSelector = form_selector;
    this.priceDetails = price_details;
    this.updatePriceDetails();
  },

  setNextActionName: function (name) {
    this.actionName = name;
  },

  setPackagingPrice: function (value) {
    this.priceDetails.packaging.value = parseFloat(value);
    this.recalculateTotalSum();
    this.updatePriceDetails();
  },

  setShipingPrice: function (value) {
    this.priceDetails.shipping.value = parseFloat(value);
    this.recalculateTotalSum();
  },

  recalculateTotalSum: function () {
    this.priceDetails.total.value = this.priceDetails.certificate.value + this.priceDetails.packaging.value + this.priceDetails.shipping.value;
  },

  updatePriceDetails: function () {
    var certificate = $('#' + this.priceDetails.certificate.id);
    var packaging = $('#' + this.priceDetails.packaging.id);
    var shipping = $('#' + this.priceDetails.shipping.id);
    var total = $('#' + this.priceDetails.total.id);

    if (certificate) {
      certificate.text(this.priceDetails.certificate.value.toFixed(2));
    }

    if (packaging) {
      packaging.text(this.priceDetails.packaging.value.toFixed(2));
    }

    if (shipping) {
      shipping.text(this.priceDetails.shipping.value.toFixed(2));
    }

    if (total) {
      total.text(this.priceDetails.total.value.toFixed(2));
    }
  },

  request: function (requestUrl, successhandler, errorHandler) {
    if (Checkout.loadWaiting !== false) return;

    if (requestUrl.includes("http://")) {
      requestUrl = requestUrl.replace("http://", "https://");
    }

    Checkout.setLoadWaiting('billing');
    $.ajax({
      cache: false,
      url: requestUrl,
      data: $(this.formSelector).serialize(),
      type: "POST",
      success: successhandler,
      complete: this.complete,
      error: errorHandler,
    });
  },

  nextRequest: function () {
    if (Checkout.loadWaiting !== false) return;

    Checkout.setLoadWaiting('billing');
    $.ajax({
      cache: false,
      url: this.actionName,
      data: $(this.formSelector).serialize(),
      type: "POST",
      success: this.success,
      complete: this.complete,
      error: this.error, /* Checkout.ajaxFailure*/
    });
  },

  complete: function () {
    //dont use "this." here when call params, it won't work;
    Checkout.setLoadWaiting(false);
  },

  error: function (response) {
    //dont use "this." here when call params, it won't work;
    alert("request error");
    console.log(response);
  },

  success: function (response) {
    //dont use "this." here when call params, it won't work;

    Checkout.setLoadWaiting(false);

    if (response.update_section) {
      $('#' + response.update_section.name).html(response.update_section.html);
    }

    if (response.next_step_request_action) {
      CheckoutLoaderHelper.setNextActionName(response.next_step_request_action);
    }
    else {
      //TODO: set error;
      alert("no response.next_step_request_action data");
    }
  },
};

var BtnsControler = {
  buyingAsGiftBtn: null,
  pickUpBlock: null,
  sendToMeBlock: null,
  SendToRecipBlock: null,

  buyingAsGiftState: false,
  useCertificate: false,
  pickupInStoreState: false,
  SendToMeState: false,
  SendToRecipientState: false,


  init: function (useCertificate, buyingAsGiftBtnId, pickupBlockId, sendToMeBlockId, sendToRecipientBlockId) {
    this.useCertificate = useCertificate;
    this.buyingAsGiftBtn = $('#' + buyingAsGiftBtnId);
    this.pickUpBlock = $('#' + pickupBlockId);
    this.sendToMeBlock = $('#' + sendToMeBlockId);
    this.SendToRecipBlock = $('#' + sendToRecipientBlockId);
    this.checkBtnState();
    this.updateDeliveryBtnsVisibility();
  },

  update: function () {
    this.checkBtnState();
    this.updateDeliveryBtnsVisibility();
  },

  checkBtnState: function () {
    if (this.buyingAsGiftBtn.is(':checked')) {
      this.buyingAsGiftState = this.buyingAsGiftBtn.val();
    }
    else {
      this.buyingAsGiftState = false;
    }
  },

  updateDeliveryBtnsVisibility: function () {
    this.hideDeliveryBtns();

    if (this.useCertificate && this.buyingAsGiftState) {
      this.sendToMeBlock.show();
      this.SendToRecipBlock.show();
    }

    if (this.useCertificate && !this.buyingAsGiftState) {
      this.sendToMeBlock.show();
    }

    if (!this.useCertificate && this.buyingAsGiftState) {
      this.pickUpBlock.show();
      this.sendToMeBlock.show();
      this.SendToRecipBlock.show();
    }

    if (!this.useCertificate && !this.buyingAsGiftState) {
      this.pickUpBlock.show();
      this.sendToMeBlock.show();
    }
  },

  hideDeliveryBtns: function () {
    this.pickUpBlock.hide();
    this.sendToMeBlock.hide();
    this.SendToRecipBlock.hide();
  },
};

CheckoutTabsController = {
  $tabs: null,
  $contents: null,
  $prevBtn: null,
  $nextBtn: null,
  $currentStepIndex: null,
  $stepKeys: null,
  $settings: null,
  $summaryContainer: null,
  $warningMessage: null,
  $successUrl: null,

  init: function (settings) {   // }, tabs, tabId_prefix, contents, contentBlockId_prefix, prevBtnId, nextBtnId) {
    this.$settings = settings;
    //Init main controll elements
    this.$tabs = $(settings.tabsSelector);
    this.$contents = $(settings.contentsSelector);
    this.$prevBtn = $(settings.prevBtnSelector);
    this.$nextBtn = $(settings.nextBtnSelector);
    this.$summaryContainer = $(settings.summaryContainer);
    this.$warningMessage = $(settings.warningMessage);
    this.$successUrl = settings.successUrl;


    //Make tab of first stet active by default
    this.$stepKeys = Object.keys(settings.tabSettings);
    this.$currentStepIndex = 0; /*put step index to 0 so we will get step number from list of keys
                                  var keys = Object.keys(this.$settings); 
                                  var stepId = keys[this.$currentStepIndex];*/

    this.$tabs.first().addClass('active');
    this.$contents.first().addClass('active');

    //Set tab click handling
    this.$tabs.click(this.switchTab.bind(this));

    // "Next" btn click handler
    this.$nextBtn.click(this.getNextStep.bind(this));

    //"Back" btn click handler
    this.$prevBtn.click(this.getPreviousStep.bind(this));
  },

  switchTab: function (e) {
    let stepKey = $(e.currentTarget).data(this.$settings.stepDataSelector);

    let index = this.$stepKeys.indexOf(String(stepKey));
    //alert(index);

    //Check if keyValue is valid and that tab is allowed to show. We can't move forvard,
    //only backward from place where he is at the moment.
    if (index >= 0 && index < this.$currentStepIndex) {
      this.$currentStepIndex = index;
      this.updateTabsAndContents();
    }
  },

  getNextStep: function () {
    if (this.$currentStepIndex < this.$tabs.length - 1) {
      let stepKey = this.$stepKeys[this.$currentStepIndex];
      let currentTabSetting = this.$settings.tabSettings[stepKey];

      CheckoutLoaderHelper.request(currentTabSetting.nextStepRequestAction, this.responseSuccess.bind(this), this.responseError.bind(this));
    }
  },

  getPreviousStep: function () {
    if (this.$currentStepIndex > 0) {
      this.$currentStepIndex--;
      this.updateTabsAndContents();
    }
  },

  responseError: function (response) {
    //dont use "this." here when call params, it won't work;
    alert("request error");
    console.log(response);
  },

  responseSuccess: function (response) {
    
    //dont use "this." here when call params, it won't work;
    Checkout.setLoadWaiting(false);

    let isError = false;
    let isWarning = false;

    if (response.error && response.error.message) {
      this.$warningMessage.show();
      this.$warningMessage.html(response.warning.message);
      isError = true;
    }

    if (response.warning && response.warning.message) {
      this.$warningMessage.show();
      this.$warningMessage.html(response.warning.message);
      isWarning = true;
    }

    if (response.update_section) {
      if (response.update_section.html) {
        if (!isError && !isWarning) {
          this.$currentStepIndex++;
          this.updateTabsAndContents();
        }

        let dataSelector = `[data-${this.$settings.stepDataSelector}=${this.getStepKey()}]`;
        this.$contents.filter(dataSelector).html(response.update_section.html);
      }

      if (response.update_section.summary_html) {
        this.$summaryContainer.html(response.update_section.summary_html);
      }
    }

    if (response.redirect) {
      location.href = response.redirect;
      return true;
    }

    if (response.redirect) {
      location.href = response.redirect;
      return;
    }
    if (response.success) {
      window.location = this.$successUrl;
    }
  },

  updateTabsAndContents: function () {
    this.$tabs.removeClass('active');
    this.$tabs.filter(`[data-${this.$settings.stepDataSelector}=${this.getStepKey()}]`).addClass('active');

    this.$contents.removeClass('active');
    this.$contents.filter(`[data-${this.$settings.stepDataSelector}=${this.getStepKey()}]`).addClass('active');

    // Check if "Next" or "Back" can be displayed
    if (this.$currentStepIndex === 0) {
      this.$prevBtn.addClass("inactive");
    } else {
      this.$prevBtn.removeClass("inactive");
    }

    if (this.$currentStepIndex === this.$tabs.length) {
      this.$nextBtn.hide();
    } else {
      this.$nextBtn.show();

      //Set correct name of button
      let currentTabSetting = this.$settings.tabSettings[this.getStepKey()];
      this.$nextBtn.text(currentTabSetting.nextButtonText);
    }

    this.$warningMessage.html('');
    this.$warningMessage.hide();
  },

  getStepKey: function () {
    return this.$stepKeys[this.$currentStepIndex];
  },

};


var Checkout = {
  loadWaiting: false,
  failureUrl: false,

  init: function (failureUrl) {
    this.loadWaiting = false;
    this.failureUrl = failureUrl;

    Accordion.disallowAccessToNextSections = true;
  },

  ajaxFailure: function () {
    location.href = Checkout.failureUrl;
  },

  _disableEnableAll: function (element, isDisabled) {
    var descendants = element.find('*');
    $(descendants).each(function () {
      if (isDisabled) {
        $(this).prop("disabled", true);
      } else {
        $(this).prop("disabled", false);
      }
    });

    if (isDisabled) {
      element.prop("disabled", true);
    } else {
      $(this).prop("disabled", false);
    }
  },

  setLoadWaiting: function (step, keepDisabled) {
    var container;
    if (step) {
      if (this.loadWaiting) {
        this.setLoadWaiting(false);
      }
      container = $('#' + step + '-buttons-container');
      container.addClass('disabled');
      container.css('opacity', '.5');
      this._disableEnableAll(container, true);
      $('#' + step + '-please-wait').show();
    } else {
      if (this.loadWaiting) {
        container = $('#' + this.loadWaiting + '-buttons-container');
        var isDisabled = keepDisabled ? true : false;
        if (!isDisabled) {
          container.removeClass('disabled');
          container.css('opacity', '1');
        }
        this._disableEnableAll(container, isDisabled);
        $('#' + this.loadWaiting + '-please-wait').hide();
      }
    }
    this.loadWaiting = step;
  },

  gotoSection: function (section) {
    section = $('#opc-' + section);
    section.addClass('allow');
    Accordion.openSection(section);
  },

  back: function () {
    if (this.loadWaiting) return;
    Accordion.openPrevSection(true, true);
  },

  setStepResponse: function (response) {
    if (response.update_section) {
      $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
    }
    if (response.allow_sections) {
      response.allow_sections.each(function (e) {
        $('#opc-' + e).addClass('allow');
      });
    }

    //TODO move it to a new method
    if ($("#billing-address-select").length > 0) {
      Billing.newAddress(!$('#billing-address-select').val());
    } else {
      Billing.newAddress(true);
    }

    if ($("#shipping-address-select").length > 0) {
      Shipping.newAddress(response.selected_id == undefined ? $('#shipping-address-select').val() : response.selected_id, $('#billing-address-select').children("option:selected").val());
    }

    if (response.goto_section) {
      Checkout.gotoSection(response.goto_section);
      return true;
    }
    if (response.redirect) {
      location.href = response.redirect;
      return true;
    }
    return false;
  }
};


var Billing = {
  form: false,
  saveUrl: false,
  disableBillingAddressCheckoutStep: false,
  guest: false,
  selectedStateId: 0,

  init: function (form, saveUrl, disableBillingAddressCheckoutStep, guest) {
    this.form = form;
    this.saveUrl = saveUrl;
    this.disableBillingAddressCheckoutStep = disableBillingAddressCheckoutStep;
    this.guest = guest;
  },

  newAddress: function (isNew) {
    $('#save-billing-address-button').hide();

    if (isNew) {
      $('#billing-new-address-form').show();
      $('#edit-billing-address-button').hide();
      $('#delete-billing-address-button').hide();
    } else {
      $('#billing-new-address-form').hide();
      $('#edit-billing-address-button').show();
      $('#delete-billing-address-button').show();
    }
    $(document).trigger({ type: "onepagecheckout_billing_address_new" });
    Billing.initializeCountrySelect();
  },

  resetSelectedAddress: function () {
    var selectElement = $('#billing-address-select');
    if (selectElement) {
      selectElement.val('');
    }
    $(document).trigger({ type: "onepagecheckout_billing_address_reset" });
  },

  save: function () {
    if (Checkout.loadWaiting !== false) return;

    Checkout.setLoadWaiting('billing');

    $.ajax({
      cache: false,
      url: this.saveUrl,
      data: $(this.form).serialize(),
      type: "POST",
      success: this.nextStep,
      complete: this.resetLoadWaiting,
      error: Checkout.ajaxFailure
    });
  },

  resetLoadWaiting: function () {
    Checkout.setLoadWaiting(false);
  },

  nextStep: function (response) {
    //ensure that response.wrong_billing_address is set
    //if not set, "true" is the default value
    if (typeof response.wrong_billing_address === 'undefined') {
      response.wrong_billing_address = false;
    }
    if (Billing.disableBillingAddressCheckoutStep) {
      if (response.wrong_billing_address) {
        Accordion.showSection('#opc-billing');
      } else {
        Accordion.hideSection('#opc-billing');
      }
    }


    if (response.error) {
      if (typeof response.message === 'string') {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    Checkout.setStepResponse(response);
    Billing.initializeCountrySelect();
  },

  initializeCountrySelect: function () {
    if ($('#opc-billing').has('select[data-trigger="country-select"]')) {
      $('#opc-billing select[data-trigger="country-select"]').countrySelect();
    }
  },

  editAddress: function (url) {
    Billing.resetBillingForm();
    //Billing.initializeStateSelect();

    var prefix = 'BillingNewAddress_';
    var selectedItem = $('#billing-address-select').children("option:selected").val();
    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      data: {
        addressId: selectedItem,
      },
      success: function (data, textStatus, jqXHR) {
        $.each(data, function (id, value) {
          if (value === null)
            return;

          if (id.indexOf("CustomAddressAttributes") >= 0 && Array.isArray(value)) {
            $.each(value, function (i, customAttribute) {
              if (customAttribute.DefaultValue) {
                $(`#${customAttribute.ControlId}`).val(
                  customAttribute.DefaultValue
                );
              } else {
                $.each(customAttribute.Values, function (j, attributeValue) {
                  if (attributeValue.IsPreSelected) {
                    $(`#${customAttribute.ControlId}`).val(attributeValue.Id);
                    $(
                      `#${customAttribute.ControlId}_${attributeValue.Id}`
                    ).prop("checked", attributeValue.Id);
                  }
                });
              }
            });

            return;
          }

          var val = $(`#${prefix}${id}`).val(value);
          if (id.indexOf("CountryId") >= 0) {
            val.trigger("change");
          }
          if (id.indexOf("StateProvinceId") >= 0) {
            Billing.setSelectedStateId(value);
          }
        });
      },
      complete: function (jqXHR, textStatus) {
        $("#billing-new-address-form").show();
        $("#edit-billing-address-button").hide();
        $("#delete-billing-address-button").hide();
        $("#save-billing-address-button").show();
      },
      error: Checkout.ajaxFailure,
    });
  },

  saveEditAddress: function (url) {
    var selectedId;
    $.ajax({
      cache: false,
      url: url + '?opc=true',
      data: $(this.form).serialize(),
      type: "POST",
      success: function (response) {
        if (response.error) {
          alert(response.message);
          return false;
        } else {
          selectedId = response.selected_id;
          Checkout.setStepResponse(response);
          Billing.resetBillingForm();
        }
      },
      complete: function () {
        var selectElement = $('#billing-address-select');
        if (selectElement && selectedId) {
          selectElement.val(selectedId);
        }
      },
      error: Checkout.ajaxFailure
    });
  },

  deleteAddress: function (url) {
    var selectedAddress = $('#billing-address-select').children("option:selected").val();
    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      data: {
        "addressId": selectedAddress,
        "opc": 'true'
      },
      success: function (response) {
        Checkout.setStepResponse(response);
      },
      error: Checkout.ajaxFailure
    });
  },

  setSelectedStateId: function (id) {
    this.selectedStateId = id;
  },

  resetBillingForm: function () {
    $(':input', '#billing-new-address-form')
      .not(':button, :submit, :reset, :hidden')
      .removeAttr('checked').removeAttr('selected')
    $(':input', '#billing-new-address-form')
      .not(':checkbox, :radio, select')
      .val('');

    $('.address-id', '#billing-new-address-form').val('0');
    $('select option[value="0"]', '#billing-new-address-form').prop('selected', true);
  }
};

var Shipping = {
  form: false,
  saveUrl: false,

  init: function (form, saveUrl) {
    this.form = form;
    this.saveUrl = saveUrl;
  },

  newAddress: function (id, billingAddressId) {
    isNew = !id;
    if (isNew) {
      this.resetSelectedAddress();
      $('#shipping-new-address-form').show();
      $('#edit-shipping-address-button').hide();
      $('#delete-shipping-address-button').hide();
    } else {
      $('#shipping-new-address-form').hide();
      if (id == billingAddressId || (id != undefined && billingAddressId == undefined)) {
        $('#edit-shipping-address-button').hide();
        $("#save-shipping-address-button").hide();
        $('#delete-shipping-address-button').hide();
      } else {
        $("#save-shipping-address-button").hide();
        $('#edit-shipping-address-button').show();
        $('#delete-shipping-address-button').show();
      }
    }
    $(document).trigger({ type: "onepagecheckout_shipping_address_new" });
    Shipping.initializeCountrySelect();
  },

  resetSelectedAddress: function () {
    var selectElement = $('#shipping-address-select');
    if (selectElement) {
      selectElement.val('');
    }
    $(document).trigger({ type: "onepagecheckout_shipping_address_reset" });
  },

  editAddress: function (url) {
    Shipping.resetShippingForm();

    var prefix = 'ShippingNewAddress_';
    var selectedItem = $('#shipping-address-select').children("option:selected").val();
    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      data: {
        addressId: selectedItem,
      },
      success: function (data, textStatus, jqXHR) {
        $.each(data, function (id, value) {
          if (value === null)
            return;

          if (id.indexOf("CustomAddressAttributes") >= 0 && Array.isArray(value)) {
            $.each(value, function (i, customAttribute) {
              if (customAttribute.DefaultValue) {
                $(`#${customAttribute.ControlId}`).val(
                  customAttribute.DefaultValue
                );
              } else {
                $.each(customAttribute.Values, function (j, attributeValue) {
                  if (attributeValue.IsPreSelected) {
                    $(`#${customAttribute.ControlId}`).val(attributeValue.Id);
                    $(
                      `#${customAttribute.ControlId}_${attributeValue.Id}`
                    ).prop("checked", attributeValue.Id);
                  }
                });
              }
            });

            return;
          }

          var val = $(`#${prefix}${id}`).val(value);
          if (id.indexOf("CountryId") >= 0) {
            val.trigger("change");
          }
          if (id.indexOf("StateProvinceId") >= 0) {
            Billing.setSelectedStateId(value);
          }
        });
      },
      complete: function (jqXHR, textStatus) {
        $("#shipping-new-address-form").show();
        $("#edit-shipping-address-button").hide();
        $("#delete-shipping-address-button").hide();
        $("#save-shipping-address-button").show();
      },
      error: Checkout.ajaxFailure,
    });
  },

  saveEditAddress: function (url) {
    var selectedId;
    $.ajax({
      cache: false,
      url: url + '?opc=true',
      data: $(this.form).serialize(),
      type: "POST",
      success: function (response) {
        if (response.error) {
          alert(response.message);
          return false;
        } else {
          selectedId = response.selected_id;
          Checkout.setStepResponse(response);
          Shipping.resetShippingForm();
        }
      },
      complete: function () {
        var selectElement = $('#shipping-address-select');
        if (selectElement && selectedId) {
          selectElement.val(selectedId);
        }
      },
      error: Checkout.ajaxFailure
    });
  },

  deleteAddress: function (url) {
    var selectedAddress = $('#shipping-address-select').children("option:selected").val();
    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      data: {
        "addressId": selectedAddress,
        "opc": 'true'
      },
      success: function (response) {
        Checkout.setStepResponse(response);
      },
      error: Checkout.ajaxFailure
    });
  },

  save: function () {
    if (Checkout.loadWaiting !== false) return;

    Checkout.setLoadWaiting('shipping');

    $.ajax({
      cache: false,
      url: this.saveUrl,
      data: $(this.form).serialize(),
      type: "POST",
      success: this.nextStep,
      complete: this.resetLoadWaiting,
      error: Checkout.ajaxFailure
    });
  },

  resetLoadWaiting: function () {
    Checkout.setLoadWaiting(false);
  },

  nextStep: function (response) {
    if (response.error) {
      if (typeof response.message === 'string') {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    Checkout.setStepResponse(response);
  },

  initializeCountrySelect: function () {
    if ($('#opc-shipping').has('select[data-trigger="country-select"]')) {
      $('#opc-shipping select[data-trigger="country-select"]').countrySelect();
    }
  },

  resetShippingForm: function () {
    $(':input', '#shipping-new-address-form')
      .not(':button, :submit, :reset, :hidden')
      .removeAttr('checked').removeAttr('selected')
    $(':input', '#shipping-new-address-form')
      .not(':checkbox, :radio, select')
      .val('');

    $('.address-id', '#shipping-new-address-form').val('0');
    $('select option[value="0"]', '#shipping-new-address-form').prop('selected', true);
  }
};



var ShippingMethod = {
  form: false,
  saveUrl: false,
  localized_data: false,

  init: function (form, saveUrl, localized_data) {
    this.form = form;
    this.saveUrl = saveUrl;
    this.localized_data = localized_data;
  },

  validate: function () {
    var methods = document.getElementsByName('shippingoption');
    if (methods.length === 0) {
      alert(this.localized_data.NotAvailableMethodsError);
      return false;
    }

    for (var i = 0; i < methods.length; i++) {
      if (methods[i].checked) {
        return true;
      }
    }
    alert(this.localized_data.SpecifyMethodError);
    return false;
  },

  save: function () {
    if (Checkout.loadWaiting !== false) return;

    if (this.validate()) {
      Checkout.setLoadWaiting('shipping-method');

      $.ajax({
        cache: false,
        url: this.saveUrl,
        data: $(this.form).serialize(),
        type: "POST",
        success: this.nextStep,
        complete: this.resetLoadWaiting,
        error: Checkout.ajaxFailure
      });
    }
  },

  resetLoadWaiting: function () {
    Checkout.setLoadWaiting(false);
  },

  nextStep: function (response) {
    if (response.error) {
      if (typeof response.message === 'string') {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    Checkout.setStepResponse(response);
  }
};



var PaymentMethod = {
  form: false,
  saveUrl: false,
  localized_data: false,

  init: function (form, saveUrl, localized_data) {
    this.form = form;
    this.saveUrl = saveUrl;
    this.localized_data = localized_data;
  },

  toggleUseRewardPoints: function (useRewardPointsInput) {
    if (useRewardPointsInput.checked) {
      $('#payment-method-block').hide();
    }
    else {
      $('#payment-method-block').show();
    }
  },

  validate: function () {
    var methods = document.getElementsByName('paymentmethod');
    if (methods.length === 0) {
      alert(this.localized_data.NotAvailableMethodsError);
      return false;
    }

    for (var i = 0; i < methods.length; i++) {
      if (methods[i].checked) {
        return true;
      }
    }
    alert(this.localized_data.SpecifyMethodError);
    return false;
  },

  save: function () {
    if (Checkout.loadWaiting !== false) return;

    if (this.validate()) {
      Checkout.setLoadWaiting('payment-method');
      $.ajax({
        cache: false,
        url: this.saveUrl,
        data: $(this.form).serialize(),
        type: "POST",
        success: this.nextStep,
        complete: this.resetLoadWaiting,
        error: Checkout.ajaxFailure
      });
    }
  },

  resetLoadWaiting: function () {
    Checkout.setLoadWaiting(false);
  },

  nextStep: function (response) {
    if (response.error) {
      if (typeof response.message === 'string') {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    Checkout.setStepResponse(response);
  }
};



var PaymentInfo = {
  form: false,
  saveUrl: false,

  init: function (form, saveUrl) {
    this.form = form;
    this.saveUrl = saveUrl;
  },

  save: function () {
    if (Checkout.loadWaiting !== false) return;

    Checkout.setLoadWaiting('payment-info');
    $.ajax({
      cache: false,
      url: this.saveUrl,
      data: $(this.form).serialize(),
      type: "POST",
      success: this.nextStep,
      complete: this.resetLoadWaiting,
      error: Checkout.ajaxFailure
    });
  },

  resetLoadWaiting: function () {
    Checkout.setLoadWaiting(false);
  },

  nextStep: function (response) {
    if (response.error) {
      if (typeof response.message === 'string') {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    Checkout.setStepResponse(response);
  }
};



var ConfirmOrder = {
  form: false,
  saveUrl: false,
  isSuccess: false,
  isCaptchaEnabled: false,
  isReCaptchaV3: false,
  recaptchaPublicKey: "",

  init: function (saveUrl, successUrl, isCaptchaEnabled, isReCaptchaV3, recaptchaPublicKey) {
    this.saveUrl = saveUrl;
    this.successUrl = successUrl;
    this.isCaptchaEnabled = isCaptchaEnabled;
    this.isReCaptchaV3 = isReCaptchaV3;
    this.recaptchaPublicKey = recaptchaPublicKey;
  },

  save: async function () {
    if (Checkout.loadWaiting !== false) return;

    //terms of service
    var termOfServiceOk = true;
    if ($('#termsofservice').length > 0) {
      //terms of service element exists
      if (!$('#termsofservice').is(':checked')) {
        $("#terms-of-service-warning-box").dialog();
        termOfServiceOk = false;
      } else {
        termOfServiceOk = true;
      }
    }
    if (termOfServiceOk) {
      Checkout.setLoadWaiting('confirm-order');
      var postData = {};

      if (ConfirmOrder.isCaptchaEnabled) {
        var captchaTok = await ConfirmOrder.getCaptchaToken('OpcConfirmOrder');
        postData['g-recaptcha-response'] = captchaTok;
      }

      addAntiForgeryToken(postData);
      $.ajax({
        cache: false,
        url: this.saveUrl,
        data: postData,
        type: "POST",
        success: this.nextStep,
        complete: this.resetLoadWaiting,
        error: Checkout.ajaxFailure
      });
    } else {
      return false;
    }
  },

  getCaptchaToken: async function (action) {
    var recaptchaToken = ''
    if (ConfirmOrder.isReCaptchaV3) {
      grecaptcha.ready(() => {
        grecaptcha.execute(this.recaptchaPublicKey, { action: action }).then((token) => {
          recaptchaToken = token;
        });
      });
    } else {
      recaptchaToken = grecaptcha.getResponse();
    }

    while (recaptchaToken == '') {
      await new Promise(t => setTimeout(t, 100));
    }

    return recaptchaToken;
  },

  resetLoadWaiting: function (transport) {
    Checkout.setLoadWaiting(false, ConfirmOrder.isSuccess);
  },

  nextStep: function (response) {
    if (response.error) {
      if (typeof response.message === 'string') {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    if (response.redirect) {
      ConfirmOrder.isSuccess = true;
      location.href = response.redirect;
      return;
    }
    if (response.success) {
      ConfirmOrder.isSuccess = true;
      window.location = ConfirmOrder.successUrl;
    }

    Checkout.setStepResponse(response);
  }
};
