import { HttpInterceptorFn } from '@angular/common/http';

export const httpClientInterceptor: HttpInterceptorFn = (req, next) => {
  // Agregar headers comunes
  const modifiedReq = req.clone({
    setHeaders: {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    }
  });

  return next(modifiedReq);
};
