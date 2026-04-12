/**
 * Harita sayfasında olayları filtrelemek için kullanılan model.
 */
export interface OlayFilterRequest {
  /** Tarih aralığı başlangıcı (sadece tarih, saat dikkate alınmaz) */
  tarihBaslangic?: Date | string | null;

  /** Tarih aralığı sonu (sadece tarih, saat dikkate alınmaz) */
  tarihBitis?: Date | string | null;

  /** Olay konusu ID'si */
  konuId?: string | null;

  /** Organizatör (Kuruluş) ID'si */
  organizatorId?: string | null;

  /** Olay türü adı */
  olayTuru?: string | null;

  /** Gerçekleşme şekli ID'si */
  gerceklesmeSekliId?: string | null;

  /** Olay durumu (0=Planlandi, 1=Gerceklesti, 2=Iptal) */
  durum?: number | null;

  /** Sayfa numarası (varsayılan: 1) */
  page?: number;

  /** Sayfa boyutu (varsayılan: 100, max: 500) */
  pageSize?: number;
}

/**
 * Harita API'sinden dönen sayfalanmış olay listesi.
 */
export interface OlayFilterResponse {
  items: OlayData[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Tek bir olay verisi.
 */
export interface OlayData {
  id: string;
  olayTuru?: string;
  organizatorId: string;
  organizatorAd?: string;
  konuId: string;
  konuAd?: string;
  tarih: string | Date;
  baslangicSaati?: string;
  bitisSaati?: string;
  il?: string;
  ilce?: string;
  mahalle?: string;
  mekan?: string;
  latitude?: number;
  longitude?: number;
  katilimciSayisi?: number;
  gozaltiSayisi?: number;
  sehitOluSayisi?: number;
  aciklama?: string;
  evrakNumarasi?: string;
  durum: number; // 0=Planlandi, 1=Gerceklesti, 2=Iptal
  gercekBaslangicTarihi?: string | Date;
  gercekBitisTarihi?: string | Date;
  createdByUserId: string;
  cityId?: number;
  olayBitisTarihi?: string | Date;
  gerceklesenKatilimciSayisi?: number;
  gerceklesmeSekliId?: string;
  takipNo?: string;
}

/**
 * Filtre seçenekleri (dropdown'lar için) - basit interface'ler.
 */
export interface Konu {
  id: string;
  ad: string;
  aciklama?: string;
}

export interface Organizator {
  id: string;
  ad: string;
}

export interface OlayTuru {
  id: string;
  name: string;
}

export interface GerceklesmeSekli {
  id: string;
  name: string;
}

/**
 * Olay Durumu enum'u (Backend'deki OlayDurum ile uyumlu).
 */
export enum OlayDurumEnum {
  Planlandi = 0,
  Gerceklesti = 1,
  Iptal = 2,
  DevamEdiyor = 3  // Frontend hesaplamalı, backend'de bu değer yoktur
}
