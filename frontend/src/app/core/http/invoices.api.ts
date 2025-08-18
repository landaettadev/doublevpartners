import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Invoice, InvoiceCreateRequest, InvoiceSearchRequest, PagedResult } from '../../shared/models/invoice';

@Injectable({
  providedIn: 'root'
})
export class InvoicesApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/invoices`;

  constructor(private http: HttpClient) {}

  createInvoice(request: InvoiceCreateRequest): Observable<Invoice> {
    return this.http.post<{ invoice: Invoice }>(this.baseUrl, request)
      .pipe(map(resp => resp.invoice));
  }

  getInvoice(id: number): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.baseUrl}/${id}`);
  }

  getInvoices(page: number = 1, pageSize: number = 20): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${this.baseUrl}?page=${page}&pageSize=${pageSize}`);
  }

  searchInvoices(request: InvoiceSearchRequest): Observable<Invoice[]> {
    return this.http.post<{ invoices: Invoice[] }>(`${this.baseUrl}/search`, request)
      .pipe(map(resp => resp.invoices));
  }

  checkInvoiceNumberExists(invoiceNumber: string): Observable<boolean> {
    return this.http.get<{ exists: boolean }>(`${this.baseUrl}/check-number/${invoiceNumber}`)
      .pipe(map(resp => resp.exists));
  }
}
