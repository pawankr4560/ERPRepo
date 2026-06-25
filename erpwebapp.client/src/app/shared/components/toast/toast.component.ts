import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styles: [
    `
      .toast-container {
        position: fixed;
        top: 18px;
        right: 18px;
        z-index: 3200;
        display: flex;
        flex-direction: column;
        gap: 10px;
        width: min(380px, calc(100vw - 32px));
        pointer-events: none;
      }

      .toast-message {
        position: relative;
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 12px;
        overflow: hidden;
        padding: 12px 42px 14px 14px;
        border-radius: 8px;
        color: #fff;
        box-shadow: 0 14px 36px rgba(15, 23, 42, 0.22);
        font-size: 14px;
        line-height: 1.4;
        pointer-events: auto;
        animation: toast-enter 140ms ease-out, toast-fade 260ms ease-in forwards;
        animation-delay: 0ms, calc(var(--toast-duration, 4000ms) - 260ms);
      }

      .toast-content {
        display: grid;
        gap: 2px;
      }

      .toast-content strong {
        font-size: 13px;
        line-height: 1.2;
      }

      .toast-message button {
        position: absolute;
        top: 10px;
        right: 10px;
        border: 0;
        background: transparent;
        color: inherit;
        cursor: pointer;
        font-size: 16px;
        line-height: 1;
      }

      .toast-progress {
        position: absolute;
        left: 0;
        right: 0;
        bottom: 0;
        height: 3px;
        background: rgba(255, 255, 255, 0.8);
        transform-origin: left center;
        animation: toast-progress var(--toast-duration, 4000ms) linear forwards;
      }

      .toast-success {
        background: #1b7f3a;
      }

      .toast-error {
        background: #b42318;
      }

      .toast-warning {
        background: #9a6700;
      }

      .toast-info {
        background: #1f5fbf;
      }

      @keyframes toast-enter {
        from {
          opacity: 0;
          transform: translateY(-8px);
        }

        to {
          opacity: 1;
          transform: translateY(0);
        }
      }

      @keyframes toast-fade {
        to {
          opacity: 0;
          transform: translateY(-6px);
        }
      }

      @keyframes toast-progress {
        to {
          transform: scaleX(0);
        }
      }
    `,
  ],
})
export class ToastComponent {
  constructor(public toastService: ToastService) {}
}
