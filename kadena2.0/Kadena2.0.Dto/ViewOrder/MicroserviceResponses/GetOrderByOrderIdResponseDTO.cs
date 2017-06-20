﻿using System.Collections.Generic;

namespace Kadena.Dto.ViewOrder.MicroserviceResponses
{
    public class GetOrderByOrderIdResponseDTO
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string OrderDate { get; set; }
        public ShippingInfoDTO ShippingInfo { get; set; }
        public PaymentInfoDTO PaymentInfo { get; set; }
        public List<OrderItemDTO> Items { get; set; }
    }
}
