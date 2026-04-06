import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Raporlar } from '../raporlar/raporlar';

type Sekme = 'gunluk';

@Component({
  selector: 'app-bulten-paneli',
  standalone: true,
  imports: [CommonModule, Raporlar],
  templateUrl: './bulten-paneli.html',
  styleUrls: ['./bulten-paneli.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BultenPaneli {
  aktifSekme: Sekme = 'gunluk';

  sekmeSec(s: Sekme): void {
    this.aktifSekme = s;
  }
}
