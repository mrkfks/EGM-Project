import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-operasyonel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './operasyonel.html',
  styleUrls: ['./operasyonel.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Operasyonel {}
