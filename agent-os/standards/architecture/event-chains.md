# Event Chain Automation

Family Hub's flagship differentiator. Automated cross-domain workflows that save 10-30 minutes per action.

## Doctor Appointment Chain Example

```
1. User schedules doctor appointment (Health)
   └→ DoctorAppointmentScheduledEvent
        ├→ Calendar: Creates calendar event
        ├→ Task: Creates "Prepare questions" task
        └→ Communication: Schedules 24h reminder

2. Doctor issues prescription (Health)
   └→ PrescriptionIssuedEvent
        ├→ Shopping: Adds medication to list
        ├→ Task: Creates "Pick up prescription" task
        └→ Health: Schedules refill reminder
```

## Event Handler Pattern

```csharp
public class CreateCalendarEventHandler
    : INotificationHandler<DoctorAppointmentScheduledEvent>
{
    private readonly ICalendarService _calendar;

    public async Task Handle(
        DoctorAppointmentScheduledEvent notification,
        CancellationToken cancellationToken)
    {
        await _calendar.CreateEventAsync(new CalendarEvent
        {
            Title = $"Doctor: {notification.DoctorName}",
            StartTime = notification.AppointmentTime,
            FamilyId = notification.FamilyId
        });
    }
}
```

## 10 Documented Event Chains

1. Doctor Appointment → Calendar + Task + Reminder
2. School Event → Calendar + Task + Shopping
3. Meal Plan → Shopping List + Calendar
4. Prescription → Shopping + Task + Refill Reminder
5. Birthday → Calendar + Task + Shopping
6. Bill Due → Finance + Task + Reminder
7. Vacation → Calendar + Task + Shopping
8. Grocery Low → Shopping + Meal Adjustment
9. Family Meeting → Calendar + Task + Notification
10. Health Checkup → Calendar + Health Record

## Rules

- Events flow through RabbitMQ
- Handlers are idempotent
- Failed handlers retry with exponential backoff
- Dead letter queue for permanent failures
