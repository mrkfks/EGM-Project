import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Province } from './pages/province/province';
import { Login } from './pages/login/login';
import { Olay } from './pages/olay/olay';
import { VIP } from './pages/vip/vip';
import { Organizasyon } from './pages/organizasyon/organizasyon';
import { Secim } from './pages/secim/secim';
import { Socialmedia } from './pages/socialmedia/socialmedia';
import { Olu } from './pages/olu/olu';
import { Supheli } from './pages/supheli/supheli';
import { Operasyonel } from './pages/operasyonel/operasyonel';
import { Sehit } from './pages/sehit/sehit';
import { Kullanicilar } from './pages/kullanicilar/kullanicilar';
import { Istatistikler } from './pages/istatistikler/istatistikler';
import { Bultenler } from './pages/bultenler/bultenler';
import { Raporlar } from './pages/raporlar/raporlar';
import { RaporKuruluslar } from './pages/rapor-kuruluslar/rapor-kuruluslar';
import { RaporKonular } from './pages/rapor-konular/rapor-konular';
import { KonuIslemleri } from './pages/konu-islemleri/konu-islemleri';
import { Konular } from './pages/konular/konular';
import { SokakOlayEkle } from './pages/sokak-olay-ekle/sokak-olay-ekle';
import { VeriYonetimi } from './pages/veri-yonetimi/veri-yonetimi';
import { FaaliyetYonetimiComponent } from './pages/faaliyet-yonetimi/faaliyet-yonetimi';
import { KurulusDetay } from './pages/kurulus-detay/kurulus-detay';
import { KonuDetay } from './pages/konu-detay/konu-detay';
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
    { path: 'login', component: Login },

    // ── Herkese açık (sadece giriş gerekli) ──────────────────────────────
    { path: 'home',              component: Home,          canActivate: [authGuard] },
    { path: 'olay',              component: Olay,          canActivate: [authGuard] },
    { path: 'istatistikler',     component: Istatistikler, canActivate: [authGuard] },
    { path: 'raporlar',          component: Bultenler,     canActivate: [authGuard] },
    { path: 'rapor-gunluk-bulten', component: Raporlar,   canActivate: [authGuard] },
    { path: 'rapor-kuruluslar',  component: RaporKuruluslar, canActivate: [authGuard] },
    { path: 'kurulus-detay/:id', component: KurulusDetay, canActivate: [authGuard] },
    { path: 'konu-detay/:id',    component: KonuDetay,    canActivate: [authGuard] },
    { path: 'rapor-konular',     component: RaporKonular, canActivate: [authGuard] },
    { path: 'konular',           component: Konular,       canActivate: [authGuard] },
    { path: 'province/:id',      component: Province,      canActivate: [authGuard] },

    // ── İl Personeli ve üzeri ────────────────────────────────────────────
    { path: 'sokak-olay-ekle', component: SokakOlayEkle, canActivate: [authGuard, roleGuard(IL_PERSONELI_VE_UZERI)] },
    { path: 'socialmedia',     component: Socialmedia,   canActivate: [authGuard, roleGuard(IL_PERSONELI_VE_UZERI)] },
    { path: 'secim',           component: Secim,         canActivate: [authGuard, roleGuard(IL_PERSONELI_VE_UZERI)] },

    // ── İl Yöneticisi ve üzeri ───────────────────────────────────────────
    { path: 'vip',             component: VIP,           canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'organizasyon',    component: Organizasyon,  canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'konu-islemleri',  component: KonuIslemleri, canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'faaliyet-yonetimi', component: FaaliyetYonetimiComponent, canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'kullanicilar',    component: Kullanicilar,  canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'olu',             component: Olu,           canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'supheli',         component: Supheli,       canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'operasyonel',     component: Operasyonel,   canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },
    { path: 'sehit',           component: Sehit,         canActivate: [authGuard, roleGuard(IL_YONETICISI_VE_UZERI)] },

    // ── Başkanlık Yöneticisi ve Yetkili ─────────────────────────────────
    { path: 'veri-yonetimi',   component: VeriYonetimi,  canActivate: [authGuard, roleGuard(HQ_YONETICISI_VE_UZERI)] },
];
