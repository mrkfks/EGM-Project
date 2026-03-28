import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, Inject, OnInit, OnDestroy, PLATFORM_ID } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Subject, Subscription, debounceTime } from 'rxjs';
import { environment } from '../../../environments/environment';

const API = environment.apiUrl + '/api';

const HASSASIYET = [
  { value: 0, label: 'Düşük',  color: '#27ae60', shadow: '0 0 12px rgba(39,174,96,0.45)' },
  { value: 1, label: 'Orta',   color: '#f39c12', shadow: '0 0 12px rgba(243,156,18,0.45)' },
  { value: 2, label: 'Yüksek', color: '#e74c3c', shadow: '0 0 12px rgba(231,76,60,0.45)'  },
  { value: 3, label: 'Kritik', color: '#8e44ad', shadow: '0 0 12px rgba(142,68,173,0.45)' },
];

const OLAY_TURLERI = [
  'Gösteri', 'Yürüyüş', 'Miting',
  'Terör Olayı', 'Silahlı Çatışma', 'Provokasyon',
  'Basın Açıklaması', 'Diğer',
];

const IL_LISTESI: { id: number; ad: string }[] = [
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
  {id:81,ad:'Düzce'},
];

interface OrganizatorOption { id: string; ad: string; }
interface KonuOption        { id: string; ad: string; }
interface RiskPreview {
  riskPuaniRaw: number;
  riskPuaniNormalized: number;
  seviye: string;
}

@Component({
  selector: 'app-sokak-olay-ekle',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './sokak-olay-ekle.html',
  styleUrl: './sokak-olay-ekle.css',
})
export class SokakOlayEkle implements OnInit, OnDestroy {

  form!: FormGroup;
  isSaving    = false;
  formError:   string | null = null;
  formSuccess: string | null = null;

  // Risk önizleme
  riskPreview: RiskPreview | null = null;
  isHighRisk   = false;
  private riskSubject = new Subject<void>();
  private riskSub?: Subscription;

  // Hassasiyet teması
  hassasiyetColor  = '#27ae60';
  hassasiyetShadow = '0 0 12px rgba(39,174,96,0.45)';

  // Lookup listeleri
  organizatorler: OrganizatorOption[] = [];
  konular: KonuOption[]               = [];

  // Token
  tokenRole:   string | null = null;
  tokenCityId: number | null = null;
  tokenUserId: string | null = null;
  readonly isBrowser: boolean;

  readonly HASSASIYET   = HASSASIYET;
  readonly OLAY_TURLERI = OLAY_TURLERI;
  readonly IL_LISTESI   = IL_LISTESI;

  userPlans: any[] = [];
  completedEvents: any[] = [];

  activeForm: 'planned' | 'completed' = 'planned';

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: object,
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    this.decodeToken();
    this.buildForm();
    this.loadLookups();
    this.loadCompletedEvents(); // Load completed events on initialization

    this.riskSub = this.riskSubject.pipe(debounceTime(600)).subscribe(() => this.fetchRisk());
  }

  ngOnDestroy(): void { this.riskSub?.unsubscribe(); }

  // ── JWT decode ──────────────────────────────────────────────────────
  private decodeToken(): void {
    if (!this.isBrowser) return;
    const token = localStorage.getItem('token');
    if (!token) return;
    try {
      const b64  = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(
        Array.prototype.map.call(atob(b64), (c: string) =>
          '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
        ).join('')
      );
      const p = JSON.parse(json);
      this.tokenUserId = p['sub'] ?? p['nameid'] ?? null;
      this.tokenRole   = p['role'] ?? null;
      const cid = p['cityId'];
      this.tokenCityId = cid && cid !== '' ? parseInt(cid, 10) : null;
    } catch { /* ignore */ }
  }

  get isCityScoped(): boolean {
    return ['IlPersoneli', 'IlYoneticisi'].includes(this.tokenRole ?? '');
  }

  // ── Form ────────────────────────────────────────────────────────────
  private buildForm(): void {
    this.form = this.fb.group({
      baslik:           ['', Validators.maxLength(250)],
      olayTuru:         ['', Validators.required],
      organizatorId:    ['', Validators.required],
      konuId:           ['', Validators.required],
      tarih:            ['', Validators.required],
      il:               ['', Validators.required],
      ilce:             ['', Validators.maxLength(100)],
      mekan:            ['', Validators.maxLength(250)],
      latitude:         [null, [Validators.min(-90),   Validators.max(90)]],
      longitude:        [null, [Validators.min(-180),  Validators.max(180)]],
      katilimciSayisi:  [null, Validators.min(0)],
      sosyalSignalSkoru:[0,   [Validators.min(0), Validators.max(100)]],
      hassasiyet:       [0,    Validators.required],
      aciklama:         ['', Validators.maxLength(1000)],
      kaynakKurum:      ['', Validators.maxLength(250)],
      cityId:           [this.tokenCityId],
      bitisTarihi:      [''],
      gerceklesenKatilimciSayisi: [null, Validators.min(0)],
      sehitSayisi:      [null, Validators.min(0)],
      oluSayisi:        [null, Validators.min(0)],
      gozaltiSayisi:    [null, Validators.min(0)],
    });

    if (this.isCityScoped && this.tokenCityId) {
      const ilAdi = IL_LISTESI.find(i => i.id === this.tokenCityId)?.ad ?? '';
      this.form.get('il')!.setValue(ilAdi);
      this.form.get('il')!.disable();
      this.form.get('cityId')!.setValue(this.tokenCityId);
      this.form.get('cityId')!.disable();
    }

    this.form.get('hassasiyet')!.valueChanges.subscribe(v => this.onHassasiyetChange(+v));
    ['katilimciSayisi', 'hassasiyet', 'olayTuru', 'sosyalSignalSkoru'].forEach(f =>
      this.form.get(f)!.valueChanges.subscribe(() => this.riskSubject.next())
    );
  }

  get f(): { [key: string]: AbstractControl } { return this.form.controls; }

  fieldError(name: string): boolean {
    const c = this.form.get(name);
    return !!(c && c.invalid && c.touched);
  }

  private onHassasiyetChange(val: number): void {
    const h = HASSASIYET.find(x => x.value === val) ?? HASSASIYET[0];
    this.hassasiyetColor  = h.color;
    this.hassasiyetShadow = h.shadow;
  }

  // ── Lookup yükleme ──────────────────────────────────────────────────
  private loadLookups(): void {
    this.http.get<any[]>(`${API}/organizator`).subscribe({
      next: res => this.organizatorler = res.map(o => ({ id: o.id, ad: o.ad })),
      error: () => {},
    });
    this.http.get<any[]>(`${API}/organizator/konu`).subscribe({
      next: res => this.konular = res.map(k => ({ id: k.id, ad: k.ad })),
      error: () => {},
    });
  }

  // ── Risk önizleme ───────────────────────────────────────────────────
  private fetchRisk(): void {
    if (!this.isBrowser) return;
    const v = this.form.getRawValue();
    this.http.post<RiskPreview>(`${API}/olay/risk-preview`, {
      katilimciSayisi:   v.katilimciSayisi ?? null,
      hassasiyet:        +v.hassasiyet,
      olayTuru:          v.olayTuru ?? '',
      sosyalSignalSkoru: +v.sosyalSignalSkoru,
    }).subscribe({
      next: res => {
        this.riskPreview = res;
        this.isHighRisk  = res.riskPuaniNormalized >= 0.8;
      },
      error: () => {},
    });
  }

  riskBarWidth(): string {
    return this.riskPreview ? `${Math.round(this.riskPreview.riskPuaniNormalized * 100)}%` : '0%';
  }

  riskBarColor(): string {
    const v = this.riskPreview?.riskPuaniNormalized ?? 0;
    if (v >= 0.8) return '#e74c3c';
    if (v >= 0.6) return '#f39c12';
    if (v >= 0.4) return '#f1c40f';
    return '#27ae60';
  }

  // ── Kaydet ──────────────────────────────────────────────────────────
  save(): void {
    if (this.form.invalid) {
      this.formError = 'Lütfen tüm zorunlu alanları doldurun.';
      return;
    }
    this.isSaving = true;
    const newPlan = this.form.value;
    this.userPlans.push(newPlan);
    this.formSuccess = 'Plan başarıyla eklendi.';
    this.isSaving = false;
    this.form.reset();
  }

  saveToPlanned(): void {
    if (this.form.invalid) {
      this.formError = 'Lütfen tüm zorunlu alanları doldurun.';
      return;
    }
    const newPlan = this.form.value;
    this.userPlans.push(newPlan);
    this.formSuccess = 'Planlananlara başarıyla kaydedildi.';
    this.form.reset();
  }

  saveToCompleted(): void {
    if (this.form.invalid) {
      this.formError = 'Lütfen tüm zorunlu alanları doldurun.';
      return;
    }
    const newEvent = this.form.value;
    this.completedEvents.push(newEvent);
    this.formSuccess = 'Gerçekleşenlere başarıyla kaydedildi.';
    this.form.reset();
  }

  cancel(): void {
    this.form.reset();
    this.formError = null;
    this.formSuccess = null;
  }

  editPlan(plan: any): void {
    this.form.patchValue(plan);
  }

  goBack(): void { this.router.navigate(['/olay']); }

  // Example function to load completed events (replace with actual API call)
  loadCompletedEvents(): void {
    this.http.get<any[]>(`${API}/sokak-olay/completed`).subscribe({
      next: res => { this.completedEvents = res; },
      error: () => {},
    });
  }

  switchToPlannedForm(): void {
    this.activeForm = 'planned';
    this.formError = null;
    this.formSuccess = null;
    this.form.get('baslik')?.clearValidators();
    this.form.get('baslik')?.updateValueAndValidity();
  }

  switchToCompletedForm(): void {
    this.activeForm = 'completed';
    this.formError = null;
    this.formSuccess = null;
  }

  kaydet(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.formError = 'Lütfen tüm zorunlu alanları doldurun.';
      return;
    }
    if (this.activeForm === 'planned') {
      const newPlan = this.form.getRawValue();
      this.userPlans.push(newPlan);
      this.formSuccess = 'Planlananlara başarıyla kaydedildi.';
    } else {
      const newEvent = this.form.getRawValue();
      this.completedEvents.push(newEvent);
      this.formSuccess = 'Gerçekleşenlere başarıyla kaydedildi.';
    }
    this.formError = null;
    this.form.reset();
    if (this.isCityScoped && this.tokenCityId) {
      const ilAdi = IL_LISTESI.find(i => i.id === this.tokenCityId)?.ad ?? '';
      this.form.get('il')!.setValue(ilAdi);
    }
  }
}
