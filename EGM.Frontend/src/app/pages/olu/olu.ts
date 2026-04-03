import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-olu',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './olu.html',
  styleUrls: ['./olu.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Olu {}
