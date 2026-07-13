export interface ApiResponse<T> {
  success: boolean;
  message?: string | null;
  errorMessage?: string | null;
  data: T;
}

export interface ConstructionQuote {
  quoteId: number;
  status: string;
  productName: string;
  categoryName: string;
  quantity: number;
  unitId: number;
  unitName: string;
  estimatedAmount: number;
  finalQuotedAmount: number;
  deliveryLocation: string;
  requiredDate: string;
  createdAt: string;
}

export interface ConstructionOrder {
  orderId: number;
  quoteId: number;
  status: string;
  productName: string;
  quantity: number;
  unitId: number;
  unitName: string;
  totalAmount: number;
  deliveryLocation: string;
  deliveryDate: string;
  createdAt: string;
}

export interface ConstructionDelivery {
  deliveryId: number;
  orderId: number;
  productName: string;
  vehicleNumber: string;
  driverName: string;
  status: string;
  eta?: string | null;
  progress: number;
}

export interface StatusOption {
  value: number;
  label: string;
}
