﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddToCartButton.ascx.cs" Inherits="Kadena.CMSWebParts.Kadena.Product.AddToCartButton" %>

<div class="input__wrapper">
    <input id="inpNumberOfItems" runat="server" type="number" class="input__text js-add-to-cart-error js-add-to-cart-quantity" value="1" min="1">        
    <span id="lblNumberOfItemsError" class="input__error input__error--noborder js-add-to-cart-message" />
    <input type="hidden" name="1" value="123" class="js-add-to-cart-property" />
    <input type="hidden" name="2" value="4" class="js-add-to-cart-property" />
    <input type="hidden" name="3" value="1" class="js-add-to-cart-property" />
    <input type="hidden" name="4" value="2" class="js-add-to-cart-property" />
</div>
<span class="add-to-cart__right-label">pcs</span>
<button runat="server" id="btnAddToCart" type="button" class="btn-action js-chili-addtocart js-add-to-cart">
    <cms:LocalizedLiteral runat="server" ResourceString="Kadena.Product.AddToCart" EnableViewState="false" />
</button>
