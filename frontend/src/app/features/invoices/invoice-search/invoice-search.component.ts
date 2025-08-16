import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { InvoicesApiService } from '../../../core/http/invoices.api';
import { Invoice } from '../../../shared/models/invoice';

@Component({
  selector: 'app-invoice-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h3>Búsqueda de Facturas</h3>
      </div>
      <div class="card-body">
        <form [formGroup]="searchForm" (ngSubmit)="onSearch()">
          <div class="row">
            <div class="col-md-4">
              <div class="mb-3">
                <label for="searchType" class="form-label">Tipo de Búsqueda</label>
                <select 
                  class="form-select" 
                  id="searchType" 
                  formControlName="searchType"
                  (change)="onSearchTypeChange()"
                >
                  <option value="">Seleccionar tipo</option>
                  <option value="Client">Por Cliente</option>
                  <option value="InvoiceNumber">Por Número de Factura</option>
                </select>
              </div>
            </div>
            <div class="col-md-6">
              <div class="mb-3">
                <label [for]="getSearchFieldId()" class="form-label">{{ getSearchFieldLabel() }}</label>
                <input 
                  [type]="getSearchFieldType()" 
                  class="form-control" 
                  [id]="getSearchFieldId()" 
                  formControlName="searchValue"
                  [placeholder]="getSearchFieldPlaceholder()"
                  [disabled]="!searchForm.get('searchType')?.value"
                >
              </div>
            </div>
            <div class="col-md-2">
              <div class="mb-3">
                <label class="form-label">&nbsp;</label>
                <button 
                  type="submit" 
                  class="btn btn-primary w-100" 
                  [disabled]="searchForm.invalid || isSearching"
                >
                  {{ isSearching ? 'Buscando...' : 'Buscar' }}
                </button>
              </div>
            </div>
          </div>
        </form>

        <div *ngIf="invoices.length > 0" class="mt-4">
          <h5>Resultados de la Búsqueda</h5>
          <div class="table-responsive">
            <table class="table table-striped table-hover">
              <thead class="table-dark">
                <tr>
                  <th>Número</th>
                  <th>Cliente</th>
                  <th>Fecha</th>
                  <th>Subtotal</th>
                  <th>IVA</th>
                  <th>Total</th>
                  <th>Estado</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let invoice of invoices">
                  <td>{{ invoice.invoiceNumber }}</td>
                  <td>{{ invoice.clientName }}</td>
                  <td>{{ invoice.invoiceDate | date:'dd/MM/yyyy' }}</td>
                  <td>{{ invoice.subtotal | currency:'COP':'symbol':'1.0-0' }}</td>
                  <td>{{ invoice.taxAmount | currency:'COP':'symbol':'1.0-0' }}</td>
                  <td><strong>{{ invoice.total | currency:'COP':'symbol':'1.0-0' }}</strong></td>
                  <td>
                    <span class="badge" [class]="getStatusBadgeClass(invoice.status)">
                      {{ invoice.status }}
                    </span>
                  </td>
                  <td>
                    <button 
                      class="btn btn-sm btn-info" 
                      (click)="viewInvoice(invoice.id)"
                      title="Ver detalles"
                    >
                      👁️
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div *ngIf="searched && invoices.length === 0" class="mt-4">
          <div class="alert alert-info">
            No se encontraron facturas con los criterios especificados.
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
    }
    .table th {
      font-weight: 600;
    }
    .badge {
      font-size: 0.875em;
    }
  `]
})
export class InvoiceSearchComponent implements OnInit {
  searchForm: FormGroup;
  invoices: Invoice[] = [];
  isSearching = false;
  searched = false;

  constructor(
    private fb: FormBuilder,
    private invoicesApi: InvoicesApiService
  ) {
    this.searchForm = this.fb.group({
      searchType: ['', Validators.required],
      searchValue: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    // Cargar facturas por defecto
    this.loadInvoices();
  }

  onSearchTypeChange(): void {
    const searchType = this.searchForm.get('searchType')?.value;
    const searchValueControl = this.searchForm.get('searchValue');
    
    if (searchValueControl) {
      searchValueControl.setValue('');
      searchValueControl.enable();
    }
  }

  getSearchFieldId(): string {
    const searchType = this.searchForm.get('searchType')?.value;
    return searchType === 'Client' ? 'clientSearch' : 'invoiceNumberSearch';
  }

  getSearchFieldLabel(): string {
    const searchType = this.searchForm.get('searchType')?.value;
    return searchType === 'Client' ? 'Nombre del Cliente' : 'Número de Factura';
  }

  getSearchFieldType(): string {
    const searchType = this.searchForm.get('searchType')?.value;
    return searchType === 'Client' ? 'text' : 'text';
  }

  getSearchFieldPlaceholder(): string {
    const searchType = this.searchForm.get('searchType')?.value;
    return searchType === 'Client' ? 'Ingrese nombre del cliente' : 'Ingrese número de factura';
  }

  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'bg-success';
      case 'cancelled':
        return 'bg-danger';
      case 'pending':
        return 'bg-warning';
      default:
        return 'bg-secondary';
    }
  }

  async onSearch(): Promise<void> {
    if (this.searchForm.invalid) {
      return;
    }

    this.isSearching = true;
    this.searched = true;

    try {
      const searchRequest = this.searchForm.value;
      this.invoices = await this.invoicesApi.searchInvoices(searchRequest).toPromise();
    } catch (error) {
      console.error('Error al buscar facturas:', error);
      alert('Error al realizar la búsqueda');
      this.invoices = [];
    } finally {
      this.isSearching = false;
    }
  }

  async loadInvoices(): Promise<void> {
    try {
      const result = await this.invoicesApi.getInvoices(1, 10).toPromise();
      this.invoices = result?.items || [];
    } catch (error) {
      console.error('Error al cargar facturas:', error);
    }
  }

  viewInvoice(id: number): void {
    // Implementar navegación a vista de detalles
    console.log('Ver factura:', id);
    alert(`Ver detalles de factura ${id}`);
  }
}
