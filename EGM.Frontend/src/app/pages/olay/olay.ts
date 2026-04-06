import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

// ── Sabitler ────────────────────────────────────────────────────────────────
const API = environment.apiUrl + '/api';

export const HASSASIYET = [
  { value: 0, label: 'Düşük',  color: '#27ae60', shadow: '0 0 12px rgba(39,174,96,0.45)' },
  { value: 1, label: 'Orta',   color: '#f39c12', shadow: '0 0 12px rgba(243,156,18,0.45)' },
  { value: 2, label: 'Yüksek', color: '#e74c3c', shadow: '0 0 12px rgba(231,76,60,0.45)'  },
  { value: 3, label: 'Kritik', color: '#8e44ad', shadow: '0 0 12px rgba(142,68,173,0.45)' }
];

export const DURUM = [
  { value: 0, label: 'Planlandı'   },
  { value: 1, label: 'Gerçekleşti' },
  { value: 2, label: 'İptal'       }
];

export const OLAY_TURLERI = [
  'Gösteri', 'Yürüyüş', 'Miting', 'Seçim Güvenliği', 'VIP Ziyaret',
  'Terör Olayı', 'Silahlı Çatışma', 'Provokasyon', 'Basın Açıklaması', 'Diğer'
];

export const IL_LISTESI: { id: number; ad: string }[] = [
  {id:1,ad:'Adana'},{id:2,ad:'Adıyaman'},{id:3,ad:'Afyonkarahisar'},{id:4,ad:'Ağrı'},
  {id:5,ad:'Amasya'},{id:6,ad:'Ankara'},{id:7,ad:'Antalya'},{id:8,ad:'Artvin'},
  {id:9,ad:'Aydın'},{id:10,ad:'Balıkesir'},{id:11,ad:'Bilecik'},{id:12,ad:'Bingöl'},
  {id:13,ad:'Bitlis'},{id:14,ad:'Bolu'},{id:15,ad:'Burdur'},{id:16,ad:'Bursa'},
  {id:17,ad:'Çanakkale'},{id:18,ad:'Çankırı'},{id:19,ad:'Çorum'},{id:20,ad:'Denizli'},
  {id:21,ad:'Diyarbakır'},{id:22,ad:'Edirne'},{id:23,ad:'Elazığ'},{id:24,ad:'Erzincan'},
  {id:25,ad:'Erzurum'},{id:26,ad:'Eskişehir'},{id:27,ad:'Gaziantep'},{id:28,ad:'Giresun'},
  {id:29,ad:'Gümüşhane'},{id:30,ad:'Hakkari'},{id:31,ad:'Hatay'},{id:32,ad:'Isparta'},
  {id:33,ad:'Mersin'},{id:34,ad:'İstanbul'},{id:35,ad:'İzmir'},{id:36,ad:'Kars'},
  {id:37,ad:'Kastamonu'},{id:38,ad:'Kayseri'},{id:39,ad:'Kırklareli'},{id:40,ad:'Kırşehir'},
  {id:41,ad:'Kocaeli'},{id:42,ad:'Konya'},{id:43,ad:'Kütahya'},{id:44,ad:'Malatya'},
  {id:45,ad:'Manisa'},{id:46,ad:'Kahramanmaraş'},{id:47,ad:'Mardin'},{id:48,ad:'Muğla'},
  {id:49,ad:'Muş'},{id:50,ad:'Nevşehir'},{id:51,ad:'Niğde'},{id:52,ad:'Ordu'},
  {id:53,ad:'Rize'},{id:54,ad:'Sakarya'},{id:55,ad:'Samsun'},{id:56,ad:'Siirt'},
  {id:57,ad:'Sinop'},{id:58,ad:'Sivas'},{id:59,ad:'Tekirdağ'},{id:60,ad:'Tokat'},
  {id:61,ad:'Trabzon'},{id:62,ad:'Tunceli'},{id:63,ad:'Şanlıurfa'},{id:64,ad:'Uşak'},
  {id:65,ad:'Van'},{id:66,ad:'Yozgat'},{id:67,ad:'Zonguldak'},{id:68,ad:'Aksaray'},
  {id:69,ad:'Bayburt'},{id:70,ad:'Karaman'},{id:71,ad:'Kırıkkale'},{id:72,ad:'Batman'},
  {id:73,ad:'Şırnak'},{id:74,ad:'Bartın'},{id:75,ad:'Ardahan'},{id:76,ad:'Iğdır'},
  {id:77,ad:'Yalova'},{id:78,ad:'Karabük'},{id:79,ad:'Kilis'},{id:80,ad:'Osmaniye'},
  {id:81,ad:'Düzce'}
];

// ── Arayüzler ────────────────────────────────────────────────────────────────
export interface OlayRow {
  id: string;
  olayTuru?: string;
  tarih: string;
  baslangicSaati?: string;
  bitisSaati?: string;
  il?: string;
  ilce?: string;
  mekan?: string;
  hassasiyet: number;
  durum: number;
  katilimciSayisi?: number;
  gozaltiSayisi?: number;
  sehitOluSayisi?: number;
  evrakNumarasi?: string;
  aciklama?: string;
  createdByUserId: string;
  cityId?: number;
  organizatorAd?: string;
  konuAd?: string;
}

export interface OrganizatorOption {
  id: string;
  ad: string;
}

export interface KonuOption {
  id: string;
  ad: string;
}

// ── Bileşen ────────────────────────────────────────────────────────────────
@Component({
  selector: 'app-olay',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './olay.html',
  styleUrl: './olay.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Olay implements OnInit {

  // ── Tablo durumu
  rows: OlayRow[]        = [];
  allRows: OlayRow[]     = [];
  filteredRows: OlayRow[] = [];
  totalCount             = 0;
  currentPage            = 1;
  pageSize               = 50;
  isLoading              = false;
  tableError: string | null = null;

  // ── Filtreler
  filterDurum            = '';
  filterIl               = '';
  filterKonuId           = '';
  filterOrganizatorId    = '';
  filterDatetimeBaslangic = '';
  filterDatetimeBitis     = '';
  filterGozalti           = '';
  filterSehitOlu          = '';

  // ── Lookup listeleri
  organizatorler: OrganizatorOption[] = [];
  konular: KonuOption[]               = [];

  // ── Token/JWT bilgileri
  tokenRole:   string | null = null;
  tokenCityId: number | null = null;
  readonly isBrowser: boolean;

  // ── Sabitler (template erişimi)
  readonly HASSASIYET = HASSASIYET;
  readonly DURUM      = DURUM;
  readonly IL_LISTESI = IL_LISTESI;

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    this.decodeToken();
    this.loadLookups();
    this.loadRows();
  }

  // ── JWT decode ────────────────────────────────────────────────────────
  private decodeToken(): void {
    if (!this.isBrowser) return;
    const token = localStorage.getItem('token');
    if (!token) return;
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return;
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(Array.prototype.map.call(atob(base64), (c: string) => {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      const payload = JSON.parse(json);
      this.tokenRole   = payload['role'] ?? null;
      const cid = payload['cityId'];
      this.tokenCityId = cid && cid !== '' ? parseInt(cid, 10) : null;
    } catch { /* sessiz başarısızlık */ }
  }

  // ── Getter kısayolları ────────────────────────────────────────────────
  get isCityScoped(): boolean {
    return ['IlPersoneli', 'IlYoneticisi'].includes(this.tokenRole ?? '');
  }
  get totalPages(): number { return Math.ceil(this.totalCount / this.pageSize) || 1; }

  // ── Lookup yükleme ────────────────────────────────────────────────────
  private loadLookups(): void {
    this.http.get<any[]>(`${API}/organizator`).subscribe({
      next: res => { this.organizatorler = res.map(o => ({ id: o.id, ad: o.ad })); this.cdr.markForCheck(); },
      error: () => {}
    });
    this.http.get<any[]>(`${API}/organizator/konu`).subscribe({
      next: res => { this.konular = res.map(k => ({ id: k.id, ad: k.ad })); this.cdr.markForCheck(); },
      error: () => {}
    });
  }

  // ── Tablo yükleme ─────────────────────────────────────────────────────
  loadRows(): void {
    this.isLoading  = true;
    this.tableError = null;
    let url = `${API}/olay?sayfa=1&sayfaBoyutu=${this.pageSize}`;
    if (this.filterDurum !== '') url += `&durum=${encodeURIComponent(this.filterDurum)}`;
    if (this.filterIl    !== '') url += `&il=${encodeURIComponent(this.filterIl)}`;
    if (this.filterKonuId !== '')         url += `&konuId=${encodeURIComponent(this.filterKonuId)}`;
    if (this.filterOrganizatorId !== '')  url += `&organizatorId=${encodeURIComponent(this.filterOrganizatorId)}`;
    if (this.filterDatetimeBaslangic !== '') url += `&tarihBaslangic=${encodeURIComponent(this.filterDatetimeBaslangic)}`;
    if (this.filterDatetimeBitis !== '')     url += `&tarihBitis=${encodeURIComponent(this.filterDatetimeBitis)}`;
    this.http.get<any>(url).subscribe({
      next: res => {
        this.allRows = (res.items ?? []).map((o: any): OlayRow => ({
          id:              o.id,
          olayTuru:        o.olayTuru,
          tarih:           o.tarih,
          baslangicSaati:  o.baslangicSaati ?? '',
          bitisSaati:      o.bitisSaati ?? '',
          il:              o.il,
          ilce:            o.ilce,
          mekan:           o.mekan,
          hassasiyet:      o.hassasiyet,
          durum:           o.durum,
          katilimciSayisi: o.katilimciSayisi,
          gozaltiSayisi:   o.gozaltiSayisi,
          sehitOluSayisi:  o.sehitOluSayisi,
          evrakNumarasi:   o.evrakNumarasi,
          aciklama:        o.aciklama,
          createdByUserId: o.createdByUserId ?? '',
          cityId:          o.cityId,
          organizatorAd:   o.organizatorAd ?? '',
          konuAd:          o.konuAd ?? '',
        }));
        this.applyClientFilters();
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.tableError = 'Veriler yüklenemedi. Lütfen tekrar deneyiniz.';
        this.isLoading  = false;
        this.cdr.markForCheck();
      }
    });
  }

  // ── İstemci tarafı filtreleme ─────────────────────────────────────────
  private applyClientFilters(): void {
    let filtered = [...this.allRows];
    if (this.filterGozalti === 'var')  filtered = filtered.filter(r => (r.gozaltiSayisi ?? 0) > 0);
    if (this.filterGozalti === 'yok')  filtered = filtered.filter(r => (r.gozaltiSayisi ?? 0) === 0);
    if (this.filterSehitOlu === 'var') filtered = filtered.filter(r => (r.sehitOluSayisi ?? 0) > 0);
    if (this.filterSehitOlu === 'yok') filtered = filtered.filter(r => (r.sehitOluSayisi ?? 0) === 0);
    this.filteredRows = filtered;
    this.totalCount   = filtered.length;
    const start       = (this.currentPage - 1) * this.pageSize;
    this.rows         = filtered.slice(start, start + this.pageSize);
  }

  // ── Özet istatistikler ────────────────────────────────────────────────
  private static readonly EYLEM_TURLERI    = ['Terör Olayı', 'Silahlı Çatışma', 'Provokasyon', 'Gösteri', 'Yürüyüş'];
  private static readonly ETKINLIK_TURLERI = ['Miting', 'Seçim Güvenliği', 'VIP Ziyaret', 'Basın Açıklaması', 'Diğer'];

  get statFarkliIlSayisi(): number {
    return new Set(this.filteredRows.map(r => r.il).filter(Boolean)).size;
  }
  get statEylemSayisi(): number {
    return this.filteredRows.filter(r => Olay.EYLEM_TURLERI.includes(r.olayTuru ?? '')).length;
  }
  get statEtkinlikSayisi(): number {
    return this.filteredRows.filter(r => Olay.ETKINLIK_TURLERI.includes(r.olayTuru ?? '')).length;
  }
  get statKatilimciSayisi(): number {
    return this.filteredRows.reduce((s, r) => s + (r.katilimciSayisi ?? 0), 0);
  }
  get statSehitOluSayisi(): number {
    return this.filteredRows.reduce((s, r) => s + (r.sehitOluSayisi ?? 0), 0);
  }

  // ── Filtre uygula ─────────────────────────────────────────────────────
  applyFilters(): void {
    this.currentPage = 1;
    this.loadRows();
  }

  clearFilters(): void {
    this.filterDurum            = '';
    this.filterIl               = '';
    this.filterKonuId           = '';
    this.filterOrganizatorId    = '';
    this.filterDatetimeBaslangic = '';
    this.filterDatetimeBitis     = '';
    this.filterGozalti           = '';
    this.filterSehitOlu          = '';
    this.currentPage            = 1;
    this.loadRows();
  }

  // ── Sayfalama ─────────────────────────────────────────────────────────
  navigatePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.applyClientFilters();
  }

  // ── Yardımcı metodlar ─────────────────────────────────────────────────
  hassasiyetLabel(v: number): string { return HASSASIYET.find(h => h.value === v)?.label ?? '-'; }
  hassasiyetColor(v: number): string { return HASSASIYET.find(h => h.value === v)?.color ?? '#aaa'; }
  durumLabel(v: number): string      { return DURUM.find(d => d.value === v)?.label ?? '-'; }

  durumBadgeClass(v: number): string {
    return ['badge-planned', 'badge-done', 'badge-cancelled'][v] ?? 'badge-planned';
  }

  formatTarih(t: string): string {
    if (!t) return '-';
    return new Date(t).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  trackById(_i: number, row: OlayRow): string { return row.id; }

  navigate(path: string): void { this.router.navigate([path]); }
}
