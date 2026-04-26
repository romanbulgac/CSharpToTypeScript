export interface Address {
    street: string;
    city: string;
}

export interface Customer {
    name: string;
    middleName: string | null;
    shippingAddress: Address | null;
    billingAddress: Address;
    tags: string[] | null;
}
