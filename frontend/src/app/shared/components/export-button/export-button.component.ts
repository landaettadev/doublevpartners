import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ExportService, ExportOptions, ExportData } from '../../../core/services/export.service';
import { NotificationService } from '../../../core/services/notification.service';
import { UtilityService } from '../../../core/services/utility.service';

export interface ExportButtonConfig {
  label?: string;
  icon?: string;
  formats?: ('pdf' | 'excel' | 'csv')[];
  showFormatSelector?: boolean;
  showOptions?: boolean;
  defaultFormat?: 'pdf' | 'excel' | 'csv';
  buttonClass?: string;
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  loading?: boolean;
}

@Component({
  selector: 'app-export-button',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="export-button-container" [class.disabled]="config.disabled">
      <!-- Botón principal de exportación -->
      <button 
        *ngIf="!config.showFormatSelector"
        [class]="getButtonClasses()"
        [disabled]="config.disabled || config.loading"
        (click)="exportWithDefaultFormat()"
        type="button">
        
        <span *ngIf="config.loading" class="spinner-border spinner-border-sm me-2" role="status">
          <span class="visually-hidden">Exportando...</span>
        </span>
        
        <i *ngIf="!config.loading && config.icon" [class]="config.icon" class="me-2"></i>
        
        {{ config.loading ? 'Exportando...' : (config.label || 'Exportar') }}
      </button>

      <!-- Selector de formato con opciones -->
      <div *ngIf="config.showFormatSelector" class="dropdown">
        <button 
          class="btn dropdown-toggle"
          [class]="getButtonClasses()"
          [disabled]="config.disabled || config.loading"
          type="button"
          data-bs-toggle="dropdown"
          aria-expanded="false">
          
          <span *ngIf="config.loading" class="spinner-border spinner-border-sm me-2" role="status">
            <span class="visually-hidden">Exportando...</span>
          </span>
          
          <i *ngIf="!config.loading && config.icon" [class]="config.icon" class="me-2"></i>
          
          {{ config.loading ? 'Exportando...' : (config.label || 'Exportar') }}
        </button>
        
        <ul class="dropdown-menu">
          <li *ngFor="let format of availableFormats">
            <button 
              class="dropdown-item"
              type="button"
              (click)="exportWithFormat(format)"
              [disabled]="config.loading">
              
              <i [class]="getFormatIcon(format)" class="me-2"></i>
              {{ getFormatLabel(format) }}
            </button>
          </li>
          
          <li *ngIf="config.showOptions">
            <hr class="dropdown-divider">
            <button 
              class="dropdown-item"
              type="button"
              (click)="showExportOptions()"
              [disabled]="config.loading">
              
              <i class="fas fa-cog me-2"></i>
              Opciones avanzadas
            </button>
          </li>
        </ul>
      </div>

      <!-- Modal de opciones avanzadas -->
      <div 
        *ngIf="showOptionsModal"
        class="modal fade show d-block"
        tabindex="-1"
        style="background-color: rgba(0,0,0,0.5);">
        
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Opciones de Exportación</h5>
              <button 
                type="button" 
                class="btn-close" 
                (click)="closeExportOptions()"
                aria-label="Cerrar"></button>
            </div>
            
            <div class="modal-body">
              <div class="row">
                <div class="col-md-6">
                  <label class="form-label">Formato</label>
                  <select 
                    class="form-select" 
                    [(ngModel)]="exportOptions.format">
                    <option value="pdf">PDF</option>
                    <option value="excel">Excel</option>
                    <option value="csv">CSV</option>
                  </select>
                </div>
                
                <div class="col-md-6">
                  <label class="form-label">Orientación</label>
                  <select 
                    class="form-select" 
                    [(ngModel)]="exportOptions.orientation">
                    <option value="portrait">Vertical</option>
                    <option value="landscape">Horizontal</option>
                  </select>
                </div>
              </div>
              
              <div class="row mt-3">
                <div class="col-md-6">
                  <label class="form-label">Tamaño de página</label>
                  <select 
                    class="form-select" 
                    [(ngModel)]="exportOptions.pageSize">
                    <option value="A4">A4</option>
                    <option value="Letter">Letter</option>
                    <option value="Legal">Legal</option>
                  </select>
                </div>
                
                <div class="col-md-6">
                  <label class="form-label">Nombre del archivo</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    [(ngModel)]="customFilename"
                    placeholder="Nombre personalizado">
                </div>
              </div>
              
              <div class="form-check mt-3">
                <input 
                  class="form-check-input" 
                  type="checkbox" 
                  [(ngModel)]="exportOptions.includeHeaders"
                  id="includeHeaders">
                <label class="form-check-label" for="includeHeaders">
                  Incluir encabezados
                </label>
              </div>
            </div>
            
            <div class="modal-footer">
              <button 
                type="button" 
                class="btn btn-secondary" 
                (click)="closeExportOptions()">
                Cancelar
              </button>
              <button 
                type="button" 
                class="btn btn-primary" 
                (click)="exportWithCustomOptions()"
                [disabled]="config.loading">
                Exportar
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .export-button-container {
      display: inline-block;
    }
    
    .export-button-container.disabled {
      opacity: 0.6;
      pointer-events: none;
    }
    
    .dropdown-menu {
      min-width: 200px;
    }
    
    .dropdown-item {
      padding: 0.5rem 1rem;
      cursor: pointer;
    }
    
    .dropdown-item:hover {
      background-color: #f8f9fa;
    }
    
    .dropdown-item:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    
    .modal.show {
      z-index: 1050;
    }
    
    .form-label {
      font-weight: 500;
      margin-bottom: 0.5rem;
    }
    
    .form-select, .form-control {
      border-radius: 0.375rem;
      border: 1px solid #ced4da;
    }
    
    .form-select:focus, .form-control:focus {
      border-color: #86b7fe;
      box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
    }
    
    .form-check-input:checked {
      background-color: #0d6efd;
      border-color: #0d6efd;
    }
  `]
})
export class ExportButtonComponent implements OnInit {
  @Input() config: ExportButtonConfig = {};
  @Input() exportData: ExportData | null = null;
  @Output() exportStarted = new EventEmitter<void>();
  @Output() exportCompleted = new EventEmitter<void>();
  @Output() exportError = new EventEmitter<string>();

  availableFormats: string[] = [];
  exportOptions: ExportOptions;
  customFilename: string = '';
  showOptionsModal = false;

  constructor(
    private exportService: ExportService,
    private notificationService: NotificationService
  ) {
    // Configuración por defecto
    this.config = {
      label: 'Exportar',
      icon: 'fas fa-download',
      formats: ['pdf', 'excel', 'csv'],
      showFormatSelector: true,
      showOptions: true,
      defaultFormat: 'pdf',
      buttonClass: 'btn-primary',
      size: 'md',
      disabled: false,
      loading: false
    };

    this.exportOptions = {
      format: 'pdf',
      includeHeaders: true,
      orientation: 'portrait',
      pageSize: 'A4'
    };
  }

  ngOnInit(): void {
    // Combinar configuración por defecto con la entrada
    this.config = { ...this.config, ...this.config };
    
    // Configurar opciones de exportación
    this.exportOptions.format = this.config.defaultFormat || 'pdf';
    
    // Obtener formatos disponibles
    this.availableFormats = this.exportService.getAvailableFormats();
    
    // Filtrar formatos según configuración
    if (this.config.formats) {
      this.availableFormats = this.availableFormats.filter(f => 
        this.config.formats!.includes(f as any)
      );
    }
  }

  /**
   * Exportar con formato por defecto
   */
  async exportWithDefaultFormat(): Promise<void> {
    if (!this.exportData) {
      this.notificationService.error('No hay datos para exportar');
      return;
    }

    await this.exportWithFormat(this.config.defaultFormat || 'pdf');
  }

  /**
   * Exportar con formato específico
   */
  async exportWithFormat(format: string): Promise<void> {
    if (!this.exportData) {
      this.notificationService.error('No hay datos para exportar');
      return;
    }

    try {
      this.config.loading = true;
      this.exportStarted.emit();

      const options: ExportOptions = {
        format: format as any,
        includeHeaders: this.exportOptions.includeHeaders,
        orientation: this.exportOptions.orientation,
        pageSize: this.exportOptions.pageSize
      };

      if (this.customFilename) {
        options.filename = this.customFilename;
      }

      await this.exportService.exportInvoices(this.exportData, options);

      this.notificationService.success(
        `Exportación completada exitosamente en formato ${format.toUpperCase()}`,
        'Exportación Exitosa'
      );

      this.exportCompleted.emit();

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Error desconocido durante la exportación';
      
      this.notificationService.error(
        errorMessage,
        'Error de Exportación'
      );
      
      this.exportError.emit(errorMessage);
      
    } finally {
      this.config.loading = false;
    }
  }

  /**
   * Mostrar modal de opciones avanzadas
   */
  showExportOptions(): void {
    this.showOptionsModal = true;
  }

  /**
   * Cerrar modal de opciones
   */
  closeExportOptions(): void {
    this.showOptionsModal = false;
  }

  /**
   * Exportar con opciones personalizadas
   */
  async exportWithCustomOptions(): Promise<void> {
    this.closeExportOptions();
    await this.exportWithFormat(this.exportOptions.format);
  }

  /**
   * Obtener clases CSS del botón
   */
  getButtonClasses(): string {
    const baseClass = this.config.buttonClass || 'btn-primary';
    const sizeClass = this.config.size ? `btn-${this.config.size}` : '';
    
    return `btn ${baseClass} ${sizeClass}`.trim();
  }

  /**
   * Obtener icono para formato específico
   */
  getFormatIcon(format: string): string {
    switch (format) {
      case 'pdf':
        return 'fas fa-file-pdf';
      case 'excel':
        return 'fas fa-file-excel';
      case 'csv':
        return 'fas fa-file-csv';
      default:
        return 'fas fa-file';
    }
  }

  /**
   * Obtener etiqueta para formato específico
   */
  getFormatLabel(format: string): string {
    switch (format) {
      case 'pdf':
        return 'Exportar a PDF';
      case 'excel':
        return 'Exportar a Excel';
      case 'csv':
        return 'Exportar a CSV';
      default:
        return `Exportar a ${format.toUpperCase()}`;
    }
  }

  /**
   * Verificar si un formato está disponible
   */
  isFormatAvailable(format: string): boolean {
    return this.exportService.isFormatAvailable(format);
  }

  /**
   * Obtener estadísticas de exportación
   */
  getExportStats(): any {
    if (!this.exportData) return null;

    const invoices = this.exportData.invoices;
    const totalInvoices = invoices.length;
    const totalAmount = invoices.reduce((sum, inv) => sum + inv.total, 0);
    const totalTax = invoices.reduce((sum, inv) => sum + inv.taxAmount, 0);

    return {
      totalInvoices,
      totalAmount,
      totalTax,
      averageAmount: totalInvoices > 0 ? totalAmount / totalInvoices : 0
    };
  }

  /**
   * Exportar resumen ejecutivo
   */
  async exportExecutiveSummary(): Promise<void> {
    if (!this.exportData) {
      this.notificationService.error('No hay datos para exportar');
      return;
    }

    try {
      this.config.loading = true;
      this.exportStarted.emit();

      await this.exportService.exportExecutiveSummary(this.exportData, this.exportOptions);

      this.notificationService.success(
        'Resumen ejecutivo exportado exitosamente',
        'Exportación Exitosa'
      );

      this.exportCompleted.emit();

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Error desconocido durante la exportación';
      
      this.notificationService.error(
        errorMessage,
        'Error de Exportación'
      );
      
      this.exportError.emit(errorMessage);
      
    } finally {
      this.config.loading = false;
    }
  }

  /**
   * Exportar factura individual
   */
  async exportSingleInvoice(invoice: any): Promise<void> {
    try {
      this.config.loading = true;
      this.exportStarted.emit();

      await this.exportService.exportInvoiceToPdf(invoice, this.exportOptions);

      this.notificationService.success(
        'Factura exportada exitosamente',
        'Exportación Exitosa'
      );

      this.exportCompleted.emit();

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Error desconocido durante la exportación';
      
      this.notificationService.error(
        errorMessage,
        'Error de Exportación'
      );
      
      this.exportError.emit(errorMessage);
      
    } finally {
      this.config.loading = false;
    }
  }
}
