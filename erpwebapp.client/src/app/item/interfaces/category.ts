export interface Category {
  id: number;
  name: string;
}

export interface SubCategory {
  id: number;
  name: string;
  description: string;
  categoryId: number;
}
