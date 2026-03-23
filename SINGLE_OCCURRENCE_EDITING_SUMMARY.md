# Single Occurrence Editing - Implementation Summary

## Overview

This document summarizes the comprehensive implementation of single occurrence editing for recurring reservations in the Room Management system. This feature allows users to edit or delete individual occurrences of recurring reservations without affecting the rest of the series.

## Implementation Date

Completed: Current session

## Features Implemented

### 1. Core Service Methods (`ReservationService.cs`)

#### `EditSingleOccurrence()`
- Edits a single occurrence by:
  - Adding EXDATE to original reservation's schedule
  - Creating a new reservation for the modified occurrence
  - Linking them via ForeignKey/ForeignGuid
- Returns the newly created reservation

#### `DeleteSingleOccurrence()`
- Deletes a single occurrence by adding EXDATE
- Only works with recurring reservations
- Preserves all other occurrences

#### `GetExceptionOccurrences()`
- Returns all exception occurrences for a given reservation
- Useful for displaying related occurrences

#### `GetExceptionOccurrenceCount()`
- Returns count of exception occurrences
- Useful for quick checks

#### `HasExceptionOccurrences()`
- Boolean check if reservation has exception occurrences

#### `IsRecurringReservation()` (Static)
- Checks if a reservation is recurring
- Useful for conditional logic

#### `IsExceptionOccurrence()` (Static)
- Checks if a reservation is an exception occurrence

#### `GetEditOccurrenceUrl()` (Static)
- Generates URL for editing a single occurrence
- Helper for UI components

### 2. API Endpoints (`ReservationsController.Partial.cs`)

#### `POST /api/Reservations/EditSingleOccurrence`
- **Parameters:**
  - `reservationId` (int): Original reservation ID
  - `occurrenceDateTime` (DateTime): Occurrence date/time
  - `modifiedReservation` (Reservation): Modified reservation data
- **Returns:** New reservation object

#### `POST /api/Reservations/DeleteSingleOccurrence`
- **Parameters:**
  - `reservationId` (int): Original reservation ID
  - `occurrenceDateTime` (DateTime): Occurrence date/time
- **Returns:** Boolean (success/failure)

#### `GET /api/Reservations/GetExceptionOccurrences/{reservationId}`
- **Parameters:**
  - `reservationId` (int): Original reservation ID
- **Returns:** Queryable list of exception occurrence reservations

### 3. UI Enhancements (`ReservationDetail.ascx.cs` & `.ascx`)

#### Single Occurrence Detection
- Detects `OccurrenceDateTime` query parameter
- Stores in hidden field `hfOccurrenceDateTime`
- Shows warning banner when editing single occurrence

#### "Edit All Occurrences" Button
- Appears when editing a single occurrence
- Allows switching to edit entire series

#### Exception Occurrences Display
- Grid showing all exception occurrences for a reservation
- Clickable to navigate to exception occurrence details
- Shows name, date, and approval state

#### Exception Occurrence Indicator
- Shows banner when viewing an exception occurrence
- Links back to original series

### 4. Data Model Enhancements (`ReservationSummary.cs`)

#### New Properties
- `IsRecurring`: Indicates if reservation is part of recurring series
- `IsExceptionOccurrence`: Indicates if this is an exception occurrence
- `OriginalReservationId`: Links exception occurrences back to original

#### Updated Methods (`ReservationExtensionMethods.cs`)
- `GetReservationSummaries()` now populates recurrence information
- Automatically detects recurring vs. exception occurrences

## Technical Implementation Details

### Exception Date Handling
- Uses standard iCalendar EXDATE pattern
- Normalizes exception dates for calendar compatibility
- Handles all-day and timed events correctly
- Removes duration specifiers for compatibility

### Linking Mechanism
- Uses `ForeignKey` pattern: `"OriginalReservation_{id}"`
- Also uses `ForeignGuid` for additional linking
- Enables easy querying of related occurrences

### Schedule Updates
- Original schedule is updated with EXDATE
- New reservation gets one-time schedule
- Both schedules remain synchronized

## Usage Examples

### Edit Single Occurrence via URL
```
/page/ReservationDetail?ReservationId=123&OccurrenceDateTime=2024-03-15T14:00:00
```

### Edit Single Occurrence via API
```csharp
POST /api/Reservations/EditSingleOccurrence
{
    "reservationId": 123,
    "occurrenceDateTime": "2024-03-15T14:00:00",
    "modifiedReservation": {
        "Name": "Modified Event Name",
        "SetupTime": 30,
        // ... other modified properties
    }
}
```

### Delete Single Occurrence via API
```csharp
POST /api/Reservations/DeleteSingleOccurrence
{
    "reservationId": 123,
    "occurrenceDateTime": "2024-03-15T14:00:00"
}
```

### Get Exception Occurrences via API
```csharp
GET /api/Reservations/GetExceptionOccurrences/123
```

### Check if Reservation is Recurring (Code)
```csharp
bool isRecurring = ReservationService.IsRecurringReservation(reservation);
```

### Generate Edit Occurrence URL
```csharp
string editUrl = ReservationService.GetEditOccurrenceUrl(
    reservationId, 
    occurrenceDateTime, 
    detailPageUrl
);
```

### Check for Exception Occurrences
```csharp
var reservationService = new ReservationService(rockContext);
var hasExceptions = reservationService.HasExceptionOccurrences(reservation);
var exceptionCount = reservationService.GetExceptionOccurrenceCount(reservation);
var exceptions = reservationService.GetExceptionOccurrences(reservation).ToList();
```

## Lava Template Examples

### Display Recurrence Information
```lava
{% if ReservationSummary.IsRecurring %}
    <span class="badge badge-info">Recurring Event</span>
{% endif %}

{% if ReservationSummary.IsExceptionOccurrence %}
    <span class="badge badge-warning">Exception Occurrence</span>
    <a href="/page/ReservationDetail?ReservationId={{ ReservationSummary.OriginalReservationId }}">
        View Original Series
    </a>
{% endif %}
```

### Add Edit Occurrence Button
```lava
{% if ReservationSummary.IsRecurring %}
    {% assign editUrl = ReservationSummary.Id | EditOccurrenceUrl: ReservationSummary.EventStartDateTime %}
    <a href="{{ editUrl }}" class="btn btn-xs btn-default">
        Edit This Occurrence
    </a>
{% endif %}
```

## Benefits

1. **Preserves Original Series**: Original recurring series remains intact
2. **Standard iCalendar Pattern**: Uses EXDATE, compatible with all calendar apps
3. **Full Data Preservation**: Locations, resources, attributes, door locks all preserved
4. **Clear User Feedback**: Warnings and indicators show what's happening
5. **Easy Navigation**: Links between original and exception occurrences
6. **Backward Compatible**: No breaking changes to existing functionality
7. **API Ready**: Full REST API support for programmatic access

## Testing Checklist

- [ ] Edit single occurrence of recurring reservation
- [ ] Verify original series unchanged
- [ ] Verify new reservation created correctly
- [ ] Verify exception date added to original schedule
- [ ] Delete single occurrence
- [ ] Verify exception date added when deleting
- [ ] View exception occurrences list
- [ ] Navigate from exception occurrence to original
- [ ] Navigate from original to exception occurrence
- [ ] Verify calendar feeds show exceptions correctly
- [ ] Test with all-day events
- [ ] Test with timed events
- [ ] Test with multiple exception occurrences
- [ ] Verify API endpoints work correctly
- [ ] Test error handling (non-existent occurrence, non-recurring reservation)

## Files Modified

1. `Model/Reservation/ReservationService.cs` - Core service methods
2. `Model/Reservation/ReservationSummary.cs` - Added recurrence properties
3. `Model/Reservation/ReservationExtensionMethods.cs` - Updated summary generation
4. `Controllers/ReservationsController.Partial.cs` - API endpoints
5. `Plugins/com_bemaservices/RoomManagement/ReservationDetail.ascx.cs` - UI logic
6. `Plugins/com_bemaservices/RoomManagement/ReservationDetail.ascx` - UI markup

## Future Enhancements (Optional)

1. Bulk edit multiple occurrences
2. Undo single occurrence edit (merge exception back)
3. Visual calendar view showing exceptions
4. Notification when exception occurrences are created
5. Reports showing exception occurrences
6. Export exception occurrences separately

## Notes

- Exception occurrences are linked via ForeignKey pattern for easy querying
- All exception dates are normalized for maximum calendar compatibility
- The system gracefully handles edge cases (non-recurring reservations, missing occurrences)
- UI clearly indicates when editing single vs. all occurrences
