import { useQuery } from "@tanstack/react-query";
import { fetchContacts, type ContactsQuery } from "../api/contactsApi";

export function useContacts(query: ContactsQuery) {
    return useQuery({
        queryKey: ["contacts", query],
        queryFn: () => fetchContacts(query),
        placeholderData: (prev) => prev,
        staleTime: 10_000,
    });
}