import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';

interface BultenKart {
  baslik: string;
  aciklama: string;
  icon: string;
  route: string;
  renk: string;
}

@Component({
  selector: 'app-bultenler',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bultenler.html',
  styleUrls: ['./bultenler.css'],
})
export class Bultenler {
  constructor(private router: Router) {}

  readonly bultenler: BultenKart[] = [
    {
      baslik: 'Başkanlık Legal Günlük Bülten',
      aciklama: 'Gerçekleşen ve beklenen eylem/etkinliklerin günlük icmali ile operasyonel faaliyet özeti.',
      icon: '📄',
      route: '/rapor-gunluk-bulten',
      renk: 'card--navy',
    },
  ];

  git(route: string): void {
    this.router.navigate([route]);
  }
}
