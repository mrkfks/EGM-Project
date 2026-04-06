import { HttpInterceptorFn } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

/**
 * Her HTTP isteğine Authorization: Bearer <token> başlığı ekler.
 * SSR sırasında (sunucu tarafı) localStorage erişilmez — istek olduğu gibi geçer.
 * Auth endpointlerine (login/register) token eklenmez.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const platformId = inject(PLATFORM_ID);

  if (!isPlatformBrowser(platformId)) {
    return next(req);
  }

  // Login ve register isteklerine token ekleme
  if (req.url.includes('/api/auth/')) {
    return next(req);
  }

  const token = localStorage.getItem('token');
  if (!token) {
    return next(req);
  }

  return next(
    req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    })
  );
};
