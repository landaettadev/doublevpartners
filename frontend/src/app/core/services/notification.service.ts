import { Injectable, Renderer2, RendererFactory2, Inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';

export interface NotificationOptions {
  type?: 'success' | 'error' | 'warning' | 'info';
  duration?: number;
  position?: 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left' | 'top-center' | 'bottom-center';
  dismissible?: boolean;
  icon?: string;
  actions?: NotificationAction[];
}

export interface NotificationAction {
  label: string;
  action: () => void;
  class?: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private renderer: Renderer2;
  private notificationContainer: HTMLElement | null = null;
  private notifications: Map<string, HTMLElement> = new Map();
  private counter = 0;

  constructor(
    private rendererFactory: RendererFactory2,
    @Inject(DOCUMENT) private document: Document
  ) {
    this.renderer = this.rendererFactory.createRenderer(null, null);
    this.createNotificationContainer();
  }

  /**
   * Mostrar notificación de éxito
   */
  success(message: string, title?: string, options?: NotificationOptions): string {
    return this.show(message, title, { ...options, type: 'success' });
  }

  /**
   * Mostrar notificación de error
   */
  error(message: string, title?: string, options?: NotificationOptions): string {
    return this.show(message, title, { ...options, type: 'error' });
  }

  /**
   * Mostrar notificación de advertencia
   */
  warning(message: string, title?: string, options?: NotificationOptions): string {
    return this.show(message, title, { ...options, type: 'warning' });
  }

  /**
   * Mostrar notificación informativa
   */
  info(message: string, title?: string, options?: NotificationOptions): string {
    return this.show(message, title, { ...options, type: 'info' });
  }

  /**
   * Mostrar notificación personalizada
   */
  show(message: string, title?: string, options: NotificationOptions = {}): string {
    const id = `notification-${++this.counter}`;
    const {
      type = 'info',
      duration = 5000,
      position = 'top-right',
      dismissible = true,
      icon,
      actions = []
    } = options;

    const notification = this.createNotificationElement(id, message, title, type, icon, actions, dismissible);
    
    // Agregar al contenedor
    this.notificationContainer?.appendChild(notification);
    this.notifications.set(id, notification);

    // Aplicar animación de entrada
    setTimeout(() => {
      this.renderer.addClass(notification, 'show');
    }, 10);

    // Auto-dismiss si se especifica duración
    if (duration > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, duration);
    }

    return id;
  }

  /**
   * Descartar notificación por ID
   */
  dismiss(id: string): void {
    const notification = this.notifications.get(id);
    if (notification) {
      this.renderer.removeClass(notification, 'show');
      setTimeout(() => {
        if (notification.parentNode) {
          notification.parentNode.removeChild(notification);
        }
        this.notifications.delete(id);
      }, 300);
    }
  }

  /**
   * Descartar todas las notificaciones
   */
  dismissAll(): void {
    this.notifications.forEach((_, id) => {
      this.dismiss(id);
    });
  }

  /**
   * Mostrar toast simple
   */
  toast(message: string, type: 'success' | 'error' | 'warning' | 'info' = 'info', duration: number = 3000): string {
    return this.show(message, undefined, {
      type,
      duration,
      position: 'bottom-center',
      dismissible: false,
      actions: []
    });
  }

  /**
   * Mostrar confirmación
   */
  confirm(message: string, title?: string): Promise<boolean> {
    return new Promise((resolve) => {
      const id = this.show(message, title, {
        type: 'warning',
        duration: 0,
        dismissible: false,
        actions: [
          {
            label: 'Cancelar',
            action: () => {
              this.dismiss(id);
              resolve(false);
            },
            class: 'btn-secondary'
          },
          {
            label: 'Confirmar',
            action: () => {
              this.dismiss(id);
              resolve(true);
            },
            class: 'btn-primary'
          }
        ]
      });
    });
  }

  /**
   * Mostrar alerta
   */
  alert(message: string, title?: string): Promise<void> {
    return new Promise((resolve) => {
      const id = this.show(message, title, {
        type: 'info',
        duration: 0,
        dismissible: false,
        actions: [
          {
            label: 'Aceptar',
            action: () => {
              this.dismiss(id);
              resolve();
            },
            class: 'btn-primary'
          }
        ]
      });
    });
  }

  /**
   * Mostrar notificación de progreso
   */
  progress(message: string, title?: string): { id: string; update: (progress: number) => void; complete: () => void } {
    const id = this.show(message, title, {
      type: 'info',
      duration: 0,
      dismissible: false,
      actions: []
    });

    const notification = this.notifications.get(id);
    if (notification) {
      const progressBar = this.renderer.createElement('div');
      this.renderer.addClass(progressBar, 'notification-progress');
      this.renderer.appendChild(notification, progressBar);

      const update = (progress: number) => {
        this.renderer.setStyle(progressBar, 'width', `${Math.min(100, Math.max(0, progress))}%`);
      };

      const complete = () => {
        this.dismiss(id);
      };

      return { id, update, complete };
    }

    return { id, update: () => {}, complete: () => {} };
  }

  private createNotificationContainer(): void {
    this.notificationContainer = this.renderer.createElement('div');
    this.renderer.addClass(this.notificationContainer, 'notification-container');
    this.renderer.addClass(this.notificationContainer, 'top-right');
    this.renderer.appendChild(this.document.body, this.notificationContainer);

    // Agregar estilos CSS
    this.addNotificationStyles();
  }

  private createNotificationElement(
    id: string,
    message: string,
    title: string | undefined,
    type: string,
    icon: string | undefined,
    actions: NotificationAction[],
    dismissible: boolean
  ): HTMLElement {
    const notification = this.renderer.createElement('div');
    this.renderer.addClass(notification, 'notification');
    this.renderer.addClass(notification, `notification-${type}`);
    this.renderer.setAttribute(notification, 'id', id);

    // Icono
    if (icon) {
      const iconElement = this.renderer.createElement('i');
      this.renderer.addClass(iconElement, icon);
      this.renderer.addClass(iconElement, 'notification-icon');
      this.renderer.appendChild(notification, iconElement);
    }

    // Contenido
    const content = this.renderer.createElement('div');
    this.renderer.addClass(content, 'notification-content');

    if (title) {
      const titleElement = this.renderer.createElement('div');
      this.renderer.addClass(titleElement, 'notification-title');
      titleElement.textContent = title;
      this.renderer.appendChild(content, titleElement);
    }

    const messageElement = this.renderer.createElement('div');
    this.renderer.addClass(messageElement, 'notification-message');
    messageElement.textContent = message;
    this.renderer.appendChild(content, messageElement);

    this.renderer.appendChild(notification, content);

    // Botón de cerrar
    if (dismissible) {
      const closeButton = this.renderer.createElement('button');
      this.renderer.addClass(closeButton, 'notification-close');
      this.renderer.setAttribute(closeButton, 'type', 'button');
      this.renderer.setAttribute(closeButton, 'aria-label', 'Cerrar');
      closeButton.textContent = '×';
      
      this.renderer.listen(closeButton, 'click', () => {
        this.dismiss(id);
      });
      
      this.renderer.appendChild(notification, closeButton);
    }

    // Acciones
    if (actions.length > 0) {
      const actionsContainer = this.renderer.createElement('div');
      this.renderer.addClass(actionsContainer, 'notification-actions');

      actions.forEach(action => {
        const actionButton = this.renderer.createElement('button');
        this.renderer.addClass(actionButton, 'btn');
        this.renderer.addClass(actionButton, action.class || 'btn-sm');
        actionButton.textContent = action.label;
        
        this.renderer.listen(actionButton, 'click', () => {
          action.action();
        });
        
        this.renderer.appendChild(actionsContainer, actionButton);
      });

      this.renderer.appendChild(notification, actionsContainer);
    }

    return notification;
  }

  private addNotificationStyles(): void {
    const style = this.renderer.createElement('style');
    this.renderer.setAttribute(style, 'type', 'text/css');
    
    const css = `
      .notification-container {
        position: fixed;
        z-index: 9999;
        pointer-events: none;
        max-width: 400px;
      }

      .notification-container.top-right {
        top: 20px;
        right: 20px;
      }

      .notification-container.top-left {
        top: 20px;
        left: 20px;
      }

      .notification-container.bottom-right {
        bottom: 20px;
        right: 20px;
      }

      .notification-container.bottom-left {
        bottom: 20px;
        left: 20px;
      }

      .notification-container.top-center {
        top: 20px;
        left: 50%;
        transform: translateX(-50%);
      }

      .notification-container.bottom-center {
        bottom: 20px;
        left: 50%;
        transform: translateX(-50%);
      }

      .notification {
        background: white;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        margin-bottom: 10px;
        padding: 16px;
        pointer-events: auto;
        position: relative;
        transform: translateX(100%);
        transition: transform 0.3s ease-in-out;
        border-left: 4px solid #ccc;
        min-width: 300px;
      }

      .notification.show {
        transform: translateX(0);
      }

      .notification-success {
        border-left-color: #28a745;
      }

      .notification-error {
        border-left-color: #dc3545;
      }

      .notification-warning {
        border-left-color: #ffc107;
      }

      .notification-info {
        border-left-color: #17a2b8;
      }

      .notification-icon {
        float: left;
        margin-right: 12px;
        font-size: 20px;
        margin-top: 2px;
      }

      .notification-success .notification-icon {
        color: #28a745;
      }

      .notification-error .notification-icon {
        color: #dc3545;
      }

      .notification-warning .notification-icon {
        color: #ffc107;
      }

      .notification-info .notification-icon {
        color: #17a2b8;
      }

      .notification-content {
        overflow: hidden;
      }

      .notification-title {
        font-weight: 600;
        margin-bottom: 4px;
        color: #333;
      }

      .notification-message {
        color: #666;
        line-height: 1.4;
      }

      .notification-close {
        position: absolute;
        top: 8px;
        right: 8px;
        background: none;
        border: none;
        font-size: 20px;
        cursor: pointer;
        color: #999;
        padding: 0;
        width: 24px;
        height: 24px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        transition: background-color 0.2s;
      }

      .notification-close:hover {
        background-color: #f0f0f0;
        color: #666;
      }

      .notification-actions {
        margin-top: 12px;
        display: flex;
        gap: 8px;
        justify-content: flex-end;
      }

      .notification-actions .btn {
        padding: 6px 12px;
        font-size: 14px;
        border-radius: 4px;
        border: 1px solid transparent;
        cursor: pointer;
        transition: all 0.2s;
      }

      .notification-actions .btn-primary {
        background-color: #007bff;
        border-color: #007bff;
        color: white;
      }

      .notification-actions .btn-primary:hover {
        background-color: #0056b3;
        border-color: #0056b3;
      }

      .notification-actions .btn-secondary {
        background-color: #6c757d;
        border-color: #6c757d;
        color: white;
      }

      .notification-actions .btn-secondary:hover {
        background-color: #545b62;
        border-color: #545b62;
      }

      .notification-progress {
        height: 4px;
        background-color: #e9ecef;
        border-radius: 2px;
        margin-top: 12px;
        overflow: hidden;
      }

      .notification-progress::before {
        content: '';
        display: block;
        height: 100%;
        background-color: #007bff;
        width: 0%;
        transition: width 0.3s ease;
      }

      @media (max-width: 768px) {
        .notification-container {
          max-width: calc(100vw - 40px);
        }

        .notification {
          min-width: auto;
          margin-left: 10px;
          margin-right: 10px;
        }
      }
    `;
    
    this.renderer.appendChild(style, this.renderer.createText(css));
    this.renderer.appendChild(this.document.head, style);
  }
}
