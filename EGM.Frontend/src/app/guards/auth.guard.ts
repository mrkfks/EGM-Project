import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { NotificationService } from '../services/notification.service';

/** Rol sabitleri — backend Roles.cs ile eşleşmeli */
export const ROLES = {
  Izleyici:           'Izleyici',
  IlPersoneli:        'IlPersoneli',
  IlYoneticisi:       'IlYoneticisi',
  BaskanlikPersoneli: 'BaskanlikPersoneli',
  BaskanlikYoneticisi:'BaskanlikYoneticisi',
  Yetkili:            'Yetkili',
} as const;

/** Token payload'ını çözen yardımcı */
function decodePayload(token: string): Record<string, any> | null {
  try {
    return JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
  } catch {
    return null;
  }
}

/** Token'ın süresi dolmuş mu? */
function isTokenExpired(token: string): boolean {
  const payload = decodePayload(token);
  if (!payload) return true;
  const exp = payload['exp'];
  if (!exp) return false;
  return Math.floor(Date.now() / 1000) >= exp;
}

/** Token'dan rolü çözen yardımcı */
function getRoleFromToken(token: string): string {
  const payload = decodePayload(token);
  if (!payload) return '';
  return payload['role']
    ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
    ?? '';
}

/**
 * JWT token varlığını kontrol eden route guard.
 * Token yoksa kullanıcıyı /login sayfasına yönlendirir.
 */
export const authGuard: CanActivateFn = (_route, _state) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const notifSvc = inject(NotificationService);

  if (!isPlatformBrowser(platformId)) {
    return true;
  }

  const token = localStorage.getItem('token');
  if (token && !isTokenExpired(token)) {
    notifSvc.connect();
    return true;
  }

  localStorage.removeItem('token');
  return router.createUrlTree(['/login']);
};

/**
 * Rol tabanlı route guard factory.
 * Kullanım: canActivate: [authGuard, roleGuard(['IlPersoneli','IlYoneticisi',...])]
 */
export const roleGuard = (allowedRoles: string[]): CanActivateFn => (_route, _state) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  if (!isPlatformBrowser(platformId)) return true;

  const token = localStorage.getItem('token');
  if (!token || isTokenExpired(token)) {
    localStorage.removeItem('token');
    return router.createUrlTree(['/login']);
  }

  const role = getRoleFromToken(token);
  if (allowedRoles.includes(role)) return true;

  return router.createUrlTree(['/ajanda']);
};
