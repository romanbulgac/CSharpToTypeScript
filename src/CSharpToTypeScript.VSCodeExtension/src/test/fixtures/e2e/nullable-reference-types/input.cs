using System.Collections.Generic;

namespace Contracts
{
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public string? MiddleName { get; set; }
        public Address? ShippingAddress { get; set; }
        public Address BillingAddress { get; set; }
        public List<string>? Tags { get; set; }
    }
}
