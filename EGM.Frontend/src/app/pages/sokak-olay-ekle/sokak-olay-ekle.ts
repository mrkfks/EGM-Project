import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OlayTuruService, OlayTuru } from '../../services/olay-turu.service';
import { GerceklesmeSekliService, GerceklesmeSekli } from '../../services/gerceklesme-sekli.service';
import { FileUploadService, FileUploadResponse } from '../../services/file-upload.service';

const API = environment.apiUrl + '/api';

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
  isSaving = false;
  formError: string | null = null;
  formSuccess: string | null = null;
  takipNo: string | null = null;

  // Lookups
  olayTurleri: OlayTuru[] = [];
  sekiller: GerceklesmeSekli[] = [];
  organizatorler: any[] = [];
  konular: any[] = [];
  iller: any[] = [];

  // Katılımcı Gruplar
  selectedParticipantOrgs: { id: string; ad: string }[] = [];
  seciliOrgId = '';

  // Belge yükleme
  belgeler: { ad: string; url: string; boyut: number }[] = [];
  belgeYukleniyor = false;
  belgeDragOver = false;
  belgeHata: string | null = null;

  private izinliUzantilar = ['.jpg','.jpeg','.png','.tiff','.tif','.gif','.pdf','.docx','.xlsx'];

  belgeDosyaIkonu(ad: string): string {
    const ext = ad.split('.').pop()?.toLowerCase();
    const map: Record<string, string> = {
      pdf: '📄', docx: '📝', xlsx: '📊',
      jpg: '🖼️', jpeg: '🖼️', png: '🖼️', tiff: '🖼️', tif: '🖼️', gif: '🖼️'
    };
    return map[ext ?? ''] ?? '📎';
  }

  belgeBoyutu(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  belgeDrop(event: DragEvent): void {
    event.preventDefault();
    this.belgeDragOver = false;
    const files = event.dataTransfer?.files;
    if (files) this.belgeDosyalariYukle(Array.from(files));
  }

  belgeDosyaSecildi(event: any): void {
    const files: File[] = Array.from(event.target.files || []);
    this.belgeDosyalariYukle(files);
    event.target.value = '';
  }

  belgeDosyalariYukle(files: File[]): void {
    this.belgeHata = null;
    for (const file of files) {
      const ext = '.' + file.name.split('.').pop()?.toLowerCase();
      if (!this.izinliUzantilar.includes(ext)) {
        this.belgeHata = `"${file.name}" desteklenmiyor. İzin verilen: ${this.izinliUzantilar.join(', ')}`;
        this.cdr.markForCheck();
        continue;
      }
      if (file.size > 10 * 1024 * 1024) {
        this.belgeHata = `"${file.name}" 10 MB sınırını aşıyor.`;
        this.cdr.markForCheck();
        continue;
      }
      this.belgeYukleniyor = true;
      this.uploadService.upload(file).subscribe({
        next: (res: FileUploadResponse) => {
          this.belgeler.push({ ad: file.name, url: res.url, boyut: file.size });
          this.belgeYukleniyor = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.belgeHata = `"${file.name}" yüklenirken hata oluştu.`;
          this.belgeYukleniyor = false;
          this.cdr.markForCheck();
        }
      });
    }
  }

  belgeSil(index: number): void {
    this.belgeler.splice(index, 1);
    this.cdr.markForCheck();
  }

  // Kişi detayları
  kisiDetaylari: Record<string, { ad: string; soyad: string; tc: string }[]> = {
    katilimci: [], supheli: [], gozalti: [], sehit: [], olu: []
  };
  acikKisiPanel: string | null = null;
  yeniKisi = { ad: '', soyad: '', tc: '' };

  get kisiPanelBaslik(): string {
    const map: Record<string, string> = {
      katilimci: 'Katılımcı', supheli: 'Şüpheli',
      gozalti: 'Gözaltı', sehit: 'Şehit', olu: 'Ölü'
    };
    return map[this.acikKisiPanel ?? ''] ?? '';
  }

  toggleKisiPanel(kategori: string): void {
    this.acikKisiPanel = this.acikKisiPanel === kategori ? null : kategori;
    this.yeniKisi = { ad: '', soyad: '', tc: '' };
    this.cdr.markForCheck();
  }

  kisiEkle(kategori: string): void {
    const kisi = { ...this.yeniKisi };
    if (!kisi.ad.trim()) return;
    this.kisiDetaylari[kategori].push(kisi);
    this.yeniKisi = { ad: '', soyad: '', tc: '' };
    // Sayıyı otomatik güncelle
    const sayiKey: Record<string, string> = {
      katilimci: 'katilimciSayisi', supheli: 'supheliSayisi',
      gozalti: 'gozaltiSayisi', sehit: 'sehitSayisi', olu: 'oluSayisi'
    };
    const ctrl = this.form.get('details')?.get(sayiKey[kategori]);
    if (ctrl && (ctrl.value === 0 || ctrl.value === null)) {
      ctrl.setValue(this.kisiDetaylari[kategori].length);
    }
    this.cdr.markForCheck();
  }

  kisiKaldir(kategori: string, index: number): void {
    this.kisiDetaylari[kategori].splice(index, 1);
    this.cdr.markForCheck();
  }

  get organizatorlerFiltreli(): any[] {
    const seciliIds = this.selectedParticipantOrgs.map(o => o.id);
    return this.organizatorler.filter(o => !seciliIds.includes(o.id));
  }

  addParticipantOrg(): void {
    if (!this.seciliOrgId) return;
    const org = this.organizatorler.find(o => o.id === this.seciliOrgId);
    if (!org) return;
    this.selectedParticipantOrgs.push({ id: org.id, ad: org.ad });
    this.seciliOrgId = '';
    this.form.patchValue({ participantOrganizatorIds: this.selectedParticipantOrgs.map(o => o.id) });
    this.cdr.markForCheck();
  }

  removeParticipantOrg(id: string): void {
    this.selectedParticipantOrgs = this.selectedParticipantOrgs.filter(o => o.id !== id);
    this.form.patchValue({ participantOrganizatorIds: this.selectedParticipantOrgs.map(o => o.id) });
    this.cdr.markForCheck();
  }
  
  // Sosyal medya platformları
  platforms: string[] = ['Twitter', 'Facebook', 'Instagram', 'TikTok', 'YouTube', 'WhatsApp', 'Telegram', 'Discord', 'Reddit', 'LinkedIn'];

  // Her konum için ilçe ve mahalle listeleri
  locationDistricts: string[][] = [[]];
  locationNeighborhoods: string[][] = [[]];

  // Custom platform input visibility (her resource için)
  customPlatformInputVisible: boolean[] = [];

  readonly isBrowser: boolean;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private olayTuruService: OlayTuruService,
    private sekilService: GerceklesmeSekliService,
    private uploadService: FileUploadService,
    private cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    this.buildForm();
    this.loadLookups();
  }

  private buildForm(): void {
    this.form = this.fb.group({
      turId: ['', Validators.required],
      sekilId: ['', Validators.required],
      konuId: ['', Validators.required],
      organizatorId: ['', Validators.required],
      baslangicTarihi: ['', Validators.required],
      bitisTarihi: [''],
      aciklama: ['', [Validators.required, Validators.maxLength(2000)]],
      cityId: [null],
      locations: this.fb.array([this.createLocationGroup()]),
      resources: this.fb.array([]),
      details: this.fb.group({
        hassasiyet: [0, Validators.required],
        katilimciSayisi: [0, Validators.min(0)],
        supheliSayisi: [0, Validators.min(0)],
        gozaltiSayisi: [0, Validators.min(0)],
        sehitSayisi: [0, Validators.min(0)],
        oluSayisi: [0, Validators.min(0)],
      }),
      participantOrganizatorIds: [[]],
      durum: [0] // Planlanan
    });

    // Durum değişikliğine göre detay zorunluluğu (Gerçekleşen = 2)
    this.form.get('durum')?.valueChanges.subscribe(status => {
      const details = this.form.get('details');
      if (status === 2) {
        details?.get('katilimciSayisi')?.setValidators([Validators.required, Validators.min(0)]);
      } else {
        details?.get('katilimciSayisi')?.setValidators([Validators.min(0)]);
      }
      details?.get('katilimciSayisi')?.updateValueAndValidity();
    });
  }

  private createLocationGroup(): FormGroup {
    return this.fb.group({
      il: ['', Validators.required],
      ilce: [''],
      mahalle: [''],
      mekan: [''],
      latitude: [null],
      longitude: [null]
    });
  }

  private createResourceGroup(platform = '', user = '', link = '', path = ''): FormGroup {
    return this.fb.group({
      platforms: this.fb.array(
        platform ? platform.split(',').map(p => this.fb.control(p.trim())) : []
      ),
      kullaniciAdi: [user],
      link: [link],
      gorselPath: [path],
      aciklama: ['']
    });
  }

  // --- Getters ---
  get locations() { return this.form.get('locations') as FormArray; }
  get resources() { return this.form.get('resources') as FormArray; }

  // --- Form Actions ---
  // FormArray'e yeni konum eklenince districts/neighborhoods dizilerini de ekle
  addLocation() {
    this.locations.push(this.createLocationGroup());
    this.locationDistricts.push([]);
    this.locationNeighborhoods.push([]);
  }
    removeLocation(index: number) {
      this.locations.removeAt(index);
      this.locationDistricts.splice(index, 1);
      this.locationNeighborhoods.splice(index, 1);
    }
  // İl değişince ilçeleri yükle (Promise döner)
  onIlChange(index: number): Promise<void> {
    return new Promise((resolve) => {
      const il = this.locations.at(index).get('il')?.value;
      if (!il) {
        this.locationDistricts[index] = [];
        this.locationNeighborhoods[index] = [];
        this.locations.at(index).patchValue({ ilce: '', mahalle: '' });
        this.cdr.markForCheck();
        resolve();
        return;
      }
      this.http.get<any[]>(`${API}/geo/districts-geopackage?province=${encodeURIComponent(il)}`)
        .subscribe(res => {
          this.locationDistricts[index] = res.map((d: any) => d.name);
          this.locationNeighborhoods[index] = [];
          this.locations.at(index).patchValue({ ilce: '', mahalle: '' });
          this.cdr.markForCheck();
          resolve();
        });
    });
  }

  // İlçe değişince mahalleleri yükle (Promise döner)
  onIlceChange(index: number): Promise<void> {
    return new Promise((resolve) => {
      const ilce = this.locations.at(index).get('ilce')?.value;
      if (!ilce) {
        this.locationNeighborhoods[index] = [];
        this.locations.at(index).patchValue({ mahalle: '' });
        this.cdr.markForCheck();
        resolve();
        return;
      }
      this.http.get<any[]>(`${API}/geo/neighborhoods-geopackage?district=${encodeURIComponent(ilce)}`)
        .subscribe(res => {
          this.locationNeighborhoods[index] = res.map((m: any) => m.name);
          this.locations.at(index).patchValue({ mahalle: '' });
          this.cdr.markForCheck();
          resolve();
        });
    });
  }

  // Mahalle değişince koordinatları otomatik doldur
  onMahalleChange(index: number) {
    const il = this.locations.at(index).get('il')?.value;
    const ilce = this.locations.at(index).get('ilce')?.value;
    const mahalle = this.locations.at(index).get('mahalle')?.value;

    if (!mahalle) {
      this.locations.at(index).patchValue({ latitude: null, longitude: null });
      this.cdr.markForCheck();
      return;
    }

    const params = new URLSearchParams();
    if (il) params.append('provinceName', il);
    if (ilce) params.append('districtName', ilce);
    if (mahalle) params.append('neighborhoodName', mahalle);

    this.http.get<any>(`${API}/geo/get-coordinates?${params.toString()}`)
      .subscribe({
        next: (res) => {
          if (res.latitude && res.longitude) {
            this.locations.at(index).patchValue({
              latitude: res.latitude,
              longitude: res.longitude
            });
            this.cdr.markForCheck();
          }
        },
        error: (err) => {
          console.error('Koordinat alma hatası:', err);
          this.locations.at(index).patchValue({ latitude: null, longitude: null });
          this.cdr.markForCheck();
        }
      });
  }

  // Şu anki konumu al ve form alanlarını otomatik doldur
  onUseCurrentLocation(index: number) {
    if (!this.isBrowser || !navigator.geolocation) {
      this.formError = 'Tarayıcı konum hizmetini desteklemiyor.';
      this.cdr.markForCheck();
      return;
    }

    // Geolocation'dan konum al
    navigator.geolocation.getCurrentPosition(
      (position) => {
        const { latitude, longitude } = position.coords;

        // Backend'den il/ilçe/mahalle bilgilerini al
        this.http.get<any>(`${API}/geo/resolve-location?latitude=${latitude}&longitude=${longitude}`)
          .subscribe({
            next: async (res) => {
              try {
                // 1. İl doldur
                this.locations.at(index).patchValue({
                  il: res.province || '',
                  latitude: latitude,
                  longitude: longitude
                });
                this.cdr.markForCheck();

                // 2. İl için ilçeleri yükle
                if (res.province) {
                  await this.onIlChange(index);
                  
                  // 3. İlçe doldur (eğer varsa)
                  if (res.district) {
                    this.locations.at(index).patchValue({ ilce: res.district });
                    this.cdr.markForCheck();

                    // 4. İlçe için mahalleleri yükle
                    await this.onIlceChange(index);

                    // 5. Mahalle doldur (eğer varsa)
                    if (res.neighborhood) {
                      this.locations.at(index).patchValue({ mahalle: res.neighborhood });
                      this.cdr.markForCheck();
                    }
                  }
                }

                this.formSuccess = 'Mevcut konum başarıyla yüklendi.';
                this.cdr.markForCheck();

                // Success mesajını 3 saniye sonra kaldır
                setTimeout(() => {
                  this.formSuccess = null;
                  this.cdr.markForCheck();
                }, 3000);
              } catch (error) {
                console.error('Konum işleme hatası:', error);
                this.formError = 'Konum işlenirken hata oluştu.';
                this.cdr.markForCheck();
              }
            },
            error: (err) => {
              this.formError = 'Konum bilgisi çözümlenemedi. Manuel olarak giriniz.';
              console.error('Konum çözümleme hatası:', err);
              this.cdr.markForCheck();
            }
          });
      },
      (error) => {
        let errorMsg = 'Konum alınamadı. ';
        if (error.code === error.PERMISSION_DENIED) {
          errorMsg += 'Lütfen tarayıcı izinlerini kontrol ediniz.';
        } else if (error.code === error.POSITION_UNAVAILABLE) {
          errorMsg += 'Konum servisi kullanılabilir değil.';
        } else if (error.code === error.TIMEOUT) {
          errorMsg += 'Konum isteği zaman aşımına uğradı.';
        }
        this.formError = errorMsg;
        this.cdr.markForCheck();
      },
      { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
    );
  }

  addResource() { 
    this.resources.push(this.createResourceGroup());
    this.customPlatformInputVisible.push(false);
  }
  removeResource(index: number) { 
    this.resources.removeAt(index);
    this.customPlatformInputVisible.splice(index, 1);
  }

  // Custom platform input görünürlüğü
  isCustomPlatformInputVisible(resourceIndex: number): boolean {
    return this.customPlatformInputVisible[resourceIndex] || false;
  }

  showCustomPlatformInput(resourceIndex: number) {
    this.customPlatformInputVisible[resourceIndex] = true;
    this.cdr.markForCheck();
    // Input'a focus ver
    setTimeout(() => {
      const input = document.querySelector(`input[placeholder="Platform adı..."]`) as HTMLInputElement;
      input?.focus();
    }, 0);
  }

  hideCustomPlatformInput(resourceIndex: number) {
    this.customPlatformInputVisible[resourceIndex] = false;
    this.cdr.markForCheck();
  }

  // Platform seçimi/kaldırma
  isPlatformSelected(resourceIndex: number, platformName: string): boolean {
    const platformsArray = this.resources.at(resourceIndex).get('platforms') as FormArray;
    return platformsArray.value.includes(platformName);
  }

  togglePlatform(resourceIndex: number, platformName: string) {
    const platformsArray = this.resources.at(resourceIndex).get('platforms') as FormArray;
    const index = platformsArray.value.indexOf(platformName);
    
    if (index >= 0) {
      platformsArray.removeAt(index);
    } else {
      platformsArray.push(this.fb.control(platformName));
    }
    this.cdr.markForCheck();
  }

  // Seçili platform'ı sil (index ile)
  removePlatform(resourceIndex: number, platformIndex: number) {
    const platformsArray = this.resources.at(resourceIndex).get('platforms') as FormArray;
    platformsArray.removeAt(platformIndex);
    this.cdr.markForCheck();
  }

  // Seçili platform'ı sil (adı ile)
  removePlatformByName(resourceIndex: number, platformName: string) {
    const platformsArray = this.resources.at(resourceIndex).get('platforms') as FormArray;
    const index = platformsArray.value.indexOf(platformName);
    if (index >= 0) {
      platformsArray.removeAt(index);
      this.cdr.markForCheck();
    }
  }

  // Platform listesinden tamamen kaldır (tüm kaynakların seçiminden de çıkar)
  removePlatformFromList(platformName: string) {
    this.platforms = this.platforms.filter(p => p !== platformName);
    // Seçili olduğu tüm kaynaklardan da kaldır
    this.resources.controls.forEach((_, idx) => {
      this.removePlatformByName(idx, platformName);
    });
    this.cdr.markForCheck();
  }

  // Custom platform ekle
  addCustomPlatform(resourceIndex: number, customPlatformInput: HTMLInputElement) {
    const customName = customPlatformInput.value?.trim();
    if (!customName) {
      this.formError = 'Platform adı boş olamaz.';
      this.cdr.markForCheck();
      return;
    }

    const platformsArray = this.resources.at(resourceIndex).get('platforms') as FormArray;
    
    // Zaten var mı kontrol et
    if (platformsArray.value.includes(customName)) {
      this.formError = 'Bu platform zaten seçili.';
      this.cdr.markForCheck();
      return;
    }

    // Yeni platform ekle
    platformsArray.push(this.fb.control(customName));
    
    // Input'u temizle ve input bölümünü gizle
    customPlatformInput.value = '';
    this.hideCustomPlatformInput(resourceIndex);
    
    this.formError = null;
    this.cdr.markForCheck();
  }

  getSecureUrl(path: string | null | undefined): string | null {
    if (!path) return null;
    const token = this.isBrowser ? localStorage.getItem('token') : null;
    return token ? `${path}?access_token=${token}` : path;
  }

  onFileSelected(event: any, index: number) {
    const file: File = event.target.files[0];
    if (file) {
      this.uploadService.upload(file).subscribe({
        next: (res) => {
          this.resources.at(index).patchValue({ gorselPath: res.url });
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.formError = "Dosya yüklenemedi: " + (err.error || "Sunucu hatası");
          this.cdr.markForCheck();
        }
      });
    }
  }

  private loadLookups(): void {
    this.olayTuruService.getAll().subscribe(res => { this.olayTurleri = res; this.cdr.markForCheck(); });
    this.sekilService.getAll().subscribe(res => { this.sekiller = res; this.cdr.markForCheck(); });
    
    this.http.get<any[]>(`${API}/organizator`).subscribe(res => { this.organizatorler = res; this.cdr.markForCheck(); });
    this.http.get<any[]>(`${API}/organizator/konu`).subscribe(res => { this.konular = res; this.cdr.markForCheck(); });
    this.http.get<any[]>(`${API}/geo/provinces-geopackage`).subscribe(res => { this.iller = res; this.cdr.markForCheck(); });

     // FormArray dinamikleri için districts/neighborhoods dizilerini başlat
     this.locations.controls.forEach((_, i) => {
      this.locationDistricts[i] = [];
      this.locationNeighborhoods[i] = [];
     });
  }

  get baslangicGecmis(): boolean {
    const val = this.form.get('baslangicTarihi')?.value;
    if (!val) return false;
    // 5 dakika tolerans: 5 dk önceye kadar girilmiş tarihler planlanan olarak kabul edilir
    const tolerans = 5 * 60 * 1000;
    return new Date(val).getTime() < (Date.now() - tolerans);
  }

  kaydet(durum: number): void {
    if (durum === 0 && this.baslangicGecmis) {
      this.formError = 'Başlangıç tarihi 5 dakikadan fazla geçmiş; planlanan olay olarak bildirilemez.';
      window.scrollTo(0, 0);
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.formError = 'Lütfen zorunlu alanları doldurun.';
      window.scrollTo(0, 0);
      return;
    }

    this.form.get('durum')?.setValue(durum);

    this.isSaving = true;
    this.formError = null;
    this.formSuccess = null;

    const payload = this.form.getRawValue();
    payload.kisiDetaylari = this.kisiDetaylari;
    payload.belgeler = this.belgeler.map(b => ({ ad: b.ad, url: b.url }));
    
    // Tarihlerin normalize edilmesi
    if (payload.baslangicTarihi) payload.baslangicTarihi = new Date(payload.baslangicTarihi).toISOString();
    payload.bitisTarihi = payload.bitisTarihi ? new Date(payload.bitisTarihi).toISOString() : null;

    // Platforms FormArray'ını string'e çevir (comma-separated)
    if (payload.resources && Array.isArray(payload.resources)) {
      payload.resources = payload.resources.map((res: any) => ({
        ...res,
        platform: Array.isArray(res.platforms) ? res.platforms.join(', ') : ''
      }));
      // platforms field'ını sil
      payload.resources.forEach((res: any) => delete res.platforms);
    }

    this.http.post<any>(`${API}/olay`, payload)
      .pipe(finalize(() => { this.isSaving = false; this.cdr.markForCheck(); }))
      .subscribe({
        next: (res) => {
          this.formSuccess = `Olay başarıyla kaydedildi. Olay No: ${res.olayNo}`;
          this.takipNo = res.olayNo;
          this.form.reset({ durum: 0, details: { hassasiyet: 0 } });
          this.locations.clear();
          this.locations.push(this.createLocationGroup());
          this.resources.clear();
          this.kisiDetaylari = { katilimci: [], supheli: [], gozalti: [], sehit: [], olu: [] };
          this.selectedParticipantOrgs = [];
          this.acikKisiPanel = null;
          this.belgeler = [];
          this.belgeHata = null;
          window.scrollTo(0, 0);
        },
        error: (err) => {
          if (err.status === 400 && err.error?.errors) {
            // Model validation hataları
            const msgs = Object.values(err.error.errors).flat().join(' ');
            this.formError = msgs || err.error?.title || 'Geçersiz veri girişi.';
          } else if (err.status === 401) {
            this.formError = 'Oturum süreniz dolmuş. Lütfen yeniden giriş yapın.';
          } else {
            this.formError = err.error?.message || err.message || `Kayıt sırasında bir hata oluştu. (HTTP ${err.status})`;
          }
        }
      });
  }

  goBack() { this.router.navigate(['/olay']); }
}
