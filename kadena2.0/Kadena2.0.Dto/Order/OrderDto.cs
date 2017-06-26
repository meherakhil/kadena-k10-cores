﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kadena.Dto.Order
{
    [DataContract]
    public class OrderDto
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "createDate")]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "deliveryDate")]
        public DateTime DeliveryDate { get; set; }

        [DataMember(Name = "items")]
        public IEnumerable<OrderItemDto> Items { get; set; }
    }
}