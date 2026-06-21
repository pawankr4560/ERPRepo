export interface BookingPayment {
  id: number;
  bookingId: number;
  bookingNumber: string;
  customerName: string;
  bookingAmount: number;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  status: string;
  transactionReference: string;
  notes?: string | null;
  createdDate: string;
}
