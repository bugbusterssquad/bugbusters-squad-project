export interface Club {
  id: number;
  name: string;
  mission?: string | null;
  management?: string | null;
  contact?: string | null;
}

export interface User {
  id: number;
  name: string;
  email: string;
}

export interface Membership {
  id: number;
  userId: number;
  clubId: number;
  status: "pending" | "approved" | "rejected";
  qrCode?: string | null;
}
