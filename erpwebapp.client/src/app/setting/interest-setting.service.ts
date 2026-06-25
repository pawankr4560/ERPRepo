import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, catchError, map, Observable, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export type InterestCalculationType = 'Flat' | 'Reducing';

export interface InterestSetting {
  interestCalculationType: InterestCalculationType;
}

@Injectable({
  providedIn: 'root',
})
export class InterestSettingService {
  private readonly storageKey = 'interestCalculationType';
  private readonly settingUrl = `${environment.apiUrl}/LoanSetting/interest-calculation-type`;
  private readonly headers = new HttpHeaders({
    'Content-Type': 'application/json; charset=utf-8',
    api_key: environment.apiKey,
  } as any);

  private readonly interestTypeSubject = new BehaviorSubject<InterestCalculationType>(
    this.readCachedType()
  );

  readonly interestType$ = this.interestTypeSubject.asObservable();

  constructor(private http: HttpClient) {}

  get currentType(): InterestCalculationType {
    return this.interestTypeSubject.value;
  }

  load(): Observable<InterestCalculationType> {
    return this.http.get<any>(this.settingUrl, { headers: this.headers }).pipe(
      map((response) =>
        this.normalizeType(
          response?.interestCalculationType ??
            response?.InterestCalculationType ??
            response?.data?.interestCalculationType
        )
      ),
      tap((type) => this.cacheType(type)),
      catchError(() => of(this.currentType))
    );
  }

  save(type: InterestCalculationType): Observable<InterestCalculationType> {
    const normalizedType = this.normalizeType(type);
    return this.http
      .put<any>(
        this.settingUrl,
        { interestCalculationType: this.toApiValue(normalizedType) },
        { headers: this.headers }
      )
      .pipe(
        map((response) =>
          this.normalizeType(
            response?.interestCalculationType ??
              response?.InterestCalculationType ??
              response?.data?.interestCalculationType ??
              normalizedType
          )
        ),
        tap((savedType) => this.cacheType(savedType))
      );
  }

  calculateEmi(
    amount: number,
    annualRate: number,
    tenureMonths: number,
    type: InterestCalculationType = this.currentType
  ): number {
    if (amount <= 0 || annualRate < 0 || tenureMonths <= 0) {
      return 0;
    }

    if (type === 'Flat') {
      const totalInterest = (amount * annualRate * tenureMonths) / 12 / 100;
      return Number(((amount + totalInterest) / tenureMonths).toFixed(2));
    }

    const monthlyRate = annualRate / 1200;
    if (monthlyRate === 0) {
      return Number((amount / tenureMonths).toFixed(2));
    }

    const factor = Math.pow(1 + monthlyRate, tenureMonths);
    return Number(((amount * monthlyRate * factor) / (factor - 1)).toFixed(2));
  }

  normalizeType(value: unknown): InterestCalculationType {
    if (typeof value === 'boolean') {
      return value ? 'Reducing' : 'Flat';
    }

    return String(value).toLowerCase() === 'reducing' ? 'Reducing' : 'Flat';
  }

  private toApiValue(type: InterestCalculationType): boolean {
    return type === 'Reducing';
  }

  private cacheType(type: InterestCalculationType): void {
    localStorage.setItem(this.storageKey, type);
    this.interestTypeSubject.next(type);
  }

  private readCachedType(): InterestCalculationType {
    return this.normalizeType(localStorage.getItem(this.storageKey));
  }
}
