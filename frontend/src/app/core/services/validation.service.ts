import { Injectable } from '@angular/core';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class ValidationService {

  /**
   * Validador para números de factura con formato FAC-XXX-YYYY
   */
  static invoiceNumberFormat(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const pattern = /^FAC-\d{3}-\d{4}$/;
      if (!pattern.test(control.value)) {
        return { 
          invoiceNumberFormat: {
            message: 'Formato inválido. Use: FAC-XXX-YYYY',
            example: 'FAC-001-2024'
          }
        };
      }
      return null;
    };
  }

  /**
   * Validador para fechas futuras
   */
  static noFutureDate(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const selectedDate = new Date(control.value);
      const today = new Date();
      today.setHours(23, 59, 59, 999);
      
      if (selectedDate > today) {
        return { 
          futureDate: {
            message: 'No se pueden crear facturas con fecha futura',
            selectedDate: control.value,
            today: today.toISOString().split('T')[0]
          }
        };
      }
      return null;
    };
  }

  /**
   * Validador para fechas muy antiguas
   */
  static noOldDate(minYear: number = 2020): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const selectedDate = new Date(control.value);
      const minDate = new Date(`${minYear}-01-01`);
      
      if (selectedDate < minDate) {
        return { 
          oldDate: {
            message: `La fecha no puede ser anterior a ${minYear}`,
            selectedDate: control.value,
            minDate: minDate.toISOString().split('T')[0]
          }
        };
      }
      return null;
    };
  }

  /**
   * Validador para nombres de cliente (solo letras y espacios)
   */
  static clientNameFormat(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const pattern = /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/;
      if (!pattern.test(control.value)) {
        return { 
          clientNameFormat: {
            message: 'Solo se permiten letras y espacios',
            invalidChars: control.value.replace(/[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, '')
          }
        };
      }
      return null;
    };
  }

  /**
   * Validador para cantidades positivas con máximo
   */
  static positiveQuantity(maxValue: number = 9999): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const value = Number(control.value);
      
      if (isNaN(value) || value <= 0) {
        return { 
          positiveQuantity: {
            message: 'La cantidad debe ser mayor a 0',
            value: control.value
          }
        };
      }
      
      if (value > maxValue) {
        return { 
          maxQuantity: {
            message: `La cantidad no puede ser mayor a ${maxValue}`,
            value: control.value,
            maxValue: maxValue
          }
        };
      }
      
      return null;
    };
  }

  /**
   * Validador para precios positivos
   */
  static positivePrice(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const value = Number(control.value);
      
      if (isNaN(value) || value <= 0) {
        return { 
          positivePrice: {
            message: 'El precio debe ser mayor a 0',
            value: control.value
          }
        };
      }
      
      return null;
    };
  }

  /**
   * Validador para emails
   */
  static emailFormat(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const pattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
      if (!pattern.test(control.value)) {
        return { 
          emailFormat: {
            message: 'Formato de email inválido',
            value: control.value
          }
        };
      }
      return null;
    };
  }

  /**
   * Validador para contraseñas seguras
   */
  static strongPassword(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const hasUpperCase = /[A-Z]/.test(control.value);
      const hasLowerCase = /[a-z]/.test(control.value);
      const hasNumbers = /\d/.test(control.value);
      const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(control.value);
      const isLongEnough = control.value.length >= 8;
      
      const errors: string[] = [];
      
      if (!hasUpperCase) errors.push('mayúscula');
      if (!hasLowerCase) errors.push('minúscula');
      if (!hasNumbers) errors.push('número');
      if (!hasSpecialChar) errors.push('carácter especial');
      if (!isLongEnough) errors.push('mínimo 8 caracteres');
      
      if (errors.length > 0) {
        return { 
          strongPassword: {
            message: `La contraseña debe contener: ${errors.join(', ')}`,
            missing: errors
          }
        };
      }
      
      return null;
    };
  }

  /**
   * Validador para confirmar contraseña
   */
  static confirmPassword(passwordControl: AbstractControl): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      if (control.value !== passwordControl.value) {
        return { 
          confirmPassword: {
            message: 'Las contraseñas no coinciden',
            password: passwordControl.value,
            confirmPassword: control.value
          }
        };
      }
      
      return null;
    };
  }

  /**
   * Validador para números de teléfono colombianos
   */
  static colombianPhone(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const pattern = /^(\+57|57)?[1-9][0-9]{9}$/;
      if (!pattern.test(control.value.replace(/\s/g, ''))) {
        return { 
          colombianPhone: {
            message: 'Formato de teléfono inválido. Use: +57 300 123 4567 o 300 123 4567',
            value: control.value
          }
        };
      }
      return null;
    };
  }

  /**
   * Validador para números de identificación colombianos
   */
  static colombianId(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const value = control.value.toString().replace(/[.-]/g, '');
      
      if (value.length < 8 || value.length > 10) {
        return { 
          colombianId: {
            message: 'El número de identificación debe tener entre 8 y 10 dígitos',
            value: control.value,
            length: value.length
          }
        };
      }
      
      if (!/^\d+$/.test(value)) {
        return { 
          colombianId: {
            message: 'El número de identificación solo debe contener dígitos',
            value: control.value
          }
        };
      }
      
      return null;
    };
  }

  /**
   * Validador para códigos postales colombianos
   */
  static colombianPostalCode(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const pattern = /^[0-9]{6}$/;
      if (!pattern.test(control.value)) {
        return { 
          colombianPostalCode: {
            message: 'El código postal debe tener 6 dígitos',
            value: control.value
          }
        };
      }
      
      return null;
    };
  }

  /**
   * Validador para URLs
   */
  static urlFormat(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      try {
        new URL(control.value);
        return null;
      } catch {
        return { 
          urlFormat: {
            message: 'Formato de URL inválido',
            value: control.value
          }
        };
      }
    };
  }

  /**
   * Validador para archivos de imagen
   */
  static imageFile(allowedTypes: string[] = ['image/jpeg', 'image/png', 'image/webp']): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const file = control.value as File;
      
      if (!allowedTypes.includes(file.type)) {
        return { 
          imageFile: {
            message: `Tipo de archivo no permitido. Tipos válidos: ${allowedTypes.join(', ')}`,
            fileType: file.type,
            allowedTypes: allowedTypes
          }
        };
      }
      
      // Validar tamaño máximo (5MB por defecto)
      const maxSize = 5 * 1024 * 1024; // 5MB
      if (file.size > maxSize) {
        return { 
          imageFile: {
            message: `El archivo es demasiado grande. Tamaño máximo: ${maxSize / (1024 * 1024)}MB`,
            fileSize: file.size,
            maxSize: maxSize
          }
        };
      }
      
      return null;
    };
  }

  /**
   * Obtener mensaje de error personalizado
   */
  static getErrorMessage(control: AbstractControl, fieldName: string): string {
    if (!control.errors) return '';
    
    const errors = control.errors;
    
    if (errors['required']) {
      return `${fieldName} es requerido`;
    }
    
    if (errors['minlength']) {
      return `${fieldName} debe tener al menos ${errors['minlength'].requiredLength} caracteres`;
    }
    
    if (errors['maxlength']) {
      return `${fieldName} no puede tener más de ${errors['maxlength'].requiredLength} caracteres`;
    }
    
    if (errors['pattern']) {
      return `${fieldName} tiene un formato inválido`;
    }
    
    if (errors['email']) {
      return `${fieldName} debe ser un email válido`;
    }
    
    if (errors['min']) {
      return `${fieldName} debe ser mayor o igual a ${errors['min'].min}`;
    }
    
    if (errors['max']) {
      return `${fieldName} debe ser menor o igual a ${errors['max'].max}`;
    }
    
    // Validadores personalizados
    if (errors['invoiceNumberFormat']) {
      return errors['invoiceNumberFormat'].message;
    }
    
    if (errors['futureDate']) {
      return errors['futureDate'].message;
    }
    
    if (errors['oldDate']) {
      return errors['oldDate'].message;
    }
    
    if (errors['clientNameFormat']) {
      return errors['clientNameFormat'].message;
    }
    
    if (errors['positiveQuantity']) {
      return errors['positiveQuantity'].message;
    }
    
    if (errors['positivePrice']) {
      return errors['positivePrice'].message;
    }
    
    if (errors['strongPassword']) {
      return errors['strongPassword'].message;
    }
    
    if (errors['confirmPassword']) {
      return errors['confirmPassword'].message;
    }
    
    if (errors['colombianPhone']) {
      return errors['colombianPhone'].message;
    }
    
    if (errors['colombianId']) {
      return errors['colombianId'].message;
    }
    
    if (errors['colombianPostalCode']) {
      return errors['colombianPostalCode'].message;
    }
    
    if (errors['urlFormat']) {
      return errors['urlFormat'].message;
    }
    
    if (errors['imageFile']) {
      return errors['imageFile'].message;
    }
    
    return `${fieldName} es inválido`;
  }

  /**
   * Verificar si un campo tiene errores
   */
  static hasError(control: AbstractControl): boolean {
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  /**
   * Verificar si un campo es válido
   */
  static isValid(control: AbstractControl): boolean {
    return !!(control && control.valid && (control.dirty || control.touched));
  }

  /**
   * Obtener el primer error de un control
   */
  static getFirstError(control: AbstractControl): string | null {
    if (!control.errors) return null;
    
    const errorKeys = Object.keys(control.errors);
    if (errorKeys.length > 0) {
      return errorKeys[0];
    }
    
    return null;
  }
}
