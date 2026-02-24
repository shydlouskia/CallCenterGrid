import { useMemo, useState } from "react";
import debounce from "lodash.debounce";
import {
    type ColumnDef,
    getCoreRowModel,
    useReactTable,
    flexRender,
} from "@tanstack/react-table";
import type { Contact } from "../api/contactsApi";
import { useContacts } from "../hooks/useContacts";

const PAGE_SIZES = [25, 50, 100, 200] as const;

export function ContactsGrid() {
    const [searchInput, setSearchInput] = useState("");
    const [q, setQ] = useState("");
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState<(typeof PAGE_SIZES)[number]>(50);
    const [sortBy, setSortBy] = useState<string | undefined>(undefined);
    const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");

    const debouncedSetQ = useMemo(
        () =>
            debounce((value: string) => {
                setPage(1);
                setQ(value.trim());
            }, 300),
        []
    );

    const { data, isFetching, isError, error } = useContacts({
        q: q || undefined,
        page,
        pageSize,
        sortBy,
        sortDir,
    });

    const totalCount = data?.totalCount ?? 0;
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    const canPrev = page > 1;
    const canNext = page < totalPages;

    const columns = useMemo<ColumnDef<Contact>[]>(
        () => [
            { accessorKey: "id", header: "ID" },
            { accessorKey: "firstName", header: "First name" },
            { accessorKey: "lastName", header: "Last name" },
            { accessorKey: "phone", header: "Phone" },
            { accessorKey: "email", header: "Email" },
            { accessorKey: "city", header: "City" },
            { accessorKey: "state", header: "State" },
            { accessorKey: "zip", header: "Zip" },
            { accessorKey: "age", header: "Age" },
            { accessorKey: "status", header: "Status" },
        ],
        []
    );

    const table = useReactTable({
        data: data?.items ?? [],
        columns,
        getCoreRowModel: getCoreRowModel(),
    });

    function toggleSort(columnId: string) {
        setPage(1);
        if (sortBy !== columnId) {
            setSortBy(columnId);
            setSortDir("asc");
            return;
        }
        setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    }

    const sortIndicator = (columnId: string) => {
        if (sortBy !== columnId) return "";
        return sortDir === "asc" ? " ▲" : " ▼";
    };

    const from = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
    const to = Math.min(page * pageSize, totalCount);

    return (
        <div style={{ padding: 16, fontFamily: "system-ui, Arial" }}>
            <h2 style={{ margin: 0 }}>Contacts</h2>

            <div style={{ display: "flex", gap: 12, alignItems: "center", marginTop: 12 }}>
                <input
                    value={searchInput}
                    onChange={(e) => {
                        const v = e.target.value;
                        setSearchInput(v);
                        debouncedSetQ(v);
                    }}
                    placeholder="Search (name, email, phone, city...)"
                    style={{ padding: 8, width: 360 }}
                />

                <label style={{ display: "flex", gap: 8, alignItems: "center" }}>
                    Page size
                    <select
                        value={pageSize}
                        onChange={(e) => {
                            setPage(1);
                            setPageSize(Number(e.target.value) as any);
                        }}
                    >
                        {PAGE_SIZES.map((s) => (
                            <option key={s} value={s}>
                                {s}
                            </option>
                        ))}
                    </select>
                </label>

                <div style={{ marginLeft: "auto", fontSize: 12, opacity: 0.8 }}>
                    {isFetching ? "Loading..." : `Showing ${from}-${to} of ${totalCount}`}
                </div>
            </div>

            {isError ? (
                <div style={{ marginTop: 12, color: "crimson" }}>
                    Error loading contacts: {String((error as any)?.message ?? error)}
                </div>
            ) : null}

            <div style={{ marginTop: 12, border: "1px solid #ddd", borderRadius: 8, overflow: "hidden" }}>
                <table style={{ width: "100%", borderCollapse: "collapse" }}>
                    <thead style={{ background: "#f6f6f6" }}>
                        {table.getHeaderGroups().map((hg) => (
                            <tr key={hg.id}>
                                {hg.headers.map((header) => {
                                    const colId = header.column.id;
                                    const sortable = [
                                        "id", "firstName", "lastName", "phone", "email", "city", "state", "zip", "age", "status"
                                    ].includes(colId);

                                    return (
                                        <th
                                            key={header.id}
                                            onClick={sortable ? () => toggleSort(colId) : undefined}
                                            style={{
                                                textAlign: "left",
                                                padding: 10,
                                                borderBottom: "1px solid #ddd",
                                                cursor: sortable ? "pointer" : "default",
                                                userSelect: "none",
                                                whiteSpace: "nowrap",
                                            }}
                                            title={sortable ? "Sort" : undefined}
                                        >
                                            {flexRender(header.column.columnDef.header, header.getContext())}
                                            {sortable ? sortIndicator(colId) : null}
                                        </th>
                                    );
                                })}
                            </tr>
                        ))}
                    </thead>

                    <tbody>
                        {table.getRowModel().rows.map((row) => (
                            <tr key={row.id} style={{ borderBottom: "1px solid #eee" }}>
                                {row.getVisibleCells().map((cell) => (
                                    <td key={cell.id} style={{ padding: 10, whiteSpace: "nowrap" }}>
                                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                    </td>
                                ))}
                            </tr>
                        ))}

                        {data && data.items.length === 0 ? (
                            <tr>
                                <td colSpan={columns.length} style={{ padding: 12, opacity: 0.7 }}>
                                    No results
                                </td>
                            </tr>
                        ) : null}
                    </tbody>
                </table>
            </div>

            <div style={{ marginTop: 12, display: "flex", alignItems: "center", gap: 8 }}>
                <button onClick={() => setPage(1)} disabled={!canPrev}>{"<<"}</button>
                <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={!canPrev}>
                    Prev
                </button>

                <span style={{ fontSize: 13 }}>
                    Page <b>{page}</b> / {totalPages}
                </span>

                <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={!canNext}>
                    Next
                </button>
                <button onClick={() => setPage(totalPages)} disabled={!canNext}>{">>"}</button>
            </div>
        </div>
    );
}