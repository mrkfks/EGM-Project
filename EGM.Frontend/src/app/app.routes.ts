import { Routes } from '@angular/router';
import { authGuard, roleGuard, ROLES } from './guards/auth.guard';

// Kısa yardımcılar
const IL_PERSONELI_VE_UZERI = [
  ROLES.IlPersoneli, ROLES.IlYoneticisi,
  ROLES.BaskanlikPersoneli, ROLES.BaskanlikYoneticisi, ROLES.Yetkili
];
const IL_YONETICISI_VE_UZERI = [
  ROLES.IlYoneticisi,
  ROLES.BaskanlikPersoneli, ROLES.BaskanlikYoneticisi, ROLES.Yetkili
];
const HQ_YONETICISI_VE_UZERI = [ROLES.BaskanlikYoneticisi, ROLES.Yetkili];

export const routes: Routes = [
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: 'login', loadComponent: () => import('./pages/login/login').then(m => m.Login) },

    // ── Herkese açık (sadece giriş gerekli) ──────────────────────────────
    { path: 'home',              loadComponent: () => import('./pages/home/home').then(m => m.Home),                         canActivate: [authGuard] },
    { path: 'olay',              loadComponent: () => import('./pages/olay/olay').then(m => m.Olay),                         canActivate: [authGuard] },
    { path: 'istatistikler',     loadComponent: () => import('./pages/istatistikler/istatistikler').then(m => m.Istatistikler), canActivate: [authGuard] },
    { path: 'raporlar',          loadComponent: () => import('./pages/bultenler/bultenler').then(m => m.Bultenler),          canActivate: [authGuard] },
    { path: 'rapor-gunluk-bulten', loadComponent: () => import('./pages/raporlar/raporlar').then(m => m.Raporlar),          canActivate: [authGuard] },
    { path: 'rapor-kuruluslar',  loadComponent: () => import('./pages/rapor-kuruluslar/rapor-kuruluslar').then(m => m.RaporKuruluslar), canActivate: [authGuard] },
    { path: 'kurulus-detay/:id', loadComponent: () => import('./pages/kurulus-detay/kurulus-detay').then(m => m.KurulusDetay), canActivate: [authGuard] },
    { path: 'konu-detay/:id',    loadComponent: () => import('./pages/konu-detay/konu-detay').then(m => m.KonuDetay),        canActivate: [authGuard] },
    { path: 'rapor-konular',     loadComponent: () => import('./pages/rapor-konular/rapor-konular').then(m => m.RaporKonular), canActivate: [authGuard] },
    { path: 'konular',           loadComponent: () => import('./pages/konular/konular').then(m => m.Konular),                canActivate: [authGuard] },
    { path: 'sokak-istatistik',        loadComponent: () => import('./pages/sokak-istatistik/sokak-istatistik').then(m => m.SokakIstatistik),              canActivate: [authGuard] },
    { path: 'sosyal-medya-istatistik', loadComponent: () => import('./pages/sosyal-medya-istatistik/sosyal-medya-istatistik').then(m => m.SosyalMedyaIstatistik), canActivate: [authGuard] },
    { path: 'secim-istatistik',        loadComponent: () => import('./pages/secim-istatistik/secim-istatistik').then(m => m.SecimIstatistik),              canActivate: [authGuard] },
    { path: 'vip-istatistik',          loadComponent: () => import('./pages/vip-istatistik/vip-istatistik').then(m => m.VipIstatistik),                    canActivate: [authGuard] },
    { path: 'province/:id',      loadComponent: () => import('./pages/province/province').then(m => m.Province),            canActivate: [authGuard] },

    // ── İl Personeli ve üzeri ────────────────────────────────────────────
    { path: 'sokak-olay-ekle', loadComponent: () => import('./pages/sokak-olay-ekle/sokak-olay-ekle').then(m => m.SokakOlayEkle), canActivate: [authGuard, roleGuard(IL_PERSONELI_VE_UZERI)] },
    { path: 'socialmedia',     loadComponent: () => import('./pages/socialmedia/socialmedia').then(m => m.Socialmedia),    canActivate: [authGuard, roleGuard(IL_PERSONELI_VE_UZERI)] },
    { path: 'secim',           loadComponent: () => import('./pages/secim/secim').then(m => m.Secim),                      canActivate: [authGuard, roleGuard(IL_PERSONELI_VE_UZERI)] },

    // ── İl Yöneticisi ve üzeri ───────────────────────────────────────────
    { path: 'vip',             loadComponent: () => import('./pages/vip/vip').then(m => m.VIP),                            canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'organizasyon',    loadComponent: () => import('./pages/organizasyon/organizasyon').then(m => m.Organizasyon),  canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'konu-islemleri',  loadComponent: () => import('./pages/konu-islemleri/konu-islemleri').then(m => m.KonuIslemleri), canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'faaliyet-yonetimi', loadComponent: () => import('./pages/faaliyet-yonetimi/faaliyet-yonetimi').then(m => m.FaaliyetYonetimiComponent), canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'kullanicilar',    loadComponent: () => import('./pages/kullanicilar/kullanicilar').then(m => m.Kullanicilar),  canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'olu',             loadComponent: () => import('./pages/olu/olu').then(m => m.Olu),                            canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'supheli',         loadComponent: () => import('./pages/supheli/supheli').then(m => m.Supheli),                canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'operasyonel',     loadComponent: () => import('./pages/operasyonel/operasyonel').then(m => m.Operasyonel),    canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'sehit',           loadComponent: () => import('./pages/sehit/sehit').then(m => m.Sehit),                      canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },

    // ── Başkanlık Yöneticisi ve Yetkili ─────────────────────────────────
    { path: 'veri-yonetimi',   loadComponent: () => import('./pages/veri-yonetimi/veri-yonetimi').then(m => m.VeriYonetimi), canActivate: [authGuard, roleGuard(HQ_YONETICISI_VE_UZERI)] },
];
