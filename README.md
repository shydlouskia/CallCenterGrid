# CallCenterGrid – 500,000 Contacts Data Grid

A full-stack web application that displays 500,000 contacts with server-side search, sorting, and pagination.  
All filtering, sorting, and pagination are executed on the server to avoid loading unnecessary data into memory.

## Architecture

### Backend
- **Platform:** .NET 8 Minimal API
- **ORM:** Entity Framework Core with SQLite
- Automatic database creation on first run
- Streaming CSV import with batch inserts (5,000 records per batch)
- Change tracking disabled during import for performance
- Indexed searchable and sortable columns

### Frontend
- **Framework:** React with Vite and TypeScript
- **Grid:** TanStack Table (manual/server mode)
- **Data Fetching:** TanStack Query
- Debounced search input (300 ms)
- Fully server-driven pagination and sorting

## Dataset

The application imports a CSV file containing 500,000 contact records with the following structure:

id, first_name, last_name, phone, email, address, city, state, zip, age, status


### On first startup:

1. The SQLite database is created automatically.  
2. The CSV file is imported in batches.  
3. Import progress is logged in the console.  
4. Dataset size is validated to ensure import completeness.  

> **Note:** First run may take 1–2 minutes depending on system performance.

---

## Running the Application

### Prerequisites
- .NET SDK 8  
- Node.js 18+  
- CSV file located at `data/contacts_500k.csv`  

### Start Backend
```bash
cd backend/CallCenter.Api
dotnet run
```
### Backend base URL:
http://localhost:5174

### Health endpoint:
GET http://localhost:5174/api/health

### Contacts endpoint:
GET http://localhost:5174/api/contacts

### Start Frontend
```bash
cd frontend
npm install
npm run dev
```

### Frontend URL:
http://localhost:5173

## API Example

### Request:
GET /api/contacts?q=john&sortBy=lastName&sortDir=asc&page=2&pageSize=25

### Response:
{
  "items": [],
  "totalCount": 500000,
  "page": 2,
  "pageSize": 25
}
