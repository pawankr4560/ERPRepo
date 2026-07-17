export interface PlotAdmin {
  id?: string;
  title: string;
  location: string;
  areaSqFt: number;
  price: number;
  status: string;
  propertyType: string;
  roadWidth: string;
  electricity: string;
  water: string;
  registration: string;
  description: string;
  thumbnailUrl?: string;
  sellerName: string;
  sellerPhone: string;
  isActive: boolean;
  imageUrls: string[];
  amenityIds: number[];
  createdAt?: string;
  updatedAt?: string;
}

export interface PlotAmenity { id: number; name: string; }
export interface PlotVisitAdmin {
  id: string; plotId: string; plotTitle: string; plotLocation: string; plotAreaSqFt: number;
  plotPrice: number; plotStatus: string; customerName: string; customerEmail: string; mobileNumber: string;
  visitDate: string; visitTime: string; remarks?: string; status: string; createdAt: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  errorMessage?: string;
  data: T;
}
