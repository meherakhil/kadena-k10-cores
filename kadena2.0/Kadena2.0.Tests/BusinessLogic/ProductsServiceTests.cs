﻿using Kadena.WebAPI.KenticoProviders.Contracts;
using Kadena.BusinessLogic.Services;
using Xunit;
using System.Collections.Generic;
using Kadena.Models.Product;
using Moq;

namespace Kadena.Tests.BusinessLogic
{
    public class ProductsServiceTests : KadenaUnitTest<ProductsService>
    {
        const string borderStyleValue = "1px";

        private void SetupBase(bool borderOnSite, bool borderOnCategory)
        {
            Setup<IKenticoFavoritesProvider, List<int>>(f => f.CheckFavoriteProductIds(new List<int>() { 1, 2 }), new List<int>() { 2 });

            Setup<IKenticoProductsProvider, List<ProductLink>>(p => p.GetProducts("/")
                , new List<ProductLink>()
                {
                    new ProductLink {Id = 1, Border = new Border { Exists = true, Value = borderStyleValue} },
                    new ProductLink {Id = 2, Border = new Border { Exists = false, Value = string.Empty} }
                });
            Setup<IKenticoProductsProvider, List<ProductCategoryLink>>(p => p.GetCategories("/")
                , new List<ProductCategoryLink>()
                {
                    new ProductCategoryLink { Id = 10, ProductBordersEnabled = true } ,
                    new ProductCategoryLink { Id = 11, ProductBordersEnabled = false }
                });
            Setup<IKenticoProductsProvider, ProductCategoryLink>(p => p.GetCategory("/")
                , new ProductCategoryLink { Id = 10, ProductBordersEnabled = borderOnCategory });

            Setup<IKenticoResourceService, bool>(r => r.GetSiteSettingsKey<bool>("KDA_ProductThumbnailBorderEnabled"), borderOnSite);
            Setup<IKenticoResourceService, string>(r => r.GetSiteSettingsKey("KDA_ProductThumbnailBorderStyle"), borderStyleValue);
        }

        [Fact(DisplayName = "ProductsService.GetProducts() | All borders turned on")]
        public void GetProductsAndCategories_BasicTest()
        {
            // Arrange
            SetupBase(true, true);

            // Act
            var actualResult = Sut.GetProducts("/");

            // Assert
            Assert.NotNull(actualResult);
            Assert.NotNull(actualResult.Products);
            Assert.NotNull(actualResult.Categories);
            Assert.Equal(2, actualResult.Products.Count);
            Assert.Equal(2, actualResult.Categories.Count);
            Assert.Equal(1, actualResult.Products[0].Id);
            Assert.Equal(2, actualResult.Products[1].Id);
            Assert.Equal(10, actualResult.Categories[0].Id);
            Assert.Equal(11, actualResult.Categories[1].Id);
        }

        [Theory(DisplayName = "ProductsService.GetProducts() | Combinations of border settings")]
        [InlineData(true, true, true, borderStyleValue)]
        [InlineData(true, false, false, "")]
        [InlineData(false, false, false, "")]
        public void GetProductsAndCategories_BordersTest(bool borderOnSite, bool borderOnCategory, bool expectedResult, string style)
        {
            // Arrange
            SetupBase(borderOnSite, borderOnCategory);

            // Act
            var actualResult = Sut.GetProducts("/");

            // Assert
            Assert.NotNull(actualResult);
            Assert.NotNull(actualResult.Products);
            Assert.NotNull(actualResult.Categories);
            Assert.Equal(2, actualResult.Products.Count);
            Assert.Equal(2, actualResult.Categories.Count);
            Assert.NotNull(actualResult.Products[0].Border);
            Assert.NotNull(actualResult.Products[1].Border);
            Assert.Equal(1, actualResult.Products[0].Id);
            Assert.Equal(2, actualResult.Products[1].Id);
            Assert.Equal(10, actualResult.Categories[0].Id);
            Assert.Equal(11, actualResult.Categories[1].Id);
            Assert.True(actualResult.Products[0].Border.Exists == expectedResult);
            Assert.Equal(style, actualResult.Products[0].Border.Value);
        }

        [Fact(DisplayName = "ProductsService.GetProducts() | Favorite products")]
        public void FavoriteProductsTest()
        {
            // Arrange
            SetupBase(true, true);

            // Act
            var actualResult = Sut.GetProducts("/");

            // Assert
            Assert.NotNull(actualResult);
            Assert.NotNull(actualResult.Products);
            Assert.NotNull(actualResult.Categories);
            Assert.Equal(2, actualResult.Products.Count);
            Assert.Equal(2, actualResult.Categories.Count);
            Assert.Equal(1, actualResult.Products[0].Id);
            Assert.Equal(2, actualResult.Products[1].Id);
            Assert.Equal(10, actualResult.Categories[0].Id);
            Assert.Equal(11, actualResult.Categories[1].Id);
            Assert.False(actualResult.Products[0].IsFavourite);
            Assert.True(actualResult.Products[1].IsFavourite);
        }

        [Fact(DisplayName = "ProductsService.GetProducts() | Ordering by order")]
        public void OrderingByOrderTest()
        {
            // Arrange
            Setup<IKenticoProductsProvider, List<ProductLink>>(p => p.GetProducts("/")
                , new List<ProductLink> {
                    new ProductLink { Order = 3, Title = "p3" },
                    new ProductLink { Order = 1, Title = "p1" },
                    new ProductLink { Order = 2, Title = "p2" },
                });
            Setup<IKenticoProductsProvider, List<ProductCategoryLink>>(p => p.GetCategories("/")
                , new List<ProductCategoryLink> {
                    new ProductCategoryLink { Order = 3, Title = "c3" },
                    new ProductCategoryLink { Order = 1, Title = "c1" },
                    new ProductCategoryLink { Order = 2, Title = "c2" },
                });

            // Act
            var actualResult = Sut.GetProducts("/");

            // Assert
            Assert.True(actualResult.Products.Count == 3);
            Assert.True(actualResult.Categories.Count == 3);
            Assert.Equal("p1", actualResult.Products[0].Title);
            Assert.Equal("p2", actualResult.Products[1].Title);
            Assert.Equal("p3", actualResult.Products[2].Title);
            Assert.Equal("c1", actualResult.Categories[0].Title);
            Assert.Equal("c2", actualResult.Categories[1].Title);
            Assert.Equal("c3", actualResult.Categories[2].Title);
        }

        [Theory(DisplayName = "ProductsService.GetAvailableProductsString() | Non inventory type")]
        [InlineData("KDA.MailingProduct")]
        [InlineData("KDA.TemplatedProduct")]
        [InlineData("KDA.ProductWithAddOns")]
        [InlineData("KDA.StaticProduct")]
        [InlineData("KDA.POD")]
        public void GetAvailableProductStringTest_NonInventory(string productType)
        {
            // Arrange
            Setup<IKenticoResourceService, string>(r => r.GetResourceString(It.IsAny<string>(), It.IsAny<string>()), "value");

            // Act
            var result = Sut.GetAvailableProductsString(productType, 10, "cz", 10);

            // Assert
            Assert.Equal(string.Empty, result);
        }


        [Theory(DisplayName = "ProductsService.GetAvailableProductsString() | Inventory type")]
        [InlineData(null, 0, "Kadena.Product.Unavailable")]
        [InlineData(0, 0, "Kadena.Product.OutOfStock")]
        [InlineData(5, 10, "10 pcs in stock")]
        public void GetAvailableProductStringTest_Inventory(int? numberOfAvailableProducts, int numberOfStockProducts, string expectedResult)
        {
            // Arrange
            const string culture = "cz-CZ";
            Setup<IKenticoResourceService, string, string, string>(r => r.GetResourceString(It.IsAny<string>(), culture), (a, b) => a);
            Setup<IKenticoResourceService, string>(r => r.GetResourceString("Kadena.Product.NumberOfAvailableProducts", culture), "{0} pcs in stock");

            // Act
            var result = Sut.GetAvailableProductsString("KDA.InventoryProduct", numberOfAvailableProducts, culture, numberOfStockProducts);

            // Assert
            Assert.Equal(expectedResult, result);
        }


        [Theory(DisplayName = "ProductsService.GetInventoryProductAvailability() | Non inventory type")]
        [InlineData("KDA.MailingProduct")]
        [InlineData("KDA.TemplatedProduct")]
        [InlineData("KDA.ProductWithAddOns")]
        [InlineData("KDA.StaticProduct")]
        [InlineData("KDA.POD")]
        public void GetInventoryProductAvailablity_NonInventory(string productType)
        {
            // Act
            var result = Sut.GetInventoryProductAvailability(productType, 10, 10);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Theory(DisplayName = "ProductsService.GetInventoryProductAvailability() | Inventory type")]
        [InlineData(0, 0, "OutOfStock")]
        [InlineData(null, 0, "Unavailable")]
        [InlineData(1, 1, "Available")]
        public void GetInventoryProductAvailablity(int? numberOfAvailableProducts, int numberOfStockProducts, string expectedResult)
        {
            // Act
            var result = Sut.GetInventoryProductAvailability("KDA.InventoryProduct", numberOfAvailableProducts, numberOfStockProducts);

            // Assert
            Assert.Equal(expectedResult, result);
        }


        [Theory(DisplayName = "ProductsService.CanDisplayAddToCartButton() | Non inventory type")]
        [InlineData("KDA.MailingProduct", false)]
        [InlineData("KDA.TemplatedProduct", false)]
        [InlineData("KDA.ProductWithAddOns", true)]
        [InlineData("KDA.StaticProduct", true)]
        [InlineData("KDA.POD", true)]
        public void CanDisplayAddToCart_NonInventory(string productType, bool expectedResult)
        {
            // Act
            var result = Sut.CanDisplayAddToCartButton(productType, 0, false);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory(DisplayName = "ProductsService.CanDisplayAddToCartButton() | Inventory type")]
        [InlineData(null, true, false)]
        [InlineData(null, false, false)]
        [InlineData(0, true, false)]
        [InlineData(0, false, true)]
        [InlineData(1, true, true)]
        [InlineData(1, false, true)]
        public void CanDisplayAddToCart_Inventory(int? numberOfAvailableProducts, bool sellOnlyAvailable, bool expectedResult)
        {
            // Act
            var result = Sut.CanDisplayAddToCartButton("KDA.InventoryProduct", numberOfAvailableProducts, sellOnlyAvailable);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
