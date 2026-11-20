
import { Component } from '@angular/core';
import {
  RouterOutlet,
  RouterLink,
  RouterLinkActive
} from '@angular/router';

@Component({
  selector: 'app-dcr-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './dcr-layout.html',
  styleUrl: './dcr-layout.css'
})
export class DcrLayout {}
