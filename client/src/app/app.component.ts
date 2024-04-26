import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  title = 'Linked App';
  users: any;
  url = 'https://localhost:5001/api/Users';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.http.get(this.url).subscribe({
      next: (response) => (this.users = response),
      error: (error) => console.warn(error),
      complete: () => console.log('Request Completed ...'),
    });
  }
}
