import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors, FormsModule } from '@angular/forms';
import { InvoicesApiService } from '../../../core/http/invoices.api';
import { Invoice } from '../../../shared/models/invoice';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, firstValueFrom } from 'rxjs';
import { ExportService, ExportData } from '../../../core/services/export.service';
import { ExportButtonComponent } from '../../../shared/components/export-button';

@Component({
  selector: 'app-invoice-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, ExportButtonComponent],
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
                  invoices: (invoicesFull.length ? invoicesFull : pagedInvoices),
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

          <!-- Controles de paginación -->
          <div class="d-flex justify-content-between align-items-center mb-2 flex-wrap gap-2">
            <div class="text-muted">
              Mostrando {{ pageStartIndex + 1 }}–{{ pageEndIndex }} de {{ totalResults }}
              • Página {{ currentPage }} de {{ totalPages }}
            </div>
            <div class="d-flex align-items-center gap-2">
              <label class="form-label mb-0 me-2">Tamaño de página</label>
              <select class="form-select form-select-sm w-auto" [(ngModel)]="pageSize" (ngModelChange)="onPageSizeChange($event)">
                <option [ngValue]="5">5</option>
                <option [ngValue]="10">10</option>
                <option [ngValue]="20">20</option>
                <option [ngValue]="50">50</option>
              </select>
              <div class="btn-group">
                <button class="btn btn-sm btn-outline-secondary" (click)="prevPage()" [disabled]="currentPage === 1">«</button>
                <button class="btn btn-sm btn-outline-secondary" (click)="nextPage()" [disabled]="currentPage === totalPages">»</button>
              </div>
            </div>
          </div>
          
          <div class="table-responsive table-container">
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
                <tr *ngFor="let invoice of pagedInvoices; trackBy: trackByInvoiceId">
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
            <div class="overlay" *ngIf="isPreloadingDetails">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando detalles...</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Modal Detalle de Factura -->
        <div *ngIf="showDetailModal" class="modal fade show d-block" tabindex="-1" style="background-color: rgba(0,0,0,0.5);">
          <div class="modal-dialog modal-lg">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">Detalle de Factura {{ selectedInvoice?.invoiceNumber }}</h5>
                <button type="button" class="btn-close" (click)="closeDetailModal()" aria-label="Cerrar"></button>
              </div>
              <div class="modal-body" *ngIf="selectedInvoice">
                <div class="row mb-3">
                  <div class="col-sm-6">
                    <div><strong>Cliente:</strong> {{ selectedInvoice.clientName }}</div>
                    <div><strong>Fecha:</strong> {{ selectedInvoice.invoiceDate | date:'dd/MM/yyyy' }}</div>
                  </div>
                  <div class="col-sm-6 text-sm-end mt-2 mt-sm-0">
                    <div><strong>Subtotal:</strong> {{ selectedInvoice.subtotal | currency:'COP':'symbol':'1.0-0' }}</div>
                    <div><strong>IVA (19%):</strong> {{ selectedInvoice.taxAmount | currency:'COP':'symbol':'1.0-0' }}</div>
                    <div class="h5 mt-2"><strong>Total:</strong> {{ selectedInvoice.total | currency:'COP':'symbol':'1.0-0' }}</div>
                  </div>
                </div>

                <div class="table-responsive">
                  <table class="table table-sm table-striped">
                    <thead class="table-light">
                      <tr>
                        <th>Producto</th>
                        <th class="text-end">Cantidad</th>
                        <th class="text-end">Precio Unit.</th>
                        <th class="text-end">Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr *ngFor="let d of selectedInvoice.details">
                        <td>{{ d.productName }}</td>
                        <td class="text-end">{{ d.quantity }}</td>
                        <td class="text-end">{{ d.unitPrice | currency:'COP':'symbol':'1.0-0' }}</td>
                        <td class="text-end">{{ d.total | currency:'COP':'symbol':'1.0-0' }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-outline-secondary" (click)="closeDetailModal()">Cerrar</button>
                <button type="button" class="btn btn-primary" (click)="exportInvoiceToPdf(selectedInvoice!)">
                  <i class="fas fa-file-pdf me-1"></i> Exportar PDF
                </button>
              </div>
            </div>
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
    .table-container { position: relative; }
    .overlay { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; background: rgba(255,255,255,.6); z-index: 5; }
    .modal.show { z-index: 1050; }
  `]
})
export class InvoiceSearchComponent implements OnInit, OnDestroy {
  searchForm: FormGroup;
  invoices: Invoice[] = [];
  invoicesFull: Invoice[] = [];
  isSearching = false;
  isValidatingSearch = false;
  searched = false;
  errorMessage = '';
  searchValueValid = false;
  searchSuggestions: string[] = [];
  searchStats: any = null;
  // Paginación
  currentPage = 1;
  pageSize = 10;
  totalResults = 0;
  isPreloadingDetails = false;
  // Modal detalle
  showDetailModal = false;
  selectedInvoice: Invoice | null = null;
  
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
    this.currentPage = 1;

    const startTime = performance.now();

    try {
      const searchRequest = this.searchForm.value;
      this.invoices = await firstValueFrom(this.invoicesApi.searchInvoices(searchRequest));
      this.totalResults = this.invoices.length;
      await this.preloadPageDetails();
      
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
      this.totalResults = 0;
    } finally {
      this.isSearching = false;
    }
  }

  async loadInvoices(): Promise<void> {
    try {
      this.errorMessage = '';
      const startTime = performance.now();
      
      const result = await firstValueFrom(this.invoicesApi.getInvoices(1, this.pageSize));
      this.invoices = result || [];
      this.totalResults = this.invoices.length;
      await this.preloadPageDetails();
      
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
      this.totalResults = 0;
    }
  }

  private async populateDetails(list: Invoice[]): Promise<Invoice[]> {
    const results = await Promise.all(
      (list || []).map(async inv => {
        try {
          const full = await firstValueFrom(this.invoicesApi.getInvoice(inv.id));
          return full || inv;
        } catch {
          return inv;
        }
      })
    );
    return results;
  }

  trackByInvoiceId(index: number, invoice: Invoice): number {
    return invoice.id;
  }

  viewInvoice(id: number): void {
    this.fetchInvoiceDetails(id);
  }

  closeDetailModal(): void {
    this.showDetailModal = false;
    this.selectedInvoice = null;
  }

  private async fetchInvoiceDetails(id: number): Promise<void> {
    try {
      const full = await firstValueFrom(this.invoicesApi.getInvoice(id));
      this.selectedInvoice = full || this.invoices.find(i => i.id === id) || null;
      this.showDetailModal = !!this.selectedInvoice;
    } catch (err) {
      console.error('Error cargando detalles de factura', err);
      // Fallback: mostrar sin detalles si falla
      this.selectedInvoice = this.invoices.find(i => i.id === id) || null;
      this.showDetailModal = !!this.selectedInvoice;
    }
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
      let inv = invoice;
      if (!inv.details || inv.details.length === 0) {
        try {
          const full = await firstValueFrom(this.invoicesApi.getInvoice(invoice.id));
          inv = full || invoice;
        } catch {}
      }
      await this.exportService.exportInvoiceToPdf(inv);
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
        invoices: this.invoicesFull.length ? this.invoicesFull : this.pagedInvoices,
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
    this.invoicesFull = [];
    this.searched = false;
    this.searchStats = null;
    this.searchSuggestions = [];
    this.errorMessage = '';
    this.totalResults = 0;
    this.currentPage = 1;
  }

  get totalPages(): number { return Math.max(1, Math.ceil(this.totalResults / this.pageSize)); }
  get pageStartIndex(): number { return (this.currentPage - 1) * this.pageSize; }
  get pageEndIndex(): number { return Math.min(this.pageStartIndex + this.pageSize, this.totalResults); }
  get pagedInvoices(): Invoice[] { return this.invoices.slice(this.pageStartIndex, this.pageEndIndex); }

  async onPageSizeChange(value: any): Promise<void> {
    // Soporta binding desde (ngModelChange) que entrega el valor o el evento
    const newSize = typeof value === 'number' ? value : Number((value?.target?.value) ?? this.pageSize);
    this.pageSize = isNaN(newSize) ? this.pageSize : newSize;
    this.currentPage = 1;
    await this.preloadPageDetails();
  }

  async nextPage(): Promise<void> {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      await this.preloadPageDetails();
    }
  }

  async prevPage(): Promise<void> {
    if (this.currentPage > 1) {
      this.currentPage--;
      await this.preloadPageDetails();
    }
  }

  private async preloadPageDetails(): Promise<void> {
    this.isPreloadingDetails = true;
    try {
      this.invoicesFull = await this.populateDetails(this.pagedInvoices);
    } finally {
      this.isPreloadingDetails = false;
    }
  }
}
