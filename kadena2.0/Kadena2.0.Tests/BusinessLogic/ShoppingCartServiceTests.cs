﻿using Kadena.AmazonFileSystemProvider;
using AutoMapper;
using Kadena.BusinessLogic.Contracts;
using Kadena.BusinessLogic.Contracts.Orders;
using Kadena.BusinessLogic.Factories.Checkout;
using Kadena.BusinessLogic.Services;
using Kadena.Models;
using Kadena.Models.Checkout;
using Kadena.Models.Membership;
using Kadena.Models.Product;
using Kadena.Models.SiteSettings;
using Kadena.WebAPI.KenticoProviders.Contracts;
using Kadena2.MicroserviceClients.Contracts;
using Kadena2.WebAPI.KenticoProviders.Contracts;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Kadena.Tests.BusinessLogic
{
    public class ShoppingCartServiceTests : KadenaUnitTest<ShoppingCartService>
    {
        string Name => "default name";
        string CustomName => "custom name";

        NewCartItem CreateNewCartItem(string customName = null)
        {
            return new NewCartItem
            {
                Quantity = 2,
                DocumentId = 1123,
                CustomProductName = customName
            };
        }

        NewCartItem CreateNewTemplatedCartItem(string customName = null)
        {
            return new NewCartItem
            {
                Quantity = 2,
                NodeId = 32,
                CustomProductName = customName
            };
        }

        private DeliveryAddress CreateDeliveryAddress()
        {
            return new DeliveryAddress()
            {
                City = "Prague"
            };
        }

        private DeliveryCarrier CreateDeliveryCarrier()
        {
            return new DeliveryCarrier()
            {
                Title = "CeskaPosta"
            };
        }

        private PaymentMethod CreatePaymentMethod()
        {
            return new PaymentMethod()
            {
                Title = "Card",
                ClassName = "PurchaseOrder"
            };
        }

        private CheckoutCartItem CreateCartitem(int id)
        {
            return new CheckoutCartItem()
            {
                Id = id,
                CartItemText = $"Item{id}",
                ProductType = ProductTypes.StaticProduct,
                TotalPrice = 10,
                UnitPrice = 2,
                Quantity = 5,
                PriceText = "10"
            };
        }

        private ShoppingCartTotals GetShoppingCartTotals(decimal totalItemsPrice)
        {
            return new ShoppingCartTotals()
            {
                TotalItemsPrice = totalItemsPrice,
                TotalShipping = 19.99m,
                TotalTax = 0.2m * totalItemsPrice
            };
        }

        private void SetupBase()
        {
            Setup<IKenticoCustomerProvider, Customer>(p => p.GetCurrentCustomer(), new Customer { DefaultShippingAddressId = 1, Id = 1 });
            Setup<IKenticoUserProvider, User>(p => p.GetCurrentUser(), new User { UserId = 1 });

            Setup<IKenticoAddressBookProvider, DeliveryAddress[]>(p => p.GetCustomerAddresses(AddressType.Shipping), new[] { CreateDeliveryAddress() });

            Setup<IShoppingCartProvider, DeliveryCarrier[]>(p => p.GetShippingCarriers(), new[] { CreateDeliveryCarrier() });
            Setup<IShoppingCartProvider, PaymentMethod[]>(p => p.GetPaymentMethods(), new[] { CreatePaymentMethod() });
            Setup<IShoppingCartProvider, ShoppingCartTotals>(p => p.GetShoppingCartTotals(), GetShoppingCartTotals(100));

            Setup<IShoppingCartItemsProvider, CheckoutCartItem[]>(p => p.GetCheckoutCartItems(It.IsAny<bool>()), new[] { CreateCartitem(1) });

            Setup<IKenticoResourceService, string>(m => m.GetResourceString("Kadena.Checkout.CountOfItems"), "You have {0} {1} in cart");
            Setup<IKenticoResourceService, string>(m => m.GetResourceString("Kadena.Checkout.ItemSingular"), "item");
            Setup<IKenticoResourceService, string>(m => m.GetSiteSettingsKey(Settings.KDA_AddressDefaultCountry), "0");

            Use<ICheckoutPageFactory, CheckoutPageFactory>();
        }

        [Theory(DisplayName = "ShoppingCartService.AddToCart() | Less than 1 item")]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task AddToCart_CannotAddZero(int quantity)
        {
            // Act
            Task action() => Sut.AddToCart(new NewCartItem { Quantity = quantity });

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(action);
        }

        [Fact(DisplayName = "ShoppingCartService.AddToCart() | Dynamic price")]
        public async Task AddToCart_DynamicPrice()
        {
            // Arrange 
            const decimal dynamicPrice = 12.34m;
            const int cartItemId = 456;
            var newCartItem = CreateNewCartItem();
            var product = new Product
            {
                ProductType = ProductTypes.StaticProduct
            };
            var originalCartItemEntity = new CartItemEntity
            {
                ProductType = ProductTypes.StaticProduct,
                Quantity = 3,
                SKUID = 123,
                CartItemID = cartItemId
            };
            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByDocumentId(newCartItem.DocumentId), product);
            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByNodeId(newCartItem.NodeId), product);
            Setup<IShoppingCartItemsProvider, CartItemEntity>(ip => ip.GetOrCreateCartItem(product.SkuId, newCartItem.Quantity, newCartItem.Options, newCartItem.TemplateId), originalCartItemEntity);
            Setup<IKenticoSkuProvider, Sku>(p => p.GetSKU(123), new Sku { });
            Setup<IProductsService, decimal>(p => p.GetPriceByCustomModel(product.Id, originalCartItemEntity.Quantity), dynamicPrice);

            // Act
            var result = await Sut.AddToCart(newCartItem);

            // Assert
            Assert.NotNull(result);
            Verify<IShoppingCartItemsProvider>(i => i.SaveCartItem(It.Is<CartItemEntity>(
                    e => e.CartItemPrice == dynamicPrice &&
                         e.CartItemID == cartItemId)
                ), Times.Once);
        }

        [Fact(DisplayName = "ShoppingCartService.AddToCart() | Static product")]
        public async Task AddToCart_StaticProduct()
        {
            // Arrange 
            var newCartItem = CreateNewCartItem();
            var product = new Product
            {
                ProductType = ProductTypes.StaticProduct,
            };
            var originalCartItemEntity = new CartItemEntity
            {
                CartItemText = Name,
                ProductType = ProductTypes.StaticProduct,
                Quantity = 3,
                SKUID = 123
            };
            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByDocumentId(newCartItem.DocumentId), product);
            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByNodeId(newCartItem.NodeId), product);
            Setup<IShoppingCartItemsProvider, CartItemEntity>(ip => ip.GetOrCreateCartItem(product.SkuId, newCartItem.Quantity, newCartItem.Options, newCartItem.TemplateId), originalCartItemEntity);
            Setup<IKenticoSkuProvider, Sku>(p => p.GetSKU(123), new Sku { });

            // Act
            var result = await Sut.AddToCart(newCartItem);

            // Assert
            Assert.NotNull(result);
            VerifyNoOtherCalls<IKListService>();
            Verify<IShoppingCartItemsProvider>(i => i.SaveCartItem(It.Is<CartItemEntity>(
                    e => e.CartItemText == originalCartItemEntity.CartItemText &&
                    e.Quantity == originalCartItemEntity.Quantity)
                ), Times.Once);
        }

        [Fact(DisplayName = "ShoppingCartService.AddToCart() | Templated product")]
        public async Task AddToCart_TemplatedProduct()
        {
            // Arrange             
            var newCartItem = CreateNewTemplatedCartItem(CustomName);
            newCartItem.Quantity = 5;
            var product = new Product
            {
                Id = 1123,
                ProductType = ProductTypes.TemplatedProduct,
            };
            var originalCartItemEntity = new CartItemEntity
            {
                CartItemText = Name,
                ProductType = ProductTypes.TemplatedProduct,
                Quantity = 10,
                SKUID = 123
            };

            const int nodeId = 32;

            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByDocumentId(newCartItem.DocumentId), product);
            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByNodeId(newCartItem.NodeId), product);
            Setup<IKenticoSkuProvider, Sku>(p => p.GetSKU(123), new Sku { });
            Setup<IShoppingCartItemsProvider, CartItemEntity>(ip => ip.GetOrCreateCartItem(product.SkuId, newCartItem.Quantity, newCartItem.Options, newCartItem.TemplateId), originalCartItemEntity);

            // Act
            var result = await Sut.AddToCart(newCartItem);

            // Assert
            Assert.NotNull(result);
            VerifyNoOtherCalls<IKListService>();
            Verify<IShoppingCartItemsProvider>(i => i.SaveCartItem(It.Is<CartItemEntity>(
                    e => e.CartItemText == CustomName &&
                         e.Quantity == 5)
                ), Times.Once);
        }

        [Fact(DisplayName = "ShoppingCartService.AddToCart() | Inventory product")]
        public async Task AddToCart_InventoryProduct_OK()
        {
            // Arrange 
            var newCartItem = CreateNewCartItem();
            var product = new Product
            {
                ProductType = ProductTypes.InventoryProduct,
            };
            var originalCartItemEntity = new CartItemEntity
            {
                CartItemText = Name,
                ProductType = ProductTypes.InventoryProduct,
                SKUID = 6654,
                Quantity = 3
            };

            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByDocumentId(newCartItem.DocumentId), product);
            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByNodeId(newCartItem.NodeId), product);
            Setup<IShoppingCartItemsProvider, CartItemEntity>(ip => ip.GetOrCreateCartItem(product.SkuId, newCartItem.Quantity, newCartItem.Options, newCartItem.TemplateId), originalCartItemEntity);
            Setup<IKenticoSkuProvider, Sku>(cp => cp.GetSKU(originalCartItemEntity.SKUID), new Sku { AvailableItems = 100 });
            Setup<IKenticoSkuProvider, Sku>(p => p.GetSKU(6654), new Sku { });

            // Act
            var result = await Sut.AddToCart(newCartItem);

            // Assert
            Assert.NotNull(result);
            VerifyNoOtherCalls<IKListService>();
            Verify<IShoppingCartItemsProvider>(i => i.SaveCartItem(It.Is<CartItemEntity>(
                    e => e.CartItemText == originalCartItemEntity.CartItemText &&
                         e.SKUID == originalCartItemEntity.SKUID &&
                         e.Quantity == originalCartItemEntity.Quantity)
                ), Times.Once);
        }

        [Fact(DisplayName = "ShoppingCartService.AddToCart() | Inventory product out of stock")]
        public async Task AddToCart_InventoryProduct_OutOfStock()
        {
            // Arrange 
            var newCartItem = CreateNewCartItem();
            var product = new Product
            {
                ProductType = ProductTypes.InventoryProduct,
            };

            var originalCartItemEntity = new CartItemEntity
            {
                CartItemText = Name,
                ProductType = ProductTypes.InventoryProduct,
                Quantity = 3
            };

            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByDocumentId(newCartItem.DocumentId), product);
            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByNodeId(newCartItem.NodeId), product);
            Setup<IShoppingCartItemsProvider, CartItemEntity>(ip => ip.GetOrCreateCartItem(product.SkuId, newCartItem.Quantity, newCartItem.Options, newCartItem.TemplateId), originalCartItemEntity);
            Setup<IKenticoSkuProvider, Sku>(cp => cp.GetSKU(originalCartItemEntity.SKUID), new Sku { AvailableItems = 1, SellOnlyIfAvailable = true });
            SetupThrows<IOrderItemCheckerService>(o => o.EnsureInventoryAmount(It.IsAny<Sku>(), 2, 3), new ArgumentException());

            // Act
            Task action() => Sut.AddToCart(newCartItem);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(action);
        }

        [Fact(DisplayName = "ShoppingCartService.AddToCart() | Mailing product")]
        public async Task AddToCart_MailingProduct()
        {
            // Arrange 
            const int quantity = 798;
            var containerId = new Guid("1a3b944e-3632-467b-a53a-206305310bae");
            var newCartItem = CreateNewCartItem();
            newCartItem.ContainerId = containerId;
            newCartItem.Quantity = quantity;
            var product = new Product
            {
                ProductType = ProductTypes.MailingProduct,
            };
            var originalCartItemEntity = new CartItemEntity
            {
                CartItemText = Name,
                ProductType = ProductTypes.MailingProduct,
                Quantity = 3,
                SKUID = 123
            };

            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByDocumentId(newCartItem.DocumentId), product);
            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByNodeId(newCartItem.NodeId), product);
            Setup<IKenticoSkuProvider, Sku>(p => p.GetSKU(123), new Sku { Name = Name });
            Setup<IShoppingCartItemsProvider, CartItemEntity>(ip => ip.GetOrCreateCartItem(product.SkuId, newCartItem.Quantity, newCartItem.Options, newCartItem.TemplateId), originalCartItemEntity);
            Setup<IKListService, Task<MailingList>>(m => m.GetMailingList(containerId)
                , Task.FromResult(new MailingList { AddressCount = quantity, Id = containerId.ToString() }));

            // Act
            var result = await Sut.AddToCart(newCartItem);

            // Assert
            Assert.NotNull(result);
            Verify<IKListService>(m => m.GetMailingList(containerId), Times.Once);
            Verify<IShoppingCartItemsProvider>(i => i.SaveCartItem(It.Is<CartItemEntity>(
                    e => e.CartItemText == Name &&
                         e.Quantity == quantity)
                ), Times.Once);
        }

        [Fact(DisplayName = "ShoppingCartService.GetCheckoutPage()")]
        public async Task GetCheckoutPageTest()
        {
            // Arrange
            SetupBase();

            // Act
            var result = await Sut.GetCheckoutPage();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.DeliveryAddresses);
            Assert.NotNull(result.EmailConfirmation);
            Assert.NotNull(result.PaymentMethods);
            Assert.NotNull(result.Submit);
            Assert.Matches("You have 1 item in cart", result.Products.Number);
            Assert.Single(result.Products.Items);
            Assert.Equal(10, result.Products.Items[0].TotalPrice);
        }

        [Fact(DisplayName = "ShoppingCartService.ChangeItemQuantity()")]
        public void ChangeItemQuantityTest()
        {
            // Arrange
            SetupBase();
            const int skuid = 456;
            var cartItem = new CartItemEntity { CartItemID = 1, ProductType = ProductTypes.POD, SKUID = skuid };
            Setup<IShoppingCartItemsProvider, CartItemEntity>(m => m.GetCartItemEntity(1), cartItem);
            Setup<IKenticoSkuProvider, Sku>(m => m.GetSKU(skuid), new Sku { });

            // Act
            var result = Sut.ChangeItemQuantity(1, 100);

            // Assert
            Assert.NotNull(result);
            Verify<IShoppingCartItemsProvider>(m => m.SetCartItemQuantity(It.Is<CartItemEntity>(e => e.CartItemID == 1), 100), Times.Once);
        }

        [Theory(DisplayName = "ShoppingCartService.GetCartItems()")]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void SeePricesPermissions_Items(bool userHasPermissions, bool priceShouldBeEmpty)
        {
            // Arrange
            SetupBase();
            Setup<IKenticoPermissionsProvider, bool>(p => p.UserCanSeePrices(), userHasPermissions);

            // Act
            var result = Sut.GetCartItems();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(string.IsNullOrEmpty(result.Items[0].PriceText), priceShouldBeEmpty);
            Assert.Equal(string.IsNullOrEmpty(result.SummaryPrice.Price), priceShouldBeEmpty);
        }

        [Fact(DisplayName = "ShoppingCartService.GetDeliveryAndTotals() | User can see prices")]
        public async Task CanSeePrices_DeliveryAndTotals()
        {
            // Arrange
            SetupBase();
            Setup<IKenticoPermissionsProvider, bool>(p => p.UserCanSeePrices(), true);

            // Act
            var result = await Sut.GetDeliveryAndTotals();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.DeliveryMethods);
            Assert.NotNull(result.Totals);
            Assert.True(result.Totals.Items.TrueForAll(i => !string.IsNullOrEmpty(i.Value)));
        }

        [Fact(DisplayName = "ShoppingCartService.GetDeliveryAndTotals() | User can't see prices")]
        public async Task CannotSeePrices_DeliveryAndTotals()
        {
            // Arrange
            SetupBase();
            Setup<IKenticoPermissionsProvider, bool>(p => p.UserCanSeePrices(), false);

            // Act
            var result = await Sut.GetDeliveryAndTotals();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.DeliveryMethods);
            Assert.NotNull(result.Totals);
            Assert.Null(result.Totals.Items);
        }

        [Fact(DisplayName = "ShoppingCartService.ItemsPreview()")]
        public void ItemPreview()
        {
            // Arrange
            SetupBase();

            // Act
            var result = Sut.ItemsPreview();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.Single(result.Items);
        }

        [Fact(DisplayName = "ShoppingCartService.AddToCart() | Inventory product out of stock without verifying availability")]
        public async Task AddToCart_InventoryProduct_OutOfStock_DontCheckForAvailability()
        {
            // Arrange 
            var newCartItem = CreateNewCartItem();
            var product = new Product
            {
                ProductType = ProductTypes.InventoryProduct
            };
            var originalCartItemEntity = new CartItemEntity
            {
                CartItemText = Name,
                ProductType = ProductTypes.InventoryProduct,
                Quantity = 3
            };

            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByDocumentId(newCartItem.DocumentId), product);
            Setup<IKenticoProductsProvider, Product>(s => s.GetProductByNodeId(newCartItem.NodeId), product);
            Setup<IShoppingCartItemsProvider, CartItemEntity>(ip => ip.GetOrCreateCartItem(product.SkuId, newCartItem.Quantity, newCartItem.Options, newCartItem.TemplateId), originalCartItemEntity);
            Setup<IKenticoSkuProvider, Sku>(cp => cp.GetSKU(originalCartItemEntity.SKUID), new Sku { AvailableItems = 1, SellOnlyIfAvailable = false });

            // Act
            var result = await Sut.AddToCart(newCartItem);

            // Assert
            Assert.NotNull(result);
        }

        [Theory(DisplayName = "ShoppingCartService()")]
        [ClassData(typeof(ShoppingCartServiceTests))]
        public void ShoppingCartService(IKenticoSiteProvider kenticoSite,
                                   IKenticoLocalizationProvider localization,
                                   IKenticoPermissionsProvider permissions,
                                   IKenticoUserProvider kenticoUsers,
                                   IKenticoCustomerProvider kenticoCustomer,
                                   IKenticoAddressBookProvider addresses,
                                   IKenticoResourceService resources,
                                   IKenticoProductsProvider productsProvider,
                                   ITaxEstimationService taxCalculator,
                                   IKListService mailingService,
                                   IUserDataServiceClient userDataClient,
                                   IShoppingCartProvider shoppingCart,
                                   IShoppingCartItemsProvider shoppingCartItems,
                                   ICheckoutPageFactory checkoutfactory,
                                   IKenticoLogger log,
                                   IProductsService productsService,
                                   IImageService imageService,
                                   IKenticoSkuProvider skus,
                                   IArtworkService artworkService,
                                   IOrderItemCheckerService orderChecker,
                                   ISettingsService settingsService,
                                   IMapper mapper)
        {
            Assert.Throws<ArgumentNullException>(() => new ShoppingCartService(kenticoSite, localization, permissions, kenticoUsers,
                kenticoCustomer, addresses, resources, productsProvider, taxCalculator, mailingService, userDataClient, shoppingCart, shoppingCartItems,
                checkoutfactory, log, productsService, imageService, skus, artworkService, orderChecker, settingsService, mapper));
        }

        [Fact(DisplayName = "ShoppingCartService.SaveTemporaryAddress() | Null address")]
        public void SaveTemporaryAddress_Null()
        {
            Assert.Throws<NullReferenceException>(() => Sut.SaveTemporaryAddress(null));
        }

        public static IEnumerable<object[]> GetTestAddresses()
        {
            yield return new[] {
                new DeliveryAddress()
            };
            yield return new[] {
                new DeliveryAddress{
                    CustomerId = 1
                }
            };
        }
        [Theory(DisplayName = "ShoppingCartService.SaveTemporaryAddress() | Null address")]
        [MemberData(nameof(GetTestAddresses))]
        public void SaveTemporaryAddress(DeliveryAddress address)
        {
            Setup<IKenticoAddressBookProvider, DeliveryAddress[]>(s => s.GetCustomerAddresses(It.IsAny<int>(), AddressType.Shipping), null);
            Setup<IKenticoAddressBookProvider, DeliveryAddress[]>(s => s.GetCustomerAddresses(AddressType.Shipping),
                new[] { new DeliveryAddress { Id = 123, AddressName = "TemporaryAddress" } });


            var actualResult = Sut.SaveTemporaryAddress(address);

            Assert.Equal(address.Id, actualResult);
        }
    }
}
