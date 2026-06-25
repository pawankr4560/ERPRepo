import { Component } from '@angular/core';

@Component({
  selector: 'app-spinner',
  standalone: true,
  templateUrl: './spinner.component.html',
  styles: [
    `
      .spinner-overlay {
        position: fixed;
        inset: 0;
        z-index: 3000;
        display: grid;
        place-items: center;
        background: rgba(15, 23, 42, 0.28);
        backdrop-filter: blur(1px);
      }

      .spinner {
        width: 52px;
        height: 52px;
        border: 5px solid rgba(255, 255, 255, 0.72);
        border-top-color: #005cbb;
        border-radius: 50%;
        animation: spin 0.8s linear infinite;
      }

      @keyframes spin {
        to {
          transform: rotate(360deg);
        }
      }
    `,
  ],
})
export class SpinnerComponent {}
