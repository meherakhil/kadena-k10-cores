﻿using System.Collections.Generic;

namespace Kadena.Dto.Checkout
{
    public class DeliveryAddressDTO
    {
        public List<string> Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public int Id { get; set; }
        public bool Checked { get; set; }
    }
}