﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="S3Uploader.aspx.cs" Inherits="Kadena.CMSFormControls.S3Uploader"
    MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" Title="S3 Uploader"
    Theme="Default" %>

<asp:Content ID="cntBody" ContentPlaceHolderID="plcBeforeContent" runat="Server">
    <asp:HyperLink runat="server" ID="lnkFile" ClientIDMode="Static" Target="_blank" />
    <cms:FormLabel runat="server" ID="lblMessage" ClientIDMode="Static" ForeColor="Red"/>
    <div style="margin-top: 5px">
        <input id="inpFile" clientidmode="Static" type="file" runat="server" style="display: none" />
        <button type="button" id="btnUpload" class="btn btn-default">
            <cms:LocalizedLiteral runat="server" EnableViewState="false" ResourceString="attach.uploadfile"/>
        </button>
    </div>
</asp:Content>