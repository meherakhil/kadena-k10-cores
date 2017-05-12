﻿using CMS.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.UI.WebControls;

namespace CMSApp.CMSPages.Kadena
{
    public partial class NewMailingList : System.Web.UI.Page
    {
        private readonly char _separator = ';';

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnProcess_Click(object sender, EventArgs e)
        {
            lblUploadStatus.Text = "";
            var map = GetColumnMapping();
            SendToService(flFile.PostedFile.InputStream, map);
            pnlColumns.Visible = false;
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (flFile.HasFile)
            {
                var headers = ParseHeaders(flFile.PostedFile.InputStream);
                BindHeaders(headers);
                pnlColumns.Visible = true;
                lblUploadStatus.Text = "Headers loaded";
            }
        }

        private Dictionary<string, int> ParseHeaders(System.IO.Stream stream)
        {
            Dictionary<string, int> result = null;
            using (var reader = StreamReader.New(stream))
            {
                // Read only first row where the name of columns stated
                string line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] productData = line.Trim().Split(_separator);

                    result = productData.Select((c, i) => new { Key = c, Value = i })
                        .ToDictionary(c => c.Key, c => c.Value);
                }
            }
            return result;
        }

        private void BindHeaders(Dictionary<string, int> headerList)
        {
            if (headerList != null)
            {
                // Bind list of column names to drop down lists
                ddlTitle.DataSource =
                    ddlName.DataSource =
                    ddlSecondName.DataSource =
                    ddlAddress1.DataSource =
                    ddlAddress2.DataSource =
                    ddlCity.DataSource =
                    ddlState.DataSource =
                    ddlZipCode.DataSource =
                    headerList;

                ddlTitle.DataBind();
                ddlName.DataBind();
                ddlSecondName.DataBind();
                ddlAddress1.DataBind();
                ddlAddress2.DataBind();
                ddlCity.DataBind();
                ddlState.DataBind();
                ddlZipCode.DataBind();
            }
        }

        /// <summary>
        /// Returns column mapping selected by user.
        /// </summary>
        /// <returns>Dictionary where key is column name and values are their index in file.</returns>
        private Dictionary<string, string> GetColumnMapping()
        {
            var result = new Dictionary<string, string>();

            result.Add("title", ddlTitle.SelectedValue);
            result.Add("firstName", ddlName.SelectedValue);
            result.Add("secondName", ddlSecondName.SelectedValue);
            result.Add("firstAddress", ddlAddress1.SelectedValue);
            result.Add("secondAddresss", ddlAddress2.SelectedValue);
            result.Add("city", ddlCity.SelectedValue);
            result.Add("state", ddlState.SelectedValue);
            result.Add("zipCode", ddlZipCode.SelectedValue);

            return result;
        }

        private void SendToService(System.IO.Stream fileStream, Dictionary<string, string> columnMapping)
        {
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(fileStream), "file", "MailingListTest.csv");
                    content.Add(new StringContent("orginal-mailing"), "bucketType");
                    content.Add(new StringContent("actum"), "customerName");
                    content.Add(new StringContent(JsonConvert.SerializeObject(new
                    {
                        Title = columnMapping["title"],
                        FirstName = columnMapping["firstName"],
                        LastName = columnMapping["secondName"],
                        Address1 = columnMapping["firstAddress"],
                        Address2 = columnMapping["secondAddresss"],
                        City = columnMapping["city"],
                        State = columnMapping["state"],
                        Zip = columnMapping["zipCode"]
                    })), "Mapping");
                    content.Add(new StringContent(Guid.Empty.ToString()), "containerId");
                    using (var message = client.PostAsync("https://0kzyrcqyyd.execute-api.us-east-1.amazonaws.com/Prod/Api/DeliveryAddress", content))
                    {
                        var input = message.Result.Content;

                        lblUploadStatus.Text = input.ReadAsStringAsync().Result;
                    }
                }
            }
        }
    }
}