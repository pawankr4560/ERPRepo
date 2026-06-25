import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { LoadingService } from './shared/services/loading.service';
import { ToastService } from './shared/services/toast.service';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];
  readonly loading$ = inject(LoadingService).loading$;

  constructor(
    private http: HttpClient,
    public toastService: ToastService
  ) {}

  ngOnInit() {
    this.getForecasts();
  }

  getForecasts() {
    this.http.get<WeatherForecast[]>('/weatherforecast').subscribe(
      (result) => {
        this.forecasts = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }

  title = 'erpwebapp.client';
}
