﻿using Kadena.Models.AddToCart;
using Kadena.Models.CustomerData;
using Kadena.Models.Product;
using System.Collections.Generic;

namespace Kadena.BusinessLogic.Contracts
{
    public interface IDistributorShoppingCartService
    {
        DistributorCart GetCartDistributorData(int skuID, CampaignProductType cartType = CampaignProductType.GeneralInventory);
        int UpdateDistributorCarts(DistributorCart cartDistributorData, int userId = 0);
        string UpdateCartQuantity(Distributor submitRequest);
        IEnumerable<DistributorCart> CreateCart(Dictionary<int, int> items, int userId, int addressId);
    }
}
