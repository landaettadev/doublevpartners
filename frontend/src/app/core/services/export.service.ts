import { Injectable } from '@angular/core';
import { Invoice, InvoiceDetail } from '../../shared/models/invoice';
import { UtilityService } from './utility.service';

export interface ExportOptions {
  format: 'pdf' | 'excel' | 'csv';
  filename?: string;
  includeHeaders?: boolean;
  orientation?: 'portrait' | 'landscape';
  pageSize?: 'A4' | 'Letter' | 'Legal';
}

export interface ExportData {
  invoices: Invoice[];
  title?: string;
  filters?: any;
  exportDate?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class ExportService {

  constructor() { }

  /**
   * Exportar facturas en el formato especificado
   */
  async exportInvoices(data: ExportData, options: ExportOptions): Promise<void> {
    const filename = options.filename || this.generateFilename('facturas', options.format);
    
    switch (options.format) {
      case 'pdf':
        await this.exportToPdf(data, options, filename);
        break;
      case 'excel':
        await this.exportToExcel(data, options, filename);
        break;
      case 'csv':
        await this.exportToCsv(data, options, filename);
        break;
      default:
        throw new Error(`Formato de exportación no soportado: ${options.format}`);
    }
  }

  /**
   * Exportar factura individual a PDF
   */
  async exportInvoiceToPdf(invoice: Invoice, options: ExportOptions = { format: 'pdf' }): Promise<void> {
    const filename = options.filename || `factura-${invoice.invoiceNumber}.pdf`;
    const data: ExportData = {
      invoices: [invoice],
      title: `Factura ${invoice.invoiceNumber}`,
      exportDate: new Date()
    };
    
    await this.exportToPdf(data, options, filename);
  }

  /**
   * Exportar a PDF usando jsPDF
   */
  private async exportToPdf(data: ExportData, options: ExportOptions, filename: string): Promise<void> {
    try {
      // Importar jsPDF dinámicamente
      const { jsPDF } = await import('jspdf');
      const doc = new jsPDF(options.orientation || 'portrait', 'mm', options.pageSize || 'A4');
      
      // Configurar el documento
      this.setupPdfDocument(doc, data, options);
      
      // Agregar contenido
      this.addPdfContent(doc, data, options);
      
      // Guardar el documento
      doc.save(filename);
      
    } catch (error) {
      console.error('Error al exportar a PDF:', error);
      throw new Error('No se pudo exportar a PDF. Verifique que jsPDF esté disponible.');
    }
  }

  /**
   * Exportar a Excel usando SheetJS
   */
  private async exportToExcel(data: ExportData, options: ExportOptions, filename: string): Promise<void> {
    try {
      // Importar SheetJS dinámicamente
      const XLSX = await import('xlsx');
      
      // Crear workbook
      const workbook = XLSX.utils.book_new();
      
      // Preparar datos para la hoja
      const worksheetData = this.prepareExcelData(data, options);
      
      // Crear worksheet
      const worksheet = XLSX.utils.aoa_to_sheet(worksheetData);
      
      // Agregar worksheet al workbook
      XLSX.utils.book_append_sheet(workbook, worksheet, 'Facturas');
      
      // Agregar hoja de resumen
      if (data.invoices.length > 1) {
        const summaryData = this.prepareExcelSummaryData(data);
        const summaryWorksheet = XLSX.utils.aoa_to_sheet(summaryData);
        XLSX.utils.book_append_sheet(workbook, summaryWorksheet, 'Resumen');
      }
      
      // Guardar archivo
      XLSX.writeFile(workbook, filename);
      
    } catch (error) {
      console.error('Error al exportar a Excel:', error);
      throw new Error('No se pudo exportar a Excel. Verifique que SheetJS esté disponible.');
    }
  }

  /**
   * Exportar a CSV
   */
  private async exportToCsv(data: ExportData, options: ExportOptions, filename: string): Promise<void> {
    try {
      // Preparar datos CSV
      const csvData = this.prepareCsvData(data, options);
      
      // Crear blob y descargar
      const blob = new Blob([csvData], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      
      if (link.download !== undefined) {
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', filename);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
      } else {
        // Fallback para navegadores que no soportan download
        UtilityService.downloadFile(csvData, filename, 'text/csv');
      }
      
    } catch (error) {
      console.error('Error al exportar a CSV:', error);
      throw new Error('No se pudo exportar a CSV.');
    }
  }

  /**
   * Configurar documento PDF
   */
  private setupPdfDocument(doc: any, data: ExportData, options: ExportOptions): void {
    // Configurar fuentes y estilos
    doc.setFont('helvetica');
    doc.setFontSize(12);
    
    // Agregar encabezado
    doc.setFontSize(20);
    doc.text('DOUBLE V PARTNERS', 105, 20, { align: 'center' });
    
    doc.setFontSize(14);
    doc.text('Sistema de Facturación', 105, 30, { align: 'center' });
    
    if (data.title) {
      doc.setFontSize(16);
      doc.text(data.title, 105, 45, { align: 'center' });
    }
    
    // Información de exportación
    doc.setFontSize(10);
    const exportDate = data.exportDate || new Date();
    doc.text(`Exportado el: ${UtilityService.formatDate(exportDate, 'long')}`, 20, 60);
    
    if (data.filters) {
      doc.text(`Filtros aplicados: ${JSON.stringify(data.filters)}`, 20, 70);
    }
  }

  /**
   * Agregar contenido al PDF
   */
  private addPdfContent(doc: any, data: ExportData, options: ExportOptions): void {
    let yPosition = 90;
    
    data.invoices.forEach((invoice, index) => {
      // Verificar si necesitamos una nueva página
      if (yPosition > 250) {
        doc.addPage();
        yPosition = 20;
      }
      
      // Encabezado de factura
      doc.setFontSize(14);
      doc.setFont('helvetica', 'bold');
      doc.text(`Factura ${invoice.invoiceNumber}`, 20, yPosition);
      
      yPosition += 10;
      
      // Información del cliente
      doc.setFontSize(10);
      doc.setFont('helvetica', 'normal');
      doc.text(`Cliente: ${invoice.clientName}`, 20, yPosition);
      yPosition += 6;
      
      doc.text(`Fecha: ${UtilityService.formatDate(invoice.invoiceDate, 'short')}`, 20, yPosition);
      yPosition += 6;
      
      doc.text(`Estado: ${invoice.status}`, 20, yPosition);
      yPosition += 10;
      
      // Tabla de detalles
      if (invoice.details && invoice.details.length > 0) {
        this.addPdfDetailsTable(doc, invoice.details, yPosition);
        yPosition += (invoice.details.length * 8) + 15;
      }
      
      // Totales
      doc.setFont('helvetica', 'bold');
      doc.text(`Subtotal: ${UtilityService.formatCurrency(invoice.subtotal)}`, 120, yPosition);
      yPosition += 6;
      
      doc.text(`IVA (19%): ${UtilityService.formatCurrency(invoice.taxAmount)}`, 120, yPosition);
      yPosition += 6;
      
      doc.text(`Total: ${UtilityService.formatCurrency(invoice.total)}`, 120, yPosition);
      yPosition += 15;
      
      // Separador entre facturas
      if (index < data.invoices.length - 1) {
        doc.line(20, yPosition, 190, yPosition);
        yPosition += 10;
      }
    });
    
    // Agregar pie de página
    this.addPdfFooter(doc, data);
  }

  /**
   * Agregar tabla de detalles al PDF
   */
  private addPdfDetailsTable(doc: any, details: InvoiceDetail[], startY: number): void {
    const headers = ['Producto', 'Cantidad', 'Precio Unit.', 'Total'];
    const columnWidths = [80, 25, 35, 35];
    const startX = 20;
    
    // Encabezados
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(9);
    
    let x = startX;
    headers.forEach((header, index) => {
      doc.text(header, x, startY);
      x += columnWidths[index];
    });
    
    // Línea separadora
    doc.line(startX, startY + 2, startX + 175, startY + 2);
    
    // Datos
    doc.setFont('helvetica', 'normal');
    details.forEach((detail, index) => {
      const y = startY + 8 + (index * 6);
      
      x = startX;
      doc.text(detail.productName, x, y);
      x += columnWidths[0];
      
      doc.text(detail.quantity.toString(), x, y);
      x += columnWidths[1];
      
      doc.text(UtilityService.formatCurrency(detail.unitPrice), x, y);
      x += columnWidths[2];
      
      doc.text(UtilityService.formatCurrency(detail.total), x, y);
    });
  }

  /**
   * Agregar pie de página al PDF
   */
  private addPdfFooter(doc: any, data: ExportData): void {
    const pageCount = doc.internal.getNumberOfPages();
    
    for (let i = 1; i <= pageCount; i++) {
      doc.setPage(i);
      
      doc.setFontSize(8);
      doc.text(`Página ${i} de ${pageCount}`, 105, 280, { align: 'center' });
      doc.text('Double V Partners - Sistema de Facturación', 105, 285, { align: 'center' });
    }
  }

  /**
   * Preparar datos para Excel
   */
  private prepareExcelData(data: ExportData, options: ExportOptions): any[][] {
    const rows: any[][] = [];
    
    // Encabezados
    if (options.includeHeaders !== false) {
      rows.push([
        'Número de Factura',
        'Cliente',
        'Fecha',
        'Estado',
        'Subtotal',
        'IVA (19%)',
        'Total',
        'Productos'
      ]);
    }
    
    // Datos de facturas
    data.invoices.forEach(invoice => {
      const products = invoice.details?.map(d => d.productName).join('; ') || '';
      
      rows.push([
        invoice.invoiceNumber,
        invoice.clientName,
        UtilityService.formatDate(invoice.invoiceDate, 'short'),
        invoice.status,
        invoice.subtotal,
        invoice.taxAmount,
        invoice.total,
        products
      ]);
    });
    
    return rows;
  }

  /**
   * Preparar datos de resumen para Excel
   */
  private prepareExcelSummaryData(data: ExportData): any[][] {
    const rows: any[][] = [];
    
    // Encabezados del resumen
    rows.push(['Resumen de Exportación']);
    rows.push([]);
    
    // Estadísticas generales
    const totalInvoices = data.invoices.length;
    const totalAmount = data.invoices.reduce((sum, inv) => sum + inv.total, 0);
    const totalTax = data.invoices.reduce((sum, inv) => sum + inv.taxAmount, 0);
    const totalSubtotal = data.invoices.reduce((sum, inv) => sum + inv.subtotal, 0);
    
    rows.push(['Total de Facturas', totalInvoices]);
    rows.push(['Subtotal Total', UtilityService.formatCurrency(totalSubtotal)]);
    rows.push(['IVA Total', UtilityService.formatCurrency(totalTax)]);
    rows.push(['Total General', UtilityService.formatCurrency(totalAmount)]);
    rows.push([]);
    
    // Resumen por estado
    const statusSummary = data.invoices.reduce((acc, inv) => {
      acc[inv.status] = (acc[inv.status] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);
    
    rows.push(['Resumen por Estado']);
    Object.entries(statusSummary).forEach(([status, count]) => {
      rows.push([status, count]);
    });
    
    return rows;
  }

  /**
   * Preparar datos para CSV
   */
  private prepareCsvData(data: ExportData, options: ExportOptions): string {
    const rows: string[] = [];
    
    // Encabezados
    if (options.includeHeaders !== false) {
      rows.push([
        'Número de Factura',
        'Cliente',
        'Fecha',
        'Estado',
        'Subtotal',
        'IVA (19%)',
        'Total',
        'Productos'
      ].join(','));
    }
    
    // Datos de facturas
    data.invoices.forEach(invoice => {
      const products = invoice.details?.map(d => d.productName).join('; ') || '';
      
      const row = [
        `"${invoice.invoiceNumber}"`,
        `"${invoice.clientName}"`,
        `"${UtilityService.formatDate(invoice.invoiceDate, 'short')}"`,
        `"${invoice.status}"`,
        invoice.subtotal.toString(),
        invoice.taxAmount.toString(),
        invoice.total.toString(),
        `"${products}"`
      ].join(',');
      
      rows.push(row);
    });
    
    return rows.join('\n');
  }

  /**
   * Generar nombre de archivo
   */
  private generateFilename(baseName: string, format: string): string {
    const date = new Date();
    const timestamp = date.toISOString().slice(0, 10).replace(/-/g, '');
    const time = date.toTimeString().slice(0, 8).replace(/:/g, '');
    
    return `${baseName}_${timestamp}_${time}.${format}`;
  }



  /**
   * Verificar si un formato de exportación está disponible
   */
  isFormatAvailable(format: string): boolean {
    switch (format) {
      case 'csv':
        return true; // CSV siempre está disponible
      case 'pdf':
        return true; // jsPDF está disponible como dependencia
      case 'excel':
        return true; // SheetJS está disponible como dependencia
      default:
        return false;
    }
  }

  /**
   * Obtener formatos disponibles
   */
  getAvailableFormats(): string[] {
    const formats = ['csv']; // CSV siempre está disponible
    
    if (this.isFormatAvailable('pdf')) {
      formats.push('pdf');
    }
    
    if (this.isFormatAvailable('excel')) {
      formats.push('excel');
    }
    
    return formats;
  }

  /**
   * Exportar con formato automático (mejor formato disponible)
   */
  async exportAuto(data: ExportData, preferredFormat?: string): Promise<void> {
    const availableFormats = this.getAvailableFormats();
    
    if (availableFormats.length === 0) {
      throw new Error('No hay formatos de exportación disponibles');
    }
    
    let format: string;
    
    if (preferredFormat && availableFormats.includes(preferredFormat)) {
      format = preferredFormat;
    } else {
      // Priorizar Excel > PDF > CSV
      if (availableFormats.includes('excel')) {
        format = 'excel';
      } else if (availableFormats.includes('pdf')) {
        format = 'pdf';
      } else {
        format = 'csv';
      }
    }
    
    await this.exportInvoices(data, { format: format as any });
  }

  /**
   * Exportar resumen ejecutivo
   */
  async exportExecutiveSummary(data: ExportData, options: ExportOptions = { format: 'pdf' }): Promise<void> {
    const filename = options.filename || `resumen_ejecutivo_${new Date().toISOString().slice(0, 10)}.${options.format}`;
    
    // Crear datos de resumen
    const summaryData: ExportData = {
      invoices: data.invoices,
      title: 'Resumen Ejecutivo de Facturación',
      exportDate: new Date(),
      filters: data.filters
    };
    
    await this.exportInvoices(summaryData, { ...options, filename });
  }

  /**
   * Exportar con plantilla personalizada
   */
  async exportWithTemplate(data: ExportData, template: string, options: ExportOptions): Promise<void> {
    // Implementar exportación con plantillas personalizadas
    // Por ahora, usar exportación estándar
    await this.exportInvoices(data, options);
  }
}
