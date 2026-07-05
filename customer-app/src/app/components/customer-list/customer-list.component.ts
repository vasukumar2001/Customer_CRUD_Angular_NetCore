import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CustomerService } from '../../services/customer.service';
import { Customer } from '../../models/customer';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-list.component.html',
})
export class CustomerListComponent implements OnInit {
  customers: Customer[] = [];

  constructor(private customerService: CustomerService) {}

  ngOnInit(): void {
    this.loadCustomers();
  }
  loading = true;
  loadCustomers() {
    this.loading = true;
    // this.customerService.getAll().subscribe(data => this.customers = data);
    this.customerService.getAll().subscribe({
      next: data => {
        this.customers = data;
        this.loading = false;
        console.log('this.customers:', this.customers); // see exactly what came back
      },
      error: err => console.error('API error:', err)
    });

  }
  


  // Grid logic: Phone Number if present, else Mobile, else Home
  getContactNumber(c: Customer): string {
    return c.phoneNumber || c.mobilePhone || c.homePhone || '';
  }

  deleteCustomer(id: number) {
    if (confirm('Are you sure you want to delete this customer?')) {
      this.customerService.delete(id).subscribe(() => this.loadCustomers());
    }
  }
}