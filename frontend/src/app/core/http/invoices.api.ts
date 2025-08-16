import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Invoice, InvoiceCreateRequest, InvoiceSearchRequest, PagedResult } from '../../shared/models/invoice';

@Injectable({
  providedIn: 'root'
})
export class InvoicesApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/invoices`;

  constructor(private http: HttpClient) {}

  createInvoice(request: InvoiceCreateRequest): Observable<Invoice> {
    return this.http.post<Invoice>(this.baseUrl, request);
  }

  getInvoice(id: number): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.baseUrl}/${id}`);
  }

  getInvoices(page: number = 1, pageSize: number = 20): Observable<PagedResult<Invoice>> {
    return this.http.get<PagedResult<Invoice>>(`${this.baseUrl}?page=${page}&pageSize=${pageSize}`);
  }

  searchInvoices(request: InvoiceSearchRequest): Observable<Invoice[]> {
    return this.http.post<Invoice[]>(`${this.baseUrl}/search`, request);
  }

  checkInvoiceNumberExists(invoiceNumber: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/check-number/${invoiceNumber}`);
  }
}
