import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';

interface IcmalVerisi {
  tur: string;
  eylemSayisi: string;
  katilimSayisi: string;
  gozaltiSayisi: string;
  oluSayisi: string;
}

interface GerceklesenDetay {
  sn: number;
  il: string;
  eylemEtkinlik: string;
  organizeEden: string;
  aciklama: string;
  katilimSayisi: string;
}

interface BeklenenDetay {
  sn: number;
  il: string;
  yer: string;
  eylemEtkinlik: string;
  organizeEden: string;
  aciklama: string;
}

interface OperasyonelDetay {
  sn: number;
  il: string;
  tarih: string;
  gozaltiSayisi: string;
  aciklama: string;
}

@Component({
  selector: 'app-raporlar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './raporlar.html',
  styleUrls: ['./raporlar.css'],
})
export class Raporlar {
  className = 'LEGAL GUNLUK BULTEN';
  tarih = '';
  sonrakiGunTarih = '';

  icmalVerileri: IcmalVerisi[] = [
    { tur: 'BASIN AÇIKLAMASI', eylemSayisi: '(-)', katilimSayisi: '(-)', gozaltiSayisi: '', oluSayisi: '' },
    { tur: 'EYLEM/ETKİNLİK', eylemSayisi: '(-)', katilimSayisi: '(-)', gozaltiSayisi: '', oluSayisi: '' }
  ];

  gerceklesenDetaylar: GerceklesenDetay[] = Array.from({ length: 12 }, (_, i) => ({
    sn: i + 1,
    il: '',
    eylemEtkinlik: '',
    organizeEden: '',
    aciklama: '',
    katilimSayisi: ''
  }));

  beklenenDetaylar: BeklenenDetay[] = Array.from({ length: 13 }, (_, i) => ({
    sn: i + 1,
    il: '',
    yer: '',
    eylemEtkinlik: '',
    organizeEden: '',
    aciklama: ''
  }));

  operasyonelFaaliyetler: OperasyonelDetay[] = Array.from({ length: 2 }, (_, i) => ({
    sn: i + 1,
    il: '',
    tarih: '',
    gozaltiSayisi: '',
    aciklama: ''
  }));

  ngOnInit(): void {
    this.setToday();
    this.fillSampleData();
  }

  printReport(): void {
    if (typeof window !== 'undefined') {
      window.print();
    }
  }

  setToday(): void {
    const now = new Date();
    this.tarih = this.toDisplayDate(now);
    this.sonrakiGunTarih = this.toDisplayDate(new Date(now.getTime() + 86_400_000));
  }

  trackBySn(_index: number, item: { sn: number }): number {
    return item.sn;
  }

  private fillSampleData(): void {
    this.gerceklesenDetaylar = [
      {
        sn: 1,
        il: 'Ankara',
        eylemEtkinlik: 'Basın Açıklaması',
        organizeEden: 'Sendika Temsilciliği',
        aciklama: 'Kamu çalışanlarının taleplerine ilişkin açıklama.',
        katilimSayisi: '120'
      },
      {
        sn: 2,
        il: 'İstanbul',
        eylemEtkinlik: 'Yürüyüş',
        organizeEden: 'STK Platformu',
        aciklama: 'İzinli yürüyüş etkinliği, olaysız sonlandı.',
        katilimSayisi: '450'
      },
      ...Array.from({ length: 10 }, (_, i) => ({
        sn: i + 3,
        il: '',
        eylemEtkinlik: '',
        organizeEden: '',
        aciklama: '',
        katilimSayisi: ''
      }))
    ];

    this.beklenenDetaylar = [
      {
        sn: 1,
        il: 'İzmir',
        yer: 'Konak Meydanı',
        eylemEtkinlik: 'Yürüyüş',
        organizeEden: 'Sivil Toplum Platformu',
        aciklama: 'Beklenen katılım orta seviyede.'
      },
      ...Array.from({ length: 12 }, (_, i) => ({
        sn: i + 2,
        il: '',
        yer: '',
        eylemEtkinlik: '',
        organizeEden: '',
        aciklama: ''
      }))
    ];

    this.operasyonelFaaliyetler = [
      {
        sn: 1,
        il: 'Ankara',
        tarih: this.tarih,
        gozaltiSayisi: '0',
        aciklama: 'Önemli bir gelişme bulunmamaktadır.'
      },
      {
        sn: 2,
        il: '',
        tarih: '',
        gozaltiSayisi: '',
        aciklama: ''
      }
    ];
  }

  private toDisplayDate(value: Date): string {
    return value.toLocaleDateString('tr-TR');
  }
}
