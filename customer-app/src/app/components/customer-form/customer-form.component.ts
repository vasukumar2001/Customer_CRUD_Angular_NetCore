import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomerService } from '../../services/customer.service';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule,RouterLink],
  templateUrl: './customer-form.component.html',
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