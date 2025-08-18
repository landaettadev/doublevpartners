import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UtilityService {

  /**
   * Formatear número como moneda colombiana
   */
  static formatCurrency(value: number, currency: string = 'COP'): string {
    if (value === null || value === undefined || isNaN(value)) {
      return '$0';
    }

    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value);
  }

  /**
   * Formatear fecha en formato colombiano
   */
  static formatDate(date: Date | string, format: 'short' | 'long' | 'time' = 'short'): string {
    if (!date) return '';

    const dateObj = typeof date === 'string' ? new Date(date) : date;
    
    if (isNaN(dateObj.getTime())) return '';

    switch (format) {
      case 'short':
        return dateObj.toLocaleDateString('es-CO', {
          day: '2-digit',
          month: '2-digit',
          year: 'numeric'
        });
      
      case 'long':
        return dateObj.toLocaleDateString('es-CO', {
          weekday: 'long',
          year: 'numeric',
          month: 'long',
          day: 'numeric'
        });
      
      case 'time':
        return dateObj.toLocaleDateString('es-CO', {
          day: '2-digit',
          month: '2-digit',
          year: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        });
      
      default:
        return dateObj.toLocaleDateString('es-CO');
    }
  }

  /**
   * Formatear número con separadores de miles
   */
  static formatNumber(value: number, decimals: number = 0): string {
    if (value === null || value === undefined || isNaN(value)) {
      return '0';
    }

    return new Intl.NumberFormat('es-CO', {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals
    }).format(value);
  }

  /**
   * Formatear porcentaje
   */
  static formatPercentage(value: number, decimals: number = 2): string {
    if (value === null || value === undefined || isNaN(value)) {
      return '0%';
    }

    return new Intl.NumberFormat('es-CO', {
      style: 'percent',
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals
    }).format(value / 100);
  }

  /**
   * Formatear tamaño de archivo
   */
  static formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  /**
   * Generar ID único
   */
  static generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }

  /**
   * Generar número de factura automático
   */
  static generateInvoiceNumber(prefix: string = 'FAC', sequence: number = 1): string {
    const year = new Date().getFullYear();
    const paddedSequence = sequence.toString().padStart(3, '0');
    return `${prefix}-${paddedSequence}-${year}`;
  }

  /**
   * Validar si una cadena es un email válido
   */
  static isValidEmail(email: string): boolean {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailRegex.test(email);
  }

  /**
   * Validar si una cadena es un número de teléfono válido
   */
  static isValidPhone(phone: string): boolean {
    const phoneRegex = /^(\+57|57)?[1-9][0-9]{9}$/;
    return phoneRegex.test(phone.replace(/\s/g, ''));
  }

  /**
   * Validar si una cadena es un número de identificación válido
   */
  static isValidId(id: string): boolean {
    const cleanId = id.replace(/[.-]/g, '');
    return /^\d{8,10}$/.test(cleanId);
  }

  /**
   * Capitalizar primera letra de cada palabra
   */
  static capitalizeWords(str: string): string {
    if (!str) return '';
    
    return str.replace(/\w\S*/g, (txt) => {
      return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    });
  }

  /**
   * Truncar texto a una longitud específica
   */
  static truncateText(text: string, maxLength: number, suffix: string = '...'): string {
    if (!text || text.length <= maxLength) return text;
    
    return text.substring(0, maxLength) + suffix;
  }

  /**
   * Generar slug a partir de un texto
   */
  static generateSlug(text: string): string {
    if (!text) return '';
    
    return text
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9\s-]/g, '')
      .replace(/\s+/g, '-')
      .replace(/-+/g, '-')
      .trim();
  }

  /**
   * Generar hash simple de una cadena
   */
  static simpleHash(str: string): number {
    let hash = 0;
    if (str.length === 0) return hash;
    
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32bit integer
    }
    
    return Math.abs(hash);
  }

  /**
   * Generar color aleatorio
   */
  static generateRandomColor(): string {
    const colors = [
      '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEAA7',
      '#DDA0DD', '#98D8C8', '#F7DC6F', '#BB8FCE', '#85C1E9',
      '#F8C471', '#82E0AA', '#F1948A', '#85C1E9', '#D7BDE2'
    ];
    
    return colors[Math.floor(Math.random() * colors.length)];
  }

  /**
   * Generar color basado en un texto
   */
  static generateColorFromText(text: string): string {
    const hash = this.simpleHash(text);
    const hue = hash % 360;
    const saturation = 70 + (hash % 20);
    const lightness = 50 + (hash % 20);
    
    return `hsl(${hue}, ${saturation}%, ${lightness}%)`;
  }

  /**
   * Calcular edad a partir de fecha de nacimiento
   */
  static calculateAge(birthDate: Date | string): number {
    const birth = typeof birthDate === 'string' ? new Date(birthDate) : birthDate;
    const today = new Date();
    
    let age = today.getFullYear() - birth.getFullYear();
    const monthDiff = today.getMonth() - birth.getMonth();
    
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
      age--;
    }
    
    return age;
  }

  /**
   * Calcular días entre dos fechas
   */
  static daysBetween(date1: Date | string, date2: Date | string): number {
    const d1 = typeof date1 === 'string' ? new Date(date1) : date1;
    const d2 = typeof date2 === 'string' ? new Date(date2) : date2;
    
    const timeDiff = Math.abs(d2.getTime() - d1.getTime());
    return Math.ceil(timeDiff / (1000 * 3600 * 24));
  }

  /**
   * Verificar si una fecha es hoy
   */
  static isToday(date: Date | string): boolean {
    const checkDate = typeof date === 'string' ? new Date(date) : date;
    const today = new Date();
    
    return checkDate.toDateString() === today.toDateString();
  }

  /**
   * Verificar si una fecha es ayer
   */
  static isYesterday(date: Date | string): boolean {
    const checkDate = typeof date === 'string' ? new Date(date) : date;
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    
    return checkDate.toDateString() === yesterday.toDateString();
  }

  /**
   * Verificar si una fecha es esta semana
   */
  static isThisWeek(date: Date | string): boolean {
    const checkDate = typeof date === 'string' ? new Date(date) : date;
    const today = new Date();
    const startOfWeek = new Date(today);
    startOfWeek.setDate(today.getDate() - today.getDay());
    startOfWeek.setHours(0, 0, 0, 0);
    
    const endOfWeek = new Date(startOfWeek);
    endOfWeek.setDate(startOfWeek.getDate() + 6);
    endOfWeek.setHours(23, 59, 59, 999);
    
    return checkDate >= startOfWeek && checkDate <= endOfWeek;
  }

  /**
   * Verificar si una fecha es este mes
   */
  static isThisMonth(date: Date | string): boolean {
    const checkDate = typeof date === 'string' ? new Date(date) : date;
    const today = new Date();
    
    return checkDate.getMonth() === today.getMonth() && 
           checkDate.getFullYear() === today.getFullYear();
  }

  /**
   * Obtener nombre del mes
   */
  static getMonthName(month: number, format: 'short' | 'long' = 'long'): string {
    const months = [
      'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
      'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
    ];
    
    const shortMonths = [
      'Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun',
      'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'
    ];
    
    return format === 'short' ? shortMonths[month] : months[month];
  }

  /**
   * Obtener nombre del día de la semana
   */
  static getDayName(day: number, format: 'short' | 'long' = 'long'): string {
    const days = [
      'Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'
    ];
    
    const shortDays = [
      'Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'
    ];
    
    return format === 'short' ? shortDays[day] : days[day];
  }

  /**
   * Obtener tiempo relativo (hace X tiempo)
   */
  static getRelativeTime(date: Date | string): string {
    const checkDate = typeof date === 'string' ? new Date(date) : date;
    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - checkDate.getTime()) / 1000);
    
    if (diffInSeconds < 60) {
      return 'hace un momento';
    }
    
    const diffInMinutes = Math.floor(diffInSeconds / 60);
    if (diffInMinutes < 60) {
      return `hace ${diffInMinutes} minuto${diffInMinutes > 1 ? 's' : ''}`;
    }
    
    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) {
      return `hace ${diffInHours} hora${diffInHours > 1 ? 's' : ''}`;
    }
    
    const diffInDays = Math.floor(diffInHours / 24);
    if (diffInDays < 7) {
      return `hace ${diffInDays} día${diffInDays > 1 ? 's' : ''}`;
    }
    
    const diffInWeeks = Math.floor(diffInDays / 7);
    if (diffInWeeks < 4) {
      return `hace ${diffInWeeks} semana${diffInWeeks > 1 ? 's' : ''}`;
    }
    
    const diffInMonths = Math.floor(diffInDays / 30);
    if (diffInMonths < 12) {
      return `hace ${diffInMonths} mes${diffInMonths > 1 ? 'es' : ''}`;
    }
    
    const diffInYears = Math.floor(diffInDays / 365);
    return `hace ${diffInYears} año${diffInYears > 1 ? 's' : ''}`;
  }

  /**
   * Copiar texto al portapapeles
   */
  static async copyToClipboard(text: string): Promise<boolean> {
    try {
      if (navigator.clipboard) {
        await navigator.clipboard.writeText(text);
        return true;
      } else {
        // Fallback para navegadores más antiguos
        const textArea = document.createElement('textarea');
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
        return true;
      }
    } catch (error) {
      console.error('Error al copiar al portapapeles:', error);
      return false;
    }
  }

  /**
   * Descargar archivo
   */
  static downloadFile(content: string, filename: string, contentType: string = 'text/plain'): void {
    const blob = new Blob([content], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Convertir archivo a base64
   */
  static fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => {
        const result = reader.result as string;
        resolve(result.split(',')[1]); // Remover el prefijo data:image/...;base64,
      };
      reader.onerror = error => reject(error);
    });
  }

  /**
   * Convertir base64 a archivo
   */
  static base64ToFile(base64: string, filename: string, contentType: string): File {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });
    
    return new File([blob], filename, { type: contentType });
  }

  /**
   * Generar contraseña aleatoria
   */
  static generateRandomPassword(length: number = 12): string {
    const charset = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*';
    let password = '';
    
    for (let i = 0; i < length; i++) {
      password += charset.charAt(Math.floor(Math.random() * charset.length));
    }
    
    return password;
  }

  /**
   * Verificar si el navegador soporta una característica
   */
  static isFeatureSupported(feature: string): boolean {
    switch (feature) {
      case 'clipboard':
        return !!navigator.clipboard;
      case 'geolocation':
        return !!navigator.geolocation;
      case 'localStorage':
        return !!window.localStorage;
      case 'sessionStorage':
        return !!window.sessionStorage;
      case 'webWorkers':
        return !!window.Worker;
      case 'serviceWorkers':
        return !!navigator.serviceWorker;
      case 'pushNotifications':
        return !!('PushManager' in window);
      case 'webGL':
        try {
          const canvas = document.createElement('canvas');
          return !!(window.WebGLRenderingContext && 
                   (canvas.getContext('webgl') || canvas.getContext('experimental-webgl')));
        } catch {
          return false;
        }
      default:
        return false;
    }
  }

  /**
   * Obtener información del navegador
   */
  static getBrowserInfo(): { name: string; version: string; os: string } {
    const userAgent = navigator.userAgent;
    let browserName = 'Unknown';
    let browserVersion = 'Unknown';
    let os = 'Unknown';

    // Detectar navegador
    if (userAgent.includes('Firefox')) {
      browserName = 'Firefox';
      browserVersion = userAgent.match(/Firefox\/(\d+)/)?.[1] || 'Unknown';
    } else if (userAgent.includes('Chrome')) {
      browserName = 'Chrome';
      browserVersion = userAgent.match(/Chrome\/(\d+)/)?.[1] || 'Unknown';
    } else if (userAgent.includes('Safari')) {
      browserName = 'Safari';
      browserVersion = userAgent.match(/Version\/(\d+)/)?.[1] || 'Unknown';
    } else if (userAgent.includes('Edge')) {
      browserName = 'Edge';
      browserVersion = userAgent.match(/Edge\/(\d+)/)?.[1] || 'Unknown';
    } else if (userAgent.includes('MSIE') || userAgent.includes('Trident')) {
      browserName = 'Internet Explorer';
      browserVersion = userAgent.match(/(?:MSIE |rv:)(\d+)/)?.[1] || 'Unknown';
    }

    // Detectar sistema operativo
    if (userAgent.includes('Windows')) {
      os = 'Windows';
    } else if (userAgent.includes('Mac')) {
      os = 'macOS';
    } else if (userAgent.includes('Linux')) {
      os = 'Linux';
    } else if (userAgent.includes('Android')) {
      os = 'Android';
    } else if (userAgent.includes('iOS')) {
      os = 'iOS';
    }

    return { name: browserName, version: browserVersion, os };
  }

  /**
   * Obtener información de la pantalla
   */
  static getScreenInfo(): { width: number; height: number; ratio: number; orientation: string } {
    const width = screen.width;
    const height = screen.height;
    const ratio = window.devicePixelRatio || 1;
    const orientation = width > height ? 'landscape' : 'portrait';
    
    return { width, height, ratio, orientation };
  }

  /**
   * Verificar si el dispositivo es móvil
   */
  static isMobileDevice(): boolean {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
  }

  /**
   * Verificar si el dispositivo es tablet
   */
  static isTabletDevice(): boolean {
    const userAgent = navigator.userAgent.toLowerCase();
    const isTablet = /(tablet|ipad|playbook|silk)|(android(?!.*mobi))/i.test(userAgent);
    const isMobile = /mobile|android|iphone|ipad|phone/i.test(userAgent);
    
    return isTablet && !isMobile;
  }

  /**
   * Verificar si el dispositivo es desktop
   */
  static isDesktopDevice(): boolean {
    return !this.isMobileDevice() && !this.isTabletDevice();
  }
}
