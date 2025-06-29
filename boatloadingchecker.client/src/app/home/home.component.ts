import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BoatService } from '../services/boat.service';
import { BoatData } from '../models/boat-data.model';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  standalone: true,
  imports: [CommonModule]
})
export class HomeComponent {
  boatData: BoatData | null = null;
  error: string | null = null;
  loading = false;

  constructor(private boatService: BoatService) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      if (!file.name.toLowerCase().endsWith('.pdf') && !file.name.toLowerCase().endsWith('.docx')) {
        this.error = 'Please upload a PDF or DOCX file.';
        return;
      }

      this.loading = true;
      this.error = null;
      this.boatData = null;

      this.boatService.uploadAndParseFile(file).subscribe({
        next: (data) => {
          this.boatData = data;
          this.loading = false;
        },
        error: (error) => {
          this.error = 'Error processing file. Please try again.';
          this.loading = false;
        }
      });
    }
  }

  // Criteria check for Length Overall
  checkLengthOverall(length?: number): boolean | null {
    if (length == null) return null;
    return length > 99.99 && length < 280.01;
  }

  getLengthOverallCheckText(length?: number): string {
    if (length == null) return '';
    return `${length} > 99.99 && < 280.01`;
  }

  // Criteria check for Freeboard Summer
  checkFreeboardSummer(freeboard?: number): boolean | null {
    if (freeboard == null) return null;
    return freeboard < 14.4;
  }

  getFreeboardSummerCheckText(freeboard?: number): string {
    if (freeboard == null) return '';
    return `${freeboard} < 14.4`;
  }

  // Criteria check for Manifold Height (Normal Ballast)
  checkManifoldHeight(height?: number): boolean | null {
    if (height == null) return null;
    return height < 16.4;
  }

  getManifoldHeightCheckText(height?: number): string {
    if (height == null) return '';
    return `${height} < 16.4`;
  }

  // Criteria check for Summer Deadweight
  checkSummerDeadweight(deadweight?: number): boolean | null {
    if (deadweight == null) return null;
    return deadweight < 130000;
  }

  getSummerDeadweightCheckText(deadweight?: number): string {
    if (deadweight == null) return '';
    return `${deadweight} < 130000`;
  }

  // Returns which arms can be loaded at, based on Aft and Forward values
  getLoadingArmResults(): { arm: string, canLoad: boolean }[] {
    if (!this.boatData) return [];
    // For this example, use parallelBody.aft.summerDWT and parallelBody.forward.summerDWT as Aft and Forward
    const aft = this.boatData.parallelBody.aft.summerDWT;
    const forward = this.boatData.parallelBody.forward.summerDWT;
    if (aft == null || forward == null) return [];
    const arms = [
      { arm: '4BB', aftMax: 28.354, forwardMax: 19.678 },
      { arm: '4SB', aftMax: 19.678, forwardMax: 28.354 },
      { arm: '5BB', aftMax: 30.994, forwardMax: 17.088 },
      { arm: '5SB', aftMax: 17.088, forwardMax: 30.994 },
      { arm: '7BB', aftMax: 36.582, forwardMax: 11.45 },
      { arm: '7SB', aftMax: 11.45, forwardMax: 36.582 },
      { arm: '8BB', aftMax: 39.172, forwardMax: 8.86 },
      { arm: '8SB', aftMax: 8.86, forwardMax: 39.172 },
    ];
    return arms.map(a => ({
      arm: a.arm,
      canLoad: (aft < a.aftMax) && (forward < a.forwardMax)
    }));
  }

  // Returns loading arm results for all ballast types
  getLoadingArmResultsAllBallast(): { ballast: string, results: { arm: string, canLoad: boolean }[] }[] {
    if (!this.boatData) return [];
    const arms = [
      { arm: '4BB', aftMax: 28.354, forwardMax: 19.678 },
      { arm: '4SB', aftMax: 19.678, forwardMax: 28.354 },
      { arm: '5BB', aftMax: 30.994, forwardMax: 17.088 },
      { arm: '5SB', aftMax: 17.088, forwardMax: 30.994 },
      { arm: '7BB', aftMax: 36.582, forwardMax: 11.45 },
      { arm: '7SB', aftMax: 11.45, forwardMax: 36.582 },
      { arm: '8BB', aftMax: 39.172, forwardMax: 8.86 },
      { arm: '8SB', aftMax: 8.86, forwardMax: 39.172 },
    ];
    const types = [
      { key: 'lightship', label: 'Lightship' },
      { key: 'normalBallast', label: 'Normal Ballast' },
      { key: 'summerDWT', label: 'Summer DWT' },
    ];
    return types.map(type => {
      const aft = (this.boatData!.parallelBody.aft as any)[type.key];
      const forward = (this.boatData!.parallelBody.forward as any)[type.key];
      return {
        ballast: type.label,
        results: arms.map(a => ({
          arm: a.arm,
          canLoad: (aft != null && forward != null) ? (aft < a.aftMax && forward < a.forwardMax) : false
        }))
      };
    });
  }
}
