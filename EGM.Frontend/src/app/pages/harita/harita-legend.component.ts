import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-harita-legend',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './harita-legend.component.html',
  styleUrls: ['./harita-legend.component.css']
})
export class HaritaLegendComponent {
  @Input() showOlaylar: boolean = true;
  @Input() showVIPZiyaretler: boolean = true;
  @Input() showProvinceBoundaries: boolean = true;
}
