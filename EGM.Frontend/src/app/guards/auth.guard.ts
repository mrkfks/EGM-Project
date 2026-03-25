import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { NotificationService } from '../services/notification.service';

/**
 * JWT token varlığını kontrol eden route guard.
 * Token yoksa kullanıcıyı /login sayfasına yönlendirir.
 */
export const authGuard: CanActivateFn = (_route, _state) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const notifSvc = inject(NotificationService);

  if (!isPlatformBrowser(platformId)) {
    // SSR sırasında guard geçilsin (sunucu tarafı rendering)
    return true;
  }

  const token = localStorage.getItem('token');
  if (token) {
      notifSvc.connect();
    return true;
  }

  return router.createUrlTree(['/login']);
};
