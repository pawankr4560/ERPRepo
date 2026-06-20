export interface Item {
   id: string;               // BaseEntity Id
  code: string;             // Product.Code
  name: string;             // Product.Name
  categorie: string;        // Product.Categorie
  stockQty: number;         // Product.StockQty
  uomIndex: number;         // Product.UOMIndex
  locationIndex: number;    // Product.LocationIndex
  status: boolean;          // Product.Status
  price: number;            // Product.Price
  description: string;      // Product.Description
  image: string;  
}
