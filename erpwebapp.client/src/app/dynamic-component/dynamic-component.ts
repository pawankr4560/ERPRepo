
import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Input, NgZone, OnInit, ViewChild } from '@angular/core';

// Google Maps Places is loaded externally; declare global to satisfy TS.
declare const google: any;

@Component({
  selector: 'app-dynamic-component',
  imports: [CommonModule],
  templateUrl: './dynamic-component.html',
  styleUrl: './dynamic-component.css',
})
export class DynamicComponent implements AfterViewInit {
  @Input() title = 'Default Title';
  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;

  place: {
    name: string;
    address: string;
    lat: number;
    lng: number;
  } | null = null;

  constructor(private ngZone: NgZone) {}

  ngAfterViewInit(): void {
    const autocomplete = new google.maps.places.Autocomplete(
      this.searchInput.nativeElement,
      {
        types: ['geocode'],
        componentRestrictions: { country: 'in' }
      }
    );

  autocomplete.addListener('place_changed', () => {
  this.ngZone.run(() => {
    const place = autocomplete.getPlace();

    if (!place.geometry || !place.geometry.location) {
      console.warn('No geometry available for selected place');
      return;
    }

    const location = place.geometry.location;

    this.place = {
      name: place.name ?? '',
      address: place.formatted_address ?? '',
      lat: location.lat(),
      lng: location.lng()
    };
  });
});
  }
}
