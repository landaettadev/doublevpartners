import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { InvoicesApiService } from '../../../core/http/invoices.api';
import { CatalogApiService } from '../../../core/http/catalog.api';
import { Client, Product } from '../../../shared/models/catalog';
import { InvoiceDetailRequest, Invoice } from '../../../shared/models/invoice';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, firstValueFrom } from 'rxjs';
import { ExportService, ExportData } from '../../../core/services/export.service';
import { ExportButtonComponent } from '../../../shared/components/export-button';

@Component({
  selector: 'app-invoice-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ExportButtonComponent],
  template: `
    <div class="card">
      <div class="card-header">
        <h3>Crear Nueva Factura</h3>
      </div>
      <div class="card-body">
        <form [formGroup]="invoiceForm" (ngSubmit)="onSubmit()">
          <div class="row">
            <div class="col-md-6">
              <div class="mb-3">
                <label for="invoiceNumber" class="form-label">
                  Número de Factura 
                  <span class="text-danger">*</span>
                </label>
                <div class="input-group">
                  <input 
                    type="text" 
                    class="form-control" 
                    id="invoiceNumber" 
                    formControlName="invoiceNumber"
                    [class.is-invalid]="isFieldInvalid('invoiceNumber')"
                    [class.is-valid]="isFieldValid('invoiceNumber')"
                    (input)="onInvoiceNumberInput()"
                    placeholder="Ej: FAC-001-2024"
                  >
                  <span class="input-group-text" *ngIf="isCheckingInvoiceNumber">
                    <div class="spinner-border spinner-border-sm" role="status">
                      <span class="visually-hidden">Verificando...</span>
                    </div>
                  </span>
                  <span class="input-group-text text-success" *ngIf="isFieldValid('invoiceNumber') && !invoiceNumberExists">
                    ✓
                  </span>
                  <span class="input-group-text text-danger" *ngIf="invoiceNumberExists">
                    ✗
                  </span>
                </div>
                <div class="invalid-feedback" *ngIf="isFieldInvalid('invoiceNumber')">
                  <div *ngIf="invoiceForm.get('invoiceNumber')?.errors?.['required']">
                    Número de factura es requerido
                  </div>
                  <div *ngIf="invoiceForm.get('invoiceNumber')?.errors?.['pattern']">
                    Formato inválido. Use: FAC-XXX-YYYY
                  </div>
                  <div *ngIf="invoiceForm.get('invoiceNumber')?.errors?.['minlength']">
                    Mínimo 8 caracteres
                  </div>
                </div>
                <div class="invalid-feedback d-block" *ngIf="invoiceNumberExists">
                  Este número de factura ya existe
                </div>
                <div class="valid-feedback" *ngIf="isFieldValid('invoiceNumber') && !invoiceNumberExists">
                  Número de factura disponible
                </div>
                <small class="form-text text-muted">
                  Formato: FAC-XXX-YYYY (ej: FAC-001-2024)
                </small>
              </div>
            </div>
            <div class="col-md-6">
              <div class="mb-3">
                <label for="clientId" class="form-label">
                  Cliente 
                  <span class="text-danger">*</span>
                </label>
                <select 
                  class="form-select" 
                  id="clientId" 
                  formControlName="clientId"
                  [class.is-invalid]="isFieldInvalid('clientId')"
                  [class.is-valid]="isFieldValid('clientId')"
                >
                  <option value="">Seleccionar cliente</option>
                  <option *ngFor="let client of clients" [value]="client.id">
                    {{ client.name }}
                  </option>
                </select>
                <div class="invalid-feedback" *ngIf="isFieldInvalid('clientId')">
                  Cliente es requerido
                </div>
                <div class="valid-feedback" *ngIf="isFieldValid('clientId')">
                  Cliente seleccionado
                </div>
              </div>
            </div>
          </div>

          <div class="mb-3">
            <label for="invoiceDate" class="form-label">
              Fecha de Factura 
              <span class="text-danger">*</span>
            </label>
            <input 
              type="date" 
              class="form-control" 
              id="invoiceDate" 
              formControlName="invoiceDate"
              [class.is-invalid]="isFieldInvalid('invoiceDate')"
              [class.is-valid]="isFieldValid('invoiceDate')"
              [max]="maxDate"
            >
            <div class="invalid-feedback" *ngIf="isFieldInvalid('invoiceDate')">
              <div *ngIf="invoiceForm.get('invoiceDate')?.errors?.['required']">
                Fecha es requerida
              </div>
              <div *ngIf="invoiceForm.get('invoiceDate')?.errors?.['futureDate']">
                No se pueden crear facturas con fecha futura
              </div>
              <div *ngIf="invoiceForm.get('invoiceDate')?.errors?.['oldDate']">
                La fecha no puede ser anterior a 2020
              </div>
            </div>
            <div class="valid-feedback" *ngIf="isFieldValid('invoiceDate')">
              Fecha válida
            </div>
          </div>

          <div class="mb-3">
            <h5>Detalles de la Factura</h5>
            <div class="alert alert-info" *ngIf="detailsArray.length === 0">
              <i class="bi bi-info-circle"></i>
              Debe agregar al menos un producto a la factura
            </div>
            
            <button type="button" class="btn btn-secondary mb-2" (click)="addProduct()">
              <i class="bi bi-plus-circle"></i> Agregar Producto
            </button>
            
            <div formArrayName="details">
              <div *ngFor="let detail of detailsArray.controls; let i = index" [formGroupName]="i" class="row mb-3 p-3 border rounded">
                <div class="col-md-3">
                  <label class="form-label">Producto <span class="text-danger">*</span></label>
                  <select 
                    class="form-select" 
                    formControlName="productId"
                    [class.is-invalid]="isDetailFieldInvalid(i, 'productId')"
                    [class.is-valid]="isDetailFieldValid(i, 'productId')"
                    (change)="onProductChange(i)"
                  >
                    <option value="">Seleccionar producto</option>
                    <option *ngFor="let product of products" [value]="product.id">
                      {{ product.name }}
                    </option>
                  </select>
                  <div class="invalid-feedback" *ngIf="isDetailFieldInvalid(i, 'productId')">
                    Producto es requerido
                  </div>
                </div>
                <div class="col-md-2">
                  <label class="form-label">Cantidad <span class="text-danger">*</span></label>
                  <input 
                    type="number" 
                    class="form-control" 
                    placeholder="Cantidad" 
                    formControlName="quantity"
                    [class.is-invalid]="isDetailFieldInvalid(i, 'quantity')"
                    [class.is-valid]="isDetailFieldValid(i, 'quantity')"
                    (input)="calculateTotal(i)"
                    min="1"
                    max="9999"
                  >
                  <div class="invalid-feedback" *ngIf="isDetailFieldInvalid(i, 'quantity')">
                    <div *ngIf="detail.get('quantity')?.errors?.['required']">
                      Cantidad es requerida
                    </div>
                    <div *ngIf="detail.get('quantity')?.errors?.['min']">
                      Mínimo 1
                    </div>
                    <div *ngIf="detail.get('quantity')?.errors?.['max']">
                      Máximo 9999
                    </div>
                  </div>
                </div>
                <div class="col-md-2">
                  <label class="form-label">Precio Unit. <span class="text-danger">*</span></label>
                  <input 
                    type="number" 
                    class="form-control" 
                    placeholder="Precio Unit." 
                    formControlName="unitPrice"
                    [class.is-invalid]="isDetailFieldInvalid(i, 'unitPrice')"
                    [class.is-valid]="isDetailFieldValid(i, 'unitPrice')"
                    readonly
                    step="0.01"
                  >
                  <div class="invalid-feedback" *ngIf="isDetailFieldInvalid(i, 'unitPrice')">
                    Precio unitario es requerido
                  </div>
                </div>
                <div class="col-md-2">
                  <label class="form-label">Total</label>
                  <input 
                    type="number" 
                    class="form-control" 
                    placeholder="Total" 
                    [value]="getDetailTotal(i)"
                    readonly
                    class="form-control-plaintext"
                  >
                </div>
                <div class="col-md-2">
                  <label class="form-label">Imagen</label>
                  <img 
                    *ngIf="getProductImage(i)" 
                    [src]="getProductImage(i)" 
                    alt="Producto" 
                    class="img-thumbnail" 
                    style="width: 50px; height: 50px;"
                  >
                  <div class="text-muted small" *ngIf="!getProductImage(i)">
                    Sin imagen
                  </div>
                </div>
                <div class="col-md-1">
                  <label class="form-label">&nbsp;</label>
                  <button type="button" class="btn btn-danger btn-sm" (click)="removeProduct(i)" 
                          [disabled]="detailsArray.length <= 1">
                    <i class="bi bi-trash"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div class="row">
            <div class="col-md-6 offset-md-6">
              <div class="card bg-light">
                <div class="card-body">
                  <h6 class="card-title">Resumen de la Factura</h6>
                  <table class="table table-borderless mb-0">
                    <tr>
                      <td><strong>Subtotal:</strong></td>
                      <td class="text-end">{{ subtotal | currency:'COP':'symbol':'1.0-0' }}</td>
                    </tr>
                    <tr>
                      <td><strong>IVA (19%):</strong></td>
                      <td class="text-end">{{ taxAmount | currency:'COP':'symbol':'1.0-0' }}</td>
                    </tr>
                    <tr class="table-active">
                      <td><strong>Total:</strong></td>
                      <td class="text-end"><strong>{{ total | currency:'COP':'symbol':'1.0-0' }}</strong></td>
                    </tr>
                  </table>
                </div>
              </div>
            </div>
          </div>

          <div class="d-flex justify-content-between mt-4">
            <div class="d-flex gap-2">
              <button type="button" class="btn btn-secondary" (click)="clearForm()">
                <i class="bi bi-plus-circle"></i> Nuevo
              </button>
              
              <!-- Botón de exportación de vista previa -->
              <app-export-button
                *ngIf="canExportPreview()"
                [config]="{
                  label: 'Exportar Vista Previa',
                  icon: 'fas fa-download',
                  formats: ['pdf', 'excel'],
                  showFormatSelector: true,
                  showOptions: false,
                  defaultFormat: 'pdf',
                  buttonClass: 'btn-outline-info',
                  size: 'sm'
                }"
                [exportData]="getPreviewExportData()"
                (exportStarted)="onExportStarted()"
                (exportCompleted)="onExportCompleted()"
                (exportError)="onExportError($event)">
              </app-export-button>
            </div>
            
            <button type="submit" class="btn btn-primary" 
                    [disabled]="invoiceForm.invalid || isSubmitting || invoiceNumberExists || !canSubmit()">
              <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm me-2" role="status"></span>
              {{ isSubmitting ? 'Guardando...' : 'Guardar Factura' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .card {
      box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
    }
    .form-control:focus, .form-select:focus {
      border-color: #86b7fe;
      box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
    }
    .border.rounded {
      background-color: #f8f9fa;
    }
    .form-control-plaintext {
      background-color: transparent;
      border: none;
      font-weight: bold;
      color: #495057;
    }
    .table-active {
      background-color: #e9ecef !important;
    }
  `]
})
export class InvoiceCreateComponent implements OnInit, OnDestroy {
  invoiceForm: FormGroup;
  clients: Client[] = [];
  products: Product[] = [];
  isSubmitting = false;
  invoiceNumberExists = false;
  isCheckingInvoiceNumber = false;
  private destroy$ = new Subject<void>();
  private invoiceNumberSubject = new Subject<string>();

  constructor(
    private fb: FormBuilder,
    private invoicesApi: InvoicesApiService,
    private catalogApi: CatalogApiService,
    private exportService: ExportService
  ) {
    this.invoiceForm = this.fb.group({
      invoiceNumber: ['', [
        Validators.required,
        Validators.pattern(/^FAC-\d{3}-\d{4}$/),
        Validators.minLength(8)
      ]],
      clientId: ['', Validators.required],
      invoiceDate: [new Date().toISOString().split('T')[0], [
        Validators.required,
        this.futureDateValidator(),
        this.oldDateValidator()
      ]],
      details: this.fb.array([])
    });

    // Configurar validación en tiempo real del número de factura
    this.invoiceNumberSubject
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(invoiceNumber => {
        this.checkInvoiceNumber(invoiceNumber);
      });
  }

  ngOnInit(): void {
    this.loadClients();
    this.loadProducts();
    this.addProduct(); // Agregar primer producto por defecto
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get maxDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  get detailsArray(): FormArray {
    return this.invoiceForm.get('details') as FormArray;
  }

  get subtotal(): number {
    return this.detailsArray.controls.reduce((sum, control) => {
      const quantity = control.get('quantity')?.value || 0;
      const unitPrice = control.get('unitPrice')?.value || 0;
      return sum + (quantity * unitPrice);
    }, 0);
  }

  get taxAmount(): number {
    return this.subtotal * 0.19;
  }

  get total(): number {
    return this.subtotal + this.taxAmount;
  }

  canSubmit(): boolean {
    return this.detailsArray.length > 0 && 
           this.detailsArray.controls.every(control => 
             control.get('productId')?.value && 
             control.get('quantity')?.value > 0 && 
             control.get('unitPrice')?.value > 0
           );
  }

  addProduct(): void {
    const detail = this.fb.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1), Validators.max(9999)]],
      unitPrice: [0, [Validators.required, Validators.min(0.01)]]
    });

    this.detailsArray.push(detail);
  }

  removeProduct(index: number): void {
    if (this.detailsArray.length > 1) {
      this.detailsArray.removeAt(index);
    }
  }

  onProductChange(index: number): void {
    const detail = this.detailsArray.at(index);
    const productId = detail.get('productId')?.value;
    
    if (productId) {
      const product = this.products.find(p => p.id === productId);
      if (product) {
        detail.patchValue({
          unitPrice: product.price
        });
        this.calculateTotal(index);
      }
    }
  }

  calculateTotal(index: number): void {
    const detail = this.detailsArray.at(index);
    const quantity = detail.get('quantity')?.value || 0;
    const unitPrice = detail.get('unitPrice')?.value || 0;
    // El total se calcula automáticamente en el getter
  }

  getDetailTotal(index: number): number {
    const detail = this.detailsArray.at(index);
    const quantity = detail.get('quantity')?.value || 0;
    const unitPrice = detail.get('unitPrice')?.value || 0;
    return quantity * unitPrice;
  }

  getProductImage(index: number): string | null {
    const detail = this.detailsArray.at(index);
    const productId = detail.get('productId')?.value;
    if (productId) {
      const product = this.products.find(p => p.id === productId);
      return product?.imageUrl || null;
    }
    return null;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.invoiceForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  isFieldValid(fieldName: string): boolean {
    const field = this.invoiceForm.get(fieldName);
    return !!(field && field.valid && (field.dirty || field.touched));
  }

  isDetailFieldInvalid(index: number, fieldName: string): boolean {
    const detail = this.detailsArray.at(index);
    const field = detail.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  isDetailFieldValid(index: number, fieldName: string): boolean {
    const detail = this.detailsArray.at(index);
    const field = detail.get(fieldName);
    return !!(field && field.valid && (field.dirty || field.touched));
  }

  onInvoiceNumberInput(): void {
    const invoiceNumber = this.invoiceForm.get('invoiceNumber')?.value;
    if (invoiceNumber && this.invoiceForm.get('invoiceNumber')?.valid) {
      this.invoiceNumberSubject.next(invoiceNumber);
    } else {
      this.invoiceNumberExists = false;
    }
  }

  async checkInvoiceNumber(invoiceNumber: string): Promise<void> {
    if (!invoiceNumber || this.isFieldInvalid('invoiceNumber')) {
      this.invoiceNumberExists = false;
      return;
    }

    this.isCheckingInvoiceNumber = true;
    try {
      this.invoiceNumberExists = await firstValueFrom(this.invoicesApi.checkInvoiceNumberExists(invoiceNumber));
    } catch (error) {
      console.error('Error al verificar número de factura:', error);
      this.invoiceNumberExists = false;
    } finally {
      this.isCheckingInvoiceNumber = false;
    }
  }

  clearForm(): void {
    this.invoiceForm.reset({
      invoiceDate: new Date().toISOString().split('T')[0]
    });
    this.detailsArray.clear();
    this.addProduct();
    this.invoiceNumberExists = false;
  }

  async onSubmit(): Promise<void> {
    if (this.invoiceForm.invalid || this.invoiceNumberExists || !this.canSubmit()) {
      return;
    }

    this.isSubmitting = true;

    try {
      const formValue = this.invoiceForm.value;
      const request = {
        ...formValue,
        details: formValue.details.map((detail: any) => ({
          productId: detail.productId,
          quantity: detail.quantity,
          unitPrice: detail.unitPrice
        }))
      };

      await firstValueFrom(this.invoicesApi.createInvoice(request));
      alert('Factura creada exitosamente');
      this.clearForm();
    } catch (error) {
      console.error('Error al crear factura:', error);
      alert('Error al crear la factura');
    } finally {
      this.isSubmitting = false;
    }
  }

  private async loadClients(): Promise<void> {
    try {
      this.clients = await firstValueFrom(this.catalogApi.getClients());
    } catch (error) {
      console.error('Error al cargar clientes:', error);
      alert('Error al cargar la lista de clientes');
    }
  }

  private async loadProducts(): Promise<void> {
    try {
      this.products = await firstValueFrom(this.catalogApi.getProducts());
    } catch (error) {
      console.error('Error al cargar productos:', error);
      alert('Error al cargar la lista de productos');
    }
  }

  // Validadores personalizados
  private futureDateValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const selectedDate = new Date(control.value);
      const today = new Date();
      today.setHours(23, 59, 59, 999);
      
      if (selectedDate > today) {
        return { futureDate: true };
      }
      return null;
    };
  }

  private oldDateValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const selectedDate = new Date(control.value);
      const minDate = new Date('2020-01-01');
      
      if (selectedDate < minDate) {
        return { oldDate: true };
      }
      return null;
    };
  }

  // Métodos de exportación
  canExportPreview(): boolean {
    return this.invoiceForm.valid && this.detailsArray.length > 0 && this.detailsArray.controls.every(control => 
      control.get('productId')?.value && control.get('quantity')?.value && control.get('unitPrice')?.value
    );
  }

  getPreviewExportData(): ExportData {
    const formValue = this.invoiceForm.value;
    const selectedClient = this.clients.find(c => c.id === formValue.clientId);
    
    // Crear factura de vista previa para exportación
    const previewInvoice: Invoice = {
      id: 0,
      invoiceNumber: formValue.invoiceNumber || 'VISTA_PREVIA',
      clientId: formValue.clientId,
      clientName: selectedClient?.name || 'Cliente no seleccionado',
      invoiceDate: new Date(formValue.invoiceDate),
      subtotal: this.subtotal,
      taxAmount: this.taxAmount,
      total: this.total,
      status: 'VISTA_PREVIA',
      createdAt: new Date(),
      updatedAt: new Date(),
      details: formValue.details.map((detail: any) => {
        const product = this.products.find(p => p.id === detail.productId);
        return {
          id: 0,
          productId: detail.productId,
          productName: product?.name || 'Producto no encontrado',
          imageUrl: product?.imageUrl || '',
          quantity: detail.quantity,
          unitPrice: detail.unitPrice,
          total: detail.quantity * detail.unitPrice
        };
      })
    };

    return {
      invoices: [previewInvoice],
      title: `Vista Previa - Factura ${formValue.invoiceNumber || 'Nueva'}`,
      exportDate: new Date()
    };
  }

  onExportStarted(): void {
    console.log('Exportación iniciada');
  }

  onExportCompleted(): void {
    console.log('Exportación completada');
  }

  onExportError(error: any): void {
    console.error('Error en exportación:', error);
    alert('Error al exportar la vista previa');
  }
}
