import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { InvoicesApiService } from '../../../core/http/invoices.api';
import { Invoice } from '../../../shared/models/invoice';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, firstValueFrom } from 'rxjs';
import { ExportService, ExportData } from '../../../core/services/export.service';
import { ExportButtonComponent } from '../../../shared/components/export-button';

@Component({
  selector: 'app-invoice-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ExportButtonComponent],
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
                <label for="searchType" class="form-label">
                  Tipo de Búsqueda 
                  <span class="text-danger">*</span>
                </label>
                <select 
                  class="form-select" 
                  id="searchType" 
                  formControlName="searchType"
                  [class.is-invalid]="isFieldInvalid('searchType')"
                  [class.is-valid]="isFieldValid('searchType')"
                  (change)="onSearchTypeChange()"
                >
                  <option value="">Seleccionar tipo</option>
                  <option value="Client">Por Cliente</option>
                  <option value="InvoiceNumber">Por Número de Factura</option>
                </select>
                <div class="invalid-feedback" *ngIf="isFieldInvalid('searchType')">
                  Tipo de búsqueda es requerido
                </div>
                <div class="valid-feedback" *ngIf="isFieldValid('searchType')">
                  Tipo seleccionado
                </div>
              </div>
            </div>
            <div class="col-md-6">
              <div class="mb-3">
                <label [for]="getSearchFieldId()" class="form-label">
                  {{ getSearchFieldLabel() }} 
                  <span class="text-danger">*</span>
                </label>
                <div class="input-group">
                  <input 
                    [type]="getSearchFieldType()" 
                    class="form-control" 
                    [id]="getSearchFieldId()" 
                    formControlName="searchValue"
                    [placeholder]="getSearchFieldPlaceholder()"
                    [disabled]="!searchForm.get('searchType')?.value"
                    [class.is-invalid]="isFieldInvalid('searchValue')"
                    [class.is-valid]="isFieldValid('searchValue')"
                    (input)="onSearchValueInput()"
                  >
                  <span class="input-group-text" *ngIf="isValidatingSearch">
                    <div class="spinner-border spinner-border-sm" role="status">
                      <span class="visually-hidden">Validando...</span>
                    </div>
                  </span>
                  <span class="input-group-text text-success" *ngIf="isFieldValid('searchValue') && searchValueValid">
                    ✓
                  </span>
                  <span class="input-group-text text-warning" *ngIf="isFieldValid('searchValue') && !searchValueValid && searchValue.length > 0">
                    ⚠
                  </span>
                </div>
                <div class="invalid-feedback" *ngIf="isFieldInvalid('searchValue')">
                  <div *ngIf="searchForm.get('searchValue')?.errors?.['required']">
                    {{ getSearchFieldLabel() }} es requerido
                  </div>
                  <div *ngIf="searchForm.get('searchValue')?.errors?.['minlength']">
                    Mínimo {{ getMinLength() }} caracteres
                  </div>
                  <div *ngIf="searchForm.get('searchValue')?.errors?.['pattern']">
                    {{ getPatternErrorMessage() }}
                  </div>
                </div>
                <div class="valid-feedback" *ngIf="isFieldValid('searchValue') && searchValueValid">
                  {{ getSearchFieldLabel() }} válido
                </div>
                <div class="form-text text-muted" *ngIf="searchForm.get('searchType')?.value">
                  {{ getSearchFieldHelpText() }}
                </div>
              </div>
            </div>
            <div class="col-md-2">
              <div class="mb-3">
                <label class="form-label">&nbsp;</label>
                <button 
                  type="submit" 
                  class="btn btn-primary w-100" 
                  [disabled]="searchForm.invalid || isSearching || !searchValueValid"
                >
                  <span *ngIf="isSearching" class="spinner-border spinner-border-sm me-2" role="status"></span>
                  {{ isSearching ? 'Buscando...' : 'Buscar' }}
                </button>
              </div>
            </div>
          </div>
        </form>

        <!-- Estadísticas de búsqueda -->
        <div *ngIf="searchStats" class="alert alert-info mt-3">
          <i class="bi bi-info-circle"></i>
          <strong>Estadísticas:</strong> 
          Se encontraron {{ searchStats.totalResults }} facturas en {{ searchStats.searchTime }}ms
          <span *ngIf="searchStats.hasFilters"> con filtros aplicados</span>
        </div>

        <div *ngIf="invoices.length > 0" class="mt-4">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h5>Resultados de la Búsqueda</h5>
            <div class="btn-group" role="group">
              <app-export-button
                [config]="{
                  label: 'Exportar',
                  icon: 'fas fa-download',
                  formats: ['pdf', 'excel', 'csv'],
                  showFormatSelector: true,
                  showOptions: true,
                  defaultFormat: 'excel',
                  buttonClass: 'btn-outline-secondary',
                  size: 'sm'
                }"
                [exportData]="{
                  invoices: invoices,
                  title: 'Resultados de Búsqueda de Facturas',
                  filters: {
                    searchType: searchForm.get('searchType')?.value,
                    searchValue: searchForm.get('searchValue')?.value
                  },
                  exportDate: currentDate
                }"
                (exportStarted)="onExportStarted()"
                (exportCompleted)="onExportCompleted()"
                (exportError)="onExportError($event)">
              </app-export-button>
              
              <button type="button" class="btn btn-outline-secondary btn-sm" 
                      (click)="exportExecutiveSummary()" [disabled]="invoices.length === 0">
                <i class="fas fa-chart-bar"></i> Resumen
              </button>
              
              <button type="button" class="btn btn-outline-secondary btn-sm" 
                      (click)="clearResults()">
                <i class="fas fa-trash"></i> Limpiar
              </button>
            </div>
          </div>
          
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
                <tr *ngFor="let invoice of invoices; trackBy: trackByInvoiceId">
                  <td>
                    <strong>{{ invoice.invoiceNumber }}</strong>
                  </td>
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
                    <div class="btn-group" role="group">
                      <button 
                        class="btn btn-sm btn-info" 
                        (click)="viewInvoice(invoice.id)"
                        title="Ver detalles"
                      >
                        <i class="fas fa-eye"></i>
                      </button>
                      <button 
                        class="btn btn-sm btn-secondary" 
                        (click)="printInvoice(invoice.id)"
                        title="Imprimir"
                      >
                        <i class="fas fa-print"></i>
                      </button>
                      <button 
                        class="btn btn-sm btn-success" 
                        (click)="exportInvoiceToPdf(invoice)"
                        title="Exportar a PDF"
                      >
                        <i class="fas fa-file-pdf"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div *ngIf="searched && invoices.length === 0" class="mt-4">
          <div class="alert alert-info">
            <i class="bi bi-search"></i>
            <strong>No se encontraron resultados</strong><br>
            No se encontraron facturas con los criterios especificados.
            <ul class="mt-2 mb-0">
              <li>Verifique que el tipo de búsqueda sea correcto</li>
              <li>Intente con términos de búsqueda más amplios</li>
              <li>Revise la ortografía de los términos de búsqueda</li>
            </ul>
          </div>
        </div>

        <div *ngIf="errorMessage" class="mt-4">
          <div class="alert alert-danger">
            <i class="bi bi-exclamation-triangle"></i>
            <strong>Error en la búsqueda:</strong> {{ errorMessage }}
          </div>
        </div>

        <!-- Sugerencias de búsqueda -->
        <div *ngIf="searchSuggestions.length > 0" class="mt-4">
          <div class="alert alert-warning">
            <i class="bi bi-lightbulb"></i>
            <strong>Sugerencias de búsqueda:</strong>
            <ul class="mt-2 mb-0">
              <li *ngFor="let suggestion of searchSuggestions">{{ suggestion }}</li>
            </ul>
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
    .btn-group .btn {
      margin-right: 2px;
    }
    .btn-group .btn:last-child {
      margin-right: 0;
    }
    .alert ul {
      padding-left: 1.2rem;
    }
  `]
})
export class InvoiceSearchComponent implements OnInit, OnDestroy {
  searchForm: FormGroup;
  invoices: Invoice[] = [];
  isSearching = false;
  isValidatingSearch = false;
  searched = false;
  errorMessage = '';
  searchValueValid = false;
  searchSuggestions: string[] = [];
  searchStats: any = null;
  
  get currentDate(): Date { return new Date(); }
  
  private destroy$ = new Subject<void>();
  private searchValueSubject = new Subject<string>();

  constructor(
    private fb: FormBuilder,
    private invoicesApi: InvoicesApiService,
    private exportService: ExportService
  ) {
    this.searchForm = this.fb.group({
      searchType: ['', Validators.required],
      searchValue: ['', [
        Validators.required,
        Validators.minLength(2)
      ]]
    });

    // Configurar validación en tiempo real del valor de búsqueda
    this.searchValueSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(searchValue => {
        this.validateSearchValue(searchValue);
      });
  }

  ngOnInit(): void {
    // Cargar facturas por defecto
    this.loadInvoices();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get searchValue(): string {
    return this.searchForm.get('searchValue')?.value || '';
  }

  onSearchTypeChange(): void {
    const searchType = this.searchForm.get('searchType')?.value;
    const searchValueControl = this.searchForm.get('searchValue');
    
    if (searchValueControl) {
      searchValueControl.setValue('');
      searchValueControl.clearValidators();
      
      // Aplicar validadores específicos según el tipo de búsqueda
      if (searchType === 'Client') {
        searchValueControl.setValidators([
          Validators.required,
          Validators.minLength(2),
          Validators.pattern(/^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/)
        ]);
      } else if (searchType === 'InvoiceNumber') {
        searchValueControl.setValidators([
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(/^FAC-\d{3}-\d{4}$/)
        ]);
      }
      
      searchValueControl.updateValueAndValidity();
    }
    
    this.errorMessage = '';
    this.searchValueValid = false;
    this.searchSuggestions = [];
    this.searchStats = null;
  }

  onSearchValueInput(): void {
    const searchValue = this.searchForm.get('searchValue')?.value;
    if (searchValue && this.searchForm.get('searchValue')?.valid) {
      this.searchValueSubject.next(searchValue);
    } else {
      this.searchValueValid = false;
    }
  }

  private async validateSearchValue(searchValue: string): Promise<void> {
    if (!searchValue || this.isFieldInvalid('searchValue')) {
      this.searchValueValid = false;
      return;
    }

    this.isValidatingSearch = true;
    try {
      // Simular validación (en un caso real, aquí se harían validaciones adicionales)
      await new Promise(resolve => setTimeout(resolve, 200));
      
      const searchType = this.searchForm.get('searchType')?.value;
      
      if (searchType === 'Client') {
        this.searchValueValid = searchValue.length >= 2 && /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/.test(searchValue);
      } else if (searchType === 'InvoiceNumber') {
        this.searchValueValid = /^FAC-\d{3}-\d{4}$/.test(searchValue);
      }
      
      // Generar sugerencias si es necesario
      this.generateSearchSuggestions(searchValue, searchType);
      
    } catch (error) {
      console.error('Error al validar valor de búsqueda:', error);
      this.searchValueValid = false;
    } finally {
      this.isValidatingSearch = false;
    }
  }

  private generateSearchSuggestions(searchValue: string, searchType: string): void {
    this.searchSuggestions = [];
    
    if (searchType === 'Client' && searchValue.length < 3) {
      this.searchSuggestions.push('Ingrese al menos 3 caracteres para obtener mejores resultados');
    } else if (searchType === 'InvoiceNumber' && !/^FAC-\d{3}-\d{4}$/.test(searchValue)) {
      this.searchSuggestions.push('Use el formato: FAC-XXX-YYYY (ej: FAC-001-2024)');
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

  getSearchFieldHelpText(): string {
    const searchType = this.searchForm.get('searchType')?.value;
    if (searchType === 'Client') {
      return 'Ingrese el nombre completo o parcial del cliente (mínimo 2 caracteres)';
    } else if (searchType === 'InvoiceNumber') {
      return 'Use el formato FAC-XXX-YYYY (ej: FAC-001-2024)';
    }
    return '';
  }

  getMinLength(): number {
    const searchType = this.searchForm.get('searchType')?.value;
    return searchType === 'Client' ? 2 : 8;
  }

  getPatternErrorMessage(): string {
    const searchType = this.searchForm.get('searchType')?.value;
    if (searchType === 'Client') {
      return 'Solo se permiten letras y espacios';
    } else if (searchType === 'InvoiceNumber') {
      return 'Formato inválido. Use: FAC-XXX-YYYY';
    }
    return 'Formato inválido';
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

  isFieldInvalid(fieldName: string): boolean {
    const field = this.searchForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  isFieldValid(fieldName: string): boolean {
    const field = this.searchForm.get(fieldName);
    return !!(field && field.valid && (field.dirty || field.touched));
  }

  async onSearch(): Promise<void> {
    if (this.searchForm.invalid || !this.searchValueValid) {
      return;
    }

    this.isSearching = true;
    this.searched = true;
    this.errorMessage = '';
    this.searchSuggestions = [];
    this.searchStats = null;

    const startTime = performance.now();

    try {
      const searchRequest = this.searchForm.value;
      this.invoices = await firstValueFrom(this.invoicesApi.searchInvoices(searchRequest));
      
      const endTime = performance.now();
      this.searchStats = {
        totalResults: this.invoices.length,
        searchTime: Math.round(endTime - startTime),
        hasFilters: true
      };
      
    } catch (error: any) {
      console.error('Error al buscar facturas:', error);
      this.errorMessage = error.error?.error || 'Error al realizar la búsqueda';
      this.invoices = [];
    } finally {
      this.isSearching = false;
    }
  }

  async loadInvoices(): Promise<void> {
    try {
      this.errorMessage = '';
      const startTime = performance.now();
      
      const result = await firstValueFrom(this.invoicesApi.getInvoices(1, 10));
      this.invoices = result || [];
      
      const endTime = performance.now();
      this.searchStats = {
        totalResults: this.invoices.length,
        searchTime: Math.round(endTime - startTime),
        hasFilters: false
      };
      
    } catch (error: any) {
      console.error('Error al cargar facturas:', error);
      this.errorMessage = error.error?.error || 'Error al cargar las facturas';
      this.invoices = [];
    }
  }

  trackByInvoiceId(index: number, invoice: Invoice): number {
    return invoice.id;
  }

  viewInvoice(id: number): void {
    // Implementar navegación a vista de detalles
    console.log('Ver factura:', id);
    alert(`Ver detalles de factura ${id}`);
  }

  printInvoice(id: number): void {
    // Implementar impresión de factura
    console.log('Imprimir factura:', id);
    alert(`Imprimiendo factura ${id}`);
  }

  async exportResults(): Promise<void> {
    if (!this.invoices || this.invoices.length === 0) {
      return;
    }

    try {
      const exportData: ExportData = {
        invoices: this.invoices,
        title: 'Resultados de Búsqueda de Facturas',
        filters: {
          searchType: this.searchForm.get('searchType')?.value,
          searchValue: this.searchForm.get('searchValue')?.value
        },
        exportDate: new Date()
      };

      await this.exportService.exportAuto(exportData);
    } catch (error) {
      console.error('Error al exportar:', error);
    }
  }

  async exportInvoiceToPdf(invoice: Invoice): Promise<void> {
    try {
      await this.exportService.exportInvoiceToPdf(invoice);
    } catch (error) {
      console.error('Error al exportar factura:', error);
    }
  }

  async exportExecutiveSummary(): Promise<void> {
    if (!this.invoices || this.invoices.length === 0) {
      return;
    }

    try {
      const exportData: ExportData = {
        invoices: this.invoices,
        title: 'Resumen Ejecutivo de Facturación',
        filters: {
          searchType: this.searchForm.get('searchType')?.value,
          searchValue: this.searchForm.get('searchValue')?.value
        },
        exportDate: new Date()
      };

      await this.exportService.exportExecutiveSummary(exportData);
    } catch (error) {
      console.error('Error al exportar resumen ejecutivo:', error);
    }
  }

  // Métodos de manejo de eventos de exportación
  onExportStarted(): void {
    console.log('Exportación iniciada');
  }

  onExportCompleted(): void {
    console.log('Exportación completada');
  }

  onExportError(error: string): void {
    console.error('Error en exportación:', error);
  }

  clearResults(): void {
    this.invoices = [];
    this.searched = false;
    this.searchStats = null;
    this.searchSuggestions = [];
    this.errorMessage = '';
  }
}
