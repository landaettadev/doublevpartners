import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs';
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
    return this.http.get<any[]>(`${this.baseUrl}/products`).pipe(
      map(items => (items || []).map(p => ({
        id: p.id,
        name: p.name,
        description: p.description,
        price: p.price,
        imageUrl: p.image?.url || p.image?.fileName || p.imageUrl || '',
        isActive: p.isActive,
        createdAt: p.createdAt,
        updatedAt: p.updatedAt
      }) as Product))
    );
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/products/${id}`);
  }
}
