
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-dcr-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dcr-dashboard.html',
  styleUrl: './dcr-dashboard.css'
})
export class DcrDashboard {

  totalDcrs = 0;
  pendingDcrs = 0;
  approvedDcrs = 0;
  rejectedDcrs = 0;

}
