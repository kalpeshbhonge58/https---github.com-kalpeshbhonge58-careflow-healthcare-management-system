import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <main class="access-page">
      <p class="eyebrow">Access control</p>
      <h1>That area is restricted.</h1>
      <p>You are signed in, but your current role does not have permission to view this page.</p>
      <a routerLink="/dashboard">Return to dashboard</a>
    </main>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; background: #f4f1eb; color: #173b3f; }
    .access-page { max-width: 38rem; padding: 12vh 1.5rem; margin: auto; }
    .eyebrow { color: #b07d3a; font-size: .7rem; font-weight: 700; letter-spacing: .15rem; text-transform: uppercase; }
    h1 { margin: 0; font: 400 clamp(2.8rem, 7vw, 5.5rem)/.98 Georgia, 'Times New Roman', serif; }
    p:not(.eyebrow) { color: #637577; line-height: 1.7; }
    a { display: inline-block; margin-top: 1.2rem; padding: .85rem 1rem; background: #1f6460; color: #fff; text-decoration: none; font-weight: 700; }
  `]
})
export class UnauthorizedComponent {}
