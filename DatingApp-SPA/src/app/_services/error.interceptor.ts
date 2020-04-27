import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpResponse,
  HTTP_INTERCEPTORS,
} from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(
    req: import('@angular/common/http').HttpRequest<any>,
    next: import('@angular/common/http').HttpHandler
  ): import('rxjs').Observable<import('@angular/common/http').HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error) => {
        if (error.status === 401) {
          return throwError(error.statusText);
        }
        if (error instanceof HttpResponse) {
          const applicationError = error.headers.get('Application-Error');
          if (applicationError) {
            throwError(applicationError);
          }
        }

        const serverError = error.error;
        let modelStateErrors = '';
        if (serverError.errors && typeof serverError.errors === 'object') {
          // tslint:disable-next-line: forin
          for (const key in serverError.errors) {
            const currentServerError = serverError.errors[key];
            if (currentServerError) {
              modelStateErrors += currentServerError + '\n';
            }
          }
        }
        return throwError(modelStateErrors || serverError || 'Server Error');
      })
    );
  }
}

// tslint:disable-next-line: one-variable-per-declaration
export const ErrorInterceptorProvider = {
  provide: HTTP_INTERCEPTORS,
  useClass: ErrorInterceptor,
  multi: true,
};
