import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { GeoService } from '../../services/geo.service';

const API = 'http://localhost:5117/api';

const HASSASIYET_LABELS = ['Düşük', 'Orta', 'Yüksek', 'Kritik'];
const HASSASIYET_RENK   = ['#27ae60', '#f39c12', '#e74c3c', '#8e44ad'];

// ── Tip tanımları ──────────────────────────────────────────────────────────
declare var ILLER: string; // Frontend'den gelir
interface SokakOlay {
  id: string; olayTuru?: string; il?: string; ilce?: string;
  tarih: string; hassasiyet: number; katilimciSayisi?: number;
  organizatorAd?: string; konuAd?: string; durum: number;
  aciklama?: string; mekan?: string; evrakNumarasi?: string;
  gozaltiSayisi?: number; sehitOluSayisi?: number;
  createdByUserId: string; cityId?: number;
  takipNo?: string;
}

interface SosyalOlay {
  id: string; platform?: string; konu?: string; paylasimLinki?: string;
  paylasimTarihi: string; icerikOzeti?: string; ilgiliKisiKurum?: string;
  il?: string; ilce?: string; hassasiyet: number;
  createdByUserId: string; createdAt: string;
  takipNo?: string;
}

interface SecimOlay {
  id: string; musahitAdi?: string; il?: string; ilce?: string;
  mahalle?: string; okul?: string; konu?: string; sandikNo: number;
  olayKategorisi?: string; olaySaati: string; aciklama?: string;
  tarih: string; createdAt: string; createdByUserId: string;
  katilimciSayisi: number; sehitSayisi: number; oluSayisi: number; gozaltiSayisi: number;
  takipNo?: string;
}

interface VipOlay {
  id: string; ziyaretEdenAdSoyad?: string; unvan?: string;
  baslangicTarihi: string; bitisTarihi: string; il?: string; mekan?: string;
  hassasiyet: number; guvenlikSeviyesi?: string; gozlemNoktalari?: string;
  ziyaretDurumu: number; createdByUserId: string; createdAt: string;
  takipNo?: string;
}

type AktifSekme = 'sokak' | 'sosyal' | 'secim' | 'vip';

@Component({
  selector: 'app-veri-yonetimi',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './veri-yonetimi.html',
  styleUrls: ['./veri-yonetimi.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VeriYonetimi implements OnInit {

  ILLER: string[] = [];

  aktifSekme: AktifSekme = 'sokak';

  yukleniyor = false;
  kaydediliyor = false;
  hata: string | null = null;
  basari: string | null = null;
  modalAcik = false;

  // Veritabanı listeleri
  sokakOlaylar: SokakOlay[] = [];
  sosyalOlaylar: SosyalOlay[] = [];
  secimOlaylar: SecimOlay[] = [];
  vipOlaylar: VipOlay[] = [];

  // Seçilen kayıt
  secilenKayit: any = null;

  // Form modelleri (basit nesne, iki yönlü binding ile)
  formSokak:  any = {};
  formSosyal: any = {};
  formSecim:  any = {};
  formVip:    any = {};

  readonly hassasiyetler = HASSASIYET_LABELS.map((l, i) => ({ value: i, label: l, renk: HASSASIYET_RENK[i] }));

  readonly PLATFORMLAR = ['Twitter / X','Facebook','Instagram','YouTube','TikTok','Telegram','WhatsApp','LinkedIn','Diğer'];
  readonly KATEGORILER = ['Müşahit Engellenmesi','Mükerrer Oy Denemesi','Seçmen Baskısı','Sandık Usulsüzlüğü','Fiziksel Çatışma','Kural İhlali','Diğer'];
  readonly VIP_UNVANLAR = ['Bakan','Milletvekili','Vali','Kaymakam','Emniyet Müdürü','Parti Yetkilisi','Diğer'];
  readonly GUVENLIK_SEVIYELERI = ['Normal','Yoğun Güvenlik','Kritik Durum'];
  readonly VIP_DURUMLAR: Record<number, string> = { 0: 'Planlandı', 1: 'Varış Yapıldı', 2: 'Ayrıldı', 3: 'İptal Edildi' };
  readonly OLAY_DURUMLAR: Record<number, string> = { 0: 'Planlandı', 1: 'Devam Ediyor', 2: 'Gerçekleşti', 3: 'İptal' };

  constructor(private http: HttpClient, private geoService: GeoService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadProvinces();
  }

  private loadProvinces(): void {
    this.geoService.getProvinces().subscribe(
      (provinces: any[]) => {
        this.ILLER = provinces.map(p => typeof p === 'string' ? p : p.name);
        this.cdr.markForCheck();
      },
      (error) => {
        console.error('Error loading provinces:', error);
      }
    );
  }

  private token(): HttpHeaders {
    const t = localStorage.getItem('token') ?? '';
    return new HttpHeaders({ Authorization: `Bearer ${t}` });
  }

  // ── Veri yükleme ────────────────────────────────────────────────────────
  tumVerileriYukle(): void {
    this.yukleniyor = true;
    this.hata = null;
    const opts = { headers: this.token() };
    let kalan = 4;
    const bitti = () => { if (--kalan === 0) { this.yukleniyor = false; this.cdr.markForCheck(); } };

    this.http.get<any>(`${API}/olay?sayfa=1&sayfaBoyutu=500`, opts).subscribe({
      next: r => { this.sokakOlaylar = r.items ?? []; bitti(); },
      error: () => bitti(),
    });
    this.http.get<SosyalOlay[]>(`${API}/sosyalmedyaolay`, opts).subscribe({
      next: r => { this.sosyalOlaylar = r; bitti(); },
      error: () => bitti(),
    });
    this.http.get<SecimOlay[]>(`${API}/secim/sandik-olay`, opts).subscribe({
      next: r => { this.secimOlaylar = r; bitti(); },
      error: () => bitti(),
    });
    this.http.get<VipOlay[]>(`${API}/vipziyaret`, opts).subscribe({
      next: r => { this.vipOlaylar = r; bitti(); },
      error: () => bitti(),
    });
  }

  // ── Sekme ────────────────────────────────────────────────────────────────
  sekmeSec(s: AktifSekme): void { this.aktifSekme = s; this.modalKapat(); }

  // ── Satıra tıklama → modal aç ────────────────────────────────────────────
  satirSec(kayit: any): void {
    this.secilenKayit = kayit;
    this.hata = null;
    this.basari = null;

    if (this.aktifSekme === 'sokak') {
      this.formSokak = {
        olayTuru: kayit.olayTuru ?? '',
        il: kayit.il ?? '',
        ilce: kayit.ilce ?? '',
        mekan: kayit.mekan ?? '',
        tarih: kayit.tarih ? kayit.tarih.substring(0, 10) : '',
        katilimciSayisi: kayit.katilimciSayisi ?? null,
        gozaltiSayisi: kayit.gozaltiSayisi ?? null,
        sehitOluSayisi: kayit.sehitOluSayisi ?? null,
        hassasiyet: kayit.hassasiyet ?? 0,
        aciklama: kayit.aciklama ?? '',
        evrakNumarasi: kayit.evrakNumarasi ?? '',
        durum: kayit.durum ?? 0,
      };
    } else if (this.aktifSekme === 'sosyal') {
      this.formSosyal = {
        platform: kayit.platform ?? '',
        konu: kayit.konu ?? '',
        paylasimLinki: kayit.paylasimLinki ?? '',
        paylasimTarihi: kayit.paylasimTarihi ? kayit.paylasimTarihi.substring(0, 16) : '',
        icerikOzeti: kayit.icerikOzeti ?? '',
        ilgiliKisiKurum: kayit.ilgiliKisiKurum ?? '',
        il: kayit.il ?? '',
        ilce: kayit.ilce ?? '',
        hassasiyet: kayit.hassasiyet ?? 0,
      };
    } else if (this.aktifSekme === 'secim') {
      this.formSecim = {
        musahitAdi: kayit.musahitAdi ?? '',
        il: kayit.il ?? '',
        ilce: kayit.ilce ?? '',
        mahalle: kayit.mahalle ?? '',
        okul: kayit.okul ?? '',
        konu: kayit.konu ?? '',
        sandikNo: kayit.sandikNo ?? 1,
        olayKategorisi: kayit.olayKategorisi ?? '',
        olaySaati: kayit.olaySaati ?? '00:00:00',
        aciklama: kayit.aciklama ?? '',
        tarih: kayit.tarih ? kayit.tarih.substring(0, 10) : '',
        katilimciSayisi: kayit.katilimciSayisi ?? 0,
        sehitSayisi: kayit.sehitSayisi ?? 0,
        oluSayisi: kayit.oluSayisi ?? 0,
        gozaltiSayisi: kayit.gozaltiSayisi ?? 0,
      };
    } else if (this.aktifSekme === 'vip') {
      this.formVip = {
        ziyaretEdenAdSoyad: kayit.ziyaretEdenAdSoyad ?? '',
        unvan: kayit.unvan ?? '',
        baslangicTarihi: kayit.baslangicTarihi ? kayit.baslangicTarihi.substring(0, 16) : '',
        bitisTarihi: kayit.bitisTarihi ? kayit.bitisTarihi.substring(0, 16) : '',
        il: kayit.il ?? '',
        mekan: kayit.mekan ?? '',
        hassasiyet: kayit.hassasiyet ?? 0,
        guvenlikSeviyesi: kayit.guvenlikSeviyesi ?? 'Normal',
        gozlemNoktalari: kayit.gozlemNoktalari ?? '',
        ziyaretDurumu: kayit.ziyaretDurumu ?? 0,
      };
    }

    this.modalAcik = true;
    this.cdr.markForCheck();
  }

  modalKapat(): void { this.modalAcik = false; this.secilenKayit = null; this.cdr.markForCheck(); }

  // ── Kaydet ──────────────────────────────────────────────────────────────
  kaydet(): void {
    if (!this.secilenKayit) return;
    this.kaydediliyor = true;
    this.hata = null;
    this.basari = null;
    const opts = { headers: this.token() };

    if (this.aktifSekme === 'sokak') {
      const body = {
        olayTuru: this.formSokak.olayTuru,
        il: this.formSokak.il,
        ilce: this.formSokak.ilce,
        mekan: this.formSokak.mekan,
        tarih: new Date(this.formSokak.tarih).toISOString(),
        katilimciSayisi: this.formSokak.katilimciSayisi,
        gozaltiSayisi: this.formSokak.gozaltiSayisi,
        sehitOluSayisi: this.formSokak.sehitOluSayisi,
        hassasiyet: Number(this.formSokak.hassasiyet),
        aciklama: this.formSokak.aciklama,
        evrakNumarasi: this.formSokak.evrakNumarasi,
        durum: Number(this.formSokak.durum),
        organizatorId: this.secilenKayit.organizatorId,
        konuId: this.secilenKayit.konuId,
        cityId: this.secilenKayit.cityId,
      };
      this.http.put(`${API}/olay/${this.secilenKayit.id}`, body, opts).subscribe({
        next: () => this.basariIsle('sokakOlaylar'),
        error: e => this.hataIsle(e),
      });
    } else if (this.aktifSekme === 'sosyal') {
      const body = {
        platform: this.formSosyal.platform,
        konu: this.formSosyal.konu,
        paylasimLinki: this.formSosyal.paylasimLinki,
        paylasimTarihi: new Date(this.formSosyal.paylasimTarihi).toISOString(),
        icerikOzeti: this.formSosyal.icerikOzeti,
        ilgiliKisiKurum: this.formSosyal.ilgiliKisiKurum,
        il: this.formSosyal.il,
        ilce: this.formSosyal.ilce,
        hassasiyet: Number(this.formSosyal.hassasiyet),
      };
      this.http.put(`${API}/sosyalmedyaolay/${this.secilenKayit.id}`, body, opts).subscribe({
        next: () => this.basariIsle('sosyalOlaylar'),
        error: e => this.hataIsle(e),
      });
    } else if (this.aktifSekme === 'secim') {
      const [gun, ay, yil] = this.formSecim.tarih.split('-').map(Number);
      const body = {
        musahitAdi: this.formSecim.musahitAdi,
        il: this.formSecim.il,
        ilce: this.formSecim.ilce,
        mahalle: this.formSecim.mahalle,
        okul: this.formSecim.okul,
        konu: this.formSecim.konu,
        sandikNo: Number(this.formSecim.sandikNo),
        olayKategorisi: this.formSecim.olayKategorisi,
        olaySaati: this.formSecim.olaySaati,
        aciklama: this.formSecim.aciklama,
        tarih: new Date(this.formSecim.tarih).toISOString(),
        katilimciSayisi: Number(this.formSecim.katilimciSayisi),
        sehitSayisi: Number(this.formSecim.sehitSayisi),
        oluSayisi: Number(this.formSecim.oluSayisi),
        gozaltiSayisi: Number(this.formSecim.gozaltiSayisi),
      };
      this.http.put(`${API}/secim/sandik-olay/${this.secilenKayit.id}`, body, opts).subscribe({
        next: () => this.basariIsle('secimOlaylar'),
        error: e => this.hataIsle(e),
      });
    } else if (this.aktifSekme === 'vip') {
      const body = {
        ziyaretEdenAdSoyad: this.formVip.ziyaretEdenAdSoyad,
        unvan: this.formVip.unvan,
        baslangicTarihi: new Date(this.formVip.baslangicTarihi).toISOString(),
        bitisTarihi: new Date(this.formVip.bitisTarihi).toISOString(),
        il: this.formVip.il,
        mekan: this.formVip.mekan,
        hassasiyet: Number(this.formVip.hassasiyet),
        guvenlikSeviyesi: this.formVip.guvenlikSeviyesi,
        gozlemNoktalari: this.formVip.gozlemNoktalari,
        ziyaretDurumu: Number(this.formVip.ziyaretDurumu),
      };
      this.http.put(`${API}/vipziyaret/${this.secilenKayit.id}`, body, opts).subscribe({
        next: () => this.basariIsle('vipOlaylar'),
        error: e => this.hataIsle(e),
      });
    }
  }

  private basariIsle(liste: string): void {
    this.kaydediliyor = false;
    this.basari = 'Kayıt başarıyla güncellendi.';
    this.tumVerileriYukle();
    setTimeout(() => { this.modalKapat(); }, 1200);
    this.cdr.markForCheck();
  }

  private hataIsle(e: any): void {
    this.kaydediliyor = false;
    this.hata = e?.error?.detail ?? e?.error?.title ?? 'Güncelleme başarısız.';
    this.cdr.markForCheck();
  }

  // ── Yardımcılar ─────────────────────────────────────────────────────────
  tarihFormat(t: string): string {
    if (!t) return '-';
    return new Date(t).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' });
  }
  hassasiyetLabel(h: number): string { return HASSASIYET_LABELS[h] ?? '-'; }
  hassasiyetRenk(h: number): string  { return HASSASIYET_RENK[h]   ?? '#888'; }
}