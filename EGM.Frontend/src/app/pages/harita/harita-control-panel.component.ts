import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Province } from '../../services/geo.service';
import { OlayFilterRequest, Konu, Organizator, OlayTuru, GerceklesmeSekli, OlayDurumEnum } from '../../models/olay-filter.model';
import { KonuService } from '../../services/konu.service';
import { OrganizatorService } from '../../services/organizator.service';
import { OlayTuruService } from '../../services/olay-turu.service';
import { GerceklesmeSekliService } from '../../services/gerceklesme-sekli.service';

@Component({
  selector: 'app-harita-control-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './harita-control-panel.component.html',
  styleUrls: ['./harita-control-panel.component.css']
})
export class HaritaControlPanelComponent implements OnInit {
  @Input() provinces: Province[] = [];
  @Input() isLoading: boolean = false;
  @Input() errorMessage: string = '';

  @Output() filterApplied = new EventEmitter<OlayFilterRequest>();
  @Output() reset = new EventEmitter<void>();

  // Filtre form alanları
  tarihBaslangic: string = '';
  tarihBitis: string = '';
  selectedKonu: string = '';
  selectedOrganizator: string = '';
  selectedOlayTuru: string = '';
  selectedGerceklesmeSekli: string = '';
  selectedDurum: string = '';

  // Dropdown seçenekleri
  konular: Konu[] = [];
  organizatorlar: Organizator[] = [];
  olayTurleri: OlayTuru[] = [];
  gerceklesmeSekilleri: GerceklesmeSekli[] = [];
  durumlar = [
    { value: '0', label: 'Planlandı' },
    { value: '1', label: 'Gerçekleşti' },
    { value: '2', label: 'İptal' },
    { value: '3', label: 'Devam Ediyor' }
  ];

  constructor(
    private konuService: KonuService,
    private organizatorService: OrganizatorService,
    private olayTuruService: OlayTuruService,
    private gerceklesmeSekliService: GerceklesmeSekliService
  ) {}

  ngOnInit(): void {
    // Varsayılan 48 saatlik aralık
    const today = new Date();
    const afterTwoDays = new Date();
    afterTwoDays.setDate(today.getDate() + 2);
    
    this.tarihBaslangic = today.toISOString().split('T')[0];
    this.tarihBitis = afterTwoDays.toISOString().split('T')[0];

    this.loadFilterOptions();
  }

  private loadFilterOptions(): void {
    // Konuları yükle
    this.konuService.getAll().subscribe({
      next: (konular) => (this.konular = konular),
      error: (err) => console.error('Konular yüklenemedi:', err)
    });

    // Organizatörleri yükle
    this.organizatorService.getAll().subscribe({
      next: (org) => (this.organizatorlar = org),
      error: (err) => console.error('Organizatörler yüklenemedi:', err)
    });

    // Olay türlerini yükle
    this.olayTuruService.getAll().subscribe({
      next: (turler) => (this.olayTurleri = turler),
      error: (err) => console.error('Olay türleri yüklenemedi:', err)
    });

    // Gerçekleşme şekillerini yükle
    this.gerceklesmeSekliService.getAll().subscribe({
      next: (sekiller) => (this.gerceklesmeSekilleri = sekiller),
      error: (err) => console.error('Gerçekleşme şekilleri yüklenemedi:', err)
    });
  }

  onFilterApply(): void {
    if (this.tarihBaslangic && this.tarihBitis) {
      const start = new Date(this.tarihBaslangic);
      const end = new Date(this.tarihBitis);
      
      if (end < start) {
        this.errorMessage = 'Bitiş tarihi başlangıç tarihinden önce olamaz.';
        return;
      }

      const diffTime = Math.abs(end.getTime() - start.getTime());
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      
      if (diffDays > 7) {
        this.errorMessage = 'Maksimum 1 haftalık tarih aralığı seçebilirsiniz.';
        return;
      }
    }

    this.errorMessage = '';
    const filter: OlayFilterRequest = {
      tarihBaslangic: this.tarihBaslangic ? new Date(this.tarihBaslangic) : null,
      tarihBitis: this.tarihBitis ? new Date(this.tarihBitis) : null,
      konuId: this.selectedKonu || null,
      organizatorId: this.selectedOrganizator || null,
      olayTuru: this.selectedOlayTuru || null,
      gerceklesmeSekliId: this.selectedGerceklesmeSekli || null,
      durum: this.selectedDurum ? parseInt(this.selectedDurum, 10) : null,
      page: 1,
      pageSize: 1000
    };

    this.filterApplied.emit(filter);
  }

  onReset(): void {
    const today = new Date();
    const afterTwoDays = new Date();
    afterTwoDays.setDate(today.getDate() + 2);
    
    this.tarihBaslangic = today.toISOString().split('T')[0];
    this.tarihBitis = afterTwoDays.toISOString().split('T')[0];
    this.selectedKonu = '';
    this.selectedOrganizator = '';
    this.selectedOlayTuru = '';
    this.selectedGerceklesmeSekli = '';
    this.selectedDurum = '';

    this.reset.emit();

    // Varsayılan filteri uygula (tüm alanlar boş)
    this.onFilterApply();
  }
}
