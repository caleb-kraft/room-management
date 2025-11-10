# Workflow Recommendations for Room Management Plugin

## Analysis Summary

After reviewing the existing workflows and available ReservationService methods, here are recommendations for new workflows and improvements.

## Current Workflows

1. ✅ **AddReservationLocation** - Adds a location to a reservation
2. ✅ **AddReservationResource** - Adds a resource to a reservation  
3. ✅ **CreateReservation** - Creates a new reservation
4. ✅ **GetApprovalGroup** - Gets an approval group based on criteria
5. ✅ **SetReservationApprovalState** - Sets the approval state of a reservation
6. ✅ **SetReservationLocationsApprovalStates** - Sets approval states for all locations
7. ✅ **SetReservationResourcesApprovalStates** - Sets approval states for all resources

## Recommended New Workflows

### High Priority

#### 1. **RemoveReservationLocation** ⭐⭐⭐
**Purpose**: Remove a specific location from a reservation

**Use Cases**:
- User cancels a location booking
- Location becomes unavailable
- Workflow needs to remove conflicting locations

**Attributes**:
- Reservation Attribute (required)
- Location Attribute (required) - The location to remove

**Implementation Notes**:
- Should check if location exists on reservation
- Should update reservation approval state if needed
- Should track history changes
- Should handle resources assigned to that location (optionally remove or reassign)

---

#### 2. **RemoveReservationResource** ⭐⭐⭐
**Purpose**: Remove a specific resource from a reservation

**Use Cases**:
- Resource becomes unavailable
- User reduces resource quantity
- Workflow needs to remove conflicting resources

**Attributes**:
- Reservation Attribute (required)
- Resource Attribute (required) - The resource to remove
- Quantity (optional) - If specified, reduce quantity instead of removing entirely

**Implementation Notes**:
- Should check if resource exists on reservation
- Should update reservation approval state if needed
- Should track history changes
- Should handle quantity reduction vs complete removal

---

#### 3. **UpdateReservation** ⭐⭐⭐
**Purpose**: Update reservation properties (name, schedule, notes, etc.)

**Use Cases**:
- Update reservation name
- Change schedule/time
- Update contact information
- Update notes or other properties

**Attributes**:
- Reservation Attribute (required)
- Name (optional) - New name
- Schedule Attribute (optional) - New schedule
- Note (optional) - New note
- Number Attending (optional) - Updated count
- Event Contact Person Attribute (optional)
- Administrative Contact Person Attribute (optional)
- Setup Time (optional)
- Cleanup Time (optional)
- Campus Attribute (optional)
- Reservation Ministry Attribute (optional)

**Implementation Notes**:
- Should only update provided fields
- Should validate schedule changes for conflicts
- Should update approval state if schedule changes create conflicts
- Should track history for all changes

---

#### 4. **CheckReservationConflicts** ⭐⭐
**Purpose**: Check if a reservation has conflicts with existing reservations

**Use Cases**:
- Validate before approval
- Check before creating/updating reservation
- Workflow decision point

**Attributes**:
- Reservation Attribute (required)
- Conflict Type (optional) - "Location", "Resource", or "Both" (default: "Both")
- Include Potential Conflicts (optional) - Boolean (default: false)

**Output Attributes**:
- Has Conflicts (Boolean) - Set to true if conflicts exist
- Conflict Details (Text) - HTML formatted conflict information
- Conflicted Location IDs (Text) - Comma-separated location IDs
- Conflicted Resource IDs (Text) - Comma-separated resource IDs

**Implementation Notes**:
- Uses `GetReservedLocationIds()` and `GetConflictsForResourceId()` methods
- Should return detailed conflict information
- Can be used as a workflow decision point

---

#### 5. **CheckResourceAvailability** ⭐⭐
**Purpose**: Check if a resource is available for a reservation

**Use Cases**:
- Validate resource availability before adding
- Check quantity available
- Workflow decision point

**Attributes**:
- Reservation Attribute (required)
- Resource Attribute (required)
- Required Quantity (optional) - Default: 1

**Output Attributes**:
- Is Available (Boolean) - True if resource is available
- Available Quantity (Integer) - Quantity available
- Booked Quantity (Integer) - Quantity already booked

**Implementation Notes**:
- Uses `GetAvailableResourceQuantity()` method
- Should handle both quantity-based and non-quantity resources

---

### Medium Priority

#### 6. **SetReservationLocationApprovalState** ⭐⭐
**Purpose**: Set approval state for a single specific location (not all locations)

**Use Cases**:
- Approve/deny a specific location
- Workflow needs to handle locations individually

**Attributes**:
- Reservation Attribute (required)
- Location Attribute (required) - The specific location
- Approval State Attribute (optional)
- Approval State (optional) - Direct value if attribute not provided

**Implementation Notes**:
- Similar to SetReservationLocationsApprovalStates but for single location
- Should update parent reservation approval state if needed
- Should track history changes

---

#### 7. **SetReservationResourceApprovalState** ⭐⭐
**Purpose**: Set approval state for a single specific resource (not all resources)

**Use Cases**:
- Approve/deny a specific resource
- Workflow needs to handle resources individually

**Attributes**:
- Reservation Attribute (required)
- Resource Attribute (required) - The specific resource
- Approval State Attribute (optional)
- Approval State (optional) - Direct value if attribute not provided

**Implementation Notes**:
- Similar to SetReservationResourcesApprovalStates but for single resource
- Should update parent reservation approval state if needed
- Should track history changes

---

#### 8. **CheckAllLocationsAndResourcesApproved** ⭐
**Purpose**: Check if all locations and resources are approved (workflow decision point)

**Use Cases**:
- Workflow decision point before final approval
- Conditional workflow branching

**Attributes**:
- Reservation Attribute (required)

**Output Attributes**:
- All Approved (Boolean) - True if all locations and resources are approved
- Changes Needed (Boolean) - True if any location/resource needs changes

**Implementation Notes**:
- Uses `AreAllLocationsAndResourcesApproved()` method
- Uses `AreLocationOrResourceChangesNeeded()` method
- Should be a decision workflow action

---

#### 9. **UpdateReservationApproval** ⭐
**Purpose**: Update reservation approval using the UpdateApproval logic (handles cascading approvals)

**Use Cases**:
- Workflow needs to use the full approval logic
- Auto-approve resources/locations without approval groups
- Handle final approval group logic

**Attributes**:
- Reservation Attribute (required)
- Approval State Attribute (required)
- Is Override (optional) - Boolean, default: false

**Implementation Notes**:
- Uses `UpdateApproval()` method which handles:
  - Auto-approving resources/locations without approval groups
  - Setting pending states when approval groups exist
  - Handling final approval group logic
  - Cascading approval states

---

### Lower Priority (Advanced)

#### 10. **DeleteSingleOccurrence** ⭐
**Purpose**: Delete a single occurrence from a recurring reservation

**Use Cases**:
- Cancel one instance of a recurring reservation
- Remove a specific date from a series

**Attributes**:
- Reservation Attribute (required)
- Occurrence DateTime Attribute (required) - The date/time of the occurrence to delete

**Implementation Notes**:
- Uses `DeleteSingleOccurrence()` method
- Only works for recurring reservations
- Adds EXDATE to the schedule
- Should validate that reservation is recurring

---

#### 11. **GetReservationScheduleText** ⭐
**Purpose**: Get human-readable schedule text for a reservation

**Use Cases**:
- Display schedule in notifications
- Log schedule information
- Set workflow attribute with schedule text

**Attributes**:
- Reservation Attribute (required)
- Schedule Text Attribute (required) - Attribute to set with schedule text

**Implementation Notes**:
- Uses `GetFriendlyReservationScheduleText()` extension method
- Should handle both single and recurring reservations

---

## Workflow Improvements

### Existing Workflow Enhancements

#### 1. **AddReservationLocation** - Add Conflict Detection
- **Enhancement**: Add optional conflict detection before adding location
- **Benefit**: Prevent adding conflicting locations automatically
- **Implementation**: Call `GetReservedLocationIds()` before adding, optionally set approval state to Denied if conflict exists

#### 2. **AddReservationResource** - Add Availability Check
- **Enhancement**: Add optional availability check before adding resource
- **Benefit**: Prevent overbooking resources
- **Implementation**: Call `GetAvailableResourceQuantity()` before adding, optionally set approval state to Denied if insufficient quantity

#### 3. **CreateReservation** - Add Auto-Conflict Detection
- **Enhancement**: Automatically check for conflicts after creation
- **Benefit**: Set approval state appropriately based on conflicts
- **Implementation**: After creating reservation, check conflicts and set approval state to ChangesNeeded if conflicts exist

#### 4. **SetReservationApprovalState** - Add Validation
- **Enhancement**: Validate that approval state change is valid
- **Benefit**: Prevent invalid state transitions
- **Implementation**: Add validation logic before setting state

---

## Implementation Priority

### Phase 1 (Critical - Do First)
1. ✅ RemoveReservationLocation
2. ✅ RemoveReservationResource
3. ✅ UpdateReservation

### Phase 2 (High Value)
4. ✅ CheckReservationConflicts
5. ✅ CheckResourceAvailability
6. ✅ SetReservationLocationApprovalState
7. ✅ SetReservationResourceApprovalState

### Phase 3 (Nice to Have)
8. ✅ CheckAllLocationsAndResourcesApproved
9. ✅ UpdateReservationApproval
10. ✅ DeleteSingleOccurrence
11. ✅ GetReservationScheduleText

### Phase 4 (Enhancements)
- Enhance existing workflows with conflict detection
- Add validation to existing workflows

---

## Code Patterns to Follow

### Standard Pattern for Workflow Actions

```csharp
public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
{
    errorMessages = new List<string>();
    
    // 1. Get reservation
    Reservation reservation = GetReservationFromAttribute( action, rockContext, errorMessages );
    if ( reservation == null ) return false;
    
    // 2. Perform operation
    // ... operation logic ...
    
    // 3. Track changes
    var changes = new History.HistoryChangeList();
    // ... track changes ...
    
    // 4. Save changes (entity first, then history)
    rockContext.SaveChanges();
    
    if ( changes.Any() )
    {
        changes.Add( new History.HistoryChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, 
            string.Format( "Updated by the '{0}' workflow", action.ActionTypeCache.ActivityType.WorkflowType.Name ) ) );
        HistoryService.SaveChanges( rockContext, typeof( Reservation ), 
            SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(), reservation.Id, changes, false );
    }
    
    return true;
}
```

### Helper Method Pattern (Recommended)

Consider creating a base class or helper methods:

```csharp
public static class ReservationWorkflowHelpers
{
    public static Reservation GetReservationFromAttribute( WorkflowAction action, RockContext rockContext, List<string> errorMessages )
    {
        // Common logic for getting reservation
    }
    
    public static void SaveReservationHistory( RockContext rockContext, Reservation reservation, 
        History.HistoryChangeList changes, WorkflowAction action )
    {
        // Common logic for saving history
    }
}
```

---

## Testing Considerations

For each new workflow:

1. **Unit Tests**: Test with valid and invalid inputs
2. **Integration Tests**: Test with real reservations
3. **Edge Cases**: 
   - Null/empty attributes
   - Non-existent reservations
   - Recurring vs single reservations
   - Reservations with/without locations/resources
4. **History Tracking**: Verify history is correctly recorded
5. **Approval States**: Verify approval states are updated correctly
6. **Performance**: Test with reservations containing many locations/resources

---

## Documentation Needs

For each new workflow:

1. **XML Documentation**: Clear description and parameter documentation
2. **User Guide**: Step-by-step instructions for workflow designers
3. **Examples**: Example workflow configurations
4. **Error Messages**: Clear, actionable error messages

---

## Summary

The most critical missing workflows are:
1. **RemoveReservationLocation** - Essential for managing reservations
2. **RemoveReservationResource** - Essential for managing reservations  
3. **UpdateReservation** - Essential for modifying existing reservations
4. **CheckReservationConflicts** - Important for validation workflows

These four workflows would significantly improve the workflow capabilities of the plugin and enable more complex reservation management scenarios.
