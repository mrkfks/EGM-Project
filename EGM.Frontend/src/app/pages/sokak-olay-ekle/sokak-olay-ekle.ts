import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OlayTuruService, OlayTuru } from '../../services/olay-turu.service';
import { GerceklesmeSekliService, GerceklesmeSekli } from '../../services/gerceklesme-sekli.service';
import { GroupService, Group } from '../../services/group.service';
import { FileUploadService } from '../../services/file-upload.service';

const API = environment.apiUrl + '/api';

@Component({
  selector: 'app-sokak-olay-ekle',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
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
  gruplar: Group[] = [];
  iller: any[] = [];

  readonly isBrowser: boolean;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private olayTuruService: OlayTuruService,
    private sekilService: GerceklesmeSekliService,
    private groupService: GroupService,
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
      participantGroupIds: [[]],
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
      platform: [platform],
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
  addLocation() { this.locations.push(this.createLocationGroup()); }
  removeLocation(index: number) { this.locations.removeAt(index); }

  addResource() { this.resources.push(this.createResourceGroup()); }
  removeResource(index: number) { this.resources.removeAt(index); }

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
    this.groupService.getAll().subscribe(res => { this.gruplar = res; this.cdr.markForCheck(); });
    
    this.http.get<any[]>(`${API}/organizator`).subscribe(res => { this.organizatorler = res; this.cdr.markForCheck(); });
    this.http.get<any[]>(`${API}/organizator/konu`).subscribe(res => { this.konular = res; this.cdr.markForCheck(); });
    this.http.get<any[]>(`${API}/geo/provinces-geopackage`).subscribe(res => { this.iller = res; this.cdr.markForCheck(); });
  }

  kaydet(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.formError = "Lütfen zorunlu alanları doldurun.";
      return;
    }

    this.isSaving = true;
    this.formError = null;
    this.formSuccess = null;

    const payload = this.form.getRawValue();
    
    // Tarihlerin normalize edilmesi
    if (payload.baslangicTarihi) payload.baslangicTarihi = new Date(payload.baslangicTarihi).toISOString();
    if (payload.bitisTarihi) payload.bitisTarihi = new Date(payload.bitisTarihi).toISOString();

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
          window.scrollTo(0, 0);
        },
        error: (err) => {
          this.formError = err.error?.message || "Kayıt sırasında bir hata oluştu.";
        }
      });
  }

  goBack() { this.router.navigate(['/olay']); }
}
