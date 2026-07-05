import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Customer } from '../models/customer';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  // Change this to switch backend: /api/adonet/customers, /api/dapper/customers, /api/ef/customers
  private baseUrl = 'https://localhost:44372/api/ef/customers';

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