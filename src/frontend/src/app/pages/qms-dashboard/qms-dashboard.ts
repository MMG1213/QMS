import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-qms-dashboard',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './qms-dashboard.html',
  styleUrl: './qms-dashboard.css'
})
export class QmsDashboardComponent {}
