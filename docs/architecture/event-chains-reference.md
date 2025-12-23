# Event Chains Reference Guide

## Family Hub - Automated Workflow Specifications

**Document Version:** 1.0
**Date:** 2025-12-19
**Purpose:** Quick reference for all implemented and planned event chains

---

## What are Event Chains?

Event chains are automated workflows that span multiple bounded contexts (services). When a user performs an action in one domain, related actions are automatically triggered in other domains without manual intervention.

**Key Benefits:**

- **Time Savings:** Reduces repetitive data entry
- **Mental Load Reduction:** Fewer things to remember
- **Error Prevention:** Ensures related actions are not forgotten
- **Consistency:** Standardized workflows across family

---

## Event Chain Catalog

### 1. Doctor Appointment Event Chain

**Trigger:** User schedules a doctor appointment in Health Service

**Event Flow:**

```
1. USER ACTION: Create health appointment
   HealthService.scheduleAppointment(...)
   ↓
2. DOMAIN EVENT PUBLISHED
   HealthAppointmentScheduledEvent {
     AppointmentId: Guid
     PatientUserId: Guid
     DoctorName: string
     AppointmentTime: DateTime
     Location: string
     Reason: string
   }
   ↓
3. CALENDAR SERVICE CONSUMES EVENT
   Creates CalendarEvent (type: MEDICAL)
   - Title: "Doctor: {DoctorName}"
   - StartTime: AppointmentTime
   - Location: Appointment.Location
   - Attendees: [PatientUserId]
   ↓
4. CALENDAR EVENT CREATED EVENT PUBLISHED
   CalendarEventCreatedEvent {...}
   ↓
5. TASK SERVICE CONSUMES APPOINTMENT EVENT
   Creates Task: "Prepare questions for Dr. {DoctorName}"
   - AssignedTo: PatientUserId
   - DueDate: 24 hours before appointment
   - Category: MEDICAL
   - Priority: MEDIUM
   ↓
6. COMMUNICATION SERVICE CONSUMES APPOINTMENT EVENT
   Schedules notifications:
   - 24 hours before: "Doctor appointment tomorrow"
   - 2 hours before: "Doctor appointment in 2 hours"
   - After appointment: "Record any prescriptions?"
   ↓
7. [OPTIONAL] USER RECORDS PRESCRIPTION POST-APPOINTMENT
   Triggers Prescription Event Chain (see #2)
```

**Expected Outcomes:**

- 1 calendar event created
- 1 preparation task created
- 3 notifications scheduled
- Optional: Prescription chain triggered

**Time Saved:** ~10 minutes per appointment
**Mental Load Reduction:** 4 fewer things to remember

**Implementation Phase:** Phase 2 (Week 13-18)

---

### 2. Prescription Event Chain

**Trigger:** User records a prescription in Health Service

**Event Flow:**

```
1. USER ACTION: Create prescription
   HealthService.createPrescription(...)
   ↓
2. DOMAIN EVENT PUBLISHED
   PrescriptionIssuedEvent {
     PrescriptionId: Guid
     PatientUserId: Guid
     MedicationName: string
     Dosage: string
     Frequency: string
     RefillsRemaining: int
     IssuedDate: DateTime
   }
   ↓
3. SHOPPING SERVICE CONSUMES EVENT
   Creates/updates shopping list: "Pharmacy Needs"
   Adds ShoppingItem:
   - Name: "{MedicationName} ({Dosage})"
   - Category: "Pharmacy"
   - Quantity: 1
   - Notes: "Prescription from {IssuedDate}"
   ↓
4. SHOPPING ITEM ADDED EVENT PUBLISHED
   ShoppingItemAddedEvent {...}
   ↓
5. TASK SERVICE CONSUMES PRESCRIPTION EVENT
   Creates Task: "Pick up prescription: {MedicationName}"
   - AssignedTo: PatientUserId
   - DueDate: IssuedDate + 1 day (urgent)
   - Category: MEDICAL
   - Priority: HIGH
   - Metadata: Links to prescription and shopping list
   ↓
6. COMMUNICATION SERVICE CONSUMES PRESCRIPTION EVENT
   Schedules refill reminder:
   - Calculates: (RefillsRemaining * 30 days) - 7 days
   - Notification: "Time to refill {MedicationName}"
   ↓
7. [OPTIONAL] USER COMPLETES TASK
   TaskCompletedEvent triggers shopping list item check-off
```

**Expected Outcomes:**

- 1 shopping list item added
- 1 pickup task created
- 1 refill reminder scheduled

**Time Saved:** ~5 minutes per prescription
**Mental Load Reduction:** 3 fewer things to remember

**Implementation Phase:** Phase 2 (Week 13-18)

---

### 3. Meal Planning Event Chain

**Trigger:** User creates a meal plan for the week

**Event Flow:**

```
1. USER ACTION: Create meal plan
   MealPlanningService.createMealPlan(...)
   Includes: List of recipes for each day
   ↓
2. DOMAIN EVENT PUBLISHED
   MealPlannedEvent {
     MealPlanId: Guid
     FamilyGroupId: Guid
     StartDate: Date
     EndDate: Date
     Recipes: List<RecipeWithServings>
     Ingredients: List<Ingredient> (aggregated)
   }
   ↓
3. SHOPPING SERVICE CONSUMES EVENT
   Creates ShoppingList: "Groceries for {Week of Date}"
   Groups ingredients by category:
   - Produce
   - Dairy
   - Meat
   - Pantry
   - Other
   Checks pantry inventory (future feature)
   Removes duplicate items
   ↓
4. SHOPPING LIST CREATED EVENT PUBLISHED
   ShoppingListCreatedEvent {...}
   ↓
5. TASK SERVICE CONSUMES SHOPPING LIST EVENT
   Creates Task: "Buy groceries for week of {Date}"
   - AssignedTo: Meal plan creator (or rotates)
   - DueDate: Day before meal plan starts
   - Category: SHOPPING
   - Priority: MEDIUM
   - Metadata: Links to meal plan and shopping list
   ↓
6. COMMUNICATION SERVICE CONSUMES MEAL PLAN EVENT
   Sends notification:
   - "Meal plan created for next week"
   - "Shopping list ready with {X} items"
   ↓
7. USER COMPLETES SHOPPING
   Marks shopping list as complete
   ↓
8. SHOPPING LIST COMPLETED EVENT PUBLISHED
   ShoppingListCompletedEvent {...}
   ↓
9. FINANCE SERVICE CONSUMES COMPLETION EVENT
   Prompts expense recording:
   - Notification: "Record grocery expense?"
   - Pre-fills category: GROCERIES
   - Links to shopping list for reference
   ↓
10. [OPTIONAL] USER RECORDS EXPENSE
    Triggers Finance Event Chain (see #4)
```

**Expected Outcomes:**

- 1 shopping list created (20-40 items)
- 1 grocery shopping task created
- 1 expense prompt notification

**Time Saved:** ~30 minutes per week (meal planning + list creation)
**Mental Load Reduction:** 5+ fewer things to remember

**Implementation Phase:** Phase 3 (Week 19-26)

---

### 4. Budget Threshold Event Chain

**Trigger:** Expense recorded that causes budget category to exceed threshold

**Event Flow:**

```
1. USER ACTION: Record expense
   FinanceService.recordExpense(...)
   ↓
2. FINANCE SERVICE LOGIC
   Updates budget category spent amount
   Checks if threshold exceeded:
   - Warning: 80% of budget
   - Critical: 100% of budget
   ↓
3. DOMAIN EVENT PUBLISHED (if threshold exceeded)
   BudgetThresholdExceededEvent {
     BudgetId: Guid
     CategoryId: Guid
     CategoryName: string
     AllocatedAmount: decimal
     SpentAmount: decimal
     PercentageUsed: decimal
     ThresholdType: Warning | Critical
   }
   ↓
4. COMMUNICATION SERVICE CONSUMES EVENT
   Sends notification to family group:
   - Warning: "{CategoryName} budget at {Percentage}%"
   - Critical: "{CategoryName} budget exceeded! ({SpentAmount}/{AllocatedAmount})"
   - Priority: Warning = NORMAL, Critical = HIGH
   ↓
5. FINANCE SERVICE PUBLISHES EXPENSE RECORDED EVENT
   ExpenseRecordedEvent {...}
   ↓
6. [OPTIONAL] ANALYTICS/REPORTING
   Tracks spending patterns
   Generates insights (future feature)
```

**Expected Outcomes:**

- Budget update
- Alert notification if threshold exceeded
- Spending visibility for family

**Time Saved:** ~2 minutes per expense (no manual budget checking)
**Mental Load Reduction:** Budget monitoring automated

**Implementation Phase:** Phase 3 (Week 19-26)

---

### 5. Recurring Task Event Chain

**Trigger:** System generates recurring task instance

**Event Flow:**

```
1. SYSTEM ACTION: Recurring task due
   TaskService.generateRecurringInstance(...)
   Based on recurrence pattern:
   - Daily, Weekly, Monthly
   ↓
2. DOMAIN EVENT PUBLISHED
   TaskCreatedEvent {
     TaskId: Guid (new instance)
     ParentTaskId: Guid (recurring template)
     Title: string
     AssignedTo: Guid
     DueDate: DateTime
     Recurrence: RecurrencePattern
   }
   ↓
3. CALENDAR SERVICE CONSUMES EVENT
   Creates CalendarEvent (if task has deadline)
   - Title: "Task due: {TaskTitle}"
   - StartTime: DueDate (all-day or specific time)
   - Attendees: [AssignedTo]
   ↓
4. COMMUNICATION SERVICE CONSUMES EVENT
   Schedules reminders:
   - 24 hours before: "Task due tomorrow"
   - 2 hours before (if time-specific)
   - On due date: "Task due today"
   ↓
5. USER COMPLETES TASK
   TaskCompletedEvent published
   ↓
6. TASK SERVICE LOGIC
   Generates next instance based on recurrence:
   - Loop back to step 1 for next occurrence
```

**Expected Outcomes:**

- New task instance created
- Calendar event for deadline
- Reminder notifications scheduled
- Next instance generated upon completion

**Common Recurring Tasks:**

- Weekly trash day
- Monthly bill payments
- Bi-weekly cleaning chores
- Daily medication reminders

**Time Saved:** ~5 minutes per recurring task setup (set once, runs forever)
**Mental Load Reduction:** Never forget recurring responsibilities

**Implementation Phase:** Phase 4 (Week 27-34)

---

### 6. Calendar Event Reminder Chain

**Trigger:** Calendar event approaching

**Event Flow:**

```
1. SYSTEM ACTION: Event reminder due
   CalendarService.checkUpcomingEvents(...)
   Based on event start time and user preferences
   ↓
2. DOMAIN EVENT PUBLISHED
   CalendarEventReminderDueEvent {
     EventId: Guid
     UserId: Guid
     EventTitle: string
     EventStartTime: DateTime
     ReminderTime: DateTime
     EventType: EventType
   }
   ↓
3. COMMUNICATION SERVICE CONSUMES EVENT
   Sends notification:
   - Title: "{EventTitle}"
   - Message: "Starting in {TimeUntil}"
   - Priority: Based on event type
   - Actions: "View details" | "Snooze"
   ↓
4. [OPTIONAL] EVENT-SPECIFIC ACTIONS
   If event type is MEDICAL:
   - Link to preparation task
   - Show doctor information
   If event type is TRAVEL:
   - Link to packing task
   - Show weather at destination
```

**Expected Outcomes:**

- Timely reminder notification
- Context-specific information
- Quick action buttons

**Reminder Schedule (configurable):**

- 1 week before (for important events)
- 1 day before
- 2 hours before
- 15 minutes before

**Implementation Phase:** Phase 1 (Week 5-12) - Basic reminders
**Enhancement:** Phase 4 (Week 27-34) - Context-specific actions

---

### 7. Task Assignment Chain

**Trigger:** User assigns task to family member

**Event Flow:**

```
1. USER ACTION: Assign task
   TaskService.assignTask(taskId, userId)
   ↓
2. DOMAIN EVENT PUBLISHED
   TaskAssignedEvent {
     TaskId: Guid
     TaskTitle: string
     AssignedTo: Guid
     AssignedBy: Guid
     DueDate: DateTime
     Priority: TaskPriority
   }
   ↓
3. COMMUNICATION SERVICE CONSUMES EVENT
   Sends notification to assignee:
   - "{AssignedBy} assigned you: {TaskTitle}"
   - "Due: {DueDate}"
   - Priority: Based on task priority
   - Actions: "Accept" | "Decline" | "View"
   ↓
4. [OPTIONAL] ASSIGNEE ACCEPTS/DECLINES
   TaskAcceptedEvent or TaskDeclinedEvent
   ↓
5. IF DECLINED
   Communication Service notifies assigner
   Task goes back to unassigned state
```

**Expected Outcomes:**

- Assignment notification
- Accept/decline workflow
- Visibility for assigner

**Use Cases:**

- Parent assigns chores to children
- Spouse delegates errands
- Rotating responsibilities

**Implementation Phase:** Phase 1 (Week 5-12)

---

### 8. Shopping List Completion Chain

**Trigger:** User marks shopping list as complete

**Event Flow:**

```
1. USER ACTION: Complete shopping list
   ShoppingService.completeList(listId)
   ↓
2. DOMAIN EVENT PUBLISHED
   ShoppingListCompletedEvent {
     ListId: Guid
     FamilyGroupId: Guid
     CompletedBy: Guid
     CompletedAt: DateTime
     ItemCount: int
     TotalEstimatedCost: decimal (if items had prices)
   }
   ↓
3. TASK SERVICE CONSUMES EVENT
   Marks related task as complete:
   - Finds task linked to shopping list
   - Updates status to COMPLETED
   - Records completed by user
   ↓
4. FINANCE SERVICE CONSUMES EVENT
   Prompts expense recording:
   - Notification: "Record shopping expense?"
   - Pre-fills amount: TotalEstimatedCost
   - Category: GROCERIES (or inferred from list)
   - Links to shopping list for details
   ↓
5. MEAL PLANNING SERVICE CONSUMES EVENT (if list from meal plan)
   Updates meal plan status:
   - Marks "shopping complete"
   - Sends reminder: "Ready to cook this week's meals!"
```

**Expected Outcomes:**

- Related task marked complete
- Expense recording prompt
- Meal plan status update (if applicable)

**Time Saved:** ~3 minutes (no manual task update or expense entry)

**Implementation Phase:** Phase 3 (Week 19-26)

---

### 9. Task Overdue Chain

**Trigger:** Task passes due date without completion

**Event Flow:**

```
1. SYSTEM ACTION: Task overdue check
   TaskService.checkOverdueTasks(...)
   Runs every hour or on schedule
   ↓
2. DOMAIN EVENT PUBLISHED
   TaskOverdueEvent {
     TaskId: Guid
     TaskTitle: string
     AssignedTo: Guid
     DueDate: DateTime
     HoursOverdue: int
     Priority: TaskPriority
   }
   ↓
3. COMMUNICATION SERVICE CONSUMES EVENT
   Sends escalating notifications:
   - First: "Task overdue: {TaskTitle}"
   - 24h later: "Task still overdue (2 days)"
   - 48h later: Notify task creator + assignee
   - Priority increases with time
   ↓
4. [OPTIONAL] FAMILY GROUP NOTIFICATION
   If task is high priority or critical:
   - Notify entire family group
   - Allows reassignment or renegotiation
```

**Expected Outcomes:**

- Overdue notification to assignee
- Escalation over time
- Family visibility for critical tasks

**Implementation Phase:** Phase 2 (Week 13-18)

---

### 10. Health Appointment Cancellation Chain

**Trigger:** User cancels doctor appointment

**Event Flow:**

```
1. USER ACTION: Cancel appointment
   HealthService.cancelAppointment(appointmentId, reason)
   ↓
2. DOMAIN EVENT PUBLISHED
   HealthAppointmentCancelledEvent {
     AppointmentId: Guid
     CancelledBy: Guid
     CancellationReason: string
     CancelledAt: DateTime
   }
   ↓
3. CALENDAR SERVICE CONSUMES EVENT
   Cancels related calendar event:
   - Marks event as CANCELLED
   - Optionally removes from calendar
   ↓
4. TASK SERVICE CONSUMES EVENT
   Cancels related tasks:
   - Preparation task
   - Follow-up tasks
   - Updates status to CANCELLED
   ↓
5. COMMUNICATION SERVICE CONSUMES EVENT
   Sends notifications:
   - To patient: "Appointment cancelled"
   - To family: "{Patient} cancelled appointment with {Doctor}"
   - Reminder: "Remember to reschedule if needed"
```

**Expected Outcomes:**

- Calendar event cancelled
- Related tasks cancelled
- Family notified

**Implementation Phase:** Phase 2 (Week 13-18)

---

## Event Chain Implementation Patterns

### Pattern 1: Direct Event Consumption

```csharp
// Service subscribes to event and takes action
public class CalendarEventHandler : IEventHandler<HealthAppointmentScheduledEvent>
{
    public async Task HandleAsync(HealthAppointmentScheduledEvent evt)
    {
        var calendarEvent = new CalendarEvent
        {
            Title = $"Doctor: {evt.DoctorName}",
            StartTime = evt.AppointmentTime,
            Type = EventType.Medical
        };

        await _calendarRepository.SaveAsync(calendarEvent);
        await _eventPublisher.PublishAsync(new CalendarEventCreatedEvent(...));
    }
}
```

### Pattern 2: Saga Pattern (for compensating transactions)

```csharp
// Multi-step workflow with rollback capability
public class ShoppingListGenerationSaga
{
    public async Task<Result> ExecuteAsync(MealPlannedEvent evt)
    {
        // Step 1: Create shopping list
        var list = await _shoppingService.CreateListAsync(...);

        // Step 2: Add ingredients
        var result = await _shoppingService.AddItemsAsync(list.Id, evt.Ingredients);

        if (!result.Success)
        {
            // Compensating action: Delete shopping list
            await _shoppingService.DeleteListAsync(list.Id);
            return Result.Failure("Failed to add items");
        }

        // Step 3: Publish event
        await _eventPublisher.PublishAsync(new ShoppingListCreatedEvent(...));
        return Result.Success();
    }
}
```

### Pattern 3: Event Enrichment

```csharp
// Service enriches event with additional context before publishing
public class PrescriptionEventEnricher
{
    public async Task<PrescriptionIssuedEvent> EnrichAsync(Prescription prescription)
    {
        var patient = await _userService.GetUserAsync(prescription.PatientUserId);
        var pharmacy = await _pharmacyService.GetPreferredPharmacyAsync(prescription.PatientUserId);

        return new PrescriptionIssuedEvent
        {
            PrescriptionId = prescription.Id,
            MedicationName = prescription.MedicationName,
            PatientName = patient.FullName, // Enriched
            PreferredPharmacy = pharmacy.Name, // Enriched
            ...
        };
    }
}
```

---

## Event Chain Monitoring

### Key Metrics

**Per Event Chain:**

- **Success Rate:** % of chains that complete without errors
- **Latency:** Time from trigger to final action
- **Step Completion:** % of steps that execute successfully
- **Error Rate:** % of chains that fail

**Target Metrics:**

- Success Rate: >98%
- Average Latency: <5 seconds
- p95 Latency: <10 seconds
- Error Rate: <2%

### Monitoring Dashboard

```
Event Chain Health Dashboard
┌────────────────────────────────────────────────────────┐
│ Chain Name                 | Success | Avg Latency    │
├────────────────────────────────────────────────────────┤
│ Doctor Appointment         | 99.2%   | 3.2s           │
│ Prescription               | 98.8%   | 2.1s           │
│ Meal Planning              | 97.5%   | 4.8s           │
│ Budget Threshold           | 99.9%   | 1.2s           │
│ Recurring Task             | 98.1%   | 2.8s           │
└────────────────────────────────────────────────────────┘

Recent Failures (Last 24h)
- Meal Planning Chain: 2 failures (database timeout)
- Recurring Task Chain: 1 failure (event bus connection)
```

### Alerting Rules

**Critical:**

- Event chain success rate <95% for 1 hour
- Event chain latency >30 seconds (p95)
- Event bus connection failures

**Warning:**

- Event chain success rate <98% for 6 hours
- Event chain latency >10 seconds (p95)
- Individual step failures >5% for 1 hour

---

## Testing Event Chains

### Unit Testing

```csharp
[Fact]
public async Task HealthAppointmentScheduled_ShouldCreateCalendarEvent()
{
    // Arrange
    var evt = new HealthAppointmentScheduledEvent(...);
    var handler = new CalendarEventHandler(...);

    // Act
    await handler.HandleAsync(evt);

    // Assert
    var calendarEvent = await _calendarRepository.GetByIdAsync(...);
    Assert.NotNull(calendarEvent);
    Assert.Equal(evt.DoctorName, calendarEvent.Title);
}
```

### Integration Testing

```csharp
[Fact]
public async Task DoctorAppointmentChain_EndToEnd_ShouldComplete()
{
    // Arrange
    var appointment = new CreateAppointmentCommand(...);

    // Act
    await _healthService.ScheduleAppointmentAsync(appointment);
    await Task.Delay(5000); // Wait for event chain

    // Assert
    var calendarEvent = await _calendarService.GetEventsAsync(...);
    Assert.Single(calendarEvent);

    var task = await _taskService.GetTasksAsync(...);
    Assert.Single(task);

    var notifications = await _notificationService.GetNotificationsAsync(...);
    Assert.Equal(3, notifications.Count);
}
```

### Load Testing

```csharp
// Simulate 100 concurrent event chains
for (int i = 0; i < 100; i++)
{
    await _healthService.ScheduleAppointmentAsync(...);
}

// Monitor:
// - Event bus throughput
// - Database query performance
// - API response times
// - Event chain completion times
```

---

## Troubleshooting Event Chains

### Common Issues

**1. Event Not Published**

- Check event publisher configuration
- Verify Redis connection
- Check event serialization

**2. Event Not Consumed**

- Verify subscriber is registered
- Check Redis channel subscription
- Review error logs for handler exceptions

**3. Event Chain Incomplete**

- Check individual service health
- Review event handler logs
- Verify database transactions committed

**4. Event Chain Slow**

- Profile database queries
- Check network latency between services
- Review event bus performance
- Optimize event payload size

### Debugging Tools

**Event Trace Viewer:**

```
Event Chain Trace: Doctor Appointment (ID: abc123)
┌────────────────────────────────────────────────────────┐
│ 1. HealthAppointmentScheduledEvent published [0ms]     │
│ 2. Calendar Service received event [120ms]             │
│ 3. CalendarEventCreatedEvent published [450ms]         │
│ 4. Task Service received appointment event [180ms]     │
│ 5. TaskCreatedEvent published [680ms]                  │
│ 6. Communication Service received events [250ms]       │
│ 7. 3 notifications scheduled [1120ms]                  │
│                                                         │
│ Total Chain Duration: 2.8 seconds                      │
│ Status: SUCCESS                                         │
└────────────────────────────────────────────────────────┘
```

---

## Future Event Chains (Backlog)

### 11. Travel Planning Chain

- User creates travel calendar event
- → Task: Book flights, hotels
- → Shopping list: Travel essentials
- → Finance: Track travel budget

### 12. Birthday Event Chain

- User adds birthday to calendar
- → Task: Buy gift (2 weeks before)
- → Task: Plan party (1 month before)
- → Shopping list: Party supplies
- → Finance: Track party budget

### 13. School Event Chain

- User adds school event (exam, project due)
- → Calendar: Study schedule
- → Task: Study reminders
- → Task: Parent signature reminders

### 14. Home Maintenance Chain

- User schedules maintenance task (HVAC, plumbing)
- → Calendar: Appointment
- → Finance: Budget for service
- → Task: Prepare home for service
- → Task: Follow-up after service

### 15. Subscription Renewal Chain

- User records subscription (Netflix, Spotify)
- → Finance: Track monthly expense
- → Task: Review before renewal
- → Notification: Renewal reminder

---

## Summary

Event chains are the core value proposition of Family Hub. They demonstrate the power of domain-driven design and event-driven architecture to solve real-world problems. By automating cross-domain coordination, Family Hub saves time, reduces mental load, and helps families stay organized.

**Key Takeaways:**

- Event chains save 5-30 minutes per workflow
- Reduce mental load by 3-5 items per workflow
- Implementation spans multiple phases (Phase 1-4)
- Monitoring and testing are critical for reliability
- Future chains can be added based on user needs

---

**Document Status:** Reference guide for developers and stakeholders
**Next Review:** After Phase 2 implementation (Week 18)
