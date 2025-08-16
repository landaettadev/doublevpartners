import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { InvoicesApiService } from '../../../core/http/invoices.api';
import { CatalogApiService } from '../../../core/http/catalog.api';
import { Client, Product } from '../../../shared/models/catalog';
import { InvoiceDetailRequest } from '../../../shared/models/invoice';

@Component({
  selector: 'app-invoice-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
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
                <label for="invoiceNumber" class="form-label">Número de Factura</label>
                <input 
                  type="text" 
                  class="form-control" 
                  id="invoiceNumber" 
                  formControlName="invoiceNumber"
                  [class.is-invalid]="isFieldInvalid('invoiceNumber')"
                  (blur)="checkInvoiceNumber()"
                >
                <div class="invalid-feedback" *ngIf="isFieldInvalid('invoiceNumber')">
                  Número de factura es requerido
                </div>
                <div class="invalid-feedback" *ngIf="invoiceNumberExists">
                  Este número de factura ya existe
                </div>
              </div>
            </div>
            <div class="col-md-6">
              <div class="mb-3">
                <label for="clientId" class="form-label">Cliente</label>
                <select 
                  class="form-select" 
                  id="clientId" 
                  formControlName="clientId"
                  [class.is-invalid]="isFieldInvalid('clientId')"
                >
                  <option value="">Seleccionar cliente</option>
                  <option *ngFor="let client of clients" [value]="client.id">
                    {{ client.name }}
                  </option>
                </select>
                <div class="invalid-feedback" *ngIf="isFieldInvalid('clientId')">
                  Cliente es requerido
                </div>
              </div>
            </div>
          </div>

          <div class="mb-3">
            <label for="invoiceDate" class="form-label">Fecha de Factura</label>
            <input 
              type="date" 
              class="form-control" 
              id="invoiceDate" 
              formControlName="invoiceDate"
              [class.is-invalid]="isFieldInvalid('invoiceDate')"
            >
            <div class="invalid-feedback" *ngIf="isFieldInvalid('invoiceDate')">
              Fecha es requerida
            </div>
          </div>

          <div class="mb-3">
            <h5>Detalles de la Factura</h5>
            <button type="button" class="btn btn-secondary mb-2" (click)="addProduct()">
              Agregar Producto
            </button>
            
            <div formArrayName="details">
              <div *ngFor="let detail of detailsArray.controls; let i = index" [formGroupName]="i" class="row mb-2">
                <div class="col-md-3">
                  <select 
                    class="form-select" 
                    formControlName="productId"
                    (change)="onProductChange(i)"
                  >
                    <option value="">Seleccionar producto</option>
                    <option *ngFor="let product of products" [value]="product.id">
                      {{ product.name }}
                    </option>
                  </select>
                </div>
                <div class="col-md-2">
                  <input 
                    type="number" 
                    class="form-control" 
                    placeholder="Cantidad" 
                    formControlName="quantity"
                    (input)="calculateTotal(i)"
                  >
                </div>
                <div class="col-md-2">
                  <input 
                    type="number" 
                    class="form-control" 
                    placeholder="Precio Unit." 
                    formControlName="unitPrice"
                    readonly
                  >
                </div>
                <div class="col-md-2">
                  <input 
                    type="number" 
                    class="form-control" 
                    placeholder="Total" 
                    [value]="getDetailTotal(i)"
                    readonly
                  >
                </div>
                <div class="col-md-2">
                  <img 
                    *ngIf="getProductImage(i)" 
                    [src]="getProductImage(i)" 
                    alt="Producto" 
                    class="img-thumbnail" 
                    style="width: 50px; height: 50px;"
                  >
                </div>
                <div class="col-md-1">
                  <button type="button" class="btn btn-danger btn-sm" (click)="removeProduct(i)">
                    X
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div class="row">
            <div class="col-md-6 offset-md-6">
              <table class="table table-borderless">
                <tr>
                  <td><strong>Subtotal:</strong></td>
                  <td class="text-end">{{ subtotal | currency:'COP':'symbol':'1.0-0' }}</td>
                </tr>
                <tr>
                  <td><strong>IVA (19%):</strong></td>
                  <td class="text-end">{{ taxAmount | currency:'COP':'symbol':'1.0-0' }}</td>
                </tr>
                <tr>
                  <td><strong>Total:</strong></td>
                  <td class="text-end"><strong>{{ total | currency:'COP':'symbol':'1.0-0' }}</strong></td>
                </tr>
              </table>
            </div>
          </div>

          <div class="d-flex justify-content-between">
            <button type="button" class="btn btn-secondary" (click)="clearForm()">
              Nuevo
            </button>
            <button type="submit" class="btn btn-primary" [disabled]="invoiceForm.invalid || isSubmitting || invoiceNumberExists">
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
  `]
})
export class InvoiceCreateComponent implements OnInit {
  invoiceForm: FormGroup;
  clients: Client[] = [];
  products: Product[] = [];
  isSubmitting = false;
  invoiceNumberExists = false;

  constructor(
    private fb: FormBuilder,
    private invoicesApi: InvoicesApiService,
    private catalogApi: CatalogApiService
  ) {
    this.invoiceForm = this.fb.group({
      invoiceNumber: ['', Validators.required],
      clientId: ['', Validators.required],
      invoiceDate: [new Date().toISOString().split('T')[0], Validators.required],
      details: this.fb.array([])
    });
  }

  ngOnInit(): void {
    this.loadClients();
    this.loadProducts();
    this.addProduct(); // Agregar primer producto por defecto
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

  addProduct(): void {
    const detail = this.fb.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unitPrice: [0, Validators.required]
    });

    this.detailsArray.push(detail);
  }

  removeProduct(index: number): void {
    this.detailsArray.removeAt(index);
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

  async checkInvoiceNumber(): Promise<void> {
    const invoiceNumber = this.invoiceForm.get('invoiceNumber')?.value;
    if (invoiceNumber && !this.isFieldInvalid('invoiceNumber')) {
      try {
        this.invoiceNumberExists = await this.invoicesApi.checkInvoiceNumberExists(invoiceNumber).toPromise();
      } catch (error) {
        console.error('Error al verificar número de factura:', error);
      }
    } else {
      this.invoiceNumberExists = false;
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
    if (this.invoiceForm.invalid || this.invoiceNumberExists) {
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

      await this.invoicesApi.createInvoice(request).toPromise();
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
      this.clients = await this.catalogApi.getClients().toPromise();
    } catch (error) {
      console.error('Error al cargar clientes:', error);
      alert('Error al cargar la lista de clientes');
    }
  }

  private async loadProducts(): Promise<void> {
    try {
      this.products = await this.catalogApi.getProducts().toPromise();
    } catch (error) {
      console.error('Error al cargar productos:', error);
      alert('Error al cargar la lista de productos');
    }
  }
}
