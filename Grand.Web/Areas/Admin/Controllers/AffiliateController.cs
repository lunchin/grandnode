﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Affiliates;
using Grand.Core;
using Grand.Core.Domain.Affiliates;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using Grand.Services.Affiliates;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Seo;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class AffiliateController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWebHelper _webHelper;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        #endregion

        #region Constructors

        public AffiliateController(ILocalizationService localizationService,
            IWorkContext workContext, IDateTimeHelper dateTimeHelper, IWebHelper webHelper,
            ICountryService countryService, IStateProvinceService stateProvinceService,
            IPriceFormatter priceFormatter, IAffiliateService affiliateService,
            ICustomerService customerService, IOrderService orderService,
            IPermissionService permissionService, 
            ILanguageService languageService, 
            IUrlRecordService urlRecordService)
        {
            _localizationService = localizationService;
            _workContext = workContext;
            _dateTimeHelper = dateTimeHelper;
            _webHelper = webHelper;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _priceFormatter = priceFormatter;
            _affiliateService = affiliateService;
            _customerService = customerService;
            _orderService = orderService;
            _permissionService = permissionService;
            _languageService = languageService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void PrepareAffiliateModel(AffiliateModel model, Affiliate affiliate, bool excludeProperties,
            bool prepareEntireAddressModel = true)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (affiliate != null)
            {
                model.Id = affiliate.Id;
                model.Url = affiliate.GenerateUrl(_webHelper);
                if (!excludeProperties)
                {
                    model.AdminComment = affiliate.AdminComment;
                    model.FriendlyUrlName = affiliate.FriendlyUrlName;
                    model.Active = affiliate.Active;
                    model.Name = affiliate.Name;
                    model.AccountNumber = affiliate.AcountNumber;
                    model.Username = affiliate.Username;
                    model.Password = affiliate.Password;
                    model.WebsiteUrl = affiliate.WebsiteUrl;
                    model.AffiliateUrl = affiliate.AffiliateUrl;
                    model.Address = affiliate.Address.ToModel();
                }
            }

            if (prepareEntireAddressModel)
            {
                model.Address.FirstNameEnabled = true;
                model.Address.FirstNameRequired = true;
                model.Address.LastNameEnabled = true;
                model.Address.LastNameRequired = true;
                model.Address.EmailEnabled = true;
                model.Address.EmailRequired = true;
                model.Address.CompanyEnabled = true;
                model.Address.CountryEnabled = true;
                model.Address.StateProvinceEnabled = true;
                model.Address.CityEnabled = true;
                model.Address.CityRequired = true;
                model.Address.StreetAddressEnabled = true;
                model.Address.StreetAddressRequired = true;
                model.Address.StreetAddress2Enabled = true;
                model.Address.ZipPostalCodeEnabled = true;
                model.Address.ZipPostalCodeRequired = true;
                model.Address.PhoneEnabled = true;
                model.Address.PhoneRequired = true;
                model.Address.FaxEnabled = true;

                //address
                model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
                foreach (var c in _countryService.GetAllCountries(showHidden: true))
                {
                    model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = affiliate != null && c.Id == affiliate.Address.CountryId });
                }

                var states = !string.IsNullOrEmpty(model.Address.CountryId) ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId, showHidden: true).ToList() : new List<StateProvince>();
                if (states.Count > 0)
                {
                    foreach (var s in states)
                    {
                        model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = affiliate != null && s.Id == affiliate.Address.StateProvinceId });
                    }
                }
                else
                {
                    model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
                }
            }
        }
        
        #endregion

        #region Methods

        //list
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
            {
                return AccessDeniedView();
            }

            var model = new AffiliateListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, AffiliateListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
            {
                return AccessDeniedView();
            }

            var affiliates = _affiliateService.GetAllAffiliates(model.SearchFriendlyUrlName,
                model.SearchFirstName, model.SearchLastName,
                model.LoadOnlyWithOrders, model.OrdersCreatedFromUtc, model.OrdersCreatedToUtc,
                command.Page - 1, command.PageSize, true);

            var gridModel = new DataSourceResult
            {
                Data = affiliates.Select(x =>
                {
                    var m = new AffiliateModel();
                    PrepareAffiliateModel(m, x, false, false);
                    return m;
                }),
                Total = affiliates.TotalCount,
            };
            return Json(gridModel);
        }

        //create
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
            {
                return AccessDeniedView();
            }

            var model = new AffiliateModel();
            PrepareAffiliateModel(model, null, false);
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Create(AffiliateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
            {
                return AccessDeniedView();
            }

            if (ModelState.IsValid)
            {
                var affiliate = new Affiliate
                {
                    Active = model.Active,
                    AdminComment = model.AdminComment,
                    WebsiteUrl = model.WebsiteUrl,
                    AffiliateUrl = model.AffiliateUrl,
                    Name = model.Name,
                    AcountNumber = model.AccountNumber,
                    Username = model.Username,
                    Password = model.Password
                };
                affiliate.Locales = UpdateLocales(affiliate, model);
                //validate friendly URL name
                var friendlyUrlName = affiliate.ValidateFriendlyUrlName(model.FriendlyUrlName);
                affiliate.FriendlyUrlName = friendlyUrlName;
                affiliate.Address = model.Address.ToEntity();
                affiliate.Address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                _affiliateService.InsertAffiliate(affiliate);
                SuccessNotification(_localizationService.GetResource("Admin.Affiliates.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = affiliate.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareAffiliateModel(model, null, true);
            return View(model);

        }


        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
            {
                return AccessDeniedView();
            }

            var affiliate = _affiliateService.GetAffiliateById(id);
            if (affiliate == null || affiliate.Deleted)
            {
                //No affiliate found with the specified id
                return RedirectToAction("List");
            }

            var model = new AffiliateModel();
            PrepareAffiliateModel(model, affiliate, false);
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Description = affiliate.GetLocalized(x => x.Description, languageId, false, false);
                locale.Benefits = affiliate.GetLocalized(x => x.Benefits, languageId, false, false);
                locale.Payouts = affiliate.GetLocalized(x => x.Payouts, languageId, false, false);
                locale.MetaDescription = affiliate.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = affiliate.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = affiliate.GetSeName(languageId, false, false);
            });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(AffiliateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
            {
                return AccessDeniedView();
            }

            var affiliate = _affiliateService.GetAffiliateById(model.Id);
            if (affiliate == null || affiliate.Deleted)
            {
                //No affiliate found with the specified id
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                affiliate.Active = model.Active;
                affiliate.AdminComment = model.AdminComment;
                affiliate.AffiliateUrl = model.AffiliateUrl;
                affiliate.Name = model.Name;
                affiliate.AcountNumber = model.AccountNumber;
                affiliate.Username = model.Username;
                affiliate.Password = model.Password;
                affiliate.WebsiteUrl = model.WebsiteUrl;
                affiliate.Locales = UpdateLocales(affiliate, model);
                //validate friendly URL name
                var friendlyUrlName = affiliate.ValidateFriendlyUrlName(model.FriendlyUrlName);
                affiliate.FriendlyUrlName = friendlyUrlName;
                affiliate.Address = model.Address.ToEntity(affiliate.Address);
                _affiliateService.UpdateAffiliate(affiliate);

                SuccessNotification(_localizationService.GetResource("Admin.Affiliates.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = affiliate.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareAffiliateModel(model, affiliate, true);
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
            {
                return AccessDeniedView();
            }

            var affiliate = _affiliateService.GetAffiliateById(id);
            if (affiliate == null)
            {
                //No affiliate found with the specified id
                return RedirectToAction("List");
            }

            _affiliateService.DeleteAffiliate(affiliate);
            SuccessNotification(_localizationService.GetResource("Admin.Affiliates.Deleted"));
            return RedirectToAction("List");
        }
        
        [HttpPost]
        public IActionResult AffiliatedOrderList(DataSourceRequest command, AffiliatedOrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
            {
                return AccessDeniedView();
            }

            var affiliate = _affiliateService.GetAffiliateById(model.AffliateId);
            if (affiliate == null)
            {
                throw new ArgumentException("No affiliate found with the specified id");
            }

            DateTime? startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)model.OrderStatusId : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)model.PaymentStatusId : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)model.ShippingStatusId : null;

            var orders = _orderService.SearchOrders(
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus, 
                affiliateId: affiliate.Id,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orders.Select(order =>
                    {
                        var orderModel = new AffiliateModel.AffiliatedOrderModel();
                        orderModel.Id = order.Id;
                        orderModel.OrderNumber = order.OrderNumber;
                        orderModel.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
                        orderModel.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
                        orderModel.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
                        orderModel.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
                        orderModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
                        return orderModel;
                    }),
                Total = orders.TotalCount
            };

            return Json(gridModel);
        }


        [HttpPost]
        public IActionResult AffiliatedCustomerList(string affiliateId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
            {
                return AccessDeniedView();
            }

            var affiliate = _affiliateService.GetAffiliateById(affiliateId);
            if (affiliate == null)
            {
                throw new ArgumentException("No affiliate found with the specified id");
            }

            var customers = _customerService.GetAllCustomers(
                affiliateId: affiliate.Id,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = customers.Select(customer =>
                    {
                        var customerModel = new AffiliateModel.AffiliatedCustomerModel();
                        customerModel.Id = customer.Id;
                        customerModel.Name = customer.Email;
                        return customerModel;
                    }),
                Total = customers.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(Affiliate affiliate, AffiliateModel model)
        {
            var localized = new List<LocalizedProperty>();

            foreach (var local in model.Locales)
            {
                var seName = affiliate.ValidateSeName(local.SeName, model.Name, false);
                _urlRecordService.SaveSlug(affiliate, seName, local.LanguageId);

                if (!string.IsNullOrEmpty(seName))
                {
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = nameof(local.SeName),
                        LocaleValue = seName
                    });
                }

                if (!string.IsNullOrEmpty(local.Description))
                {
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = nameof(local.Description),
                        LocaleValue = local.Description
                    });
                }

                if (!string.IsNullOrEmpty(local.Benefits))
                {
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = nameof(local.Benefits),
                        LocaleValue = local.Benefits
                    });
                }

                if (!string.IsNullOrEmpty(local.Payouts))
                {
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = nameof(local.Payouts),
                        LocaleValue = local.Payouts
                    });
                }

                if (!string.IsNullOrEmpty(local.MetaDescription))
                {
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = nameof(local.MetaDescription),
                        LocaleValue = local.MetaDescription
                    });
                }

                if (!string.IsNullOrEmpty(local.MetaTitle))
                {
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = nameof(local.MetaTitle),
                        LocaleValue = local.MetaTitle
                    });
                }
            }
            return localized;
        }
    }
}
