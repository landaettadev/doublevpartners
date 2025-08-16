import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
      <div class="container">
        <a class="navbar-brand" href="#">Sistema de Facturación</a>
        <div class="navbar-nav">
          <a class="nav-link" routerLink="/invoices/create" routerLinkActive="active">Crear Factura</a>
          <a class="nav-link" routerLink="/invoices/search" routerLinkActive="active">Buscar Facturas</a>
        </div>
      </div>
    </nav>

    <div class="container mt-4">
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [`
    .navbar-nav .nav-link.active {
      font-weight: bold;
    }
  `]
})
export class AppComponent {
  title = 'Sistema de Facturación';
}
