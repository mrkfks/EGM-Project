import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-supheli',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './supheli.html',
  styleUrls: ['./supheli.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Supheli {}
