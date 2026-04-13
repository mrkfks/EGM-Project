import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, switchMap, catchError, of, forkJoin, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OlayTuruService, OlayTuru as OlayTuruDto } from '../../services/olay-turu.service';
import { GerceklesmeSekliService, GerceklesmeSekli as GerceklesmeSekliDto } from '../../services/gerceklesme-sekli.service';
import { GeoService, District } from '../../services/geo.service';

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
  id: string; // Guid string format
  olayTuru?: string;
  il?: string;
  ilce?: string;
  tarih: string;
  hassasiyet: number;
  katilimciSayisi?: number;
  organizatorAd?: string;
  durum: number;
  takipNo?: string | null;
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
  filtreliIlceler: District[] = [];
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

  /** Gerceğleşenler formuna yüklenen planlanan olayın takipNo'su (null = yeni kayıt) */
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
    this.buildForm();
    this.loadLookups();
    this.loadPlannedEvents();
    this.loadCompletedEvents();

    // Başlama saatine göre kontrol mekanizması
    setInterval(() => {
      this.kontrolEtVeGuncelle();
    }, 60000); // Her 1 dakikada bir kontrol et
  }

  private decodeToken(): void {
    this.http.get<any>(`${API}/me`).subscribe({
      next: user => {
        this.tokenUserId = user.id;
        this.tokenRole = user.role;
        this.tokenCityId = user.cityId;
        this.cdr.markForCheck();
      },
      error: () => {
        this.tokenUserId = null;
        this.tokenRole = null;
        this.tokenCityId = null;
        this.cdr.markForCheck();
      }
    });
  }

  get isCityScoped(): boolean {
    return ['IlPersoneli', 'IlYoneticisi'].includes(this.tokenRole ?? '');
  }

  private normalizeDt(dt: string | null | undefined): string | null {
    if (!dt) return null;
    return dt.length === 16 ? dt + ':00' : dt;
  }

  private buildForm(): void {
    this.form = this.fb.group({
      olayTuru: ['', Validators.required],
      organizatorId: ['', Validators.required],
      konuId: ['', Validators.required],
      tarih: ['', Validators.required],
      latitude: [null, [Validators.min(-90), Validators.max(90)]],
      longitude: [null, [Validators.min(-180), Validators.max(180)]],
      katilimciSayisi: [null, Validators.min(0)],
      gozaltiSayisi: [null, Validators.min(0)],
      sehitOluSayisi: [null, Validators.min(0)],
      aciklama: ['', Validators.maxLength(1000)],
      evrakNumarasi: ['', Validators.maxLength(100)],
    });

    if (this.isCityScoped && this.tokenCityId) {
      const ilAdi = this.tumIller.find(i => i.osmId === this.tokenCityId)?.name ?? '';
      this.form.get('il')!.setValue(ilAdi);
      this.form.get('il')!.disable();
      this.form.get('cityId')!.setValue(this.tokenCityId);
      this.form.get('cityId')!.disable();
    }

    this.form.get('hassasiyet')!.valueChanges.subscribe(v => this.onHassasiyetChange(+v));

    this.form.get('il')!.valueChanges.subscribe((ilAdi: string) => {
      if (!ilAdi || ilAdi.trim() === '') {
        this.filtreliIlceler = [];
        this.filtreliMahalleler = [];
      } else {
        this.http.get<any[]>(`${API}/geo/districts-geopackage?province=${encodeURIComponent(ilAdi)}`)
          .pipe(catchError(() => of([])))
          .subscribe(districts => {
            this.filtreliIlceler = districts;
            this.form.get('ilce')!.setValue('');
            this.filtreliMahalleler = [];
            this.cdr.markForCheck();
          });
      }
    });

    this.form.get('ilce')?.valueChanges.subscribe((ilceName: string) => {
      this.form.get('mahalle')!.setValue('');
      if (!ilceName || ilceName.trim() === '') {
        this.filtreliMahalleler = [];
        this.form.get('mahalle')!.disable();
      } else {
        this.form.get('mahalle')!.enable();
        this.http.get<any[]>(`${API}/geo/neighborhoods-geopackage?district=${encodeURIComponent(ilceName)}`)
          .pipe(catchError(() => of([])))
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
    const matches = val.match(/-?\d+([.,]\d+)?/g);
    if (matches && matches.length === 2) {
      const lat = parseFloat(matches[0].replace(',', '.'));
      const lng = parseFloat(matches[1].replace(',', '.'));
      if (isNaN(lat) || isNaN(lng)) return null;
      return { lat, lng };
    }
    return null;
  }

  private isManualKonum = false;

  private setupLocationAutoFill(): void {
    this.form.get('il')?.valueChanges
      .pipe(distinctUntilChanged(), switchMap(n => n ? this.http.get<any>(`${API}/geo/get-coordinates?provinceName=${encodeURIComponent(n)}`).pipe(catchError(() => of(null))) : of(null)))
      .subscribe(c => {
        if (!this.isManualKonum && c?.latitude) {
          this.form.patchValue({ baslangicKonum: `${c.latitude.toFixed(6)}, ${c.longitude.toFixed(6)}`, latitude: c.latitude, longitude: c.longitude }, { emitEvent: false });
          this.cdr.markForCheck();
        }
      });

    this.form.get('ilce')?.valueChanges
      .pipe(distinctUntilChanged(), switchMap(n => {
        const p = this.form.get('il')?.value;
        if (!n || !p) return of(null);
        return this.http.get<any>(`${API}/geo/get-coordinates?provinceName=${encodeURIComponent(p)}&districtName=${encodeURIComponent(n)}`).pipe(catchError(() => of(null)));
      }))
      .subscribe(c => {
        if (!this.isManualKonum && c?.latitude) {
          this.form.patchValue({ baslangicKonum: `${c.latitude.toFixed(6)}, ${c.longitude.toFixed(6)}`, latitude: c.latitude, longitude: c.longitude }, { emitEvent: false });
          this.cdr.markForCheck();
        }
      });

    this.form.get('mahalle')?.valueChanges
      .pipe(distinctUntilChanged(), switchMap(n => {
        const p = this.form.get('il')?.value;
        const d = this.form.get('ilce')?.value;
        if (!n || !p || !d) return of(null);
        return this.http.get<any>(`${API}/geo/get-coordinates?provinceName=${encodeURIComponent(p)}&districtName=${encodeURIComponent(d)}&neighborhoodName=${encodeURIComponent(n)}`).pipe(catchError(() => of(null)));
      }))
      .subscribe(c => {
        if (!this.isManualKonum && c?.latitude) {
          this.form.patchValue({ baslangicKonum: `${c.latitude.toFixed(6)}, ${c.longitude.toFixed(6)}`, latitude: c.latitude, longitude: c.longitude }, { emitEvent: false });
          this.cdr.markForCheck();
        }
      });

    this.form.get('baslangicKonum')?.valueChanges
      .pipe(debounceTime(500), distinctUntilChanged(), switchMap(konum => {
        if (this.form.get('baslangicKonum')?.dirty) this.isManualKonum = true;
        const coords = this.parseKonum(konum);
        if (!coords) return of(null);
        return this.http.get<any>(`${API}/geo/resolve-location?latitude=${coords.lat}&longitude=${coords.lng}`).pipe(catchError(() => of(null)));
      }))
      .subscribe(location => {
        if (location) {
          const updateObj: any = {};
          if (location.province && !this.form.get('il')?.value) updateObj['il'] = location.province;
          if (location.district && !this.form.get('ilce')?.value) updateObj['ilce'] = location.district;
          if (location.neighborhood && !this.form.get('mahalle')?.value) updateObj['mahalle'] = location.neighborhood;
          if (Object.keys(updateObj).length > 0) {
            this.form.patchValue(updateObj, { emitEvent: false });
            this.cdr.markForCheck();
          }
        }
      });
  }

  private onHassasiyetChange(val: number): void {
    const h = HASSASIYET.find(x => x.value === val) ?? HASSASIYET[0];
    this.hassasiyetColor  = h.color;
    this.hassasiyetShadow = h.shadow;
  }

  private loadLookups(): void {
    this.http.get<any[]>(`${API}/geo/provinces-geopackage`).subscribe({
      next: res => { this.tumIller = res; this.cdr.markForCheck(); },
      error: () => {},
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
      next: res => { this.tumGerceklesmeSekilleri = res; this.filtreliGerceklesmeSekilleri = res; this.cdr.markForCheck(); },
      error: () => {},
    });

    this.form.get('olayTuru')!.valueChanges.subscribe((secilenId: string) => {
      this.form.get('olayinGerceklesmeSekli')!.setValue('');
      if (secilenId) {
        this.filtreliGerceklesmeSekilleri = this.tumGerceklesmeSekilleri.filter(s => s.olayTuruId === secilenId);
      } else {
        this.filtreliGerceklesmeSekilleri = this.tumGerceklesmeSekilleri;
      }
    });
  }

  goBack(): void { this.router.navigate(['/olay']); }

  loadCompletedEvents(): void {
    this.isLoadingCompleted = true;
    this.http.get<any>(`${API}/olay?durum=1&sayfaBoyutu=100`).subscribe({
      next: res => {
        this.completedEvents = res?.items ?? [];
        this.isLoadingCompleted = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoadingCompleted = false;
        this.cdr.markForCheck();
      }
    });
  }

  loadPlannedEvents(): void {
    this.isLoadingPlanned = true;
    this.http.get<any>(`${API}/olay?durum=0&sayfaBoyutu=100`).subscribe({
      next: res => {
        this.userPlans = res?.items ?? [];
        this.isLoadingPlanned = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoadingPlanned = false;
        this.cdr.markForCheck();
      }
    });
  }

  switchToPlannedForm(): void {
    this.activeForm = 'planned';
    this.selectedPlanId = null;
    this.formError = null;
    this.formSuccess = null;
    this.ekKonumlar = [];
    this.form.reset();
    this.isManualKonum = false;
    if (this.isCityScoped && this.tokenCityId) {
      const ilAdi = this.tumIller.find((i: any) => i.osmId === this.tokenCityId)?.name ?? '';
      this.form.get('il')!.setValue(ilAdi);
    }
    this.cdr.markForCheck();
  }

  switchToCompletedForm(): void {
    this.activeForm = 'completed';
    this.selectedPlanId = null;
    this.formError = null;
    this.formSuccess = null;
    this.ekKonumlar = [];
    this.form.reset();
    this.isManualKonum = false;
    if (this.isCityScoped && this.tokenCityId) {
      const ilAdi = this.tumIller.find((i: any) => i.osmId === this.tokenCityId)?.name ?? '';
      this.form.get('il')!.setValue(ilAdi);
    }
    this.cdr.markForCheck();
  }

  selectPlanForCompletion(plan: OlayListItem): void {
    this.selectedPlanId = plan.takipNo ?? null; // undefined durumunda null atanıyor
    this.activeForm = 'completed';
    this.formError = null;
    this.formSuccess = null;

    this.http.get<any>(`${API}/olay/${plan.takipNo}`).subscribe({
      next: detail => {
        this.form.patchValue({
          olayTuru:         this.olayTurleri.find(t => t.name === detail.olayTuru)?.id ?? '',
          organizatorId:    detail.organizatorId ?? '',
          konuId:           detail.konuId ?? '',
          tarih:            detail.tarih ? detail.tarih.substring(0, 16) : '',
          il:               detail.il ?? '',
          ilce:             detail.ilce ?? '',
          mahalle:          detail.mahalle ?? '',
          baslangicKonum: detail.latitude != null && detail.longitude != null ? `${detail.latitude},${detail.longitude}` : '',
          mekan:            detail.mekan ?? '',
          katilimciSayisi:  detail.katilimciSayisi ?? null,
          hassasiyet:       detail.hassasiyet ?? 0,
          aciklama:         detail.aciklama ?? '',
          evrakNumarasi:    detail.evrakNumarasi ?? '',
          cityId:           detail.cityId ?? null
        });
        if (this.isCityScoped) this.form.get('il')!.disable();
        this.cdr.markForCheck();
      },
      error: () => {
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
      il:              v.il,
      ilce:            v.ilce || null,
      mahalle:         v.mahalle || null,
      mekan:           v.mekan || null,
      latitude:        this.parseKonum(v.baslangicKonum)?.lat ?? v.latitude ?? null,
      longitude:       this.parseKonum(v.baslangicKonum)?.lng ?? v.longitude ?? null,
      katilimciSayisi: v.katilimciSayisi ?? null,
      hassasiyet:      +v.hassasiyet,
      cityId:          v.cityId ?? null,
      durum:           2
    };

    this.http.put<void>(`${API}/olay/${planId}`, payload).subscribe({
      next: () => {
        this.isSaving = false;
        this.formSuccess = 'Etkinlik iptal edildi.';
        const plan = this.userPlans.find(p => p.takipNo === planId);
        if (!plan) {
            console.error('Plan bulunamadı.');
            return;
        }
        this.userPlans = this.userPlans.filter(p => p.takipNo !== planId);

        this.http.put<void>(`${API}/olay/${plan.takipNo}`, { ...plan, durum: 3 }).subscribe({
          next: () => {
              plan.durum = 3;
              this.cdr.markForCheck();
          },
          error: (err) => {
              console.error('Hata:', err);
          }
        });
      },
      error: () => {
        this.isSaving = false;
        this.formError = 'İptal işlemi sırasında bir hata oluştu.';
        this.cdr.markForCheck();
      }
    });
  }

  kaydet(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.formError = 'Lütfen tüm zorunlu alanları doldurun.';
      return;
    }

    this.isSaving = true;
    this.formError = null;
    const payload = this.form.getRawValue();

    if (this.selectedPlanId && this.activeForm === 'completed') {
      this.http.put<void>(`${API}/olay/${this.selectedPlanId}`, payload).subscribe({
        next: () => {
          this.isSaving = false;
          this.formSuccess = 'Olay gerçekleşenlere taşındı.';
          this.loadPlannedEvents();
          this.loadCompletedEvents();
          this.selectedPlanId = null;
          this.form.reset();
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.isSaving = false;
          this.formError = err?.error?.detail ?? 'Kayıt sırasında hata oluştu.';
          this.cdr.markForCheck();
        }
      });
      return;
    }

    this.http.post<OlayListItem>(`${API}/olay`, payload).subscribe({
      next: created => {
        this.isSaving = false;
        this.formSuccess = 'Olay başarıyla kaydedildi.';
        if (this.activeForm === 'planned') {
          this.userPlans = [created, ...this.userPlans];
        } else {
          this.completedEvents = [created, ...this.completedEvents];
        }
        this.form.reset();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isSaving = false;
        this.formError = err?.error?.detail ?? 'Kayıt sırasında hata oluştu.';
        this.cdr.markForCheck();
      }
    });
  }

  private kontrolEtVeGuncelle(): void {
    const simdi = new Date();
    this.userPlans.forEach(plan => {
      const baslangicTarihi = new Date(plan.tarih);
      if (baslangicTarihi <= simdi && plan.durum === 0) {
        this.http.put<void>(`${API}/olay/${plan.takipNo}`, { ...plan, durum: 3 }).subscribe({
          next: () => { plan.durum = 3; this.cdr.markForCheck(); }
        });
      }
    });
  }

  trackById(_index: number, item: { id: string }): string { return item.id; }
  
  saveEkKonumlar(olayId: string): void {
     // Implement ek konum saving logic if needed
  }

  ekKonumEkle(): void {
    this.ekKonumlar.push({
      il: '',
      ilce: '',
      mahalle: '',
      konum: '',
      mekan: '',
      katilimciSayisi: null,
      ilceler: [],
      mahalleler: []
    });
  }

  ekKonumSil(index: number): void {
    this.ekKonumlar.splice(index, 1);
  }

  ekKonumIlDegisti(index: number): void {
    console.log(`İl değişti: ${this.ekKonumlar[index].il}`);
  }

  ekKonumIlceDegisti(index: number): void {
    console.log(`İlçe değişti: ${this.ekKonumlar[index].ilce}`);
  }
}
