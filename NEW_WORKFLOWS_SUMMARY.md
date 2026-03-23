# New Workflow Actions Summary

## Overview

Three new high-priority workflow actions have been created for the Room Management plugin:

1. **RemoveReservationLocation** - Removes a location from a reservation
2. **RemoveReservationResource** - Removes a resource from a reservation (with quantity support)
3. **UpdateReservation** - Updates reservation properties

## 1. RemoveReservationLocation

### File
`Workflow/Action/Reservations/RemoveReservationLocation.cs`

### Purpose
Removes a specific location from an existing reservation. Handles resources assigned to that location and updates approval states.

### Attributes
- **Reservation Attribute** (required) - The reservation to modify
- **Location Attribute** (required) - The location to remove
- **Remove Assigned Resources** (boolean, default: true) - If true, removes resources assigned to the location. If false, keeps resources but unassigns them.

### Features
- ✅ Removes location from reservation
- ✅ Handles resources assigned to location (remove or unassign)
- ✅ Updates reservation approval state if needed
- ✅ Tracks all changes in history
- ✅ Validates location exists on reservation before removal
- ✅ Recalculates approval state after removal

### Use Cases
- Location becomes unavailable
- Conflict resolution
- User cancellation
- Automated cleanup

## 2. RemoveReservationResource

### File
`Workflow/Action/Reservations/RemoveReservationResource.cs`

### Purpose
Removes a specific resource from an existing reservation. Supports both complete removal and quantity reduction for quantity-based resources.

### Attributes
- **Reservation Attribute** (required) - The reservation to modify
- **Resource Attribute** (required) - The resource to remove
- **Quantity To Remove** (optional) - For quantity-based resources, the quantity to remove. If not specified or 0, removes the resource entirely.

### Features
- ✅ Removes resource from reservation
- ✅ Supports quantity reduction (e.g., reduce from 10 to 8 tables)
- ✅ Handles both quantity-based and non-quantity resources
- ✅ Updates reservation approval state if needed
- ✅ Tracks all changes in history
- ✅ Validates resource exists on reservation before removal

### Use Cases
- Resource becomes unavailable
- Quantity reduction (e.g., reduce table count)
- Overbooking resolution
- Maintenance windows

## 3. UpdateReservation

### File
`Workflow/Action/Reservations/UpdateReservation.cs`

### Purpose
Updates various properties of an existing reservation. Only provided fields will be updated (partial updates supported).

### Attributes
- **Reservation Attribute** (required) - The reservation to update
- **Name** (optional) - New reservation name
- **Schedule Attribute** (optional) - New schedule
- **Note** (optional) - New note
- **Number Attending** (optional) - New attendance count
- **Event Contact Person Attribute** (optional) - New event contact person
- **Event Contact Phone** (optional) - New event contact phone
- **Event Contact Email** (optional) - New event contact email
- **Administrative Contact Person Attribute** (optional) - New administrative contact person
- **Administrative Contact Phone** (optional) - New administrative contact phone
- **Administrative Contact Email** (optional) - New administrative contact email
- **Setup Time** (optional) - New setup time in minutes
- **Cleanup Time** (optional) - New cleanup time in minutes
- **Campus Attribute** (optional) - New campus
- **Reservation Ministry Attribute** (optional) - New reservation ministry (GUID or ID)

### Features
- ✅ Updates any combination of reservation properties
- ✅ Only updates fields that are provided (partial updates)
- ✅ Validates schedule changes and checks for conflicts
- ✅ Updates FirstOccurrenceStartDateTime and LastOccurrenceEndDateTime when schedule changes
- ✅ Updates approval state if schedule changes create conflicts
- ✅ Tracks all changes in history
- ✅ Supports Lava merge fields for text values

### Use Cases
- Schedule change requests
- Contact information updates
- Bulk schedule updates (e.g., daylight saving time)
- Notes/details updates
- Attendance count updates

## Implementation Details

### Code Quality
- ✅ Follows existing workflow patterns
- ✅ Proper error handling
- ✅ History tracking for all changes
- ✅ Correct SaveChanges ordering (entity first, then history)
- ✅ Null safety checks
- ✅ Consistent with existing workflow actions

### History Tracking
All workflows track changes in the reservation history:
- What was changed
- Old value vs new value
- Who/what made the change (workflow name)

### Approval State Management
- Workflows automatically update approval states when appropriate
- Uses `ReservationService.UpdateApproval()` for recalculation
- Handles conflicts appropriately

### Error Handling
- Validates inputs before processing
- Returns clear error messages
- Handles edge cases (null values, missing entities, etc.)

## Testing Recommendations

### RemoveReservationLocation
1. Test removing location that exists
2. Test removing location that doesn't exist (should error)
3. Test with resources assigned to location (both remove and unassign options)
4. Test with no locations remaining after removal
5. Test approval state updates

### RemoveReservationResource
1. Test removing resource that exists
2. Test removing resource that doesn't exist (should error)
3. Test quantity reduction (e.g., 10 to 8)
4. Test removing non-quantity resource
5. Test removing quantity-based resource entirely
6. Test approval state updates

### UpdateReservation
1. Test updating single field
2. Test updating multiple fields
3. Test schedule update with conflicts
4. Test schedule update without conflicts
5. Test updating contact information
6. Test partial updates (only some fields provided)
7. Test with invalid values (should handle gracefully)

## Next Steps

These workflows are ready to use. Consider:

1. **Testing** - Test each workflow in a development environment
2. **Documentation** - Add to user documentation/guides
3. **Examples** - Create example workflow configurations
4. **Training** - Train workflow designers on new capabilities

## Related Workflows

These workflows complement existing workflows:
- `AddReservationLocation` - Now you can add AND remove locations
- `AddReservationResource` - Now you can add AND remove resources
- `CreateReservation` - Now you can create AND update reservations
- `SetReservationApprovalState` - Works with update workflows

## Future Enhancements

Consider adding:
- `CheckReservationConflicts` - Check for conflicts before updating
- `CheckResourceAvailability` - Check availability before adding resources
- `SetReservationLocationApprovalState` - Set approval for single location
- `SetReservationResourceApprovalState` - Set approval for single resource
