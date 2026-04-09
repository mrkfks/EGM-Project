import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  Component,
  OnInit,
  AfterViewInit,
  OnDestroy,
  ViewChild,
  ElementRef,
  HostListener,
} from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import {
  Chart,
  ChartConfiguration,
  registerables,
} from 'chart.js';
import ExcelJS from 'exceljs';
import { IL_LISTESI } from '../olay/olay';

Chart.register(...registerables);

interface OlayRow {
  id: string;
  olayTuru: string;
  tarih: string;
  baslangicSaati?: string;
  bitisSaati?: string;
  il: string;
  ilce?: string;
  hassasiyet: number;
  durum: number;
  katilimciSayisi: number;
  gozaltiSayisi?: number;
  sehitOluSayisi?: number;
  organizatorAd?: string;
  konuAd?: string;
}

@Component({
  selector: 'app-istatistikler',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './istatistikler.html',
  styleUrls: ['./istatistikler.css'],
})
export class Istatistikler implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('durumChart') durumChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('turChart') turChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('hassasiyetChart') hassasiyetChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('ilChart') ilChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('trendChart') trendChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('ozelChart') ozelChartRef!: ElementRef<HTMLCanvasElement>;

  private apiBase = 'http://localhost:5117/api';
  private charts: Chart[] = [];
  private ozelGrafikInstance: Chart | null = null;

  tumVeri: OlayRow[] = [];
  filtreliVeri: OlayRow[] = [];
  yukleniyor = false;
  hata: string | null = null;

  filterTarihBaslangic = '';
  filterTarihBitis = '';
  filterDatetimeBaslangic = '';
  filterDatetimeBitis = '';
  filterIl = '';
  filterKonuId = '';
  filterOrganizatorId = '';
  filterOlayTuru = '';
  filterDurum = '';
  filterGozalti = '';
  filterSehitOlu = '';

  organizatorler: { id: string; ad: string }[] = [];
  konular: { id: string; ad: string }[] = [];
  ilListesi = IL_LISTESI;

  toplamOlay = 0;
  gerceklesti = 0;
  planlandiSayisi = 0;
  iptal = 0;
  ortalamaRisk = 0;

  olayTurleri: string[] = [];
  viewReady = false;

  secilenGrafikTuru = 'bar';
  secilenEksen = 'olayTuru';
  ozelGrafikVar = false;
  indirMenuAcik = false;

  @HostListener('document:click', ['$event'])
  onDocumentClick(e: MouseEvent): void {
    const target = e.target as HTMLElement;
    if (!target.closest('.indir-dropdown-wrap')) {
      this.indirMenuAcik = false;
    }
  }

  indirMenuToggle(e: MouseEvent): void {
    e.stopPropagation();
    this.indirMenuAcik = !this.indirMenuAcik;
  }

  indirExcel(): void {
    this.indirMenuAcik = false;
    this.excelExport();
  }

  indirGrafikPng(): void {
    this.indirMenuAcik = false;
    this.tumGrafikleriIndir();
  }

  readonly grafikTurleri = [
    { deger: 'line',    etiket: 'Çizgi' },
    { deger: 'bar',     etiket: 'Sütun / Çubuk' },
    { deger: 'pie',     etiket: 'Pasta' },
    { deger: 'scatter', etiket: 'Dağılım (XY)' },
    { deger: 'area',    etiket: 'Alan' },
    { deger: 'radar',   etiket: 'Radar' },
  ];

  readonly eksenSecenekleri = [
    { deger: 'olayTuru',   etiket: 'Olay Türü' },
    { deger: 'il',         etiket: 'İl' },
    { deger: 'durum',      etiket: 'Durum' },
    { deger: 'hassasiyet', etiket: 'Hassasiyet' },
    { deger: 'aylik',      etiket: 'Aylık Trend' },
  ];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.lookupYukle();
    this.veriCek();
  }

  ngAfterViewInit(): void {
    this.viewReady = true;
    if (this.filtreliVeri.length > 0) {
      this.grafikleriOlustur();
    }
  }

  ngOnDestroy(): void {
    this.charts.forEach(c => c.destroy());
    this.ozelGrafikInstance?.destroy();
  }

  private lookupYukle(): void {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
    this.http.get<any[]>(`${this.apiBase}/organizator`, { headers }).subscribe({
      next: res => this.organizatorler = res.map(o => ({ id: o.id, ad: o.ad })),
      error: () => {}
    });
    this.http.get<any[]>(`${this.apiBase}/organizator/konu`, { headers }).subscribe({
      next: res => this.konular = res.map(k => ({ id: k.id, ad: k.ad })),
      error: () => {}
    });
  }

  veriCek(): void {
    this.yukleniyor = true;
    this.hata = null;
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    let params = `sayfa=1&sayfaBoyutu=1000`;
    if (this.filterDatetimeBaslangic) params += `&tarihBaslangic=${encodeURIComponent(this.filterDatetimeBaslangic)}`;
    if (this.filterDatetimeBitis)     params += `&tarihBitis=${encodeURIComponent(this.filterDatetimeBitis)}`;
    if (this.filterIl)                params += `&il=${encodeURIComponent(this.filterIl)}`;
    if (this.filterKonuId)            params += `&konuId=${encodeURIComponent(this.filterKonuId)}`;
    if (this.filterOrganizatorId)     params += `&organizatorId=${encodeURIComponent(this.filterOrganizatorId)}`;
    if (this.filterDurum !== '')      params += `&durum=${this.filterDurum}`;

    this.http
      .get<any>(`${this.apiBase}/olay?${params}`, { headers })
      .subscribe({
        next: (res) => {
          this.tumVeri = res.items ?? [];
          this.olayTurleri = [...new Set<string>(this.tumVeri.map((o: OlayRow) => o.olayTuru))].filter(Boolean).sort();
          this.filtrele();
          this.yukleniyor = false;
        },
        error: () => {
          this.hata = 'Veriler yuklenirken hata olustu.';
          this.yukleniyor = false;
        },
      });
  }

  filtrele(): void {
    let veri = [...this.tumVeri];
    if (this.filterOlayTuru) veri = veri.filter(o => o.olayTuru === this.filterOlayTuru);
    if (this.filterGozalti === 'var')  veri = veri.filter(o => (o.gozaltiSayisi ?? 0) > 0);
    if (this.filterGozalti === 'yok')  veri = veri.filter(o => (o.gozaltiSayisi ?? 0) === 0);
    if (this.filterSehitOlu === 'var') veri = veri.filter(o => (o.sehitOluSayisi ?? 0) > 0);
    if (this.filterSehitOlu === 'yok') veri = veri.filter(o => (o.sehitOluSayisi ?? 0) === 0);
    this.filtreliVeri = veri;
    this.ozetHesapla();
    if (this.viewReady) this.grafikleriOlustur();
    if (this.viewReady && this.ozelGrafikVar) this.ozelGrafikOlustur();
  }

  filtreUygula(): void {
    this.veriCek();
  }

  filtreTemizle(): void {
    this.filterDatetimeBaslangic = '';
    this.filterDatetimeBitis = '';
    this.filterIl = '';
    this.filterKonuId = '';
    this.filterOrganizatorId = '';
    this.filterOlayTuru = '';
    this.filterDurum = '';
    this.filterGozalti = '';
    this.filterSehitOlu = '';
    this.veriCek();
  }

  private ozetHesapla(): void {
    const v = this.filtreliVeri;
    this.toplamOlay = v.length;
    this.gerceklesti = v.filter(o => o.durum === 1).length;
    this.planlandiSayisi = v.filter(o => o.durum === 0).length;
    this.iptal = v.filter(o => o.durum === 2).length;
  }

  private grafikleriOlustur(): void {
    this.charts.forEach(c => c.destroy());
    this.charts = [];
    this.charts.push(this.olusturDurumChart());
    this.charts.push(this.olusturTurChart());
    this.charts.push(this.olusturHassasiyetChart());
    this.charts.push(this.olusturIlChart());
    this.charts.push(this.olusturTrendChart());
  }

  private olusturDurumChart(): Chart {
    const sayac = [0, 0, 0];
    this.filtreliVeri.forEach(o => {
      if (o.durum === 0) sayac[0]++;
      else if (o.durum === 1) sayac[1]++;
      else if (o.durum === 2) sayac[2]++;
    });
    return new Chart(this.durumChartRef.nativeElement, {
      type: 'doughnut',
      data: {
        labels: ['Planlanmis', 'Gerceklesti', 'Iptal'],
        datasets: [{
          data: sayac,
          backgroundColor: ['#3b82f6', '#10b981', '#ef4444'],
          borderWidth: 2,
          borderColor: '#fff',
        }],
      },
      options: {
        responsive: true,
        plugins: { legend: { position: 'bottom' }, title: { display: false } },
      },
    } as ChartConfiguration);
  }

  private olusturTurChart(): Chart {
    const sayac: Record<string, number> = {};
    this.filtreliVeri.forEach(o => {
      const tur = o.olayTuru || 'Bilinmiyor';
      sayac[tur] = (sayac[tur] ?? 0) + 1;
    });
    const labels = Object.keys(sayac).sort((a, b) => sayac[b] - sayac[a]);
    return new Chart(this.turChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Olay Sayisi',
          data: labels.map(l => sayac[l]),
          backgroundColor: 'rgba(59,130,246,0.75)',
          borderColor: '#3b82f6',
          borderWidth: 1,
          borderRadius: 6,
        }],
      },
      options: {
        responsive: true,
        indexAxis: 'y' as const,
        plugins: { legend: { display: false } },
        scales: { x: { beginAtZero: true, ticks: { precision: 0 } } },
      },
    } as ChartConfiguration);
  }

  private olusturHassasiyetChart(): Chart {
    const labeller = ['Dusuk', 'Orta', 'Yuksek', 'Kritik'];
    const renkler = ['#10b981', '#f59e0b', '#f97316', '#ef4444'];
    const sayac = [0, 0, 0, 0];
    this.filtreliVeri.forEach(o => {
      if (o.hassasiyet >= 0 && o.hassasiyet <= 3) sayac[o.hassasiyet]++;
    });
    return new Chart(this.hassasiyetChartRef.nativeElement, {
      type: 'pie',
      data: {
        labels: labeller,
        datasets: [{
          data: sayac,
          backgroundColor: renkler,
          borderWidth: 2,
          borderColor: '#fff',
        }],
      },
      options: {
        responsive: true,
        plugins: { legend: { position: 'bottom' } },
      },
    } as ChartConfiguration);
  }

  private olusturIlChart(): Chart {
    const sayac: Record<string, number> = {};
    this.filtreliVeri.forEach(o => {
      const il = o.il || 'Bilinmiyor';
      sayac[il] = (sayac[il] ?? 0) + 1;
    });
    const sirali = Object.entries(sayac)
      .sort((a, b) => b[1] - a[1])
      .slice(0, 15);
    return new Chart(this.ilChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels: sirali.map(x => x[0]),
        datasets: [{
          label: 'Olay Sayisi',
          data: sirali.map(x => x[1]),
          backgroundColor: 'rgba(139,92,246,0.75)',
          borderColor: '#8b5cf6',
          borderWidth: 1,
          borderRadius: 6,
        }],
      },
      options: {
        responsive: true,
        indexAxis: 'y' as const,
        plugins: { legend: { display: false } },
        scales: { x: { beginAtZero: true, ticks: { precision: 0 } } },
      },
    } as ChartConfiguration);
  }

  private olusturTrendChart(): Chart {
    const aylik: Record<string, number> = {};
    this.filtreliVeri.forEach(o => {
      const d = new Date(o.tarih);
      if (!isNaN(d.getTime())) {
        const anahtar = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
        aylik[anahtar] = (aylik[anahtar] ?? 0) + 1;
      }
    });
    const labels = Object.keys(aylik).sort();
    return new Chart(this.trendChartRef.nativeElement, {
      type: 'line',
      data: {
        labels: labels.map(l => {
          const [yil, ay] = l.split('-');
          return `${ay}/${yil}`;
        }),
        datasets: [{
          label: 'Olay Sayisi',
          data: labels.map(l => aylik[l]),
          fill: true,
          backgroundColor: 'rgba(16,185,129,0.12)',
          borderColor: '#10b981',
          borderWidth: 2,
          pointRadius: 4,
          tension: 0.3,
        }],
      },
      options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
      },
    } as ChartConfiguration);
  }

  durumEtiket(durum: number): string {
    return ['Planlanmis', 'Gerceklesti', 'Iptal'][durum] ?? 'Bilinmiyor';
  }

  hassasiyetEtiket(h: number): string {
    return ['Dusuk', 'Orta', 'Yuksek', 'Kritik'][h] ?? 'Bilinmiyor';
  }

  async excelExport(): Promise<void> {
    const wb = new ExcelJS.Workbook();

    // Olaylar sayfası
    const ws = wb.addWorksheet('Olaylar');
    ws.columns = [
      { header: 'Tur', key: 'Tur' },
      { header: 'Tarih', key: 'Tarih' },
      { header: 'Baslangic Saati', key: 'Baslangic Saati' },
      { header: 'Bitis Saati', key: 'Bitis Saati' },
      { header: 'Il', key: 'Il' },
      { header: 'Ilce', key: 'Ilce' },
      { header: 'Hassasiyet', key: 'Hassasiyet' },
      { header: 'Durum', key: 'Durum' },
      { header: 'Katilimci Sayisi', key: 'Katilimci Sayisi' },
      { header: 'Organizator', key: 'Organizator' },
      { header: 'Konu', key: 'Konu' },
    ];
    const satirlar = this.filtreliVeri.map(o => ({
      'Tur': o.olayTuru,
      'Tarih': o.tarih ? new Date(o.tarih).toLocaleDateString('tr-TR') : '',
      'Baslangic Saati': o.baslangicSaati ?? '',
      'Bitis Saati': o.bitisSaati ?? '',
      'Il': o.il,
      'Ilce': o.ilce ?? '',
      'Hassasiyet': this.hassasiyetEtiket(o.hassasiyet),
      'Durum': this.durumEtiket(o.durum),
      'Katilimci Sayisi': o.katilimciSayisi,
      'Organizator': o.organizatorAd ?? '',
      'Konu': o.konuAd ?? '',
    }));
    ws.addRows(satirlar);

    // Özet sayfası
    const wsOzet = wb.addWorksheet('Ozet');
    wsOzet.columns = [
      { header: 'Metrik', key: 'Metrik' },
      { header: 'Deger', key: 'Deger' },
    ];
    wsOzet.addRows([
      { 'Metrik': 'Toplam Olay', 'Deger': this.toplamOlay },
      { 'Metrik': 'Gerceklesti', 'Deger': this.gerceklesti },
      { 'Metrik': 'Planlanmis', 'Deger': this.planlandiSayisi },
      { 'Metrik': 'Iptal', 'Deger': this.iptal },
    ]);

    const tarih = new Date().toISOString().slice(0, 10);
    const buffer = await wb.xlsx.writeBuffer();
    const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `EGM_Istatistikler_${tarih}.xlsx`;
    a.click();
    URL.revokeObjectURL(url);
  }

  grafigIndir(chartRef: ElementRef<HTMLCanvasElement>, dosyaAdi: string): void {
    const url = chartRef.nativeElement.toDataURL('image/png');
    const a = document.createElement('a');
    a.href = url;
    a.download = dosyaAdi;
    a.click();
  }

  tumGrafikleriIndir(): void {
    const grafler = [
      { ref: this.durumChartRef, ad: 'durum-dagilimi.png' },
      { ref: this.turChartRef, ad: 'olay-turu-dagilimi.png' },
      { ref: this.hassasiyetChartRef, ad: 'hassasiyet-dagilimi.png' },
      { ref: this.ilChartRef, ad: 'il-dagilimi.png' },
      { ref: this.trendChartRef, ad: 'aylik-trend.png' },
    ];
    grafler.forEach(g => {
      setTimeout(() => this.grafigIndir(g.ref, g.ad), 100);
    });
  }

  ozelGrafikOlustur(): void {
    if (this.ozelGrafikInstance) {
      this.ozelGrafikInstance.destroy();
      this.ozelGrafikInstance = null;
    }
    this.ozelGrafikVar = false;

    const veri = this.filtreliVeri;
    if (!veri.length) return;

    const canvas = this.ozelChartRef.nativeElement;

    // ── Dağılım (Scatter) kaldırıldı ─────────────────
    if (this.secilenGrafikTuru === 'scatter') {
      this.ozelGrafikVar = false;
      return;
    }

    const sayac = this.grupla(veri, this.secilenEksen);
    const labels = Object.keys(sayac);
    const values = labels.map(l => sayac[l]);
    const renkler = this.renkleriUret(labels.length);

    // ── Radar ─────────────────────────────────────────
    if (this.secilenGrafikTuru === 'radar') {
      const topLabels = labels.slice(0, 10);
      const topValues = topLabels.map(l => sayac[l]);
      this.ozelGrafikInstance = new Chart(canvas, {
        type: 'radar',
        data: {
          labels: topLabels,
          datasets: [{
            label: 'Olay Sayısı',
            data: topValues,
            backgroundColor: 'rgba(99,102,241,0.2)',
            borderColor: '#6366f1',
            borderWidth: 2,
            pointBackgroundColor: '#6366f1',
            pointBorderColor: '#fff',
          }],
        },
        options: {
          responsive: true,
          plugins: { legend: { position: 'bottom', display: true } },
          scales: { r: { beginAtZero: true, ticks: { precision: 0 } } },
        },
      } as ChartConfiguration);
      this.ozelGrafikVar = true;
      return;
    }

    // ── Pasta ─────────────────────────────────────────
    if (this.secilenGrafikTuru === 'pie') {
      this.ozelGrafikInstance = new Chart(canvas, {
        type: 'pie',
        data: {
          labels,
          datasets: [{ data: values, backgroundColor: renkler, borderColor: '#fff', borderWidth: 2 }],
        },
        options: {
          responsive: true,
          plugins: { legend: { position: 'right', display: true } },
        },
      } as ChartConfiguration);
      this.ozelGrafikVar = true;
      return;
    }

    // ── Çizgi / Alan / Sütun ──────────────────────────
    const isLine = this.secilenGrafikTuru === 'line' || this.secilenGrafikTuru === 'area';
    const isArea = this.secilenGrafikTuru === 'area';
    const dataset: any = { label: 'Olay Sayısı', data: values };

    if (isLine) {
      dataset.fill = isArea;
      dataset.tension = 0.35;
      dataset.pointRadius = 4;
      dataset.backgroundColor = isArea ? 'rgba(16,185,129,0.15)' : 'rgba(59,130,246,0.15)';
      dataset.borderColor    = isArea ? '#10b981' : '#3b82f6';
      dataset.borderWidth = 2;
      dataset.pointBackgroundColor = isArea ? '#10b981' : '#3b82f6';
    } else {
      dataset.backgroundColor = renkler;
      dataset.borderColor = renkler;
      dataset.borderWidth = 1;
      dataset.borderRadius = 6;
    }

    this.ozelGrafikInstance = new Chart(canvas, {
      type: isLine ? 'line' : 'bar',
      data: { labels, datasets: [dataset] },
      options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
      },
    } as ChartConfiguration);
    this.ozelGrafikVar = true;
  }

  private grupla(veri: OlayRow[], eksen: string): Record<string, number> {
    const sayac: Record<string, number> = {};
    veri.forEach(o => {
      let anahtar = '';
      switch (eksen) {
        case 'olayTuru':   anahtar = o.olayTuru || 'Bilinmiyor'; break;
        case 'il':         anahtar = o.il || 'Bilinmiyor'; break;
        case 'durum':      anahtar = this.durumEtiket(o.durum); break;
        case 'hassasiyet': anahtar = this.hassasiyetEtiket(o.hassasiyet); break;
        case 'aylik': {
          const d = new Date(o.tarih);
          if (!isNaN(d.getTime())) {
            const ay = String(d.getMonth() + 1).padStart(2, '0');
            anahtar = `${ay}/${d.getFullYear()}`;
          } else { anahtar = 'Bilinmiyor'; }
          break;
        }
        default: anahtar = 'Bilinmiyor';
      }
      sayac[anahtar] = (sayac[anahtar] ?? 0) + 1;
    });
    const entries = Object.entries(sayac);
    if (eksen === 'aylik') {
      entries.sort((a, b) => {
        const [aAy, aYil] = a[0].split('/');
        const [bAy, bYil] = b[0].split('/');
        return new Date(+aYil, +aAy - 1).getTime() - new Date(+bYil, +bAy - 1).getTime();
      });
    } else {
      entries.sort((a, b) => b[1] - a[1]);
    }
    return Object.fromEntries(entries);
  }

  private renkleriUret(n: number): string[] {
    const p = ['#3b82f6','#10b981','#f59e0b','#ef4444','#8b5cf6',
               '#f97316','#06b6d4','#ec4899','#14b8a6','#a3e635'];
    return Array.from({ length: n }, (_, i) => p[i % p.length]);
  }
}