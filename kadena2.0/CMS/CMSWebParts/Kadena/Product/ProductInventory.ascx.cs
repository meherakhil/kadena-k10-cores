using CMS.CustomTables;
using CMS.CustomTables.Types.KDA;
using CMS.DataEngine;
using CMS.DocumentEngine.Types.KDA;
using CMS.DocumentEngine.Web.UI;
using CMS.Ecommerce;
using CMS.EventLog;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using Kadena.WebAPI.KenticoProviders.Contracts;
using Kadena.Container.Default;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Kadena.Models.Shipping;
using CMS.Membership;
using Kadena.Models.Product;
using KenticoCampaignProduct = CMS.DocumentEngine.Types.KDA.CampaignsProduct;

public partial class CMSWebParts_Kadena_Product_ProductInventory : CMSAbstractWebPart
{
    private readonly IKenticoProductsProvider productsProvider;

    #region "Properties"

    /// <summary>
    /// Get the Product type.
    /// </summary>
    public int ProductType
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("ProductType"), default(int));
        }
        set
        {
            SetValue("ProductType", value);
        }
    }
    /// <summary>
    /// get the open campaign
    /// </summary>
    public Campaign OpenCampaign
    {
        get
        {
            return CampaignProvider.GetCampaigns().Columns("CampaignID,Name,StartDate,EndDate")
                                .WhereEquals("OpenCampaign", true)
                                .Where(new WhereCondition().WhereEquals("CloseCampaign", false).Or()
                                .WhereEquals("CloseCampaign", null))
                                .WhereEquals("NodeSiteID", CurrentSite.SiteID).FirstOrDefault();
        }
        set
        {
            SetValue("OpenCampaign", value);
        }
    }
    // <summary>
    /// Get the Product type.
    /// </summary>
    public string DefaultShipping
    {
        get
        {
            return ValidationHelper.GetString(GetValue("DefaultShipping"), ShippingOption.Ground);
        }
        set
        {
            SetValue("DefaultShipping", value);
        }
    }

    /// <summary>
    /// Add to cart Link text
    /// </summary>
    public string AddToCartLinkText
    {
        get
        {
            return ValidationHelper.GetString(ResHelper.GetString("Kadena.Product.AddToCartText"), string.Empty);
        }
        set
        {
            SetValue("AddToCartLinkText", value);
        }
    }
    /// <summary>
    /// NoDataText Link text
    /// </summary>
    public string NoDataText
    {
        get
        {
            return ResHelper.GetString("Kadena.ItemList.NoDataFoundText");
        }
        set
        {
            SetValue("NoDataText", value);
        }
    }

    /// <summary>
    /// No campaign open text
    /// </summary>
    public string NoCampaignOpen
    {
        get
        {
            return ResHelper.GetString("Kadena.ItemList.NoCampaignOpen");
        }
        set
        {
            SetValue("NoCampaignOpen", value);
        }
    }
    /// <summary>
    /// Search placeholder text
    /// </summary>
    public string PosSearchPlaceholder
    {
        get
        {
            return ValidationHelper.GetString(ResHelper.GetString("Kadena.Product.PosSearchPlaceholderText"), string.Empty);
        }
        set
        {
            SetValue("PosSearchPlaceholder", value);
        }
    }

    /// <summary>
    /// Checks whether the start an dend dates of campaign are in range
    /// </summary>
    public bool EnableAddToCart
    {
        get
        {
            if (OpenCampaign != null)
            {
                return OpenCampaign.StartDate <= DateTime.Now.Date && OpenCampaign.EndDate >= DateTime.Now.Date ? true : false;
            }
            else
            {
                return false;
            }
        }
        set
        {
            SetValue("EnableAddToCart", value);
        }
    }

    /// <summary>
    /// POS number text
    /// </summary>
    public string POSNumberText
    {
        get
        {
            return ValidationHelper.GetString(ResHelper.GetString("Kadena.Product.POSNumberText"), string.Empty);
        }
        set
        {
            SetValue("POSNumberText", value);
        }
    }
    #endregion "Properties"

    public CMSWebParts_Kadena_Product_ProductInventory()
    {
        productsProvider = DIContainer.Resolve<IKenticoProductsProvider>();
    }

    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindPrograms();
            BindCategories();
            BindBrands();
            BindData();
        }
    }

    /// <summary>
    /// Content loaded event handler.
    /// </summary>
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        SetupControl();
    }

    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        if (this.StopProcessing)
        {
            // Do not process
        }
        else
        {
            chkOnlyAllocatedToMe.Visible = ProductType == (int)CampaignProductType.GeneralInventory;
            chkOnlyAllocatedToMe.InputAttributes.Add("class", "input__checkbox"); // if specified in markup asp generates extra span around

            BindUnipagerTransformations();
            divNoRecords.Visible = false;
            txtSearch.Attributes.Add("placeholder", PosSearchPlaceholder);
        }
    }

    private void BindUnipagerTransformations()
    {
        unipager.PageNumbersTemplate = TransformationHelper.LoadTransformation(unipager, "KDA.Transformations.General-Pages");
        unipager.CurrentPageTemplate = TransformationHelper.LoadTransformation(unipager, "KDA.Transformations.General-CurrentPage");
        unipager.PreviousPageTemplate = TransformationHelper.LoadTransformation(unipager, "KDA.Transformations.General-PreviousPage");
        unipager.NextPageTemplate = TransformationHelper.LoadTransformation(unipager, "KDA.Transformations.General-NextPage");
        unipager.PreviousGroupTemplate = TransformationHelper.LoadTransformation(unipager, "CMS.PagerTransformations.General-PreviousGroup");
        unipager.NextGroupTemplate = TransformationHelper.LoadTransformation(unipager, "CMS.PagerTransformations.General-NextGroup");
        unipager.PageNumbersSeparatorTemplate = TransformationHelper.LoadTransformation(unipager, "CMS.PagerTransformations.General-PageSeparator");
    }

    /// <summary>
    /// Reloads the control data.
    /// </summary>
    public override void ReloadData()
    {
        base.ReloadData();
        SetupControl();
    }

    /// <summary>
    /// Get Product details
    /// </summary>
    /// <param name="programID"></param>
    /// <param name="categoryID"></param>
    /// <param name="searchText"></param>
    /// <returns></returns>
    private List<KenticoCampaignProduct> GetProductsDetails(int categoryID, int brandID, string searchText, List<int> excludeIds, List<int> includeIds)
    {
        var query = CampaignsProductProvider.GetCampaignsProducts()
            .OnCurrentSite()
            .WhereTrue(nameof(SKUInfo.SKUEnabled));
        try
        {
            var programIds = GetProgramIDs();
            if (ProductType == (int)CampaignProductType.GeneralInventory || ProductType == (int)CampaignProductType.PreBuy)
            {
                ddlCategory.Visible = true;
                ddlBrand.Visible = true;
            }

            if (excludeIds?.Any() ?? false)
            {
                query = query.WhereNotIn(nameof(KenticoCampaignProduct.CampaignsProductID), excludeIds);
            }

            if (includeIds?.Any() ?? false)
            {
                query = query.WhereIn(nameof(KenticoCampaignProduct.CampaignsProductID), includeIds);
            }

            if (DataHelper.DataSourceIsEmpty(programIds))
            {
                query = query.WhereEqualsOrNull(nameof(KenticoCampaignProduct.ProgramID), 0);
            }
            else
            {
                query = query.WhereIn(nameof(KenticoCampaignProduct.ProgramID), programIds.ToList());
            }
            if (categoryID > 0)
            {
                query = query.WhereEquals(nameof(KenticoCampaignProduct.CategoryID), categoryID);
            }
            if (brandID > 0)
            {
                query = query.WhereEquals(nameof(KenticoCampaignProduct.BrandID), brandID);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var where = new WhereCondition()
                    .WhereContains("SKUProductCustomerReferenceNumber", searchText)
                    .Or()
                    .WhereContains(nameof(SKUInfo.SKUName), searchText)
                    .Or()
                    .WhereContains(nameof(SKUInfo.SKUDescription), searchText)
                    .Or()
                    .WhereContains("SKUNumberOfItemsInPackage", searchText);

                var brandIds = CustomTableItemProvider.GetItems<BrandItem>()
                    .WhereContains(nameof(BrandItem.BrandName), searchText)
                    .Columns(nameof(BrandItem.ItemID))
                    .Select(i => i.Field<int>(nameof(BrandItem.ItemID)))
                    .ToList();
                if (brandIds.Any())
                {
                    where = where.Or().WhereIn(nameof(KenticoCampaignProduct.BrandID), brandIds);
                }

                query = query.Where(where);
            }
        }
        catch (Exception ex)
        {
            EventLogProvider.LogException("Get Product Details", "GetProductsDetails()", ex, CurrentSite.SiteID, ex.Message);
        }

        return query.ToList();
    }

    private void BindBrands()
    {
        try
        {
            var brands = CustomTableItemProvider.GetItems<BrandItem>()
                            .Columns("ItemId,BrandName")
                            .OrderBy("BrandName")
                            .ToList();
            if (!DataHelper.DataSourceIsEmpty(brands))
            {
                ddlBrand.DataSource = brands;
                ddlBrand.DataTextField = "BrandName";
                ddlBrand.DataValueField = "ItemId";
                ddlBrand.DataBind();
                string selectText = ValidationHelper.GetString(ResHelper.GetString("Kadena.CampaignProduct.SelectBrandText"), string.Empty);
                ddlBrand.Items.Insert(0, new ListItem(selectText, "0"));
            }
        }
        catch (Exception ex)
        {
            EventLogProvider.LogException("Bind Brands to Dropdown", "BindBrands()", ex, CurrentSite.SiteID, ex.Message);
        }
    }

    /// <summary>
    /// Bind the Products data to repeater
    /// </summary>
    /// <param name="categoryID"></param>
    private void BindData(int categoryID = 0, string searchText = null, int brandID = 0, bool onlyAllocatedToMe = false)
    {
        try
        {
            divNoRecords.Visible = false;
            rptProductLists.DataSource = null;
            rptProductLists.DataBind();

            List<int> notAllowedProducts = null;
            if (ProductType == (int)CampaignProductType.GeneralInventory && !onlyAllocatedToMe)
            {
                notAllowedProducts = productsProvider
                    .GetAllocatedProductQuantityForUser(MembershipContext.AuthenticatedUser.UserID)
                    .Where(i => i.Value == 0)
                    .Select(i => i.Key)
                    .ToList();
            }

            List<int> allocatedToMe = null;
            if (ProductType == (int)CampaignProductType.GeneralInventory && onlyAllocatedToMe)
            {
                allocatedToMe = productsProvider
                    .GetAllocatedProductQuantityForUser(MembershipContext.AuthenticatedUser.UserID)
                    .Where(apq => apq.Value > 0)
                    .Select(apq => apq.Key)
                    .ToList();
            }

            var productsDetails = GetProductsDetails(categoryID, brandID, searchText, notAllowedProducts, allocatedToMe);

            if (!DataHelper.DataSourceIsEmpty(productsDetails))
            {
                var productAndSKUDetails = productsDetails
                    .Select((cp) => new
                    {
                        cp.ProgramID,
                        cp.CategoryID,
                        QtyPerPack = cp.GetIntegerValue("SKUNumberOfItemsInPackage", 1),
                        cp.EstimatedPrice,
                        cp.Product.SKUNumber,
                        cp.Product.SKUProductCustomerReferenceNumber,
                        SKUName = cp.Product.Name,
                        SKUPrice = cp.GetDoubleValue(nameof(SKUInfo.SKUPrice), 0.0d),
                        SKUEnabled = cp.GetBooleanValue(nameof(SKUInfo.SKUEnabled), false),
                        cp.ProductImage,
                        SKUAvailableItems = cp.GetIntegerValue(nameof(SKUInfo.SKUAvailableItems), 0),
                        SKUID = cp.Product.ID,
                        SKUDescription = cp.Product.Description
                    })
                    .OrderBy(p => p.SKUName)
                    .ToList();
                rptProductLists.DataSource = productAndSKUDetails;
                rptProductLists.DataBind();
                rptProductLists.UniPagerControl = unipager;
                unipager.PagedControl = rptProductLists;
            }
            else if (OpenCampaign == null && ProductType == (int)CampaignProductType.PreBuy)
            {
                orderControls.Visible = false;
                divNoRecords.Visible = false;
                divNoCampaign.Visible = true;
                rptProductLists.DataSource = null;
                rptProductLists.DataBind();
            }
            else
            {
                divNoRecords.Visible = true;
            }
        }
        catch (Exception ex)
        {
            EventLogProvider.LogException("Bind products to repeater", "BindData()", ex, CurrentSite.SiteID, ex.Message);
        }
    }

    /// <summary>
    /// Get the Program Ids in Open Campaign
    /// </summary>
    /// <returns></returns>
    private IEnumerable<int> GetProgramIDs()
    {
        try
        {
            if (OpenCampaign != null && ProductType == (int)CampaignProductType.PreBuy)
            {
                return ProgramProvider.GetPrograms()
                    .WhereEquals("CampaignID", OpenCampaign.CampaignID)
                    .Columns("ProgramID")
                    .Select(x => x.ProgramID);
            }
        }
        catch (Exception ex)
        {
            EventLogProvider.LogException("Get ProgramsIDs from CVampaign", "GetProgramIDs()", ex, CurrentSite.SiteID, ex.Message);
        }
        return Enumerable.Empty<int>();
    }

    /// <summary>
    /// Bind the Program to dropdown
    /// </summary>
    public void BindPrograms()
    {
        try
        {
            if (OpenCampaign != null)
            {
                if (OpenCampaign.CampaignID != default(int))
                {
                    var programs = ProgramProvider.GetPrograms()
                        .WhereEquals("NodeSiteID", CurrentSite.SiteID)
                        .WhereEquals("CampaignID", OpenCampaign.CampaignID)
                        .Columns("ProgramName,ProgramID")
                        .ToList();
                    if (!DataHelper.DataSourceIsEmpty(programs))
                    {
                        ddlProgram.DataSource = programs;
                        ddlProgram.DataTextField = "ProgramName";
                        ddlProgram.DataValueField = "ProgramID";
                        ddlProgram.DataBind();
                        string selectText = ValidationHelper.GetString(ResHelper.GetString("Kadena.CampaignProduct.SelectProgramText"), string.Empty);
                        ddlProgram.Items.Insert(0, new ListItem(selectText, "0"));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider.LogException("Bind Programs to dropdown", "BindPrograms()", ex, CurrentSite.SiteID, ex.Message);
        }
    }

    /// <summary>
    /// Bind Categories to Dropdown
    /// </summary>
    public void BindCategories()
    {
        try
        {
            var categories = ProductCategoryProvider.GetProductCategories()
                            .WhereEquals("NodeSiteID", CurrentSite.SiteID)
                            .Columns("ProductCategoryID,ProductCategoryTitle")
                            .Select(x => new ProductCategory { ProductCategoryID = x.ProductCategoryID, ProductCategoryTitle = x.ProductCategoryTitle })
                            .ToList();
            if (!DataHelper.DataSourceIsEmpty(categories))
            {
                ddlCategory.DataSource = categories;
                ddlCategory.DataTextField = "ProductCategoryTitle";
                ddlCategory.DataValueField = "ProductCategoryID";
                ddlCategory.DataBind();
                string selectText = ValidationHelper.GetString(ResHelper.GetString("Kadena.CampaignProduct.SelectCategoryText"), string.Empty);
                ddlCategory.Items.Insert(0, new ListItem(selectText, "0"));
            }
        }
        catch (Exception ex)
        {
            EventLogProvider.LogException("Bind Categories to Dropdown", "BindCategories()", ex, CurrentSite.SiteID, ex.Message);
        }
    }

    private void SetFilter()
    {
        BindData(
            ValidationHelper.GetInteger(ddlCategory.SelectedValue, default(int)), 
            ValidationHelper.GetString(txtSearch.Text, string.Empty), 
            ValidationHelper.GetInteger(ddlBrand.SelectedValue, default(int)),
            chkOnlyAllocatedToMe.Checked
        );
    }

    public string GetDemandCount(int SKUID)
    {
        return string.Format("{0:n0}", DIContainer.Resolve<IShoppingCartProvider>().GetPreBuyDemandCount(SKUID));
    }

    protected void OnFilterChanged(object sender, EventArgs e)
    {
        SetFilter();
    }

    #endregion "Methods"

    protected void unipager_OnPageChanged(object sender, int pageNumber)
    {
        SetFilter();
    }
}