﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Kadena.WebAPI.Contracts;
using Kadena.Models.Checkout;
using Kadena.Models;
using Kadena.WebAPI.KenticoProviders.Contracts;
using Kadena2.MicroserviceClients.Contracts;

namespace Kadena.WebAPI.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IMapper mapper;
        private readonly IKenticoProviderService kenticoProvider;
        private readonly IKenticoResourceService resources;
        private readonly IKenticoLogger kenticoLog;
        private readonly ITaxEstimationService taxCalculator;
        private readonly IKListService mailingService;
        private readonly ITemplatedProductService templateService;

        public ShoppingCartService(IMapper mapper,
                                   IKenticoProviderService kenticoProvider,
                                   IKenticoResourceService resources,
                                   ITaxEstimationService taxCalculator,
                                   IKListService mailingService,
                                   IKenticoLogger kenticoLog,
                                   ITemplatedProductService templateService)
        {
            this.mapper = mapper;
            this.kenticoProvider = kenticoProvider;
            this.resources = resources;
            this.taxCalculator = taxCalculator;
            this.mailingService = mailingService;
            this.kenticoLog = kenticoLog;
            this.templateService = templateService;
        }

        public CheckoutPage GetCheckoutPage()
        {
            var addresses = kenticoProvider.GetCustomerAddresses("Shipping");
            var paymentMethods = kenticoProvider.GetPaymentMethods();
            var cartItems = kenticoProvider.GetShoppingCartItems();
            var cartItemsTotals = kenticoProvider.GetShoppingCartTotals();
            var items = cartItems.Length == 1 ? "item" : "items"; // todo configurable

            var checkoutPage = new CheckoutPage()
            {
                Products = new CartItems()
                {
                    Number = $"You have {cartItems.Length} {items} in your shopping cart",
                    Items = cartItems.ToList(),
                    SummaryPrice = new CartPrice
                    {
                        PricePrefix = resources.GetResourceString("Kadena.Checkout.ItemPricePrefix"),
                        Price = string.Format("{0:#,0.00}", cartItemsTotals.TotalItemsPrice)
                    }
                },
                DeliveryAddresses = new DeliveryAddresses()
                {
                    IsDeliverable = true,
                    UnDeliverableText = resources.GetResourceString("Kadena.Checkout.UndeliverableText"),
                    NewAddress = new NewAddressButton()
                    {
                        Label = resources.GetResourceString("Kadena.Checkout.NewAddress"),
                        Url = "/settings?tab=t4"
                    },
                    Title = resources.GetResourceString("Kadena.Checkout.DeliveryAddress.Title"),
                    Description = resources.GetResourceString("Kadena.Checkout.DeliveryDescription"),
                    EmptyMessage = resources.GetResourceString("Kadena.Checkout.NoAddressesMessage"),
                    items = addresses.ToList()
                },
                PaymentMethods = new PaymentMethods()
                {
                    IsPayable = true,
                    UnPayableText = resources.GetResourceString("Kadena.Checkout.UnpayableText"),
                    Title = resources.GetResourceString("Kadena.Checkout.Payment.Title"),
                    Description = null, // resources.GetResourceString("Kadena.Checkout.Payment.Description"), if needed
                    Items = ArrangePaymentMethods(paymentMethods)
                },
                Submit = new SubmitButton()
                {
                    BtnLabel = resources.GetResourceString("Kadena.Checkout.ButtonPlaceOrder"),
                    DisabledText = resources.GetResourceString("Kadena.Checkout.ButtonWaitingForTemplateService"),
                    IsDisabled = false
                },
                ValidationMessage = resources.GetResourceString("Kadena.Checkout.ValidationError")
            };

            CheckCurrentOrDefaultAddress(checkoutPage);

            checkoutPage.PaymentMethods.CheckDefault();
            checkoutPage.PaymentMethods.CheckPayability();
            checkoutPage.SetDisplayType();
            SetPricesVisibility(checkoutPage);
            return checkoutPage;
        }

        public async Task<CheckoutPageDeliveryTotals> GetDeliveryAndTotals()
        {
            var carriers = kenticoProvider.GetShippingCarriers();

            var result = new CheckoutPageDeliveryTotals()
            {
                DeliveryMethods = new DeliveryCarriers()
                {
                    Title = resources.GetResourceString("Kadena.Checkout.Delivery.Title"),
                    Description = resources.GetResourceString("Kadena.Checkout.DeliveryMethodDescription"),
                    items = carriers.ToList()
                },
                Totals = new Totals()
                {
                    Title = string.Empty,
                    Description = null // resources.GetResourceString("Kadena.Checkout.Totals.Description"), if needed
                }
            };

            result.DeliveryMethods.RemoveCarriersWithoutOptions();
            CheckCurrentOrDefaultShipping(result);
            result.DeliveryMethods.UpdateSummaryText(
                    resources.GetResourceString("Kadena.Checkout.ShippingPriceFrom"),
                    resources.GetResourceString("Kadena.Checkout.ShippingPrice"),
                    resources.GetResourceString("Kadena.Checkout.CannotBeDelivered"),
                    resources.GetResourceString("Kadena.Checkout.CustomerPrice")
                );

            if (kenticoProvider.UserCanSeePrices())
            {
                await UpdateTotals(result);
            }

            SetPricesVisibility(result);
            return result;
        }

        private async Task UpdateTotals(CheckoutPageDeliveryTotals page)
        {
            var totals = page.Totals;
            totals.Title = resources.GetResourceString("Kadena.Checkout.Totals.Title");
            var shoppingCartTotals = kenticoProvider.GetShoppingCartTotals();
            shoppingCartTotals.TotalTax = await taxCalculator.EstimateTotalTax();
            totals.Items = new Total[]
            {
                new Total()
                {
                    Title = resources.GetResourceString("Kadena.Checkout.Totals.Summary"),
                    Value = String.Format("$ {0:#,0.00}", shoppingCartTotals.TotalItemsPrice)
                },
                new Total()
                {
                    Title = resources.GetResourceString("Kadena.Checkout.Totals.Shipping"),
                    Value = String.Format("$ {0:#,0.00}", shoppingCartTotals.TotalShipping)
                },
                new Total()
                {
                    Title = resources.GetResourceString("Kadena.Checkout.Totals.Subtotal"),
                    Value = String.Format("$ {0:#,0.00}", shoppingCartTotals.Subtotal)
                },
                new Total()
                {
                    Title = resources.GetResourceString("Kadena.Checkout.Totals.Tax"),
                    Value = String.Format("$ {0:#,0.00}", shoppingCartTotals.TotalTax)
                },
                new Total()
                {
                    Title = resources.GetResourceString("Kadena.Checkout.Totals.Totals"),
                    Value = String.Format("$ {0:#,0.00}", shoppingCartTotals.TotalPrice)
                }
            }.ToList();
        }

        private void CheckCurrentOrDefaultAddress(CheckoutPage page)
        {
            int currentAddress = kenticoProvider.GetCurrentCartAddresId();
            if (currentAddress != 0 && page.DeliveryAddresses.items.Any(a => a.Id == currentAddress))
            {
                page.DeliveryAddresses.CheckAddress(currentAddress);
            }
            else
            {
                int defaultAddressId = page.DeliveryAddresses.GetDefaultAddressId();
                if (defaultAddressId != 0)
                {
                    kenticoProvider.SetShoppingCartAddres(defaultAddressId);
                    page.DeliveryAddresses.CheckAddress(defaultAddressId);
                }
            }
        }

        private void CheckCurrentOrDefaultShipping(CheckoutPageDeliveryTotals page)
        {
            int currentShipping = kenticoProvider.GetCurrentCartShippingOptionId();

            if (page.DeliveryMethods.IsPresent(currentShipping) && !page.DeliveryMethods.IsDisabled(currentShipping))
            {
                page.DeliveryMethods.CheckMethod(currentShipping);
            }
            else
            {
                SetDefaultShipping(page);
            }
        }

        private void SetDefaultShipping(CheckoutPageDeliveryTotals page)
        {
            int defaultMethodId = page.DeliveryMethods.GetDefaultMethodId();
            kenticoProvider.SelectShipping(defaultMethodId);
            page.DeliveryMethods.CheckMethod(defaultMethodId);
        }

        private void SetPricesVisibility(CheckoutPage page)
        {
            if (!kenticoProvider.UserCanSeePrices())
            {
                page.Products.HidePrices();
            }
        }

        private void SetPricesVisibility(CheckoutPageDeliveryTotals page)
        {
            if (!kenticoProvider.UserCanSeePrices())
            {
                page.DeliveryMethods.HidePrices();
            }
        }

        private List<PaymentMethod> ArrangePaymentMethods(PaymentMethod[] allMethods)
        {
            var purchaseOrderMethod = allMethods.Where(m => m.ClassName.Contains("PurchaseOrder")).FirstOrDefault();
            if (purchaseOrderMethod != null)
            {
                purchaseOrderMethod.HasInput = true;
                purchaseOrderMethod.InputPlaceholder = resources.GetResourceString("Kadena.Checkout.InsertPONumber");
            }

            return allMethods.ToList();
        }

        public CheckoutPage SelectShipipng(int id)
        {
            kenticoProvider.SelectShipping(id);
            return GetCheckoutPage();
        }

        public CheckoutPage SelectAddress(int id)
        {
            kenticoProvider.SetShoppingCartAddres(id);
            var checkoutPage = GetCheckoutPage();
            checkoutPage.DeliveryAddresses.CheckAddress(id);
            return checkoutPage;
        }

        public CheckoutPage ChangeItemQuantity(int id, int quantity)
        {
            kenticoProvider.SetCartItemQuantity(id, quantity);
            return GetCheckoutPage();
        }

        public CheckoutPage RemoveItem(int id)
        {
            kenticoProvider.RemoveCartItem(id);
            return GetCheckoutPage();
        }

        public CartItemsPreview ItemsPreview()
        {
            var cartItemsTotals = kenticoProvider.GetShoppingCartTotals();
            var cartItems = kenticoProvider.GetShoppingCartItems();

            return new CartItemsPreview
            {
                EmptyCartMessage = resources.GetResourceString("Kadena.Checkout.CartIsEmpty"),
                Cart = new CartButton
                {
                    Label = resources.GetResourceString("Kadena.Checkout.ProceedToCheckout"),
                    Url = "/checkout"
                },
                SummaryPrice = new CartPrice
                {
                    PricePrefix = resources.GetResourceString("Kadena.Checkout.ItemPricePrefix"),
                    Price = string.Format("{0:#,0.00}", cartItemsTotals.TotalItemsPrice)
                },
                Items = cartItems.ToList()
            };
        }

        public async Task<CartItemsPreview> AddToCart(NewCartItem item)
        {
            var addedItem = kenticoProvider.AddCartItem(item);
            if (addedItem != null)
            {
                if (addedItem.IsTemplated)
                {
                    await CallRunGeneratePdfTask(addedItem);
                    //if (!(await CallRunGeneratePdfTask(addedItem)))
                    //{
                    //    ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "Error", ScriptHelper.GetScript("alert('Unable to add item into cart because start generating hires PDF failed');"));
                    //    return;
                    //}
                }

                //ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "Alert", ScriptHelper.GetScript("alert('" + ResHelper.GetString("Kadena.Product.ItemsAddedToCart", LocalizationContext.CurrentCulture.CultureCode) + "');"));
            }
            return ItemsPreview();
        }

        private async Task<bool> CallRunGeneratePdfTask(CartItem cartItem)
        {
            string endpoint = resources.GetSettingsKey("KDA_TemplatingServiceEndpoint");
            var response = await templateService.RunGeneratePdfTask(endpoint, cartItem.EditorTemplateId.ToString(), cartItem.ProductChiliPdfGeneratorSettingsId.ToString());
            if (response.Success && response.Payload != null)
            {
                kenticoProvider.SetPdfGenerationTaskId(cartItem.Id, new Guid(response.Payload.TaskId));
                return true;
            }
            else
            {
                kenticoLog.LogError($"Template service client with templateId={cartItem.EditorTemplateId} and settingsId={cartItem.ProductChiliPdfGeneratorSettingsId}",
                    response?.Error?.Message ?? string.Empty);
                return false;
            }
        }
    }
}
