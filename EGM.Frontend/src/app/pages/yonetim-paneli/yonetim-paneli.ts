import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Kullanicilar } from '../kullanicilar/kullanicilar';
import { Organizasyon } from '../organizasyon/organizasyon';
import { KonuIslemleri } from '../konu-islemleri/konu-islemleri';
import { FaaliyetYonetimiComponent } from '../faaliyet-yonetimi/faaliyet-yonetimi';
import { VeriYonetimi } from '../veri-yonetimi/veri-yonetimi';

type Sekme = 'kullanici' | 'kurulus' | 'konu' | 'faaliyet' | 'veri';

const HQ_ROLLER = ['BaskanlikYoneticisi', 'Yetkili'];

@Component({
  selector: 'app-yonetim-paneli',
  standalone: true,
  imports: [CommonModule, Kullanicilar, Organizasyon, KonuIslemleri, FaaliyetYonetimiComponent, VeriYonetimi],
  templateUrl: './yonetim-paneli.html',
  styleUrls: ['./yonetim-paneli.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class YonetimPaneli implements OnInit {
  aktifSekme: Sekme = 'kullanici';
  veriSekmesiGorunsun = false;

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    try {
      const token = localStorage.getItem('token');
      if (token) {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const rol = payload['role']
          ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
          ?? '';
        this.veriSekmesiGorunsun = HQ_ROLLER.includes(rol);
      }
    } catch {}
    this.cdr.markForCheck();
  }

  sekmeSec(s: Sekme): void {
    this.aktifSekme = s;
    this.cdr.markForCheck();
  }
}
