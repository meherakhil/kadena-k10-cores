﻿using CMS.EventLog;
using CMS.UIControls;
using System;
using System.Linq;
using System.IO;
using System.Web;
using Kadena.Old_App_Code.Kadena.Imports;
using Kadena.Old_App_Code.Kadena.Imports.Products;

namespace Kadena.CMSModules.Kadena.Pages.Products
{
    public partial class ProductsImport : CMSPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HideResultMessage();
        }

        private int SelectedSiteID => Convert.ToInt32(siteSelector.Value);

        protected void btnUploadProductList_Click(object sender, EventArgs e)
        {
            var file = importFile.PostedFile;
            var fileData = ReadBytes(file);
            if (fileData == null)
            {
                return;
            }
            var excelType = ImportHelper.GetExcelTypeFromFileName(file.FileName);

            try
            {
                var result = new ProductImportService().ProcessProductsImportFile(fileData, excelType, SelectedSiteID);
                if (result.ErrorMessages.Length > 0)
                {
                    ShowErrorMessage(FormatImportResult(result));
                }
                else
                {
                    ShowSuccessMessage("Operation successfully completed");
                }
                
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Import products", "EXCEPTION", ex);
                ShowErrorMessage("There was an error while processing the request. Detailed information was placed in Event log.");
            }
        }

        private string FormatImportResult(ImportResult result)
        {
            var headline = $"There was {result.AllMessagesCount} error(s) while processing the request. Make sure all mandatory fields are filled in sheet and/or see Event log for details.<br /><br />First {result.ErrorMessages?.Length ?? 0} errors:<br /><br />";
            return headline + string.Join("<br/>", result.ErrorMessages);
        }

        protected void btnDownloadTemplate_Click(object sender, EventArgs e)
        {
            var bytes = new ProductTemplateService().GetTemplateFile<ProductDto>(SelectedSiteID);
            var templateFileName = "products-upload-template.xlsx";
            WriteFileToResponse(templateFileName, bytes);
        }

        private byte[] ReadBytes(HttpPostedFile fileRequest)
        {
            if (string.IsNullOrWhiteSpace(fileRequest.FileName))
            {
                ShowErrorMessage("You need to choose the import file.");
                return null;
            }

            if (SelectedSiteID == 0)
            {
                ShowErrorMessage("You need to choose the Site.");
                return null;
            }

            using (var binaryReader = new BinaryReader(fileRequest.InputStream))
            {
                return binaryReader.ReadBytes(fileRequest.ContentLength);
            }
        }

        private void WriteFileToResponse(string filename, byte[] data)
        {
            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + filename);

            Response.OutputStream.Write(data, 0, data.Length);
            Response.Flush();

            Response.Close();
        }

        private void ShowErrorMessage(string message)
        {
            errorMessageContainer.Visible = true;
            errorMessage.Text = message;
        }

        private void ShowSuccessMessage(string message)
        {
            successMessageContainer.Visible = true;
            successMessage.Text = message;
        }

        private void HideResultMessage()
        {
            errorMessageContainer.Visible = false;
            successMessageContainer.Visible = false;
        }

        protected void btnDownloadProductCategoryTemplate_Click(object sender, EventArgs e)
        {
            var bytes = new TemplateServiceBase().GetTemplateFile<ProductCategoryImportDto>(SelectedSiteID);
            var templateFileName = "productcategories-upload-template.xlsx";
            WriteFileToResponse(templateFileName, bytes);
        }
    }
}