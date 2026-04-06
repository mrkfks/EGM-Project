import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RaporKuruluslar } from '../rapor-kuruluslar/rapor-kuruluslar';
import { Konular } from '../konular/konular';
import { SokakIstatistik } from '../sokak-istatistik/sokak-istatistik';
import { SosyalMedyaIstatistik } from '../sosyal-medya-istatistik/sosyal-medya-istatistik';
import { SecimIstatistik } from '../secim-istatistik/secim-istatistik';
import { VipIstatistik } from '../vip-istatistik/vip-istatistik';

type Sekme = 'kurulus' | 'konu' | 'sokak' | 'sosyal' | 'secim' | 'vip';

@Component({
  selector: 'app-istatistik-paneli',
  standalone: true,
  imports: [CommonModule, RaporKuruluslar, Konular, SokakIstatistik, SosyalMedyaIstatistik, SecimIstatistik, VipIstatistik],
  templateUrl: './istatistik-paneli.html',
  styleUrls: ['./istatistik-paneli.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IstatistikPaneli {
  aktifSekme: Sekme = 'kurulus';

  sekmeSec(s: Sekme): void {
    this.aktifSekme = s;
  }
}
