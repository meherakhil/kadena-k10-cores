﻿using CMS.Ecommerce;
using Kadena.Models;
using AutoMapper;
using System.Linq;
using CMS.SiteProvider;
using CMS.Helpers;
using System;
using CMS.DataEngine;
using CMS.Globalization;
using System.Collections.Generic;
using CMS.DocumentEngine;
using CMS.Membership;
using Newtonsoft.Json;
using Kadena.Models.Checkout;
using CMS.Localization;
using CMS.EventLog;
using Kadena.WebAPI.KenticoProviders.Contracts;
using Kadena2.WebAPI.KenticoProviders.Factories;

namespace Kadena.WebAPI.KenticoProviders
{
    public class KenticoProviderService : IKenticoProviderService
    {
        private readonly IMapper mapper;
        private readonly IKenticoResourceService resources;

        public KenticoProviderService(IMapper mapper, IKenticoResourceService resources)
        {
            this.mapper = mapper;
            this.resources = resources;
        }

        public DeliveryAddress[] GetCustomerAddresses(string addressType = null)
        {
            var customer = ECommerceContext.CurrentCustomer;
            var query = AddressInfoProvider.GetAddresses(customer.CustomerID);
            if (!string.IsNullOrWhiteSpace(addressType))
            {
                query = query.Where($"AddressType ='{addressType}'");
            }
            var addresses = query.ToArray();
            return AddressFactory.CreateDeliveryAddresses(addresses);
        }

        public DeliveryAddress[] GetCustomerShippingAddresses(int customerId)
        {
            var addresses = AddressInfoProvider.GetAddresses(customerId)
                .Where(a => a.GetStringValue("AddressType", string.Empty) == "Shipping")
                .ToArray();

            return AddressFactory.CreateDeliveryAddresses(addresses);
        }

        public DeliveryAddress GetCurrentCartShippingAddress()
        {
            var address = ECommerceContext.CurrentShoppingCart.ShoppingCartShippingAddress;
            return AddressFactory.CreateDeliveryAddress(address);            
        }

        public BillingAddress GetDefaultBillingAddress()
        {
            var streets = new[]
            {
                SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".KDA_EstimateDeliveryPrice_SenderAddressLine1"),
                SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".KDA_EstimateDeliveryPrice_SenderAddressLine2")
            }.Where(i => !string.IsNullOrEmpty(i)).ToList();

            string countryName = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".KDA_EstimateDeliveryPrice_SenderCountry");
            string stateName = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".KDA_EstimateDeliveryPrice_SenderState");
            int countryId = CountryInfoProvider.GetCountryInfoByCode(countryName).CountryID;
            int stateId = StateInfoProvider.GetStateInfoByCode(stateName).StateID;

            return new BillingAddress()
            {
                Street = streets,
                City = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".KDA_EstimateDeliveryPrice_SenderCity"),
                Country = countryName,
                CountryId = countryId,
                Zip = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".KDA_EstimateDeliveryPrice_SenderPostal"),
                State = stateName,
                StateId = stateId
            };
        }

        public DeliveryCarrier[] GetShippingCarriers()
        {
            var shippingOptions = GetShippingOptions();
            var carriers = CarrierInfoProvider.GetCarriers(SiteContext.CurrentSiteID).ToArray();

            var deliveryMethods = DeliveryFactory.CreateCarriers(carriers);

            foreach (DeliveryCarrier dm in deliveryMethods)
            {
                dm.SetShippingOptions(shippingOptions);
                dm.Icon = GetShippingProviderIcon(dm.Title);
            }

            return deliveryMethods;
        }

        public string GetSkuImageUrl(int skuid)
        {
            if (skuid <= 0)
                return string.Empty;

            var sku = SKUInfoProvider.GetSKUInfo(skuid);
            var skuurl = sku?.SKUImagePath ?? string.Empty;
            return URLHelper.GetAbsoluteUrl(skuurl);
        }

        /// <summary>
        /// Hardcoded until finding some convinient way to configure it in Kentico
        /// </summary>
        public string GetShippingProviderIcon(string title)
        {
            var dictionary = new Dictionary<string, string>()
            {
                {"fedex","fedex-delivery"},
                {"usps","usps-delivery" },
                {"ups","ups-delivery" }
            };

            foreach (var kvp in dictionary)
            {
                if (title.ToLower().Contains(kvp.Key))
                    return kvp.Value;
            }
            
            return string.Empty;
        }

        public DeliveryOption[] GetShippingOptions()
        {
            var services = ShippingOptionInfoProvider.GetShippingOptions(SiteContext.CurrentSiteID).Where(s => s.ShippingOptionEnabled).ToArray();
            var result = DeliveryFactory.CreateOptions(services);
            GetShippingPrice(result);          
            return result;
        }

        public DeliveryOption GetShippingOption(int id)
        {
            var service = ShippingOptionInfoProvider.GetShippingOptionInfo(id);
            var result = DeliveryFactory.CreateOption(service);
            var carrier = CarrierInfoProvider.GetCarrierInfo(service.ShippingOptionCarrierID);
            result.CarrierCode = carrier.CarrierName;
            return result;
        }

        private void GetShippingPrice(DeliveryOption[] services)
        {
            // this method's approach comes from origial kentico webpart (ShippingSeletion)
            int originalCartShippingId = ECommerceContext.CurrentShoppingCart.ShoppingCartShippingOptionID;

            foreach (var s in services)
            {
                var cart = ECommerceContext.CurrentShoppingCart;
                cart.ShoppingCartShippingOptionID = s.Id;
                s.PriceAmount = cart.TotalShipping;
                s.Price = String.Format("$ {0:#,0.00}", ECommerceContext.CurrentShoppingCart.TotalShipping);

                if (cart.TotalShipping == 0.0d && !s.IsCustomerPrice)
                {
                    s.Disabled = true;
                }
            }

            ECommerceContext.CurrentShoppingCart.ShoppingCartShippingOptionID = originalCartShippingId;
        }


        public PaymentMethod[] GetPaymentMethods()
        {
            var methods = PaymentOptionInfoProvider.GetPaymentOptions(SiteContext.CurrentSiteID).Where(p => p.PaymentOptionEnabled).ToArray();            
            return PaymentOptionFactory.CreateMethods(methods);
        }

        public PaymentMethod GetPaymentMethod(int id)
        {
            var method = PaymentOptionInfoProvider.GetPaymentOptionInfo(id);
            return PaymentOptionFactory.CreateMethod(method);
        }

        public ShoppingCartTotals GetShoppingCartTotals()
        {
            return new ShoppingCartTotals()
            {
                TotalItemsPrice = ECommerceContext.CurrentShoppingCart.TotalItemsPrice,
                TotalShipping = ECommerceContext.CurrentShoppingCart.TotalShipping,
                TotalTax = ECommerceContext.CurrentShoppingCart.TotalTax
            };
        }

        public void SetShoppingCartAddres(int addressId)
        {
            var cart = ECommerceContext.CurrentShoppingCart;

            if (cart.ShoppingCartShippingAddress == null || cart.ShoppingCartShippingAddress.AddressID != addressId)
            {
                var address = AddressInfoProvider.GetAddressInfo(addressId);
                cart.ShoppingCartShippingAddress = address;
                cart.SubmitChanges(true);
            }
        }

        public void SelectShipping(int shippingOptionId)
        {
            var cart = ECommerceContext.CurrentShoppingCart;

            if (cart.ShoppingCartShippingOptionID != shippingOptionId)
            {
                cart.ShoppingCartShippingOptionID = shippingOptionId;
                cart.SubmitChanges(true);
            }
        }

        public int GetCurrentCartAddresId()
        {
            var address = ECommerceContext.CurrentShoppingCart.ShoppingCartShippingAddress;
            return address?.AddressID ?? 0;
        }

        public int GetCurrentCartShippingOptionId()
        {
            return ECommerceContext.CurrentShoppingCart.ShoppingCartShippingOptionID;
        }

        public Customer GetCurrentCustomer()
        {
            var customer = ECommerceContext.CurrentCustomer;

            if (customer == null)
                return null;

            return new Customer()
            {
                Id = customer.CustomerID,
                FirstName = customer.CustomerFirstName,
                LastName = customer.CustomerLastName,
                Email = customer.CustomerEmail,
                CustomerNumber = customer.CustomerGUID.ToString(),
                Phone = customer.CustomerPhone,
                UserID = customer.CustomerUserID,
                Company = customer.CustomerCompany
            };
        }

        public Customer GetCustomer(int customerId)
        {
            var customer = CustomerInfoProvider.GetCustomerInfo(customerId);

            if (customer == null)
                return null;

            return new Customer()
            {
                Id = customer.CustomerID,
                FirstName = customer.CustomerFirstName,
                LastName = customer.CustomerLastName,
                Email = customer.CustomerEmail,
                CustomerNumber = customer.CustomerGUID.ToString(),
                Phone = customer.CustomerPhone,
                UserID = customer.CustomerUserID,
                Company = customer.CustomerCompany,
                SiteId = customer.CustomerSiteID
            };
        }

        public CartItem[] GetShoppingCartItems()
        {
            var items = ECommerceContext.CurrentShoppingCart.CartItems;
            var result = items.Select(i => new CartItem()
            {
                Id = i.CartItemID,
                CartItemText = i.CartItemText,
                DesignFilePath = i.GetValue("DesignFilePath", string.Empty),
                MailingListGuid = i.GetValue("MailingListGuid", Guid.Empty), // seem to be redundant parameter, microservice doesn't use it
                ChilliEditorTemplateId = i.GetValue("ChilliEditorTemplateID", Guid.Empty),
                ProductChilliPdfGeneratorSettingsId = i.GetValue("ProductChiliPdfGeneratorSettingsId", Guid.Empty),
                ProductChiliWorkspaceId = i.GetValue("ProductChiliWorkspaceId", Guid.Empty),
                ChilliTemplateId = i.GetValue("ChiliTemplateID", Guid.Empty),
                DesignFilePathObtained = i.GetValue("DesignFilePathObtained", false),
                DesignFilePathTaskId = i.GetStringValue("DesignFilePathTaskId", string.Empty),
                SKUName = i.SKU?.SKUName,
                SKUNumber = i.SKU?.SKUNumber,
                TotalTax = 0.0d,
                UnitPrice = i.UnitPrice,
                UnitOfMeasure = "EA",
                Image = URLHelper.GetAbsoluteUrl(i.SKU.SKUImagePath),
                ProductType = i.GetValue("ProductType", string.Empty),
                Quantity = i.CartItemUnits,
                TotalPrice = i.UnitPrice * i.CartItemUnits,
				PriceText = string.Format("{0:#,0.00}", i.UnitPrice * i.CartItemUnits),
                PricePrefix = resources.GetResourceString("Kadena.Checkout.ItemPricePrefix"),
                QuantityPrefix = resources.GetResourceString("Kadena.Checkout.QuantityPrefix"),
                MailingListName = i.GetValue("MailingListName", string.Empty),
                Template = !string.IsNullOrEmpty(i.CartItemText) ? i.CartItemText : i.SKU.SKUName,
                EditorTemplateId = i.GetValue("ChilliEditorTemplateID", string.Empty),
                ProductPageId = i.GetIntegerValue("ProductPageID", 0),
                SKUID = i.SKUID,
                StockQuantity = i.SKU.SKUAvailableItems				
            }
            ).ToArray();

            return result;
        }

        public void RemoveCartItem(int id)
        {
            // Method approach inspired by \CMS\CMSModules\Ecommerce\Controls\Checkout\CartItemRemove.ascx.cs

            var cart = ECommerceContext.CurrentShoppingCart;
            var item = cart.CartItems.FirstOrDefault(i => i.CartItemID == id);

            if (item == null)
                return;

            // Delete all the children from the database if available        
            foreach (ShoppingCartItemInfo scii in cart.CartItems)
            {
                if ((scii.CartItemBundleGUID == item.CartItemGUID) || (scii.CartItemParentGUID == item.CartItemGUID))
                {
                    ShoppingCartItemInfoProvider.DeleteShoppingCartItemInfo(scii);
                }
            }

            // Deletes the CartItem from the database
            ShoppingCartItemInfoProvider.DeleteShoppingCartItemInfo(item.CartItemGUID);
            // Delete the CartItem form the shopping cart object (session)
            ShoppingCartInfoProvider.RemoveShoppingCartItem(cart, item.CartItemGUID);

            // Recalculate shopping cart
            ShoppingCartInfoProvider.EvaluateShoppingCart(cart);
        }

        public void SetCartItemDesignFilePath(int id, string path)
        {
            var cart = ECommerceContext.CurrentShoppingCart;
            var item = ECommerceContext.CurrentShoppingCart.CartItems.Where(i => i.CartItemID == id).FirstOrDefault();

            if (item != null)
            {
                item.SetValue("DesignFilePathObtained", true);
                item.SetValue("DesignFilePath", path);
                item.Update();
            }
        }

        public void SetCartItemQuantity(int id, int quantity)
        {
            var cart = ECommerceContext.CurrentShoppingCart;
            var item = ECommerceContext.CurrentShoppingCart.CartItems.Where(i => i.CartItemID == id).FirstOrDefault();

            if (item == null)
            {

                throw new ArgumentOutOfRangeException(string.Format(
                    ResHelper.GetString("Kadena.Product.ItemInCartNotFound", LocalizationContext.CurrentCulture.CultureCode),
                    id));
            }

            if (quantity < 1)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    ResHelper.GetString("Kadena.Product.NegativeQuantityError", LocalizationContext.CurrentCulture.CultureCode), quantity));
            }

            var productType = item.GetStringValue("ProductType", string.Empty);

            if (!productType.Contains("KDA.InventoryProduct") && !productType.Contains("KDA.POD") && !productType.Contains("KDA.StaticProduct"))
            {

                throw new Exception(ResHelper.GetString("Kadena.Product.QuantityForTypeError", LocalizationContext.CurrentCulture.CultureCode));
            }

            if (productType.Contains("KDA.InventoryProduct") && quantity > item.SKU.SKUAvailableItems)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    ResHelper.GetString("Kadena.Product.SetQuantityForItemError", LocalizationContext.CurrentCulture.CultureCode), quantity, id));
            }

            var documentId = item.GetIntegerValue("ProductPageID", 0);          
            var ranges = GetDynamicPricingRanges(documentId);

            if ((ranges?.Count() ?? 0) > 0)
            {
                var price = GetDynamicPrice(quantity, ranges);

                if (price != 0.0m)
                {
                    item.CartItemPrice = (double)price;
                    ShoppingCartItemInfoProvider.UpdateShoppingCartItemUnits(item, quantity);
                }

                else
                {
                    throw new Exception(ResHelper.GetString("Kadena.Product.QuantityOutOfRange", LocalizationContext.CurrentCulture.CultureCode));
                }

            }
            else
            {
                ShoppingCartItemInfoProvider.UpdateShoppingCartItemUnits(item, quantity);
            }

            cart.InvalidateCalculations();
            ShoppingCartInfoProvider.EvaluateShoppingCart(cart);
        }

        public string GetDocumentUrl(int documentId)
        {
            var doc = DocumentHelper.GetDocument(documentId, new TreeProvider(MembershipContext.AuthenticatedUser));
            return doc?.AbsoluteURL ?? "#";
        }

        public Product GetProductByDocumentId(int documentId)
        {
            var doc = DocumentHelper.GetDocument(documentId, new TreeProvider(MembershipContext.AuthenticatedUser));
            var sku = SKUInfoProvider.GetSKUInfo(doc.NodeSKUID);

            var product = new Product()
            {
                Id = documentId,
                Name = doc.DocumentName,
                DocumentUrl = doc.AbsoluteURL,
                Category = doc.Parent?.DocumentName ?? string.Empty,
                ProductType = doc.GetValue("ProductType", string.Empty)
        };

            if (sku != null)
            {
                product.SkuImageUrl = URLHelper.GetAbsoluteUrl(sku.SKUImagePath);
                product.StockItems = sku.SKUAvailableItems;
                product.Availability = sku.SKUAvailableItems > 0 ? "available" : "out";
            }

            return product;
        }

        private decimal GetDynamicPrice(int quantity, IEnumerable<DynamicPricingRange> ranges)
        {

            if (ranges != null)
            {
                var matchingRange = ranges.FirstOrDefault(i => quantity >= i.MinVal && quantity <= i.MaxVal);
                if (matchingRange != null)
                    return matchingRange.Price;
            }
            return 0.0m;
        }

        private IEnumerable<DynamicPricingRange> GetDynamicPricingRanges(int documentId)
        {
            var rawJson = DocumentHelper.GetDocument(documentId, new TreeProvider(MembershipContext.AuthenticatedUser))?.GetStringValue("ProductDynamicPricing", string.Empty);
            var ranges = JsonConvert.DeserializeObject<List<DynamicPricingRange>>(rawJson ?? string.Empty);

            return ranges;
 
        }
        public void RemoveCurrentItemsFromStock()
        {
            var items = ECommerceContext.CurrentShoppingCart.CartItems;

            foreach (var i in items)
            {
                if (i.GetValue("ProductType", string.Empty).Contains("KDA.InventoryProduct"))
                {
                    int toRemove = i.CartItemUnits <= i.SKU.SKUAvailableItems ? i.CartItemUnits : i.SKU.SKUAvailableItems;
                    i.SKU.SKUAvailableItems -= toRemove;
                    i.SKU.SubmitChanges(false);
                    i.SKU.MakeComplete(true);
                    i.SKU.Update();
                }
            }
        }

        public void RemoveCurrentItemsFromCart()
        {
            ShoppingCartInfoProvider.EmptyShoppingCart(ECommerceContext.CurrentShoppingCart);
        }

        public IEnumerable<State> GetStates()
        {
            return StateInfoProvider
                .GetStates()
                .Column("StateCode")
                .Select(s => new State
                {
                    StateCode = s["StateCode"].ToString()
                });
        }

        public void SaveShippingAddress(DeliveryAddress address)
        {
            var customer = ECommerceContext.CurrentCustomer;
            var state = StateInfoProvider.GetStateInfoByCode(address.State);
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state), "Incorrect state was selected.");
            }
            var info = new AddressInfo
            {
                AddressID = address.Id,
                AddressLine1 = address.Street.Count > 0 ? address.Street[0] : null,
                AddressLine2 = address.Street.Count > 1 ? address.Street[1] : null,
                AddressCity = address.City,
                AddressStateID = state.StateID,
                AddressCountryID = state.CountryID,
                AddressZip = address.Zip,
                AddressCustomerID = customer.CustomerID,
                AddressPersonalName = $"{customer.CustomerFirstName} {customer.CustomerLastName}"
            };
            info.AddressName = $"{info.AddressPersonalName}, {info.AddressLine1}, {info.AddressCity}";
            info.SetValue("AddressType", "Shipping");

            AddressInfoProvider.SetAddressInfo(info);
            address.Id = info.AddressID;
        }

        public double GetCurrentCartTotalItemsPrice()
        {
            return ECommerceContext.CurrentShoppingCart.TotalItemsPrice;
        }

        public double GetCurrentCartShippingCost()
        {
            return ECommerceContext.CurrentShoppingCart.Shipping;
        }

		public bool UserCanSeePrices()
        {
            return UserInfoProvider.IsAuthorizedPerResource("Kadena_Orders", "KDA_SeePrices", SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }

        public bool UserCanSeeAllOrders()
        {
            return UserInfoProvider.IsAuthorizedPerResource("Kadena_Orders", "KDA_SeeAllOrders", SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }

        public bool IsAuthorizedPerResource(string resourceName, string permissionName, string siteName)
        {
            return MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(resourceName, permissionName, siteName);
        }
		

        public List<string> GetBreadcrumbs(int documentId)
        {
            var breadcrubs = new List<string>();
            var doc = DocumentHelper.GetDocument(documentId, new TreeProvider(MembershipContext.AuthenticatedUser));

            while (doc != null && doc.Parent != null)
            {
                breadcrubs.Add(doc.DocumentName);
                doc = doc.Parent;
            };

            breadcrubs.Reverse();
            return breadcrubs;
        }

        public Site GetSite(int siteId)
        {
            var site = SiteInfoProvider.GetSiteInfo(siteId);
            if (site == null)
            {
                return null;
            }
            else
            {
                return new Site
                {
                    Id = site.SiteID,
                    Name = site.SiteName
                };
            }
        }
    }
}