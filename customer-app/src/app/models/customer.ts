export interface Customer {
  customerId: number;
  name: string;
  emailId: string;
  phoneNumber?: string;
  mobilePhone?: string;
  homePhone?: string;
  address?: string;
  contactNumber?: string; // comes from API, read-only
}