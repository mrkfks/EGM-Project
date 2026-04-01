import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { OlayTuruService, OlayTuru } from '../../services/olay-turu.service';
import { GerceklesmeSekliService, GerceklesmeSekli } from '../../services/gerceklesme-sekli.service';

@Component({
  selector: 'app-faaliyet-yonetimi',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './faaliyet-yonetimi.html',
  styleUrls: ['./faaliyet-yonetimi.css']
})
export class FaaliyetYonetimiComponent implements OnInit {
  olayTurleri: OlayTuru[] = [];
  gerceklesmeSekilleri: GerceklesmeSekli[] = [];

  newOlayTuru: string = '';
  newGerceklesmeSekli: string = '';
  selectedOlayTuruId: string | null = null;
  errorMessage: string = '';

  constructor(
    private olayTuruService: OlayTuruService,
    private gerceklesmeSekliService: GerceklesmeSekliService
  ) {}

  ngOnInit(): void {
    this.loadOlayTurleri();
    this.loadGerceklesmeSekilleri();
  }

  loadOlayTurleri() {
    this.olayTuruService.getAll().subscribe({
      next: data => this.olayTurleri = data,
      error: () => this.errorMessage = 'Olay türleri yüklenemedi.'
    });
  }

  loadGerceklesmeSekilleri() {
    this.gerceklesmeSekliService.getAll().subscribe({
      next: data => this.gerceklesmeSekilleri = data,
      error: () => this.errorMessage = 'Gerçekleşme şekilleri yüklenemedi.'
    });
  }

  addOlayTuru(): void {
    if (this.newOlayTuru.trim()) {
      this.olayTuruService.create({ name: this.newOlayTuru }).subscribe({
        next: () => { this.newOlayTuru = ''; this.loadOlayTurleri(); },
        error: () => this.errorMessage = 'Olay türü eklenemedi.'
      });
    }
  }

  editOlayTuru(olay: OlayTuru): void {
    const newName = prompt('Yeni Olay Türü Adı:', olay.name);
    if (newName) {
      this.olayTuruService.update(olay.id, { name: newName }).subscribe({
        next: () => this.loadOlayTurleri(),
        error: () => this.errorMessage = 'Olay türü güncellenemedi.'
      });
    }
  }

  deleteOlayTuru(id: string): void {
    this.olayTuruService.delete(id).subscribe({
      next: () => {
        this.loadOlayTurleri();
        this.loadGerceklesmeSekilleri();
        this.selectedOlayTuruId = null;
      },
      error: () => this.errorMessage = 'Olay türü silinemedi.'
    });
  }

  addGerceklesmeSekli(): void {
    if (this.newGerceklesmeSekli.trim() && this.selectedOlayTuruId) {
      this.gerceklesmeSekliService.create({
        name: this.newGerceklesmeSekli,
        olayTuruId: this.selectedOlayTuruId
      }).subscribe({
        next: () => { this.newGerceklesmeSekli = ''; this.loadGerceklesmeSekilleri(); },
        error: () => this.errorMessage = 'Gerçekleşme şekli eklenemedi.'
      });
    } else {
      alert('Lütfen bir olay türü seçin.');
    }
  }

  editGerceklesmeSekli(sekil: GerceklesmeSekli): void {
    const newName = prompt('Yeni Gerçekleşme Şekli Adı:', sekil.name);
    if (newName) {
      this.gerceklesmeSekliService.update(sekil.id, {
        name: newName,
        olayTuruId: sekil.olayTuruId
      }).subscribe({
        next: () => this.loadGerceklesmeSekilleri(),
        error: () => this.errorMessage = 'Gerçekleşme şekli güncellenemedi.'
      });
    }
  }

  deleteGerceklesmeSekli(id: string): void {
    this.gerceklesmeSekliService.delete(id).subscribe({
      next: () => this.loadGerceklesmeSekilleri(),
      error: () => this.errorMessage = 'Gerçekleşme şekli silinemedi.'
    });
  }

  getFilteredGerceklesmeSekilleri(): GerceklesmeSekli[] {
    if (!this.selectedOlayTuruId) {
      return [];
    }
    return this.gerceklesmeSekilleri.filter(
      (sekil) => sekil.olayTuruId === this.selectedOlayTuruId
    );
  }

  selectOlayTuru(id: string): void {
    this.selectedOlayTuruId = id;
  }
}