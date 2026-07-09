export interface Item {
  id: string;
  code: string;
  name: string;
  categorie: string;
  categoryId: number;
  subcategoryId: number;
  categoryName?: string;
  subcategoryName?: string;
  stockQty: number;
  uomIndex: number;
  unitId: number;
  locationIndex: number;
  status: boolean;
  price: number;
  description: string;
  image: string;
}
