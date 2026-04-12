import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, switchMap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OlayTuruService, OlayTuru as OlayTuruDto } from '../../services/olay-turu.service';
import { GerceklesmeSekliService, GerceklesmeSekli as GerceklesmeSekliDto } from '../../services/gerceklesme-sekli.service';

const API = environment.apiUrl + '/api';

const HASSASIYET = [
  { value: 0, label: 'Düşük',  color: '#27ae60', shadow: '0 0 12px rgba(39,174,96,0.45)' },
  { value: 1, label: 'Orta',   color: '#f39c12', shadow: '0 0 12px rgba(243,156,18,0.45)' },
  { value: 2, label: 'Yüksek', color: '#e74c3c', shadow: '0 0 12px rgba(231,76,60,0.45)'  },
  { value: 3, label: 'Kritik', color: '#8e44ad', shadow: '0 0 12px rgba(142,68,173,0.45)' },
];



interface OrganizatorOption { id: string; ad: string; }
interface KonuOption        { id: string; ad: string; }
interface OlayListItem {
  id: string;
  olayTuru?: string;
  il?: string;
  ilce?: string;
  tarih: string;
  hassasiyet: number;
  katilimciSayisi?: number;
  organizatorAd?: string;
  durum: number;
  takipNo?: string;
}

@Component({
  selector: 'app-sokak-olay-ekle',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './sokak-olay-ekle.html',
  styleUrl: './sokak-olay-ekle.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SokakOlayEkle implements OnInit {

  form!: FormGroup;
  isSaving    = false;
  formError:   string | null = null;
  formSuccess: string | null = null;
  takipNo:     string | null = null;

  get minTarih(): string {
    const now = new Date();
    now.setSeconds(0, 0);
    return now.toISOString().slice(0, 16);
  }

  // Hassasiyet teması
  hassasiyetColor  = '#27ae60';
  hassasiyetShadow = '0 0 12px rgba(39,174,96,0.45)';

  // Lookup listeleri
  organizatorler: OrganizatorOption[] = [];
  konular: KonuOption[]               = [];
  olayTurleri: OlayTuruDto[] = [];
  tumGerceklesmeSekilleri: GerceklesmeSekliDto[] = [];
  filtreliGerceklesmeSekilleri: GerceklesmeSekliDto[] = [];

  // Token
  tokenRole:   string | null = null;
  tokenCityId: number | null = null;
  tokenUserId: string | null = null;
  readonly isBrowser: boolean;

  readonly HASSASIYET = HASSASIYET;
  
  // API'den yüklenen veriler
  tumIller: { name: string; osmId: number }[] = [];
  filtreliIlceler: { name: string; osmId: number }[] = [];
  filtreliMahalleler: { name: string; osmId: number }[] = [];

  userPlans: OlayListItem[] = [];
  completedEvents: OlayListItem[] = [];
  isLoadingPlanned  = false;
  isLoadingCompleted = false;

  ekKonumlar: {
    il: string; ilce: string; mahalle: string;
    konum: string; mekan: string; katilimciSayisi: number | null;
    ilceler: { name: string; osmId: number }[];
    mahalleler: { name: string; osmId: number }[];
  }[] = [];

  /** Gerçekleşenler formuna yüklenen planlanan olayın id'si (null = yeni kayıt) */
  selectedPlanId: string | null = null;

  activeForm: 'planned' | 'completed' = 'planned';

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private olayTuruService: OlayTuruService,
    private gerceklesmeSekliService: GerceklesmeSekliService,
    private cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) platformId: object,
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    this.decodeToken();
    this.buildForm();
    this.loadLookups();
    this.loadPlannedEvents();
    this.loadCompletedEvents();
  }

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

  /**
   * datetime-local input "YYYY-MM-DDTHH:mm" (16 karakter) döndürür.
   * System.Text.Json DateTime binding için saniye zorunlu → eksikse ":00" ekle.
   */
  private normalizeDt(dt: string | null | undefined): string | null {
    if (!dt) return null;
    return dt.length === 16 ? dt + ':00' : dt;
  }

  // ── Form ────────────────────────────────────────────────────────────
  private buildForm(): void {
    this.form = this.fb.group({
      olayTuru:         ['', Validators.required],
      olayinGerceklesmeSekli: ['', Validators.required],
      organizatorId:    ['', Validators.required],
      konuId:           ['', Validators.required],
      tarih:            ['', Validators.required],
      il:               ['', Validators.required],
      ilce:             ['', Validators.maxLength(100)],      mahalle:          [{ value: '', disabled: true }, Validators.maxLength(100)],      baslangicKonum:   ['', [Validators.pattern(/^-?\d{1,3}(\.\d+)?,\s*-?\d{1,3}(\.\d+)?$/)]],
      mekan:            ['', Validators.maxLength(250)],
      latitude:         [null, [Validators.min(-90),   Validators.max(90)]],
      longitude:        [null, [Validators.min(-180),  Validators.max(180)]],
      katilimciSayisi:  [null, Validators.min(0)],
      hassasiyet:       [0,    Validators.required],
      aciklama:         ['', Validators.maxLength(1000)],
      cityId:           [this.tokenCityId],
      bitisTarihi:      [''],
      gerceklesenKatilimciSayisi: [null, Validators.min(0)],
      sehitSayisi:      [null, Validators.min(0)],
      oluSayisi:        [null, Validators.min(0)],
      gozaltiSayisi:    [null, Validators.min(0)],
      evrakNumarasi:    [''],
    });


    if (this.isCityScoped && this.tokenCityId) {
      // Seçili şehri adıyla bul
      const ilAdi = this.tumIller.find(i => i.osmId === this.tokenCityId)?.name ?? '';
      this.form.get('il')!.setValue(ilAdi);
      this.form.get('il')!.disable();
      this.form.get('cityId')!.setValue(this.tokenCityId);
      this.form.get('cityId')!.disable();
    }

    this.form.get('hassasiyet')!.valueChanges.subscribe(v => this.onHassasiyetChange(+v));

    // İl değişince ilçeleri API'den yükle
    this.form.get('il')!.valueChanges.subscribe((ilAdi: string) => {
      if (!ilAdi || ilAdi.trim() === '') {
        this.filtreliIlceler = [];
        this.filtreliMahalleler = [];
      } else {
        this.http.get<any[]>(`${API}/geo/districts-geopackage?province=${encodeURIComponent(ilAdi)}`)
          .pipe(
            catchError(err => {
              console.error('İlçe yükleme hatası:', err);
              return of([]);
            })
          )
          .subscribe(districts => {
            this.filtreliIlceler = districts;
            this.form.get('ilce')!.setValue('');
            this.filtreliMahalleler = [];
            this.cdr.markForCheck();
          });
      }
    });

    // İlçe değişince mahalleri API'den yükle
    this.form.get('ilce')?.valueChanges.subscribe((ilceName: string) => {
      this.form.get('mahalle')!.setValue('');
      if (!ilceName || ilceName.trim() === '') {
        this.filtreliMahalleler = [];
        this.form.get('mahalle')!.disable();
      } else {
        this.form.get('mahalle')!.enable();
        this.http.get<any[]>(`${API}/geo/neighborhoods-geopackage?district=${encodeURIComponent(ilceName)}`)
          .pipe(
            catchError(err => {
              console.error('Mahalle yükleme hatası:', err);
              return of([]);
            })
          )
          .subscribe(neighborhoods => {
            this.filtreliMahalleler = neighborhoods;
            this.cdr.markForCheck();
          });
      }
    });

    this.setupLocationAutoFill();
  }

  get f(): { [key: string]: AbstractControl } { return this.form.controls; }

  fieldError(name: string): boolean {
    const c = this.form.get(name);
    return !!(c && c.invalid && c.touched);
  }

  parseKonum(val: string): { lat: number; lng: number } | null {
    if (!val?.trim()) return null;
    const parts = val.split(',');
    if (parts.length !== 2) return null;
    const lat = parseFloat(parts[0].trim());
    const lng = parseFloat(parts[1].trim());
    if (isNaN(lat) || isNaN(lng)) return null;
    return { lat, lng };
  }

  // ── İl/İlçleden Koordinat Otomatik Doldurma ──────────────────────────
  private setupLocationAutoFill(): void {
    // İL seçimi değiştiğinde
    this.form.get('il')?.valueChanges
      .pipe(
        distinctUntilChanged(),
        switchMap(provinceName => {
          if (!provinceName) return of(null);
          
          return this.http.get<any>(
            `${API}/geo/get-coordinates?provinceName=${encodeURIComponent(provinceName)}`
          ).pipe(
            catchError(err => {
              console.error('İl için koordinat hatası:', err);
              return of(null);
            })
          );
        })
      )
      .subscribe(coords => {
        if (coords?.latitude && coords?.longitude) {
          this.form.patchValue({
            baslangicKonum: `${coords.latitude.toFixed(6)}, ${coords.longitude.toFixed(6)}`
          }, { emitEvent: false });
          
          this.cdr.markForCheck();
        }
      });

    // İLÇE seçimi değiştiğinde
    this.form.get('ilce')?.valueChanges
      .pipe(
        distinctUntilChanged(),
        switchMap(districtName => {
          if (!districtName) return of(null);
          
          const provinceName = this.form.get('il')?.value;
          if (!provinceName) return of(null);
          
          return this.http.get<any>(
            `${API}/geo/get-coordinates?` +
            `provinceName=${encodeURIComponent(provinceName)}&` +
            `districtName=${encodeURIComponent(districtName)}`
          ).pipe(
            catchError(err => {
              console.error('İlçe için koordinat hatası:', err);
              return of(null);
            })
          );
        })
      )
      .subscribe(coords => {
        if (coords?.latitude && coords?.longitude) {
          this.form.patchValue({
            baslangicKonum: `${coords.latitude.toFixed(6)}, ${coords.longitude.toFixed(6)}`
          }, { emitEvent: false });
          
          this.cdr.markForCheck();
        }
      });

    // REVERSE LOOKUP: KOORDİNAT GİRİŞİ → İL/İLÇE DOLDURMA
    // Başlangıç Konum alanı koordinat değiştiğinde il/ilçe'yi otomatik bulur
    this.form.get('baslangicKonum')?.valueChanges
      .pipe(
        debounceTime(500), // 500ms bekleme (kullanıcı yazması bitsin)
        distinctUntilChanged(),
        switchMap(konum => {
          if (!konum || konum.trim() === '') return of(null);
          
          // Koordinat parse etme: "39.925533, 32.866287" veya "39.925533,32.866287"
          const parts = konum.split(',').map((p: string) => parseFloat(p.trim()));
          
          if (parts.length !== 2 || isNaN(parts[0]) || isNaN(parts[1])) {
            // Geçerli koordinat değil
            return of(null);
          }
          
          const latitude = parts[0];
          const longitude = parts[1];
          
          // Koordinat aralığını kontrol et
          if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180) {
            return of(null);
          }
          
          // API'ye gönder: koordinat → il/ilçe bulma
          return this.http.get<any>(
            `${API}/geo/resolve-location?latitude=${latitude}&longitude=${longitude}`
          ).pipe(
            catchError(err => {
              console.warn('Koordinat çözümleme başarısız:', err);
              return of(null);
            })
          );
        })
      )
      .subscribe(location => {
        if (location?.province || location?.district || location?.neighborhood) {
          const updateObj: any = {};
          
          // İl doldur (sadece boşsa)
          if (location.province && !this.form.get('il')?.value) {
            updateObj['il'] = location.province;
          }
          
          // İlçe doldur (sadece boşsa ve il seçildiyse)
          if (location.district && !this.form.get('ilce')?.value) {
            updateObj['ilce'] = location.district;
          }
          
          // Mahalle doldur (sadece boşsa)
          if (location.neighborhood && !this.form.get('mahalle')?.value) {
            updateObj['mahalle'] = location.neighborhood;
          }
          
          if (Object.keys(updateObj).length > 0) {
            this.form.patchValue(updateObj, { emitEvent: false });
            this.cdr.markForCheck();
            
            // İl değişti, ilçe listesini API'den güncelle
            if (updateObj['il']) {
              const ilAdi = updateObj['il'];
              this.http.get<any[]>(`${API}/geo/districts-geopackage?province=${encodeURIComponent(ilAdi)}`)
                .pipe(
                  catchError(err => {
                    console.error('İlçe yükleme hatası:', err);
                    return of([]);
                  })
                )
                .subscribe(districts => {
                  this.filtreliIlceler = districts;
                  this.cdr.markForCheck();
                });
            }
          }
        }
      });
  }

  private onHassasiyetChange(val: number): void {
    const h = HASSASIYET.find(x => x.value === val) ?? HASSASIYET[0];
    this.hassasiyetColor  = h.color;
    this.hassasiyetShadow = h.shadow;
  }

  // ── Lookup yükleme ──────────────────────────────────────────────────
  private loadLookups(): void {
    // İlleri TurkeyRehber'dan yükle
    this.http.get<any[]>(`${API}/geo/provinces-geopackage`).subscribe({
      next: res => { 
        this.tumIller = res;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('İlleri yükleme hatası:', err);
      },
    });

    this.http.get<any[]>(`${API}/organizator`).subscribe({
      next: res => { this.organizatorler = res.map(o => ({ id: o.id, ad: o.ad })); this.cdr.markForCheck(); },
      error: () => {},
    });
    this.http.get<any[]>(`${API}/organizator/konu`).subscribe({
      next: res => { this.konular = res.map(k => ({ id: k.id, ad: k.ad })); this.cdr.markForCheck(); },
      error: () => {},
    });
    this.olayTuruService.getAll().subscribe({
      next: res => { this.olayTurleri = res; this.cdr.markForCheck(); },
      error: () => {},
    });
    this.gerceklesmeSekliService.getAll().subscribe({
      next: res => {
        this.tumGerceklesmeSekilleri = res;
        this.filtreliGerceklesmeSekilleri = res;
        this.cdr.markForCheck();
      },
      error: () => {},
    });

    this.form.get('olayTuru')!.valueChanges.subscribe((secilenId: string) => {
      this.form.get('olayinGerceklesmeSekli')!.setValue('');
      if (secilenId) {
        this.filtreliGerceklesmeSekilleri = this.tumGerceklesmeSekilleri.filter(
          s => s.olayTuruId === secilenId
        );
      } else {
        this.filtreliGerceklesmeSekilleri = this.tumGerceklesmeSekilleri;
      }
    });
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

  goBack(): void { this.router.navigate(['/olay']); }

  loadCompletedEvents(): void {
    this.isLoadingCompleted = true;
    this.http.get<any>(`${API}/olay?durum=1&sayfaBoyutu=100`).subscribe({
      next: res => {
        this.completedEvents = res?.items ?? res ?? [];
        this.isLoadingCompleted = false;
        this.cdr.markForCheck();
      },
      error: () => { this.isLoadingCompleted = false; this.cdr.markForCheck(); }
    });
  }

  loadPlannedEvents(): void {
    this.isLoadingPlanned = true;
    this.http.get<any>(`${API}/olay?durum=0&sayfaBoyutu=100`).subscribe({
      next: res => {
        this.userPlans = res?.items ?? res ?? [];
        this.isLoadingPlanned = false;
        this.cdr.markForCheck();
      },
      error: () => { this.isLoadingPlanned = false; this.cdr.markForCheck(); }
    });
  }

  switchToPlannedForm(): void {
    this.activeForm = 'planned';
    this.selectedPlanId = null;
    this.formError = null;
    this.formSuccess = null;
    this.ekKonumlar = [];
    this.form.reset();
    if (this.isCityScoped && this.tokenCityId) {
      const ilAdi = this.tumIller.find((i: any) => i.osmId === this.tokenCityId)?.name ?? '';
      this.form.get('il')!.setValue(ilAdi);
    }
  }

  switchToCompletedForm(): void {
    this.activeForm = 'completed';
    this.selectedPlanId = null;
    this.formError = null;
    this.formSuccess = null;
    this.ekKonumlar = [];
    this.form.reset();
    if (this.isCityScoped && this.tokenCityId) {
      const ilAdi = this.tumIller.find((i: any) => i.osmId === this.tokenCityId)?.name ?? '';
      this.form.get('il')!.setValue(ilAdi);
    }
  }

  /** Planlanan olayı gerçekleşenler formuna yükler */
  selectPlanForCompletion(plan: OlayListItem): void {
    this.selectedPlanId = plan.id;
    this.activeForm = 'completed';
    this.formError = null;
    this.formSuccess = null;

    // API'den tam detayı çek (organizatorId, konuId gibi alanlar için)
    this.http.get<any>(`${API}/olay/${plan.id}`).subscribe({
      next: detail => {
        this.form.patchValue({
          olayTuru:         this.olayTurleri.find(t => t.name === detail.olayTuru)?.id ?? '',
          organizatorId:    detail.organizatorId ?? '',
          konuId:           detail.konuId ?? '',
          tarih:            detail.tarih ? detail.tarih.substring(0, 16) : '',
          il:               detail.il ?? '',
          ilce:             detail.ilce ?? '',
          baslangicKonum: detail.baslangicLat != null && detail.baslangicLng != null
            ? `${detail.baslangicLat},${detail.baslangicLng}` : '',
          mekan:            detail.mekan ?? '',
          katilimciSayisi:  detail.katilimciSayisi ?? null,
          hassasiyet:       detail.hassasiyet ?? 0,
          aciklama:         detail.aciklama ?? '',
          evrakNumarasi:    detail.evrakNumarasi ?? '',
        });
        // İl personeli ise il kilidi koru
        if (this.isCityScoped) {
          this.form.get('il')!.disable();
        }
        this.cdr.markForCheck();
      },
      error: () => {
        // Detay çekilemezse mevcut özet veriyle doldur
        this.form.patchValue({
          olayTuru:        this.olayTurleri.find(t => t.name === (plan.olayTuru ?? ''))?.id ?? '',
          il:              plan.il ?? '',
          tarih:           plan.tarih ? plan.tarih.substring(0, 16) : '',
          katilimciSayisi: plan.katilimciSayisi ?? null,
          hassasiyet:      plan.hassasiyet ?? 0,
        });
        this.cdr.markForCheck();
      }
    });
  }

  /** Seçili planlanan olayı iptal olarak işaretle (durum=2) */
  iptalEt(): void {
    if (!this.selectedPlanId) return;
    const planId = this.selectedPlanId;
    this.isSaving = true;
    this.formError = null;
    const v = this.form.getRawValue();
    const payload = {
      olayTuru:        this.olayTurleri.find(t => t.id === v.olayTuru)?.name ?? v.olayTuru,
      organizatorId:   v.organizatorId,
      konuId:          v.konuId,
      tarih:           this.normalizeDt(v.tarih)!,
      baslangicSaati:  v.tarih ? (this.normalizeDt(v.tarih)!.split('T')[1] ?? null) : null,
      il:              v.il,
      ilce:            v.ilce || null,
      baslangicLat:     this.parseKonum(v.baslangicKonum)?.lat ?? null,
      baslangicLng:     this.parseKonum(v.baslangicKonum)?.lng ?? null,
      mekan:           v.mekan || null,
      latitude:        v.latitude ?? null,
      longitude:       v.longitude ?? null,
      katilimciSayisi: v.katilimciSayisi ?? null,
      gozaltiSayisi:   v.gozaltiSayisi ?? null,
      evrakNumarasi:   v.evrakNumarasi || null,
      sehitOluSayisi:  null,
      aciklama:        v.aciklama || null,
      hassasiyet:      +v.hassasiyet,
      cityId:          v.cityId ?? null,
      durum:           2,
      gerceklesmeSekliId:           v.olayinGerceklesmeSekli || null,
      gerceklesenKatilimciSayisi:   v.gerceklesenKatilimciSayisi ?? null,
      olayBitisTarihi:              this.normalizeDt(v.bitisTarihi)
    };
    this.http.put<void>(`${API}/olay/${planId}`, payload).subscribe({
      next: () => {
        this.isSaving = false;
        this.formSuccess = 'Etkinlik iptal edildi.';
        const plan = this.userPlans.find(p => p.id === planId);
        this.userPlans = this.userPlans.filter(p => p.id !== planId);
        if (plan) {
          const cancelled: OlayListItem = { ...plan, durum: 2 };
          this.completedEvents = [cancelled, ...this.completedEvents];
        }
        this.selectedPlanId = null;
        this.form.reset();
        if (this.isCityScoped && this.tokenCityId) {
          const ilAdi = this.tumIller.find((i: any) => i.osmId === this.tokenCityId)?.name ?? '';
          this.form.get('il')!.setValue(ilAdi);
        }
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.isSaving = false;
        this.formError = err?.error?.detail ?? err?.error?.title ?? 'İptal işlemi sırasında bir hata oluştu.';
        this.cdr.markForCheck();
      }
    });
  }

  kaydet(): void {    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.formError = 'Lütfen tüm zorunlu alanları doldurun.';
      return;
    }

    const v = this.form.getRawValue();

    // Planlanan formda geçmiş tarih girilemesin
    if (this.activeForm === 'planned' && v.tarih) {
      const secilenTarih = new Date(v.tarih);
      const simdi = new Date();
      if (secilenTarih < simdi) {
        this.form.get('tarih')!.setErrors({ gecmisTarih: true });
        this.form.markAllAsTouched();
        this.formError = 'Planlanan olay geçmiş bir tarihe girilemez.';
        return;
      }
    }

    this.isSaving = true;
    this.formError = null;
    const durum = this.activeForm === 'planned' ? 0 : 1;
    const payload = {
      olayTuru:        this.olayTurleri.find(t => t.id === v.olayTuru)?.name ?? v.olayTuru,
      organizatorId:   v.organizatorId,
      konuId:          v.konuId,
      tarih:           this.normalizeDt(v.tarih)!,
      baslangicSaati:  v.tarih ? (this.normalizeDt(v.tarih)!.split('T')[1] ?? null) : null,
      il:              v.il,
      ilce:            v.ilce || null,
      baslangicLat:     this.parseKonum(v.baslangicKonum)?.lat ?? null,
      baslangicLng:     this.parseKonum(v.baslangicKonum)?.lng ?? null,
      mekan:           v.mekan || null,
      latitude:        v.latitude ?? null,
      longitude:       v.longitude ?? null,
      katilimciSayisi: v.katilimciSayisi ?? null,
      gozaltiSayisi:   v.gozaltiSayisi ?? null,
      evrakNumarasi:   v.evrakNumarasi || null,
      sehitOluSayisi:  (Number(v.sehitSayisi) || 0) + (Number(v.oluSayisi) || 0) || null,
      aciklama:        v.aciklama || null,
      hassasiyet:      +v.hassasiyet,
      cityId:          v.cityId ?? null,
      durum,
      gerceklesmeSekliId:           v.olayinGerceklesmeSekli || null,
      gerceklesenKatilimciSayisi:   v.gerceklesenKatilimciSayisi ?? null,
      olayBitisTarihi:              this.normalizeDt(v.bitisTarihi)
    };

    // Planlanan olay gerçekleşenlere taşınıyorsa → PUT
    if (this.selectedPlanId && this.activeForm === 'completed') {
      const planId = this.selectedPlanId;
      this.http.put<void>(`${API}/olay/${planId}`, payload).subscribe({
        next: () => {
          this.isSaving = false;
          this.formSuccess = 'Olay gerçekleşenlere taşındı.';
          // Sol panelden kaldır, sağ panele ekle
          const plan = this.userPlans.find(p => p.id === planId);
          this.userPlans = this.userPlans.filter(p => p.id !== planId);
          if (plan) {
            const updated: OlayListItem = { ...plan, ...payload, id: planId };
            this.completedEvents = [updated, ...this.completedEvents];
          }
          this.selectedPlanId = null;
          this.form.reset();
          if (this.isCityScoped && this.tokenCityId) {
            const ilAdi = this.tumIller.find((i: any) => i.osmId === this.tokenCityId)?.name ?? '';
            this.form.get('il')!.setValue(ilAdi);
          }
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          this.isSaving = false;
          this.formError = err?.error?.detail ?? err?.error?.title ?? 'Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.';
          this.cdr.markForCheck();
        }
      });
      return;
    }

    // Yeni kayıt → POST
    console.log('POST /api/olay payload:', JSON.stringify(payload, null, 2));
    this.http.post<OlayListItem>(`${API}/olay`, payload).subscribe({
      next: created => {
        this.isSaving = false;
        this.takipNo = (created as any).takipNo ?? null;
        this.formSuccess = this.activeForm === 'planned'
          ? 'Olay planlananlara kaydedildi.'
          : 'Olay gerçekleşenlere kaydedildi.';
        // Ek konumlar varsa kaydet
        if (this.ekKonumlar.length > 0) {
          this.saveEkKonumlar((created as any).id);
        }
        this.form.reset();
        if (this.isCityScoped && this.tokenCityId) {
          const ilAdi = this.tumIller.find((i: any) => i.osmId === this.tokenCityId)?.name ?? '';
          this.form.get('il')!.setValue(ilAdi);
        }
        if (this.activeForm === 'planned') {
          this.userPlans = [created, ...this.userPlans];
        } else {
          this.completedEvents = [created, ...this.completedEvents];
        }
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.isSaving = false;
        console.error('POST /api/olay hata:', err?.error);
        if (err?.error?.errors) {
          const msgs: string[] = [];
          for (const key of Object.keys(err.error.errors)) {
            msgs.push(`${key}: ${(err.error.errors[key] as string[]).join(', ')}`);
          }
          this.formError = msgs.join(' | ');
        } else {
          this.formError = err?.error?.detail ?? err?.error?.title ?? 'Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.';
        }
        this.cdr.markForCheck();
      }
    });
  }

  trackById(_index: number, item: { id: string }): string { return item.id; }
  trackByValue(_index: number, item: string | number): string | number { return item; }

  ekKonumEkle(): void {
    this.ekKonumlar = [...this.ekKonumlar, {
      il: '', ilce: '', mahalle: '',
      konum: '', mekan: '', katilimciSayisi: null,
      ilceler: [], mahalleler: []
    }];
    this.cdr.markForCheck();
  }

  ekKonumIlDegisti(i: number): void {
    const ek = this.ekKonumlar[i];
    ek.ilce = '';
    ek.mahalle = '';
    ek.ilceler = [];
    ek.mahalleler = [];
    ek.konum = '';
    if (!ek.il) { this.cdr.markForCheck(); return; }
    // İl koordinatını otomatik doldur
    this.http.get<any>(`${API}/geo/get-coordinates?provinceName=${encodeURIComponent(ek.il)}`)
      .pipe(catchError(() => of(null)))
      .subscribe(coords => {
        if (coords?.latitude && coords?.longitude) {
          ek.konum = `${coords.latitude.toFixed(6)}, ${coords.longitude.toFixed(6)}`;
        }
        this.cdr.markForCheck();
      });
    this.http.get<any[]>(`${API}/geo/districts-geopackage?province=${encodeURIComponent(ek.il)}`)
      .pipe(catchError(() => of([])))
      .subscribe(districts => { ek.ilceler = districts; this.cdr.markForCheck(); });
  }

  ekKonumIlceDegisti(i: number): void {
    const ek = this.ekKonumlar[i];
    ek.mahalle = '';
    ek.mahalleler = [];
    if (!ek.ilce) { this.cdr.markForCheck(); return; }
    // İlçe koordinatını otomatik doldur
    this.http.get<any>(
      `${API}/geo/get-coordinates?provinceName=${encodeURIComponent(ek.il)}&districtName=${encodeURIComponent(ek.ilce)}`
    ).pipe(catchError(() => of(null)))
      .subscribe(coords => {
        if (coords?.latitude && coords?.longitude) {
          ek.konum = `${coords.latitude.toFixed(6)}, ${coords.longitude.toFixed(6)}`;
        }
        this.cdr.markForCheck();
      });
    this.http.get<any[]>(`${API}/geo/neighborhoods-geopackage?district=${encodeURIComponent(ek.ilce)}`)
      .pipe(catchError(() => of([])))
      .subscribe(neighborhoods => { ek.mahalleler = neighborhoods; this.cdr.markForCheck(); });
  }

  ekKonumSil(index: number): void {
    this.ekKonumlar = this.ekKonumlar.filter((_, i) => i !== index);
    this.cdr.markForCheck();
  }

  private saveEkKonumlar(olayId: string): void {
    this.ekKonumlar.forEach((ek, index) => {
      const coords = this.parseKonum(ek.konum);
      if (!coords) return;
      this.http.post(`${API}/olay/${olayId}/rota`, {
        noktaAdi: ek.mekan || null,
        latitude: coords.lat,
        longitude: coords.lng,
        siraNo: index + 1
      }).subscribe();
    });
    this.ekKonumlar = [];
  }
}
