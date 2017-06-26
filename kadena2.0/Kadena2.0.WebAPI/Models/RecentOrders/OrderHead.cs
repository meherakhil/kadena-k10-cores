﻿using System.Collections.Generic;

namespace Kadena.WebAPI.Models.RecentOrders
{
    public class OrderHead
    {
        public List<string> Headings { get; set; }
        public Pagination PageInfo { get; set; }
        public string NoOrdersMessage { get; set; }
    }
}