export interface CarCategory {
  id: number;
  name: string;
}

export interface Car {
  id: number;
  brand: string;
  model: string;
  year: number;
  categoryId: number;
  category?: CarCategory | null;
  transmission: string;
  fuelType: string;
  seats: number;
  pricePerDay: number;
  imageUrl?: string | null;
  status: string;
}

export type CarDialogMode = 'create' | 'edit' | 'view';

export interface CarDialogData {
  mode: CarDialogMode;
  car?: Car;
  categories: CarCategory[];
}
