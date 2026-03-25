import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, Inject, OnInit, OnDestroy, PLATFORM_ID } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Subscription, Subject, debounceTime, distinctUntilChanged } from 'rxjs';

// ── Sabitler ────────────────────────────────────────────────────────────────
const API = 'http://localhost:5000/api';

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
  baslik?: string;
  olayTuru?: string;
  tarih: string;
  il?: string;
  ilce?: string;
  mekan?: string;
  hassasiyet: number;
  durum: number;
  riskPuani: number;
  katilimciSayisi?: number;
  aciklama?: string;
  createdByUserId: string;
  cityId?: number;
}

export interface OrganizatorOption {
  id: string;
  ad: string;
}

export interface KonuOption {
  id: string;
  ad: string;
}

export interface RiskPreview {
  riskPuaniRaw: number;
  riskPuaniNormalized: number;
  seviye: string;
}

// ── Bileşen ────────────────────────────────────────────────────────────────
@Component({
  selector: 'app-olay',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './olay.html',
  styleUrl: './olay.css',
})
export class Olay implements OnInit, OnDestroy {

  // ── Tablo durumu
  rows: OlayRow[] = [];
  totalCount   = 0;
  currentPage  = 1;
  pageSize     = 20;
  filterDurum  = '';
  isLoading    = false;
  tableError: string | null = null;

  // ── Form durumu
  showForm    = false;
  editId: string | null = null;
  formError: string | null = null;
  formSuccess: string | null = null;
  isSaving    = false;

  // ── Risk önizleme
  riskPreview: RiskPreview | null = null;
  isHighRisk   = false;
  private riskSubject = new Subject<void>();
  private riskSub?: Subscription;

  // ── Hassasiyet teması
  currentHassasiyetColor  = '#27ae60';
  currentHassasiyetShadow = '0 0 12px rgba(39,174,96,0.45)';

  // ── Lookup listeleri
  organizatorler: OrganizatorOption[] = [];
  konular: KonuOption[] = [];

  // ── Token/JWT bilgileri
  tokenUserId: string | null = null;
  tokenRole:   string | null = null;
  tokenCityId: number | null = null;
  readonly isBrowser: boolean;

  // ── Sabitler (template erişimi)
  readonly HASSASIYET   = HASSASIYET;
  readonly DURUM        = DURUM;
  readonly OLAY_TURLERI = OLAY_TURLERI;
  readonly IL_LISTESI   = IL_LISTESI;

  // ── Form grubu
  form!: FormGroup;

  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    this.decodeToken();
    this.buildForm();
    this.loadLookups();
    this.loadRows();

    // Risk önizleme debounce
    this.riskSub = this.riskSubject.pipe(debounceTime(600), distinctUntilChanged()).subscribe(() => {
      this.triggerRiskPreview();
    });
  }

  ngOnDestroy(): void {
    this.riskSub?.unsubscribe();
  }

  // ── JWT decode ────────────────────────────────────────────────────────
  private decodeToken(): void {
    if (!this.isBrowser) return;
    const token = localStorage.getItem('token');
    if (!token) return;
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return;
      const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
      this.tokenUserId = payload['sub'] ?? payload['nameid'] ?? null;
      this.tokenRole   = payload['role'] ?? null;
      const cid = payload['cityId'];
      this.tokenCityId = cid && cid !== '' ? parseInt(cid, 10) : null;
    } catch { /* sessiz başarısızlık */ }
  }

  // ── Form oluşturma ────────────────────────────────────────────────────
  private buildForm(): void {
    this.form = this.fb.group({
      baslik:           ['', [Validators.required, Validators.maxLength(250)]],
      olayTuru:         ['', Validators.required],
      organizatorId:    ['', Validators.required],
      konuId:           ['', Validators.required],
      tarih:            ['', Validators.required],
      baslangicSaati:   [''],
      bitisSaati:       [''],
      il:               ['', Validators.required],
      ilce:             ['', Validators.maxLength(100)],
      mekan:            ['', Validators.maxLength(250)],
      latitude:         [null, [Validators.min(-90), Validators.max(90)]],
      longitude:        [null, [Validators.min(-180), Validators.max(180)]],
      katilimciSayisi:  [null, Validators.min(0)],
      aciklama:         ['', Validators.maxLength(1000)],
      kaynakKurum:      ['', Validators.maxLength(250)],
      hassasiyet:       [0, Validators.required],
      cityId:           [this.tokenCityId],
      sosyalSignalSkoru:[0, [Validators.min(0), Validators.max(100)]],
      // Seçim Güvenliği – koşullu
      sandikNo:         [''],
      okulAdi:          [''],
      secimIlce:        [''],
      mudahaleDurumu:   [false],
      // VIP Ziyaret – koşullu
      guzergah:         [''],
      guvenlikSeviyesi: [''],
    });

    // İl kısıtlaması: şehir kısıtlı kullanıcı için il alanını kilitle
    if (this.isCityScoped && this.tokenCityId) {
      const ilAdi = IL_LISTESI.find(i => i.id === this.tokenCityId)?.ad ?? '';
      this.form.get('il')!.setValue(ilAdi);
      this.form.get('il')!.disable();
      this.form.get('cityId')!.setValue(this.tokenCityId);
      this.form.get('cityId')!.disable();
    }

    // Hassasiyet değiştiğinde tema ve risk güncelle
    this.form.get('hassasiyet')!.valueChanges.subscribe(v => this.onHassasiyetChange(+v));
    // Risk tetikleyici alanlar
    ['katilimciSayisi', 'hassasiyet', 'olayTuru', 'sosyalSignalSkoru'].forEach(f =>
      this.form.get(f)!.valueChanges.subscribe(() => this.riskSubject.next())
    );
    // Olay türü → koşullu alanlar
    this.form.get('olayTuru')!.valueChanges.subscribe(v => this.onOlayTuruChange(v));
  }

  // ── Getter kısayolları ────────────────────────────────────────────────
  get f(): { [key: string]: AbstractControl } { return this.form.controls; }
  get isCityScoped(): boolean {
    return ['IlPersoneli', 'IlYoneticisi'].includes(this.tokenRole ?? '');
  }
  get isSecimGuvenligi(): boolean { return this.form.get('olayTuru')?.value === 'Seçim Güvenliği'; }
  get isVIPZiyaret():     boolean { return this.form.get('olayTuru')?.value === 'VIP Ziyaret'; }
  get totalPages(): number { return Math.ceil(this.totalCount / this.pageSize) || 1; }

  // ── Koşullu alan yönetimi ─────────────────────────────────────────────
  private onOlayTuruChange(tur: string): void {
    const secimFields = ['sandikNo', 'okulAdi', 'secimIlce'];
    const vipFields   = ['guzergah', 'guvenlikSeviyesi'];

    if (tur === 'Seçim Güvenliği') {
      secimFields.forEach(f => {
        this.form.get(f)!.setValidators([Validators.required]);
        this.form.get(f)!.updateValueAndValidity();
      });
      this.form.get('mudahaleDurumu')!.setValidators([Validators.required]);
      vipFields.forEach(f => { this.form.get(f)!.clearValidators(); this.form.get(f)!.updateValueAndValidity(); });
    } else if (tur === 'VIP Ziyaret') {
      vipFields.forEach(f => {
        this.form.get(f)!.setValidators([Validators.required]);
        this.form.get(f)!.updateValueAndValidity();
      });
      secimFields.forEach(f => { this.form.get(f)!.clearValidators(); this.form.get(f)!.updateValueAndValidity(); });
      this.form.get('mudahaleDurumu')!.clearValidators();
    } else {
      [...secimFields, ...vipFields, 'mudahaleDurumu'].forEach(f => {
        this.form.get(f)!.clearValidators();
        this.form.get(f)!.updateValueAndValidity();
      });
    }
  }

  // ── Hassasiyet teması ─────────────────────────────────────────────────
  private onHassasiyetChange(val: number): void {
    const h = HASSASIYET.find(x => x.value === val) ?? HASSASIYET[0];
    this.currentHassasiyetColor  = h.color;
    this.currentHassasiyetShadow = h.shadow;
  }

  // ── Risk önizleme ─────────────────────────────────────────────────────
  private triggerRiskPreview(): void {
    if (!this.isBrowser) return;
    const v = this.form.getRawValue();
    this.http.post<RiskPreview>(`${API}/olay/risk-preview`, {
      katilimciSayisi:   v.katilimciSayisi ?? null,
      hassasiyet:        +v.hassasiyet,
      olayTuru:          v.olayTuru ?? '',
      sosyalSignalSkoru: +v.sosyalSignalSkoru
    }).subscribe({
      next: res => {
        this.riskPreview = res;
        this.isHighRisk  = res.riskPuaniNormalized >= 0.8;
      },
      error: () => { /* risk önizleme başarısız olursa sessizce geç */ }
    });
  }

  riskBarWidth(): string {
    if (!this.riskPreview) return '0%';
    return `${Math.round(this.riskPreview.riskPuaniNormalized * 100)}%`;
  }

  riskBarColor(): string {
    const v = this.riskPreview?.riskPuaniNormalized ?? 0;
    if (v >= 0.8) return '#e74c3c';
    if (v >= 0.6) return '#f39c12';
    if (v >= 0.4) return '#f1c40f';
    return '#27ae60';
  }

  // ── Lookup yükleme ────────────────────────────────────────────────────
  private loadLookups(): void {
    this.http.get<any[]>(`${API}/organizator`).subscribe({
      next: res => this.organizatorler = res.map(o => ({ id: o.id, ad: o.ad })),
      error: () => {}
    });
    this.http.get<any[]>(`${API}/organizator/konu`).subscribe({
      next: res => this.konular = res.map(k => ({ id: k.id, ad: k.ad })),
      error: () => {}
    });
  }

  // ── Tablo yükleme ─────────────────────────────────────────────────────
  loadRows(): void {
    this.isLoading  = true;
    this.tableError = null;
    let url = `${API}/olay?sayfa=${this.currentPage}&sayfaBoyutu=${this.pageSize}`;
    if (this.filterDurum !== '') url += `&durum=${this.filterDurum}`;
    this.http.get<any>(url).subscribe({
      next: res => {
        this.totalCount = res.totalCount ?? 0;
        this.rows = (res.items ?? []).map((o: any): OlayRow => ({
          id:              o.id,
          baslik:          o.baslik,
          olayTuru:        o.olayTuru,
          tarih:           o.tarih,
          il:              o.il,
          ilce:            o.ilce,
          mekan:           o.mekan,
          hassasiyet:      o.hassasiyet,
          durum:           o.durum,
          riskPuani:       o.riskPuani,
          katilimciSayisi: o.katilimciSayisi,
          aciklama:        o.aciklama,
          createdByUserId: o.createdByUserId ?? '',
          cityId:          o.cityId,
        }));
        this.isLoading = false;
      },
      error: () => {
        this.tableError = 'Veriler yüklenemedi. Lütfen tekrar deneyiniz.';
        this.isLoading  = false;
      }
    });
  }

  // ── Sayfalama ─────────────────────────────────────────────────────────
  navigatePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadRows();
  }

  onFilterDurumChange(): void {
    this.currentPage = 1;
    this.loadRows();
  }

  // ── Form açma/kapama ──────────────────────────────────────────────────
  openCreate(): void {
    this.editId = null;
    this.form.reset({ hassasiyet: 0, sosyalSignalSkoru: 0, mudahaleDurumu: false });
    if (this.isCityScoped && this.tokenCityId) {
      const ilAdi = IL_LISTESI.find(i => i.id === this.tokenCityId)?.ad ?? '';
      this.form.get('il')!.setValue(ilAdi);
      this.form.get('cityId')!.setValue(this.tokenCityId);
    }
    this.riskPreview = null;
    this.isHighRisk  = false;
    this.formError   = null;
    this.formSuccess = null;
    this.onHassasiyetChange(0);
    this.showForm    = true;
  }

  openEdit(row: OlayRow): void {
    if (!this.canEdit(row)) return;
    this.editId = row.id;
    this.form.patchValue({
      baslik:          row.baslik ?? '',
      olayTuru:        row.olayTuru ?? '',
      tarih:           row.tarih ? row.tarih.substring(0, 10) : '',
      il:              row.il ?? '',
      ilce:            row.ilce ?? '',
      mekan:           row.mekan ?? '',
      hassasiyet:      row.hassasiyet,
      katilimciSayisi: row.katilimciSayisi,
      aciklama:        row.aciklama ?? '',
      cityId:          row.cityId,
      sosyalSignalSkoru: 0,
    });
    this.onHassasiyetChange(row.hassasiyet);
    this.riskPreview = null;
    this.isHighRisk  = false;
    this.formError   = null;
    this.formSuccess = null;
    this.showForm    = true;
  }

  closeForm(): void {
    this.showForm    = false;
    this.editId      = null;
    this.riskPreview = null;
    this.isHighRisk  = false;
    this.form.reset();
  }

  // ── RBAC: düzenleme yetkisi ───────────────────────────────────────────
  canEdit(row: OlayRow): boolean {
    if (!this.tokenUserId) return false;
    const role = this.tokenRole ?? '';
    if (['BaskanlikYoneticisi', 'IlYoneticisi'].includes(role)) return true;
    return row.createdByUserId === this.tokenUserId;
  }

  canCreateOrEdit(): boolean {
    return ['IlPersoneli', 'IlYoneticisi', 'BaskanlikPersoneli', 'BaskanlikYoneticisi']
      .includes(this.tokenRole ?? '');
  }

  // ── Kaydet ────────────────────────────────────────────────────────────
  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.formError = 'Lütfen tüm zorunlu alanları doldurunuz.';
      return;
    }
    if (this.isCityScoped && this.tokenCityId) {
      const cityId = +this.form.getRawValue().cityId;
      if (cityId !== this.tokenCityId) {
        this.formError = 'Yalnızca kendi yetkili olduğunuz il için veri girişi yapabilirsiniz.';
        return;
      }
    }
    this.isSaving  = true;
    this.formError = null;
    const raw = this.form.getRawValue();
    const payload: Record<string, unknown> = {
      baslik:          raw.baslik,
      olayTuru:        raw.olayTuru,
      organizatorId:   raw.organizatorId,
      konuId:          raw.konuId,
      tarih:           raw.tarih,
      baslangicSaati:  raw.baslangicSaati  || null,
      bitisSaati:      raw.bitisSaati       || null,
      il:              raw.il,
      ilce:            raw.ilce             || null,
      mekan:           raw.mekan            || null,
      latitude:        raw.latitude         ? +raw.latitude  : null,
      longitude:       raw.longitude        ? +raw.longitude : null,
      katilimciSayisi: raw.katilimciSayisi  ? +raw.katilimciSayisi : null,
      aciklama:        raw.aciklama         || null,
      kaynakKurum:     raw.kaynakKurum      || null,
      hassasiyet:      +raw.hassasiyet,
      cityId:          raw.cityId           ? +raw.cityId : null,
    };
    const obs = this.editId
      ? this.http.put(`${API}/olay/${this.editId}`, payload)
      : this.http.post(`${API}/olay`, payload);
    obs.subscribe({
      next: () => {
        this.isSaving    = false;
        this.formSuccess = this.editId ? 'Kayıt başarıyla güncellendi.' : 'Yeni kayıt oluşturuldu.';
        this.loadRows();
        setTimeout(() => { this.closeForm(); }, 1500);
      },
      error: err => {
        this.isSaving  = false;
        const detail   = err?.error?.title ?? err?.error ?? null;
        this.formError = typeof detail === 'string' ? detail : 'İşlem sırasında bir hata oluştu.';
      }
    });
  }

  // ── Durum işlemleri ───────────────────────────────────────────────────
  baslat(id: string): void { this.statusAction(id, 'baslat', 'Olay başlatıldı.'); }
  bitir(id: string):   void { this.statusAction(id, 'bitir',  'Olay tamamlandı.'); }
  iptal(id: string):   void {
    if (!confirm('Bu olayı iptal etmek istediğinizden emin misiniz?')) return;
    this.statusAction(id, 'iptal', 'Olay iptal edildi.');
  }

  private statusAction(id: string, action: string, msg: string): void {
    this.http.put(`${API}/olay/${id}/${action}`, {}).subscribe({
      next: () => { this.loadRows(); },
      error: () => { this.tableError = `${msg} sırasında hata oluştu.`; }
    });
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

  fieldError(name: string): boolean {
    const c = this.form.get(name);
    return !!(c && c.invalid && c.touched);
  }

  navigate(path: string): void { this.router.navigate([path]); }

  logout(): void {
    if (this.isBrowser) localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}
