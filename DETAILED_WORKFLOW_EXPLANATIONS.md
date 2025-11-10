# Detailed Workflow Explanations

This document provides comprehensive explanations of each recommended workflow, including what it does, use cases, and why it's currently missing.

---

## 1. RemoveReservationLocation ⭐⭐⭐

### What It Does
Removes a specific location from an existing reservation. This includes:
- Deleting the `ReservationLocation` record
- Optionally removing or reassigning resources that were assigned to that location
- Updating the reservation's approval state if needed
- Tracking the change in history

### Current State: **MISSING**
**Why It Doesn't Work Now:**
- There is **no workflow action** to remove a location from a reservation
- The only way to remove a location is through the UI (`ReservationDetail.ascx.cs` has a `RemoveLocation()` method, but this is UI-only)
- Workflows cannot programmatically remove locations, even though the underlying service supports it

**What Exists:**
- ✅ `AddReservationLocation` workflow action (can add locations)
- ✅ UI code that removes locations (`ReservationDetail.ascx.cs` line 4020)
- ✅ `ReservationLocationService` with standard CRUD operations
- ❌ **No workflow action to remove locations**

### Detailed Use Cases

#### Use Case 1: Location Becomes Unavailable
**Scenario:** A location (e.g., "Main Auditorium") becomes unavailable due to maintenance or emergency.

**Current Workflow Attempt:**
```
1. User reports location unavailable
2. Workflow triggered
3. ❌ CANNOT remove location from reservation
4. Must manually edit reservation in UI
```

**With RemoveReservationLocation:**
```
1. User reports location unavailable
2. Workflow triggered
3. ✅ RemoveReservationLocation workflow action removes location
4. Optionally notify requester
5. Optionally suggest alternative locations
```

**Real Example:**
- Church has "Main Auditorium" reserved for Sunday service
- HVAC system fails Friday night
- Maintenance team triggers workflow
- Workflow removes "Main Auditorium" from all affected reservations
- Workflow sends notification to reservation requester
- Workflow suggests alternative locations (e.g., "Chapel", "Gym")

#### Use Case 2: Conflict Resolution
**Scenario:** Two reservations conflict for the same location/time.

**Current Workflow Attempt:**
```
1. Conflict detected
2. Workflow triggered
3. ❌ CANNOT automatically remove conflicting location
4. Must manually resolve in UI
```

**With RemoveReservationLocation:**
```
1. Conflict detected
2. Workflow checks priority (e.g., by reservation type, date created)
3. ✅ RemoveReservationLocation removes location from lower-priority reservation
4. Notify affected parties
5. Suggest alternative times/locations
```

**Real Example:**
- "Youth Group" reserves "Gym" for Friday 7pm
- "Basketball League" also reserves "Gym" for Friday 7pm
- Conflict detection workflow runs
- Workflow removes "Gym" from "Basketball League" (lower priority)
- Workflow sends email: "Your reservation was modified due to a conflict. Alternative times available..."

#### Use Case 3: Cancellation Workflow
**Scenario:** User cancels a location booking but keeps the reservation.

**Current Workflow Attempt:**
```
1. User cancels location booking
2. Workflow triggered
3. ❌ CANNOT remove location
4. Must manually edit reservation
```

**With RemoveReservationLocation:**
```
1. User cancels location booking
2. Workflow triggered
3. ✅ RemoveReservationLocation removes location
4. If no locations remain, optionally cancel entire reservation
5. Update approval state
```

**Real Example:**
- "Women's Bible Study" reserves "Room 101" and "Room 102"
- Group decides they only need "Room 101"
- User cancels "Room 102" booking
- Workflow removes "Room 102" from reservation
- Reservation continues with just "Room 101"

#### Use Case 4: Automated Cleanup
**Scenario:** Remove locations from reservations that are in "Denied" state for too long.

**Current Workflow Attempt:**
```
1. Scheduled job finds denied locations
2. ❌ CANNOT remove them via workflow
3. Must use custom code or manual cleanup
```

**With RemoveReservationLocation:**
```
1. Scheduled job finds denied locations older than 30 days
2. ✅ RemoveReservationLocation removes them
3. Clean up reservation state
```

### Technical Details

**What Happens When Removing a Location:**
1. Find the `ReservationLocation` record
2. Check for resources assigned to that location
3. Optionally remove those resources or reassign them
4. Delete the `ReservationLocation` record
5. Update reservation approval state (may need to recalculate)
6. Save history change
7. Save to database

**Resources Assigned to Location:**
When removing a location, there are resources that might be assigned to it. The workflow should handle:
- **Option 1:** Remove resources assigned to the location
- **Option 2:** Keep resources but make them "unassigned" (set `ReservationLocationId` to null)
- **Option 3:** Reassign resources to another location

**Approval State Impact:**
- If location was "Approved" and removal causes all locations to be removed, reservation might need approval state change
- If location was "Denied", removing it might allow reservation to proceed

---

## 2. RemoveReservationResource ⭐⭐⭐

### What It Does
Removes a specific resource from an existing reservation. This includes:
- Deleting the `ReservationResource` record (or reducing quantity)
- Updating the reservation's approval state if needed
- Tracking the change in history

### Current State: **MISSING**
**Why It Doesn't Work Now:**
- There is **no workflow action** to remove a resource from a reservation
- The only way to remove a resource is through the UI (`ReservationDetail.ascx.cs` has a `RemoveResource()` method, but this is UI-only)
- Workflows cannot programmatically remove resources

**What Exists:**
- ✅ `AddReservationResource` workflow action (can add resources)
- ✅ UI code that removes resources (`ReservationDetail.ascx.cs` line 4435)
- ✅ `ReservationResourceService` with standard CRUD operations
- ❌ **No workflow action to remove resources**

### Detailed Use Cases

#### Use Case 1: Resource Becomes Unavailable
**Scenario:** A resource (e.g., "Projector", "Microphone Set") becomes unavailable due to damage or maintenance.

**Current Workflow Attempt:**
```
1. Resource reported unavailable
2. Workflow triggered
3. ❌ CANNOT remove resource from reservations
4. Must manually edit each reservation
```

**With RemoveReservationResource:**
```
1. Resource reported unavailable
2. Workflow finds all reservations using that resource
3. ✅ RemoveReservationResource removes resource from each reservation
4. Notify reservation requesters
5. Suggest alternative resources if available
```

**Real Example:**
- "Projector #1" breaks down
- 15 reservations have "Projector #1" booked
- Maintenance workflow runs
- Workflow removes "Projector #1" from all 15 reservations
- Workflow sends notifications: "Your reservation was updated. Projector #1 is unavailable. Alternative projectors available..."

#### Use Case 2: Quantity Reduction
**Scenario:** User realizes they don't need as many of a resource.

**Current Workflow Attempt:**
```
1. User requests quantity reduction
2. Workflow triggered
3. ❌ CANNOT reduce quantity
4. Must manually edit reservation
```

**With RemoveReservationResource:**
```
1. User requests quantity reduction (e.g., from 5 to 3 tables)
2. Workflow triggered
3. ✅ RemoveReservationResource with quantity option reduces from 5 to 3
4. Or removes 2 instances of the resource
```

**Real Example:**
- "Wedding Reception" reserves 10 tables
- Couple realizes they only need 8 tables
- User triggers workflow to reduce quantity
- Workflow reduces table quantity from 10 to 8
- Frees up 2 tables for other reservations

#### Use Case 3: Overbooking Resolution
**Scenario:** Resource was overbooked (more quantity reserved than available).

**Current Workflow Attempt:**
```
1. Overbooking detected
2. Workflow triggered
3. ❌ CANNOT automatically remove excess bookings
4. Must manually resolve
```

**With RemoveReservationResource:**
```
1. Overbooking detected
2. Workflow identifies excess bookings (by priority, date created, etc.)
3. ✅ RemoveReservationResource removes excess quantity
4. Notify affected reservations
```

**Real Example:**
- "Tables" resource has quantity of 20
- Reservation A books 15 tables
- Reservation B books 10 tables (total 25, but only 20 available)
- Conflict detection workflow runs
- Workflow removes 5 tables from Reservation B (lower priority)
- Reservation B now has 5 tables, Reservation A keeps 15

#### Use Case 4: Resource Maintenance Window
**Scenario:** Resource needs to be taken out of service for maintenance.

**Current Workflow Attempt:**
```
1. Maintenance scheduled
2. ❌ CANNOT remove resource from affected reservations
3. Must manually contact each reservation requester
```

**With RemoveReservationResource:**
```
1. Maintenance scheduled for "Sound System" from Jan 15-20
2. Workflow finds all reservations using "Sound System" during that period
3. ✅ RemoveReservationResource removes resource from those reservations
4. Notify requesters with maintenance explanation
5. Suggest alternative dates/resources
```

### Technical Details

**Quantity Handling:**
The workflow should support two modes:
1. **Complete Removal:** Remove the resource entirely (delete `ReservationResource` record)
2. **Quantity Reduction:** Reduce the quantity (e.g., from 5 to 3)

**Approval State Impact:**
- If resource was "Approved" and removal causes issues, reservation might need approval state change
- If resource was "Denied", removing it might allow reservation to proceed

**Location Assignment:**
- Resources can be assigned to a location (`ReservationLocationId`)
- If removing a resource assigned to a location, should it:
  - Just remove the resource?
  - Also remove the location if no other resources assigned?
  - Leave location but mark it differently?

---

## 3. UpdateReservation ⭐⭐⭐

### What It Does
Updates various properties of an existing reservation, such as:
- Name
- Schedule (time/date)
- Notes
- Contact information (event contact, administrative contact)
- Number attending
- Setup/Cleanup time
- Campus
- Reservation Ministry

### Current State: **MISSING**
**Why It Doesn't Work Now:**
- There is **no workflow action** to update reservation properties
- The only way to update a reservation is through the UI or direct database access
- Workflows cannot programmatically modify reservation details

**What Exists:**
- ✅ `CreateReservation` workflow action (can create new reservations)
- ✅ UI code that updates reservations (`ReservationDetail.ascx.cs`)
- ✅ `ReservationService` with standard update capabilities
- ❌ **No workflow action to update existing reservations**

### Detailed Use Cases

#### Use Case 1: Schedule Change Request
**Scenario:** User requests to change the time of their reservation.

**Current Workflow Attempt:**
```
1. User requests schedule change
2. Workflow triggered
3. ❌ CANNOT update schedule
4. Must manually edit reservation in UI
```

**With UpdateReservation:**
```
1. User requests schedule change (e.g., from 2pm to 3pm)
2. Workflow validates new schedule
3. ✅ UpdateReservation updates schedule
4. Check for conflicts with new schedule
5. Update approval state if conflicts found
6. Notify approvers if needed
```

**Real Example:**
- "Bible Study" reserved "Room 101" for Tuesday 2pm-4pm
- Leader requests change to Tuesday 3pm-5pm
- Approval workflow runs
- Workflow updates schedule to 3pm-5pm
- Workflow checks for conflicts (none found)
- Workflow sends confirmation: "Your reservation time has been updated to Tuesday 3pm-5pm"

#### Use Case 2: Contact Information Update
**Scenario:** Update contact information for a reservation.

**Current Workflow Attempt:**
```
1. Contact information changes
2. Workflow triggered
3. ❌ CANNOT update contact info
4. Must manually edit reservation
```

**With UpdateReservation:**
```
1. Person updates their profile (new phone/email)
2. Workflow finds all their reservations
3. ✅ UpdateReservation updates contact information
4. Ensures communication can reach them
```

**Real Example:**
- "John Smith" has "Room 201" reserved
- John updates his phone number in Rock
- Workflow automatically updates `EventContactPhone` on his reservation
- Facility manager can now reach John if needed

#### Use Case 3: Bulk Schedule Updates
**Scenario:** Need to adjust all reservations by 1 hour due to daylight saving time or facility policy change.

**Current Workflow Attempt:**
```
1. Policy change requires time adjustment
2. ❌ CANNOT update multiple reservations
3. Must manually edit each reservation
```

**With UpdateReservation:**
```
1. Policy change requires all reservations to shift by 1 hour
2. Scheduled workflow runs
3. Finds all affected reservations
4. ✅ UpdateReservation updates each schedule (+1 hour)
5. Notifies all affected parties
```

**Real Example:**
- Facility changes operating hours (everything shifts 1 hour later)
- 50 reservations affected
- Scheduled workflow runs at midnight
- Workflow updates all 50 reservations (adds 1 hour to each)
- Workflow sends bulk notification: "Due to facility policy changes, your reservation time has been adjusted..."

#### Use Case 4: Notes/Details Update
**Scenario:** Add or update notes on a reservation (e.g., special requirements, setup instructions).

**Current Workflow Attempt:**
```
1. Special requirements identified
2. Workflow triggered
3. ❌ CANNOT add notes
4. Must manually edit reservation
```

**With UpdateReservation:**
```
1. Special requirements identified (e.g., "Need wheelchair access")
2. Workflow triggered
3. ✅ UpdateReservation adds note: "Wheelchair access required"
4. Facility staff can see note when preparing room
```

**Real Example:**
- "Wedding Reception" reservation created
- Couple requests: "Please set up round tables, not rectangular"
- Workflow adds note to reservation
- Setup crew sees note when preparing room

#### Use Case 5: Attendance Count Update
**Scenario:** Update the number of attendees as event approaches.

**Current Workflow Attempt:**
```
1. Attendance count changes
2. Workflow triggered
3. ❌ CANNOT update attendance
4. Must manually edit reservation
```

**With UpdateReservation:**
```
1. RSVPs come in, attendance count changes
2. Workflow triggered (e.g., from form submission)
3. ✅ UpdateReservation updates NumberAttending
4. May trigger location/resource changes if count significantly different
```

**Real Example:**
- "Youth Event" reserved for 50 people
- RSVPs come in: 75 people registered
- Form submission triggers workflow
- Workflow updates `NumberAttending` from 50 to 75
- Workflow checks if current location can accommodate 75 (if not, suggests larger location)

### Technical Details

**Schedule Updates:**
- Must validate new schedule doesn't conflict with existing reservations
- Must update `FirstOccurrenceStartDateTime` and `LastOccurrenceEndDateTime` if schedule changes
- Must handle recurring reservations correctly
- Should use `ReservationService.UpdateScheduleWithMaxEndDate()` for validation

**Conflict Detection:**
- After updating schedule, should check for conflicts
- If conflicts found, may need to set approval state to `ChangesNeeded`
- Should notify approvers if conflicts detected

**Partial Updates:**
- Workflow should only update fields that are provided
- If a field attribute is empty/null, don't update that field
- Allows flexible updates (e.g., only update name, or only update schedule)

**History Tracking:**
- Should track all changes in history
- Compare old values to new values
- Record who/what made the change (workflow name)

---

## 4. CheckReservationConflicts ⭐⭐

### What It Does
Checks if a reservation has conflicts with existing reservations for locations and/or resources. Returns:
- Whether conflicts exist (boolean)
- Detailed conflict information (HTML formatted)
- List of conflicted location IDs
- List of conflicted resource IDs

### Current State: **MISSING**
**Why It Doesn't Work Now:**
- There is **no workflow action** to check for conflicts
- Conflict checking only happens in the UI when creating/editing reservations
- Workflows cannot programmatically check for conflicts
- Cannot use conflict information as a workflow decision point

**What Exists:**
- ✅ `ReservationService.GetReservedLocationIds()` method (checks location conflicts)
- ✅ `ReservationService.GetConflictsForLocationId()` method (gets conflict details)
- ✅ `ReservationService.GetConflictsForResourceId()` method (gets resource conflicts)
- ✅ `ReservationService.GenerateConflictInfo()` method (generates HTML conflict info)
- ❌ **No workflow action to check conflicts**

### Detailed Use Cases

#### Use Case 1: Pre-Approval Validation
**Scenario:** Before approving a reservation, check if it conflicts with existing reservations.

**Current Workflow Attempt:**
```
1. Reservation submitted for approval
2. Approval workflow triggered
3. ❌ CANNOT check for conflicts
4. Approver must manually check calendar
5. May approve conflicting reservation by mistake
```

**With CheckReservationConflicts:**
```
1. Reservation submitted for approval
2. Approval workflow triggered
3. ✅ CheckReservationConflicts checks for conflicts
4. If conflicts found:
   - Set approval state to "ChangesNeeded"
   - Send conflict details to approver
   - Notify requester about conflicts
5. If no conflicts:
   - Proceed with approval
```

**Real Example:**
- "Sunday Service" reserves "Main Auditorium" for Sunday 9am
- "Baptism Service" also reserves "Main Auditorium" for Sunday 9am
- Approval workflow runs
- `CheckReservationConflicts` finds conflict
- Workflow sets approval state to "ChangesNeeded"
- Workflow sends email to approver: "Conflict detected: Main Auditorium is already reserved by Sunday Service..."
- Workflow sends email to requester: "Your reservation needs changes due to a conflict..."

#### Use Case 2: Automated Conflict Resolution
**Scenario:** Automatically resolve conflicts based on priority rules.

**Current Workflow Attempt:**
```
1. Conflict detected
2. ❌ CANNOT automatically resolve
3. Must manually resolve
```

**With CheckReservationConflicts:**
```
1. Conflict detected
2. ✅ CheckReservationConflicts identifies conflicts
3. Workflow checks priority (e.g., by reservation type, date created)
4. Lower priority reservation gets location/resource removed
5. Notify affected parties
```

**Real Example:**
- "Regular Meeting" (priority: Normal) reserves "Room 101" for Tuesday 2pm
- "Special Event" (priority: High) also reserves "Room 101" for Tuesday 2pm
- Conflict detection workflow runs
- `CheckReservationConflicts` finds conflict
- Workflow checks priorities: "Special Event" has higher priority
- Workflow removes "Room 101" from "Regular Meeting"
- Workflow sends notification: "Your reservation was modified due to a higher-priority conflict..."

#### Use Case 3: Conflict Notification Workflow
**Scenario:** Notify all parties when a conflict is detected.

**Current Workflow Attempt:**
```
1. Conflict exists
2. ❌ CANNOT programmatically notify
3. Must manually contact parties
```

**With CheckReservationConflicts:**
```
1. New reservation created
2. ✅ CheckReservationConflicts checks for conflicts
3. If conflicts found:
   - Get conflict details
   - Notify new reservation requester
   - Notify existing reservation requesters
   - Provide conflict details to all parties
```

**Real Example:**
- "Youth Group" creates reservation for "Gym" Friday 7pm
- Conflict exists with "Basketball League" (also Friday 7pm)
- `CheckReservationConflicts` detects conflict
- Workflow sends email to "Youth Group": "Your reservation conflicts with Basketball League..."
- Workflow sends email to "Basketball League": "A new reservation conflicts with yours..."

#### Use Case 4: Workflow Decision Point
**Scenario:** Use conflict check as a decision point in workflow (branch based on conflicts).

**Current Workflow Attempt:**
```
1. Workflow needs to branch based on conflicts
2. ❌ CANNOT check conflicts
3. Must use other criteria
```

**With CheckReservationConflicts:**
```
1. Workflow needs to branch
2. ✅ CheckReservationConflicts checks for conflicts
3. Sets "Has Conflicts" attribute (true/false)
4. Workflow branches:
   - If conflicts: Send to "Conflict Resolution" activity
   - If no conflicts: Send to "Approval" activity
```

**Real Example:**
- Reservation submission workflow
- `CheckReservationConflicts` runs
- Sets workflow attribute "HasConflicts" = true/false
- Workflow branches:
  - `HasConflicts = true` → "Conflict Resolution" activity → Notify requester, suggest alternatives
  - `HasConflicts = false` → "Auto Approval" activity → Approve if meets criteria

### Technical Details

**Conflict Types:**
- **Location Conflicts:** Same location reserved at overlapping times
- **Resource Conflicts:** Same resource reserved at overlapping times (for quantity-based resources, checks if total exceeds available quantity)
- **Both:** Check both location and resource conflicts

**Potential Conflicts:**
- Option to include "potential conflicts" (reservations that might conflict but aren't confirmed)
- Useful for planning and early warning

**Conflict Information:**
- Returns HTML-formatted conflict details
- Includes: conflicting reservation names, times, locations/resources
- Can be used in notifications or displayed to users

**Output Attributes:**
- `Has Conflicts` (Boolean): True if any conflicts exist
- `Conflict Details` (Text): HTML formatted conflict information
- `Conflicted Location IDs` (Text): Comma-separated list of location IDs with conflicts
- `Conflicted Resource IDs` (Text): Comma-separated list of resource IDs with conflicts

**Performance:**
- Should be efficient even with many reservations
- Uses existing `ReservationService` methods that are optimized
- Can be used frequently without performance issues

---

## 5. CheckResourceAvailability ⭐⭐

### What It Does
Checks if a specific resource is available for a reservation. Returns:
- Whether the resource is available (boolean)
- Available quantity (for quantity-based resources)
- Booked quantity (for quantity-based resources)

### Current State: **MISSING**
**Why It Doesn't Work Now:**
- There is **no workflow action** to check resource availability
- Availability checking only happens in the UI
- Workflows cannot programmatically check availability
- Cannot use availability as a workflow decision point

**What Exists:**
- ✅ `ReservationService.GetAvailableResourceQuantity()` method (checks availability)
- ✅ `ReservationService.GetBookedResourceQuantity()` method (gets booked quantity)
- ❌ **No workflow action to check availability**

### Detailed Use Cases

#### Use Case 1: Pre-Add Validation
**Scenario:** Before adding a resource to a reservation, check if it's available.

**Current Workflow Attempt:**
```
1. Workflow wants to add resource
2. ❌ CANNOT check availability first
3. Adds resource anyway
4. Resource might be overbooked
```

**With CheckResourceAvailability:**
```
1. Workflow wants to add resource
2. ✅ CheckResourceAvailability checks availability
3. If available:
   - Add resource
   - Proceed
4. If not available:
   - Don't add resource
   - Notify requester
   - Suggest alternatives
```

**Real Example:**
- Workflow wants to add "Projector" to reservation
- `CheckResourceAvailability` checks: 0 projectors available (all booked)
- Workflow doesn't add projector
- Workflow sends notification: "Projector unavailable. Alternative: Projector #2 available..."

#### Use Case 2: Availability-Based Workflow Branching
**Scenario:** Branch workflow based on resource availability.

**Current Workflow Attempt:**
```
1. Workflow needs to branch based on availability
2. ❌ CANNOT check availability
3. Must use other criteria
```

**With CheckResourceAvailability:**
```
1. Workflow needs to branch
2. ✅ CheckResourceAvailability checks availability
3. Sets "Is Available" attribute
4. Workflow branches:
   - If available: Add resource, proceed
   - If not available: Notify, suggest alternatives
```

**Real Example:**
- Reservation requires "Sound System"
- `CheckResourceAvailability` checks availability
- Sets workflow attribute "IsAvailable" = false
- Workflow branches to "Resource Unavailable" activity
- Sends notification with alternative options

#### Use Case 3: Quantity Validation
**Scenario:** Check if enough quantity of a resource is available.

**Current Workflow Attempt:**
```
1. Need 5 tables
2. ❌ CANNOT check if 5 available
3. Adds resource anyway
4. Might overbook
```

**With CheckResourceAvailability:**
```
1. Need 5 tables
2. ✅ CheckResourceAvailability checks: 3 available
3. Workflow doesn't add resource (insufficient quantity)
4. Notifies requester: "Only 3 tables available. Need 5."
```

**Real Example:**
- "Wedding Reception" needs 10 tables
- `CheckResourceAvailability` checks: 7 tables available
- Workflow sets "IsAvailable" = false (insufficient quantity)
- Workflow sends notification: "Only 7 tables available. You requested 10. Would you like to proceed with 7 or choose alternative date?"

### Technical Details

**Quantity-Based Resources:**
- Some resources have a `Quantity` property (e.g., "Tables" quantity = 20)
- Checks if requested quantity is available
- Returns available quantity and booked quantity

**Non-Quantity Resources:**
- Some resources don't have quantity (e.g., "Projector" - either available or not)
- Returns boolean availability

**Time-Based Checking:**
- Checks availability for the reservation's schedule
- Considers all occurrences if recurring reservation
- Checks against all existing reservations

**Output Attributes:**
- `Is Available` (Boolean): True if resource is available in requested quantity
- `Available Quantity` (Integer): Quantity available (null if not quantity-based)
- `Booked Quantity` (Integer): Quantity already booked (null if not quantity-based)

---

## Summary: Why These Workflows Are Critical

### The Core Problem

**Current State:** Workflows can CREATE and MODIFY APPROVAL STATES, but cannot:
- ❌ Remove locations/resources
- ❌ Update reservation properties
- ❌ Check for conflicts
- ❌ Check availability

**Impact:** This severely limits workflow automation. Many common scenarios require manual intervention because workflows cannot perform these basic operations.

### Real-World Impact

**Without These Workflows:**
- Conflict resolution requires manual UI interaction
- Resource unavailability requires manual editing of each reservation
- Schedule changes require manual editing
- Availability checking requires manual calendar review

**With These Workflows:**
- Fully automated conflict resolution
- Automated resource management
- Automated schedule updates
- Automated availability checking and notifications

### Priority Justification

**High Priority (⭐⭐⭐):**
1. **RemoveReservationLocation** - Essential for conflict resolution and cancellations
2. **RemoveReservationResource** - Essential for resource management
3. **UpdateReservation** - Essential for schedule/contact updates

**Medium Priority (⭐⭐):**
4. **CheckReservationConflicts** - Important for validation workflows
5. **CheckResourceAvailability** - Important for resource management workflows

These workflows would transform the plugin from "workflow-capable" to "workflow-powered" for reservation management.
