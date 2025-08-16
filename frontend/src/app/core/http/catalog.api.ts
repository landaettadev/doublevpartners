import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Client, Product } from '../../shared/models/catalog';

@Injectable({
  providedIn: 'root'
})
export class CatalogApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/catalog`;

  constructor(private http: HttpClient) {}

  getClients(): Observable<Client[]> {
    return this.http.get<Client[]>(`${this.baseUrl}/clients`);
  }

  getClient(id: number): Observable<Client> {
    return this.http.get<Client>(`${this.baseUrl}/clients/${id}`);
  }

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/products`);
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/products/${id}`);
  }
}
