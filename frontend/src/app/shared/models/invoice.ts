export interface Invoice {
  id: number;
  invoiceNumber: string;
  clientId: number;
  clientName: string;
  invoiceDate: Date;
  subtotal: number;
  taxAmount: number;
  total: number;
  status: string;
  createdAt: Date;
  updatedAt: Date;
  details: InvoiceDetail[];
}

export interface InvoiceDetail {
  id: number;
  productId: number;
  productName: string;
  imageUrl: string;
  quantity: number;
  unitPrice: number;
  total: number;
}

export interface InvoiceCreateRequest {
  invoiceNumber: string;
  clientId: number;
  invoiceDate: Date;
  details: InvoiceDetailRequest[];
}

export interface InvoiceDetailRequest {
  productId: number;
  quantity: number;
  unitPrice: number;
}

export interface InvoiceSearchRequest {
  searchType: 'Client' | 'InvoiceNumber';
  searchValue: string;
}

export interface PagedResult<T> {
  items: T[];
  totalRecords: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
