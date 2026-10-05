import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from './models';

export function readApiError(error: unknown, fallback = 'No se pudo completar la operación.'): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as ApiError | string | null;
    if (typeof body === 'string' && body.trim()) {
      return body;
    }
    if (body && typeof body === 'object' && body.message) {
      return body.message;
    }
  }

  return fallback;
}
