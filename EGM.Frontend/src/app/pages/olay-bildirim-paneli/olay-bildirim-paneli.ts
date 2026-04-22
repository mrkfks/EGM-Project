import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SokakOlayEkle } from '../sokak-olay-ekle/sokak-olay-ekle';
import { Socialmedia } from '../socialmedia/socialmedia';
import { Secim } from '../secim/secim';
import { VIP } from '../vip/vip';
import { YaklasanOlaylar } from '../yaklasan-olaylar/yaklasan-olaylar';

type Sekme = 'sokak' | 'yaklasan' | 'sosyal' | 'secim' | 'vip';

const IL_Y_VE_UZERI = ['IlYoneticisi', 'BaskanlikPersoneli', 'BaskanlikYoneticisi', 'Yetkili'];

@Component({
  selector: 'app-olay-bildirim-paneli',
  standalone: true,
  imports: [CommonModule, SokakOlayEkle, Socialmedia, Secim, VIP, YaklasanOlaylar],
  templateUrl: './olay-bildirim-paneli.html',
  styleUrls: ['./olay-bildirim-paneli.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OlayBildirimPaneli implements OnInit {
  aktifSekme: Sekme = 'sokak';
  vipGorunsun = false;

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    try {
      const token = localStorage.getItem('token');
      if (token) {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const rol = payload['role']
          ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
          ?? '';
        this.vipGorunsun = IL_Y_VE_UZERI.includes(rol);
      }
    } catch {}
    this.cdr.markForCheck();
  }

  sekmeSec(s: Sekme): void {
    this.aktifSekme = s;
    this.cdr.markForCheck();
  }
}
