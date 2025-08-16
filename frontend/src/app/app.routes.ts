import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/invoices/create',
    pathMatch: 'full'
  },
  {
    path: 'invoices',
    children: [
      {
        path: 'create',
        loadComponent: () => import('./features/invoices/invoice-create/invoice-create.component')
          .then(m => m.InvoiceCreateComponent)
      },
      {
        path: 'search',
        loadComponent: () => import('./features/invoices/invoice-search/invoice-search.component')
          .then(m => m.InvoiceSearchComponent)
      }
    ]
  }
];
