﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kadena2._0.Dto
{
    public class DeliveryAddressesContainerDTO : ContainerDTO
    {
        public string AddAddressLabel { get; set; }

        public List<DeliveryAddressDTO> items { get; set; }
    }
}
