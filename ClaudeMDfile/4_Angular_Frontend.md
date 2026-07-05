# Customer Master — Angular Frontend

This Angular app works against any of the three backends (ADO.NET, Dapper, or Entity Framework) — just change one line in the service to point at whichever API route you want to test.

## Step 1: Create the project

```bash
npm install -g @angular/cli
ng new customer-app --routing --style=css
cd customer-app
```

## Step 2: Model — `src/app/models/customer.ts`

```typescript
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
```

## Step 3: Service — `src/app/services/customer.service.ts`

```bash
ng generate service services/customer
```

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Customer } from '../models/customer';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  // Change this to switch backend: /api/adonet/customers, /api/dapper/customers, /api/ef/customers
  private baseUrl = 'https://localhost:7050/api/adonet/customers';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Customer[]> {
    return this.http.get<Customer[]>(this.baseUrl);
  }

  getById(id: number): Observable<Customer> {
    return this.http.get<Customer>(`${this.baseUrl}/${id}`);
  }

  add(customer: Customer): Observable<any> {
    return this.http.post(this.baseUrl, customer);
  }

  update(id: number, customer: Customer): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, customer);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}
```

## Step 4: Enable HttpClient — `src/app/app.config.ts` (Angular 17+ standalone)

```typescript
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [provideRouter(routes), provideHttpClient()]
};
```

*(On an older Angular version with NgModules, add `HttpClientModule` and `ReactiveFormsModule` to `AppModule`'s imports array instead.)*

## Step 5: Customer List component (the grid)

```bash
ng generate component components/customer-list
```

`customer-list.component.ts`:
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CustomerService } from '../../services/customer.service';
import { Customer } from '../../models/customer';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-list.component.html'
})
export class CustomerListComponent implements OnInit {
  customers: Customer[] = [];

  constructor(private customerService: CustomerService) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers() {
    this.customerService.getAll().subscribe(data => this.customers = data);
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
```

`customer-list.component.html`:
```html
<h2>Customer List</h2>
<a routerLink="/add">+ Add Customer</a>

<table border="1" cellpadding="8">
  <thead>
    <tr>
      <th>Customer Name</th>
      <th>Email Id</th>
      <th>Contact Number</th>
      <th>Actions</th>
    </tr>
  </thead>
  <tbody>
    <tr *ngFor="let c of customers">
      <td>{{ c.name }}</td>
      <td>{{ c.emailId }}</td>
      <td>{{ getContactNumber(c) }}</td>
      <td>
        <a [routerLink]="['/edit', c.customerId]">Edit</a> |
        <button (click)="deleteCustomer(c.customerId)">Delete</button>
      </td>
    </tr>
  </tbody>
</table>
```

## Step 6: Customer Form component (Add/Edit + validation)

```bash
ng generate component components/customer-form
```

`customer-form.component.ts`:
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomerService } from '../../services/customer.service';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './customer-form.component.html'
})
export class CustomerFormComponent implements OnInit {
  form: FormGroup;
  customerId: number | null = null;

  // Simple custom email validator (mirrors the backend rule)
  static emailValidator(control: any) {
    const regex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
    if (!control.value) return null; // let 'required' handle empty
    return regex.test(control.value) ? null : { invalidEmail: true };
  }

  constructor(
    private fb: FormBuilder,
    private customerService: CustomerService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      emailId: ['', [Validators.required, CustomerFormComponent.emailValidator]],
      phoneNumber: [''],
      mobilePhone: [''],
      homePhone: [''],
      address: ['']
    });
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.customerId = +idParam;
      this.customerService.getById(this.customerId).subscribe(c => this.form.patchValue(c));
    }
  }

  save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const customer = { ...this.form.value, customerId: this.customerId ?? 0 };

    const request = this.customerId
      ? this.customerService.update(this.customerId, customer)
      : this.customerService.add(customer);

    request.subscribe(() => this.router.navigate(['/']));
  }
}
```

`customer-form.component.html`:
```html
<h2>{{ customerId ? 'Edit' : 'Add' }} Customer</h2>

<form [formGroup]="form" (ngSubmit)="save()">
  <div>
    <label>Name *</label>
    <input formControlName="name" />
    <span style="color:red" *ngIf="form.get('name')?.touched && form.get('name')?.invalid">
      Name is required
    </span>
  </div>

  <div>
    <label>Email *</label>
    <input formControlName="emailId" />
    <span style="color:red" *ngIf="form.get('emailId')?.touched && form.get('emailId')?.errors?.['required']">
      Email is required
    </span>
    <span style="color:red" *ngIf="form.get('emailId')?.touched && form.get('emailId')?.errors?.['invalidEmail']">
      Email format is invalid
    </span>
  </div>

  <div>
    <label>Phone Number</label>
    <input formControlName="phoneNumber" />
  </div>

  <div>
    <label>Mobile Phone</label>
    <input formControlName="mobilePhone" />
  </div>

  <div>
    <label>Home Phone</label>
    <input formControlName="homePhone" />
  </div>

  <div>
    <label>Address</label>
    <input formControlName="address" />
  </div>

  <button type="submit">Save</button>
</form>
```

## Step 7: Routing — `src/app/app.routes.ts`

```typescript
import { Routes } from '@angular/router';
import { CustomerListComponent } from './components/customer-list/customer-list.component';
import { CustomerFormComponent } from './components/customer-form/customer-form.component';

export const routes: Routes = [
  { path: '', component: CustomerListComponent },
  { path: 'add', component: CustomerFormComponent },
  { path: 'edit/:id', component: CustomerFormComponent }
];
```

## Step 8: Root component — `src/app/app.component.html`

```html
<router-outlet></router-outlet>
```

## Step 9: Run

```bash
ng serve
```

Open `http://localhost:4200`. Make sure the matching backend (ADO.NET, Dapper, or EF) is also running via `dotnet run`, and that the `baseUrl` in `customer.service.ts` points at the right route.
