import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-sehit',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sehit.html',
  styleUrls: ['./sehit.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sehit {}
