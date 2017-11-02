﻿using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Localization;
using CMS.Membership;
using Kadena.Models;
using Kadena.Models.Product;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kadena.Old_App_Code.Kadena.Imports.Products
{
    public class ProductImportService : ImportServiceBase
    {
        protected static JsonSerializerSettings camelCaseSerializer = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        public ImportResult ProcessImportFile(byte[] importFileData, ExcelType type, int siteID)
        {
            var site = GetSite(siteID);
            var rows = GetExcelRows(importFileData, type);
            var products = GetDtosFromExcelRows<ProductDto>(rows);
            var statusMessages = new List<string>();

            var currentItemNumber = 1;
            foreach (var productDto in products)
            {
                List<string> validationResults;
                if (!ValidateImportItem(productDto, out validationResults))
                {
                    statusMessages.Add($"Item number {currentItemNumber} has invalid values ({ string.Join("; ", validationResults) })");
                    continue;
                }

                try
                {
                    SaveProduct(productDto);
                }
                catch (Exception ex)
                {
                    statusMessages.Add("There was an error when processing item number " + currentItemNumber);
                    EventLogProvider.LogException("Import users", "EXCEPTION", ex);
                }

                currentItemNumber++;
            }

            return new ImportResult
            {
                ErrorMessages = statusMessages.ToArray()
            };
        }

        private bool ValidateImportItem(ProductDto product, out List<string> validationErrors)
        {
            var errorMessageFormat = "field {0} - {1}";
            bool isValid = ValidatorHelper.ValidateDto(product, out validationErrors, errorMessageFormat);

            // validate special rules
            if (product.ProductType.Contains(ProductTypes.TemplatedProduct))
            {
                if (string.IsNullOrWhiteSpace(product.ChiliTemplateID) ||
                    string.IsNullOrWhiteSpace(product.ChiliWorkgroupID) ||
                    string.IsNullOrWhiteSpace(product.ChiliPdfGeneratorSettingsID))
                {

                    isValid = false;
                    validationErrors.Add("ChiliTemplateID, ChiliWorkgroupID and ChiliPdfGeneratorSettingsID are mandatory for Templated product");
                }
            }

            return isValid;
        }

        private void SaveProduct(ProductDto productDto)
        {
            var categories = productDto.ProductCategory.Split('\n');
            var productParent = CreateProductCategory(categories);
            var newProduct = AppendProduct(productParent, productDto);
        }

        private TreeNode AppendProduct(TreeNode parent, ProductDto product)
        {
            if (parent == null)
                return null;

            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            TreeNode newProduct = parent.Children.FirstOrDefault(c => c.NodeName == product.ProductName);

            if (newProduct == null)
            {
                newProduct = TreeNode.New("KDA.Product", tree);
                newProduct.NodeName = product.ProductName;
                newProduct.DocumentName = product.ProductName;
                newProduct.DocumentCulture = "en-us";
                newProduct.SetValue("ProductType", product.ProductType);
                newProduct.SetValue("ProductSKUWeight", Convert.ToDecimal(product.PackageWeight));
                newProduct.SetValue("ProductNumberOfItemsInPackage", Convert.ToInt32(product.ItemsInPackage));

                newProduct.SetValue("ProductChiliTemplateID", product.ChiliTemplateID ?? string.Empty);
                newProduct.SetValue("ProductChiliWorkgroupID", product.ChiliWorkgroupID ?? string.Empty);
                newProduct.SetValue("ProductChiliPdfGeneratorSettingsId", product.ChiliPdfGeneratorSettingsID ?? string.Empty);
                newProduct.SetValue("ProductSKUNeedsShipping", product.NeedsShipping.ToLower() == "true");
                newProduct.SetValue("ProductDynamicPricing", GetDynamicPricingJson(product.DynamicPriceMinItems, product.DynamicPriceMaxItems, product.DynamicPrice));

                // Inserts the new page as a child of the parent page
                newProduct.Insert(parent);
            }

            return newProduct;
        }

        private string GetDynamicPricingJson(string min, string max, string price)
        {
            int[] mins, maxes;
            decimal[] prices;

            if (string.IsNullOrWhiteSpace(min) && string.IsNullOrWhiteSpace(max) && string.IsNullOrWhiteSpace(price))
            {
                return string.Empty;
            }

            try
            {
                mins = min.Split('\n').Select(m => Convert.ToInt32(m)).ToArray();
                maxes = max.Split('\n').Select(m => Convert.ToInt32(m)).ToArray();
                prices = price.Split('\n').Select(m => Convert.ToDecimal(m)).ToArray();
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException("Bad format of Dynamic Pricing definitions.", ex);
            }


            if (mins.Length != maxes.Length || mins.Length != prices.Length)
            {
                throw new ArgumentOutOfRangeException("Dynamic Pricing definition cells must contain the same count of rows in one product.");
            }


            var ranges = mins.Select((item, index) => new DynamicPricingRange { MinVal = item, MaxVal = maxes[index], Price = prices[index] }).ToList();

            return JsonConvert.SerializeObject(ranges, camelCaseSerializer);
        }


        private TreeNode CreateProductCategory(string[] path)
        {
            var root = DocumentHelper.GetDocuments("KDA.ProductsModule")
                            .Path("/", PathTypeEnum.Children)
                            .WhereEquals("ClassName", "KDA.ProductsModule")
                            .Culture(LocalizationContext.CurrentCulture.CultureCode)
                            .CheckPermissions()
                            .NestingLevel(1)
                            .OnCurrentSite()
                            .Published()
                            .FirstObject;

            return AppendProductCategory(root, path);
        }

        private TreeNode AppendProductCategory(TreeNode parentPage, string[] subnodes)
        {
            if (parentPage == null || subnodes == null || subnodes.Length <= 0)
                return parentPage;

            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            //try to find existing category
            TreeNode category = parentPage.Children.FirstOrDefault(c => c.NodeName == subnodes[0]);

            if (category == null)
            {
                category = TreeNode.New("KDA.ProductCategory", tree);
                category.DocumentName = subnodes[0];
                category.DocumentCulture = "en-us";

                // Inserts the new page as a child of the parent page
                category.Insert(parentPage);
            }

            return AppendProductCategory(category, subnodes.Skip(1).ToArray());
        }
    }
}