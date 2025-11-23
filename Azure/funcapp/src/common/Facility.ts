export interface Facility {
  facility_id: string;
  client_id: string;
  name: string;
  facility_type?: string | null;
  address?: string | null;
  created_on: string;   // ISO datetime string from DB
  modified_on: string;  // ISO datetime string from DB
}