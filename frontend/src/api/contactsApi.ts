import axios from "axios";

export type Contact = {
    id: number;
    firstName: string;
    lastName: string;
    phone: string;
    email: string;
    address: string;
    city: string;
    state: string;
    zip: string;
    age: number;
    status: string;
};

export type ContactsResponse = {
    items: Contact[];
    totalCount: number;
    page: number;
    pageSize: number;
};

export type ContactsQuery = {
    q?: string;
    page: number;
    pageSize: number;
    sortBy?: string;
    sortDir?: "asc" | "desc";
};

export async function fetchContacts(params: ContactsQuery): Promise<ContactsResponse> {
    const res = await axios.get<ContactsResponse>("/api/contacts", { params });
    return res.data;
}