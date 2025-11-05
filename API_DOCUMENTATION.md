# Room Management API Documentation

## Overview

This document provides a comprehensive guide to accessing reservation occurrences, locations, details, and resource details through the Room Management API.

## Table of Contents

1. [Reservation Occurrences API](#reservation-occurrences-api)
2. [Reservation Details API](#reservation-details-api)
3. [Reservation Location Details](#reservation-location-details)
4. [Reservation Resource Details](#reservation-resource-details)
5. [Data Models](#data-models)
6. [Examples](#examples)

---

## Reservation Occurrences API

### Endpoint

**GET** `/api/Reservations/GetReservationOccurrences`

This endpoint returns reservation occurrences (instances) within a specified date range. Each occurrence represents a single instance of a recurring reservation.

### Authentication

Requires authentication: `[Authenticate, Secured]`

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `startDateTime` | DateTime? | No | Current DateTime | Start date/time for filtering occurrences |
| `endDateTime` | DateTime? | No | Current DateTime + 1 month | End date/time for filtering occurrences |
| `reservationTypeIds` | string | No | null | Comma-separated list of reservation type IDs to filter by |
| `reservationIds` | string | No | null | Comma-separated list of reservation IDs to filter by |
| `locationIds` | string | No | null | Comma-separated list of location IDs to filter by |
| `resourceIds` | string | No | null | Comma-separated list of resource IDs to filter by |
| `approvalStates` | string | No | "Approved" | Comma-separated list of approval states. If null, only returns approved reservations. Values: `Draft`, `PendingInitialApproval`, `Approved`, `Denied`, `ChangesNeeded`, `PendingFinalApproval`, `PendingSpecialApproval`, `Cancelled` |
| `filterTimeBy` | string | No | "Reservation" | Filter by reservation time, door lock time, or both. Values: `Reservation`, `DoorLock`, `Both` |
| `includeAttributes` | bool | No | false | Include entity attributes and attribute values in the response |

### Response

Returns an `IQueryable<ReservationSummary>` containing a list of reservation occurrences.

### ReservationSummary Properties

Each occurrence in the response is a `ReservationSummary` object with the following properties:

#### Basic Information
- `Id` (int): Reservation ID
- `ReservationId` (int): Same as Id (for backward compatibility)
- `ReservationName` (string): Name of the reservation
- `ReservationType` (ReservationType): The reservation type object
- `ApprovalState` (ReservationApprovalState): Current approval state

#### Date/Time Information
- `ReservationStartDateTime` (DateTime): Start datetime including setup time
- `ReservationEndDateTime` (DateTime): End datetime including cleanup time
- `EventStartDateTime` (DateTime): Actual event start datetime (without setup/cleanup)
- `EventEndDateTime` (DateTime): Actual event end datetime (without setup/cleanup)
- `EventDateTimeDescription` (string): Friendly formatted event date/time string
- `EventTimeDescription` (string): Friendly formatted event time string (no date)
- `ReservationDateTimeDescription` (string): Friendly formatted reservation date/time string (includes setup/cleanup)
- `ReservationTimeDescription` (string): Friendly formatted reservation time string (no date, includes setup/cleanup)

#### Location and Resource Collections
- `ReservationLocations` (List<ReservationLocation>): List of locations for this occurrence
- `ReservationResources` (List<ReservationResource>): List of all resources for this occurrence
- `UnassignedReservationResources` (List<ReservationResource>): Resources not assigned to a specific location
- `ReservationDoorLockTimes` (List<ReservationDoorLockTime>): Door lock schedule times

#### Contact Information
- `EventContactPersonAlias` (PersonAlias): Person alias for event contact
- `EventContactEmail` (string): Event contact email
- `EventContactPhoneNumber` (string): Event contact phone
- `AdministrativeContactPersonAlias` (PersonAlias): Person alias for administrative contact
- `AdministrativeContactEmail` (string): Administrative contact email
- `AdministrativeContactPhoneNumber` (string): Administrative contact phone
- `RequesterAlias` (PersonAlias): Person alias who requested the reservation

#### Additional Information
- `ReservationMinistry` (ReservationMinistry): Associated ministry
- `NumberAttending` (int?): Number of attendees
- `Note` (string): Reservation notes
- `SetupPhotoId` (int?): Setup photo file ID
- `SetupPhotoGuid` (Guid?): Setup photo file GUID
- `ModifiedDateTime` (DateTime?): Last modification datetime
- `ScheduleId` (int?): Schedule ID

#### Attributes (when `includeAttributes=true`)
- `Attributes` (Dictionary<string, AttributeCache>): Dictionary of attribute definitions
- `AttributeValues` (Dictionary<string, AttributeValueCache>): Dictionary of attribute values

---

## Reservation Details API

### Standard REST Endpoints

The Room Management API follows Rock's standard REST API patterns for CRUD operations:

#### Get Reservation by ID

**GET** `/api/Reservations/{id}`

Returns a single `Reservation` entity with all its properties.

**Response**: Full `Reservation` object with navigation properties that can be expanded using OData `$expand` syntax.

#### Get All Reservations

**GET** `/api/Reservations`

Returns all reservations. Supports OData query options:
- `$filter`: Filter reservations
- `$expand`: Expand related entities (e.g., `$expand=ReservationLocations,ReservationResources,ReservationType`)
- `$select`: Select specific properties
- `$orderby`: Order results
- `$top`: Limit number of results
- `$skip`: Skip number of results

#### Example: Get Reservation with Locations and Resources

```
GET /api/Reservations/123?$expand=ReservationLocations($expand=Location,LocationLayout),ReservationResources($expand=Resource)
```

#### Create Reservation

**POST** `/api/Reservations`

Creates a new reservation.

**Request Body**: `Reservation` object (JSON)

#### Update Reservation

**PUT** `/api/Reservations/{id}`

Updates an existing reservation.

**Request Body**: `Reservation` object (JSON)

#### Delete Reservation

**DELETE** `/api/Reservations/{id}`

Deletes a reservation (if authorized).

---

## Reservation Location Details

### Standard REST Endpoints

#### Get Reservation Location by ID

**GET** `/api/ReservationLocations/{id}`

Returns a single `ReservationLocation` entity.

**Response**: `ReservationLocation` object with navigation properties.

#### Get All Reservation Locations

**GET** `/api/ReservationLocations`

Returns all reservation locations. Supports OData query options.

#### Accessing Location Details from ReservationSummary

When retrieving reservation occurrences via `GetReservationOccurrences`, each `ReservationSummary` contains a `ReservationLocations` list. Each item in this list is a `ReservationLocation` object with:

#### ReservationLocation Properties

- `Id` (int): ReservationLocation ID
- `ReservationId` (int): Parent reservation ID
- `LocationId` (int): Location ID
- `Location` (Location): Full Location object (loaded when using `$expand` or `includeAttributes=true`)
  - `Name`: Location name
  - `Street1`, `Street2`, `City`, `State`, `PostalCode`: Address information
  - Other Rock Location properties
- `LocationLayoutId` (int?): Associated layout ID
- `LocationLayout` (LocationLayout): Layout object (if loaded)
  - `Name`: Layout name
  - `Description`: Layout description
  - `LayoutPhotoId`: Photo file ID
  - `LayoutPhotoUrl`: Photo URL (computed property)
  - `IsActive`: Whether layout is active
  - `IsDefault`: Whether this is the default layout
- `ApprovalState` (ReservationLocationApprovalState): Location approval state
  - `Unapproved = 1`
  - `Approved = 2`
  - `Denied = 3`
- `ReservationResources` (ICollection<ReservationResource>): Resources assigned to this location

#### Example: Get Locations for a Reservation Occurrence

```javascript
// Get reservation occurrences
const occurrences = await fetch('/api/Reservations/GetReservationOccurrences?startDateTime=2024-01-01&endDateTime=2024-12-31&includeAttributes=true');

// Access locations from the first occurrence
const locations = occurrences[0].ReservationLocations;

// Each location contains:
locations.forEach(location => {
  console.log(location.Location.Name); // Location name
  console.log(location.LocationLayout?.Name); // Layout name (if set)
  console.log(location.ApprovalState); // Approval state
  console.log(location.ReservationResources); // Resources assigned to this location
});
```

#### Accessing Location Details via Standard API

**GET** `/api/ReservationLocations/{id}?$expand=Location,LocationLayout,ReservationResources($expand=Resource)`

---

## Reservation Resource Details

### Standard REST Endpoints

#### Get Reservation Resource by ID

**GET** `/api/ReservationResources/{id}`

Returns a single `ReservationResource` entity.

#### Get All Reservation Resources

**GET** `/api/ReservationResources`

Returns all reservation resources. Supports OData query options.

#### Accessing Resource Details from ReservationSummary

When retrieving reservation occurrences via `GetReservationOccurrences`, each `ReservationSummary` contains:

1. **`ReservationResources`**: All resources for the reservation
2. **`UnassignedReservationResources`**: Resources not assigned to a specific location

#### ReservationResource Properties

- `Id` (int): ReservationResource ID
- `ReservationId` (int): Parent reservation ID
- `ResourceId` (int): Resource ID
- `Resource` (Resource): Full Resource object (loaded when using `$expand` or `includeAttributes=true`)
  - `Name`: Resource name
  - `CategoryId`: Category ID
  - `Category`: Category object
  - `CampusId`: Campus ID
  - `Campus`: Campus object
  - `LocationId`: Default location ID
  - `Location`: Default location object
  - `ApprovalGroupId`: Approval group ID
  - `ApprovalGroup`: Approval group object
  - `Quantity`: Available quantity
  - `Note`: Resource notes
  - `IsActive`: Whether resource is active
  - `PhotoId`: Photo file ID
  - `PhotoUrl`: Photo URL (computed property)
- `ReservationLocationId` (int?): ID of the location this resource is assigned to (null if unassigned)
- `ReservationLocation` (ReservationLocation): Location assignment (if loaded)
- `Quantity` (int?): Quantity reserved for this occurrence
- `ApprovalState` (ReservationResourceApprovalState): Resource approval state
  - `Unapproved = 1`
  - `Approved = 2`
  - `Denied = 3`

#### Example: Get Resources for a Reservation Occurrence

```javascript
// Get reservation occurrences
const occurrences = await fetch('/api/Reservations/GetReservationOccurrences?startDateTime=2024-01-01&endDateTime=2024-12-31&includeAttributes=true');

// Access all resources
const allResources = occurrences[0].ReservationResources;

// Access unassigned resources
const unassignedResources = occurrences[0].UnassignedReservationResources;

// Each resource contains:
allResources.forEach(resource => {
  console.log(resource.Resource.Name); // Resource name
  console.log(resource.Quantity); // Quantity reserved
  console.log(resource.ApprovalState); // Approval state
  console.log(resource.ReservationLocationId); // Location ID (null if unassigned)
});
```

#### Accessing Resource Details via Standard API

**GET** `/api/ReservationResources/{id}?$expand=Resource($expand=Category,Campus,Location),ReservationLocation($expand=Location)`

---

## Data Models

### ReservationApprovalState Enum

```csharp
public enum ReservationApprovalState
{
    Draft = 0,
    PendingInitialApproval = 1,
    Approved = 2,
    Denied = 3,
    ChangesNeeded = 4,
    PendingFinalApproval = 5,
    PendingSpecialApproval = 6,
    Cancelled = 7
}
```

### ReservationLocationApprovalState Enum

```csharp
public enum ReservationLocationApprovalState
{
    Unapproved = 1,
    Approved = 2,
    Denied = 3
}
```

### ReservationResourceApprovalState Enum

```csharp
public enum ReservationResourceApprovalState
{
    Unapproved = 1,
    Approved = 2,
    Denied = 3
}
```

### FilterTimeBy Enum

Used in `GetReservationOccurrences` to filter by time type:

```csharp
public enum FilterTimeBy
{
    Reservation = 0,  // Filter by reservation time (includes setup/cleanup)
    DoorLock = 1,    // Filter by door lock time
    Both = 2         // Return if either matches
}
```

---

## Examples

### Example 1: Get All Reservation Occurrences for Next Month

```javascript
const startDate = new Date().toISOString();
const endDate = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString();

const response = await fetch(
  `/api/Reservations/GetReservationOccurrences?startDateTime=${startDate}&endDateTime=${endDate}&includeAttributes=true`
);
const occurrences = await response.json();

occurrences.forEach(occurrence => {
  console.log(`Reservation: ${occurrence.ReservationName}`);
  console.log(`Event: ${occurrence.EventDateTimeDescription}`);
  console.log(`Reservation: ${occurrence.ReservationDateTimeDescription}`);
  console.log(`Locations: ${occurrence.ReservationLocations.length}`);
  console.log(`Resources: ${occurrence.ReservationResources.length}`);
});
```

### Example 2: Filter Occurrences by Location

```javascript
const locationIds = "1,2,3"; // Comma-separated location IDs

const response = await fetch(
  `/api/Reservations/GetReservationOccurrences?locationIds=${locationIds}&includeAttributes=true`
);
const occurrences = await response.json();
```

### Example 3: Get Occurrences with Pending Approval

```javascript
const approvalStates = "PendingInitialApproval,PendingFinalApproval";

const response = await fetch(
  `/api/Reservations/GetReservationOccurrences?approvalStates=${approvalStates}&includeAttributes=true`
);
const occurrences = await response.json();
```

### Example 4: Access Location Details from Occurrence

```javascript
const response = await fetch(
  `/api/Reservations/GetReservationOccurrences?startDateTime=2024-01-01&endDateTime=2024-12-31&includeAttributes=true`
);
const occurrences = await response.json();

occurrences.forEach(occurrence => {
  occurrence.ReservationLocations.forEach(location => {
    console.log(`Location: ${location.Location.Name}`);
    console.log(`Layout: ${location.LocationLayout?.Name || 'None'}`);
    console.log(`Approval: ${location.ApprovalState}`);
    
    // Resources assigned to this location
    location.ReservationResources.forEach(resource => {
      console.log(`  Resource: ${resource.Resource.Name} (Qty: ${resource.Quantity})`);
    });
  });
  
  // Unassigned resources
  occurrence.UnassignedReservationResources.forEach(resource => {
    console.log(`Unassigned Resource: ${resource.Resource.Name} (Qty: ${resource.Quantity})`);
  });
});
```

### Example 5: Get Full Reservation Details via Standard API

```javascript
const reservationId = 123;

// Get reservation with all related data
const response = await fetch(
  `/api/Reservations/${reservationId}?$expand=ReservationLocations($expand=Location,LocationLayout),ReservationResources($expand=Resource),ReservationType,ReservationMinistry`
);
const reservation = await response.json();

console.log(reservation.Name);
console.log(reservation.ReservationType.Name);
console.log(reservation.ReservationLocations);
console.log(reservation.ReservationResources);
```

### Example 6: Get Reservation Location via Standard API

```javascript
const reservationLocationId = 456;

const response = await fetch(
  `/api/ReservationLocations/${reservationLocationId}?$expand=Location,LocationLayout,ReservationResources($expand=Resource)`
);
const reservationLocation = await response.json();

console.log(reservationLocation.Location.Name);
console.log(reservationLocation.LocationLayout?.Name);
console.log(reservationLocation.ReservationResources);
```

### Example 7: Get Reservation Resource via Standard API

```javascript
const reservationResourceId = 789;

const response = await fetch(
  `/api/ReservationResources/${reservationResourceId}?$expand=Resource($expand=Category,Campus,Location),ReservationLocation($expand=Location)`
);
const reservationResource = await response.json();

console.log(reservationResource.Resource.Name);
console.log(reservationResource.Quantity);
console.log(reservationResource.ReservationLocation?.Location.Name);
```

### Example 8: Filter by Multiple Criteria

```javascript
const params = new URLSearchParams({
  startDateTime: '2024-01-01T00:00:00',
  endDateTime: '2024-12-31T23:59:59',
  reservationTypeIds: '1,2',
  locationIds: '10,20,30',
  resourceIds: '5,6',
  approvalStates: 'Approved,PendingFinalApproval',
  filterTimeBy: 'Reservation',
  includeAttributes: 'true'
});

const response = await fetch(
  `/api/Reservations/GetReservationOccurrences?${params.toString()}`
);
const occurrences = await response.json();
```

---

## Notes

### Important Considerations

1. **Occurrences vs. Reservations**: A single reservation can have multiple occurrences (if it's recurring). The `GetReservationOccurrences` endpoint returns one `ReservationSummary` per occurrence.

2. **Setup and Cleanup Time**: 
   - `ReservationStartDateTime` = `EventStartDateTime` - `SetupTime`
   - `ReservationEndDateTime` = `EventEndDateTime` + `CleanupTime`
   - Use `ReservationDateTimeDescription` to see times including setup/cleanup
   - Use `EventDateTimeDescription` to see actual event times

3. **Approval States**: By default, `GetReservationOccurrences` only returns approved reservations unless you specify `approvalStates` parameter.

4. **Resource Assignment**: Resources can be:
   - Unassigned: `ReservationLocationId` is null, appears in `UnassignedReservationResources`
   - Assigned to a location: `ReservationLocationId` is set, appears in `ReservationLocation.ReservationResources`

5. **Location Approval**: Locations have their own approval state separate from the reservation approval state.

6. **Resource Approval**: Resources have their own approval state separate from the reservation approval state.

7. **Attributes**: Use `includeAttributes=true` to get entity attributes. Attributes are loaded for:
   - Reservation (if `includeAttributes=true`)
   - ReservationLocations (if `includeAttributes=true`)
   - ReservationResources (if `includeAttributes=true`)

8. **Filtering**: The `filterTimeBy` parameter allows filtering by:
   - `Reservation`: Filter by reservation time (includes setup/cleanup)
   - `DoorLock`: Filter by door lock schedule times
   - `Both`: Return if either time matches

9. **Performance**: For large datasets, consider:
   - Using specific date ranges
   - Filtering by location, resource, or reservation type
   - Using OData `$top` and `$skip` for pagination

10. **Authentication**: All endpoints require authentication. Ensure your API requests include proper authentication headers.

---

## Related Endpoints

### Resources API

- **GET** `/api/Resources`: Get all resources
- **GET** `/api/Resources/{id}`: Get resource by ID

### Reservation Types API

- **GET** `/api/ReservationTypes`: Get all reservation types
- **GET** `/api/ReservationTypes/{id}`: Get reservation type by ID

### Location Layouts API

- **GET** `/api/LocationLayouts`: Get all location layouts
- **GET** `/api/LocationLayouts/{id}`: Get location layout by ID

---

## Summary

### Quick Reference

| What You Need | Endpoint | Key Properties |
|---------------|----------|---------------|
| **Reservation Occurrences** | `GET /api/Reservations/GetReservationOccurrences` | Returns `ReservationSummary[]` with dates, locations, resources |
| **Single Reservation** | `GET /api/Reservations/{id}` | Full `Reservation` object |
| **Reservation Locations** | From `ReservationSummary.ReservationLocations` or `GET /api/ReservationLocations/{id}` | `Location`, `LocationLayout`, `ApprovalState` |
| **Reservation Resources** | From `ReservationSummary.ReservationResources` or `GET /api/ReservationResources/{id}` | `Resource`, `Quantity`, `ApprovalState`, `ReservationLocationId` |
| **Unassigned Resources** | From `ReservationSummary.UnassignedReservationResources` | Resources where `ReservationLocationId` is null |

### Recommended Approach

1. **For Calendar/List Views**: Use `GetReservationOccurrences` to get all occurrences with locations and resources in one call
2. **For Detail Views**: Use standard REST endpoints with `$expand` to get full details
3. **For Filtering**: Use query parameters in `GetReservationOccurrences` or OData `$filter` in standard endpoints
4. **For Attributes**: Set `includeAttributes=true` or use `$expand` appropriately
