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
import { Raporlar } from './pages/raporlar/raporlar';
import { KonuIslemleri } from './pages/konu-islemleri/konu-islemleri';
import { SokakOlayEkle } from './pages/sokak-olay-ekle/sokak-olay-ekle';
import { VeriYonetimi } from './pages/veri-yonetimi/veri-yonetimi';
import { authGuard } from './guards/auth.guard';


export const routes: Routes = [
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: 'login', component: Login },
    { path: 'home',         component: Home,         canActivate: [authGuard] },
    { path: 'olay',             component: Olay,         canActivate: [authGuard] },
    { path: 'sokak-olay-ekle',  component: SokakOlayEkle, canActivate: [authGuard] },
    { path: 'vip',          component: VIP,           canActivate: [authGuard] },
    { path: 'organizasyon', component: Organizasyon,  canActivate: [authGuard] },
    { path: 'secim',        component: Secim,         canActivate: [authGuard] },
    { path: 'socialmedia',  component: Socialmedia,   canActivate: [authGuard] },
    { path: 'olu',          component: Olu,           canActivate: [authGuard] },
    { path: 'supheli',      component: Supheli,       canActivate: [authGuard] },
    { path: 'operasyonel',  component: Operasyonel,   canActivate: [authGuard] },
    { path: 'istatistikler', component: Istatistikler, canActivate: [authGuard] },
    { path: 'raporlar',      component: Raporlar,      canActivate: [authGuard] },
    { path: 'konu-islemleri', component: KonuIslemleri, canActivate: [authGuard] },
    { path: 'sehit',        component: Sehit,         canActivate: [authGuard] },
    { path: 'kullanicilar', component: Kullanicilar,  canActivate: [authGuard] },
    { path: 'veri-yonetimi', component: VeriYonetimi,  canActivate: [authGuard] },
    { path: 'province/:id', component: Province,      canActivate: [authGuard] },
];
