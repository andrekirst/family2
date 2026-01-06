# Information Architecture: Family Hub

**Version:** 1.0
**Date:** 2025-12-19
**Status:** Final
**Owner:** UX Research Team

---

## Executive Summary

This document defines the complete information architecture (IA) for Family Hub, including site structure, navigation patterns, content organization, and multi-role experience. The IA is designed to support a privacy-first, mobile-optimized family organization platform with event chain automation as the primary differentiator.

### IA Philosophy

1. **Mobile-First**: Bottom navigation on mobile (5 tabs max), sidebar on desktop
2. **Role-Aware**: Different navigation and content visibility for Parent, Teen, Child, Extended Family, Guest
3. **Privacy-First**: Clear privacy controls, transparent data management
4. **Automation-Forward**: Event chains prominently featured (not buried in menus)
5. **Discoverable**: Flat hierarchy (max 3 levels deep), clear labels, search-friendly

### Primary Navigation (6 Top-Level Sections)

1. **Dashboard** (Home) - Today's overview, quick actions
2. **Calendar** - Events, appointments, schedules
3. **Lists** - Shopping, to-do, packing, meal plans
4. **Tasks & Chores** - Assignments, gamification, completion tracking
5. **Event Chains** ⭐ - Automation templates, chain builder, history
6. **Family & Settings** - Members, notifications, privacy, account

---

## Complete Site Map

```
Family Hub
│
├── 📊 Dashboard (Home)
│   ├── Today's Overview
│   │   ├── Today's Events (from Calendar)
│   │   ├── Today's Tasks (assigned to me)
│   │   ├── Shopping Lists (active)
│   │   └── Upcoming This Week
│   ├── Quick Actions
│   │   ├── Add Event
│   │   ├── Add Task
│   │   ├── Add List Item
│   │   └── Create Event Chain
│   ├── Active Event Chains
│   │   ├── Chain Status (running, paused, completed)
│   │   └── Recent Chain Activity
│   ├── Family Activity Feed
│   │   ├── Recent Updates (event added, task completed, list modified)
│   │   └── Notifications
│   └── Widgets (Customizable)
│       ├── Calendar Widget
│       ├── Task Widget
│       ├── Shopping List Widget
│       ├── Meal Plan Widget
│       └── Budget Summary Widget (Phase 2+)
│
├── 📅 Calendar
│   ├── Views
│   │   ├── Month View (default)
│   │   ├── Week View
│   │   ├── Day View
│   │   ├── Agenda View (list format)
│   │   └── Year View (planning)
│   ├── Event Management
│   │   ├── Create Event
│   │   │   ├── Title, Date/Time, Location
│   │   │   ├── Assign to Family Members
│   │   │   ├── Recurrence (daily, weekly, monthly, custom)
│   │   │   ├── Reminders (15 min, 1 hour, 1 day before)
│   │   │   ├── Event Chain Trigger (optional)
│   │   │   └── Attachments (documents, photos)
│   │   ├── Edit Event
│   │   ├── Delete Event
│   │   ├── Duplicate Event
│   │   └── Share Event (via link, email)
│   ├── Event Details
│   │   ├── Description & Notes
│   │   ├── Attendees (family members)
│   │   ├── Location (with map integration)
│   │   ├── Related Tasks (auto-created by event chains)
│   │   ├── Related Shopping Items
│   │   └── Event History (created, modified, completed)
│   ├── Recurring Events
│   │   ├── View All Recurrences
│   │   ├── Edit Single Instance
│   │   ├── Edit All Future Instances
│   │   └── Delete Recurrence Pattern
│   ├── Filters
│   │   ├── Show All Family Events
│   │   ├── Show My Events Only
│   │   ├── Filter by Person
│   │   ├── Filter by Event Type (work, school, personal, family)
│   │   └── Filter by Calendar Source (Family Hub, Google, Apple)
│   └── Integrations
│       ├── Sync with Google Calendar (two-way)
│       ├── Sync with Apple Calendar (two-way)
│       ├── Sync with Outlook Calendar (two-way)
│       └── Export to iCal (.ics file)
│
├── 📝 Lists
│   ├── Shopping Lists
│   │   ├── Groceries (default)
│   │   ├── Household Items
│   │   ├── Pharmacy
│   │   ├── Custom Lists
│   │   ├── List Management
│   │   │   ├── Create List
│   │   │   ├── Add Item (with quantity, notes)
│   │   │   ├── Check Off Item (swipe gesture)
│   │   │   ├── Categorize Items (Produce, Dairy, Meat, Pantry)
│   │   │   ├── Assign to Person (who's shopping)
│   │   │   ├── Share List (via link, email)
│   │   │   └── Duplicate List
│   │   └── Smart Features
│   │       ├── Auto-Add from Meal Plan (event chain)
│   │       ├── Frequent Items (quick add from history)
│   │       ├── Barcode Scanner (add items by scanning)
│   │       └── Store Aisle Mapping (Phase 2+)
│   ├── To-Do Lists
│   │   ├── Personal To-Dos
│   │   ├── Family To-Dos
│   │   ├── Project Lists
│   │   └── Archive Completed Lists
│   ├── Meal Plans
│   │   ├── This Week (default view)
│   │   ├── Next Week
│   │   ├── Meal Plan Templates (Phase 2+)
│   │   ├── Recipe Library
│   │   │   ├── Favorite Recipes
│   │   │   ├── Quick Meals (<30 min)
│   │   │   ├── Healthy Meals
│   │   │   ├── Kid-Friendly Meals
│   │   │   └── Dietary Filters (vegetarian, gluten-free, etc.)
│   │   └── Meal Plan Management
│   │       ├── Assign Meal to Day (drag-drop)
│   │       ├── Add Recipe to Meal Plan
│   │       ├── View Ingredients (all ingredients for the week)
│   │       ├── Auto-Generate Shopping List (event chain)
│   │       └── Share Meal Plan (via link)
│   └── Packing Lists (Phase 2+)
│       ├── Vacation Packing
│       ├── School Packing
│       └── Templates
│
├── ✅ Tasks & Chores
│   ├── Task Views
│   │   ├── All Tasks (family-wide)
│   │   ├── My Tasks (assigned to me)
│   │   ├── Assigned by Me (parent view)
│   │   ├── Completed Tasks (archive)
│   │   └── Recurring Chores
│   ├── Task Management
│   │   ├── Create Task
│   │   │   ├── Title, Description, Due Date/Time
│   │   │   ├── Assign to Family Member(s)
│   │   │   ├── Priority (Low, Medium, High, Urgent)
│   │   │   ├── Recurrence (daily, weekly, monthly)
│   │   │   ├── Gamification (points value, badge unlock)
│   │   │   ├── Event Chain Trigger (optional)
│   │   │   └── Attachments (photos, documents)
│   │   ├── Edit Task
│   │   ├── Mark Complete (swipe gesture)
│   │   ├── Reassign Task
│   │   ├── Duplicate Task
│   │   └── Delete Task
│   ├── Task Details
│   │   ├── Description & Notes
│   │   ├── Assigned To (with profile photo)
│   │   ├── Created By (parent/admin)
│   │   ├── Due Date & Time
│   │   ├── Priority Level
│   │   ├── Points Value (for gamification)
│   │   ├── Related Event (if auto-created by event chain)
│   │   └── Task History (created, modified, completed)
│   ├── Gamification (Focus for Children)
│   │   ├── Points Dashboard
│   │   │   ├── Total Points Earned
│   │   │   ├── Points This Week
│   │   │   ├── Points Breakdown (by task type)
│   │   │   └── Points Leaderboard (family ranking)
│   │   ├── Badges & Achievements
│   │   │   ├── Earned Badges (with unlock date)
│   │   │   ├── Locked Badges (with unlock criteria)
│   │   │   └── Special Achievements (7-day streak, 100 tasks completed)
│   │   ├── Rewards Store (Parent-Configured)
│   │   │   ├── Available Rewards (100 points = $10, 50 points = extra screen time)
│   │   │   ├── Redeem Points
│   │   │   └── Redemption History
│   │   └── Streaks
│   │       ├── Current Streak (consecutive days)
│   │       ├── Longest Streak
│   │       └── Streak Milestones (7-day, 30-day, 100-day)
│   ├── Filters
│   │   ├── Filter by Person
│   │   ├── Filter by Priority
│   │   ├── Filter by Due Date (Today, This Week, Overdue)
│   │   └── Filter by Status (Pending, In Progress, Completed)
│   └── Task Templates (Phase 2+)
│       ├── Weekly Chore Rotation
│       ├── Homework Checklist
│       └── Bedtime Routine
│
├── ⚡ Event Chains (Primary Differentiator)
│   ├── Active Chains
│   │   ├── Currently Running Chains
│   │   │   ├── Chain Name & Description
│   │   │   ├── Status (Running, Paused, Completed, Failed)
│   │   │   ├── Progress (Step 2 of 5 complete)
│   │   │   ├── Last Triggered (timestamp)
│   │   │   └── Next Scheduled Trigger
│   │   ├── Chain Actions
│   │   │   ├── Pause Chain
│   │   │   ├── Resume Chain
│   │   │   ├── Edit Chain
│   │   │   ├── Duplicate Chain
│   │   │   └── Delete Chain
│   │   └── Chain Details
│   │       ├── Visual Flow Diagram (arrows showing steps)
│   │       ├── Trigger Conditions (when chain activates)
│   │       ├── Actions (what chain does)
│   │       ├── Assigned To (who receives tasks/notifications)
│   │       └── Chain History (all activations, results)
│   ├── Chain Templates (Pre-Built)
│   │   ├── Template Gallery
│   │   │   ├── Doctor Appointment Chain
│   │   │   │   └── Steps: Calendar event → Prep task → Prescription reminder
│   │   │   ├── Meal Planning → Shopping List Chain
│   │   │   │   └── Steps: Meal plan finalized → Auto-generate shopping list
│   │   │   ├── Recurring Chore Chain
│   │   │   │   └── Steps: Task assigned → Reminder (night before) → Reminder (morning of) → Points earned
│   │   │   ├── School Morning Routine Chain
│   │   │   │   └── Steps: Wake-up reminder → Breakfast reminder → Backpack check → Bus reminder
│   │   │   ├── Birthday Party Planning Chain
│   │   │   │   └── Steps: Event created → Guest list → Shopping list → Reminder (day before)
│   │   │   ├── Grocery Shopping Chain
│   │   │   │   └── Steps: Shopping list created → Assign to person → Reminder → Mark complete
│   │   │   ├── Prescription Refill Chain
│   │   │   │   └── Steps: Prescription added → Reminder (5 days before out) → Pickup task
│   │   │   ├── Weekly Family Meeting Chain
│   │   │   │   └── Steps: Calendar event (Sunday 2pm) → Agenda prep → Reminder
│   │   │   ├── Bedtime Routine Chain (Kids)
│   │   │   │   └── Steps: Brush teeth → Read book → Lights out → Points earned
│   │   │   └── Budget Alert Chain (Phase 2+)
│   │   │       └── Steps: Spending threshold → Alert parent → Review expenses
│   │   ├── Template Preview
│   │   │   ├── Visual Flow Diagram
│   │   │   ├── Example Scenarios (when it would trigger)
│   │   │   ├── Time Savings ("Saves 20 minutes per week")
│   │   │   └── User Reviews ("4.8★ from 1,234 families")
│   │   └── Template Actions
│   │       ├── Use Template (one-tap activation)
│   │       ├── Customize Template (edit before activation)
│   │       └── Preview Chain (see what will happen)
│   ├── Chain Builder (Advanced)
│   │   ├── Visual Drag-Drop Builder
│   │   │   ├── Trigger Block (what starts the chain)
│   │   │   │   ├── Event Created
│   │   │   │   ├── Task Completed
│   │   │   │   ├── Date/Time Trigger
│   │   │   │   ├── Shopping List Finalized
│   │   │   │   └── Meal Plan Created
│   │   │   ├── Action Blocks (what chain does)
│   │   │   │   ├── Create Task
│   │   │   │   ├── Create Calendar Event
│   │   │   │   ├── Add to Shopping List
│   │   │   │   ├── Send Notification
│   │   │   │   ├── Assign to Person
│   │   │   │   ├── Wait (delay before next action)
│   │   │   │   └── Conditional Logic (if-then)
│   │   │   ├── Connect Blocks (draw arrows between triggers and actions)
│   │   │   └── Preview Flow (validate before saving)
│   │   ├── Chain Configuration
│   │   │   ├── Chain Name & Description
│   │   │   ├── Enable/Disable Toggle
│   │   │   ├── Trigger Conditions (when, how often)
│   │   │   ├── Action Settings (assign to, due dates, points)
│   │   │   └── Notification Preferences
│   │   └── Test Mode
│   │       ├── Simulate Chain (see what would happen without executing)
│   │       ├── Test Trigger (manually trigger chain once)
│   │       └── Validate Chain (check for errors, missing data)
│   ├── Chain History
│   │   ├── All Chain Activations (timeline view)
│   │   ├── Filter by Chain Type
│   │   ├── Filter by Date Range
│   │   ├── View Chain Results (success, failure, partial)
│   │   ├── Undo Chain Actions (if needed)
│   │   └── Export Chain Data (CSV, JSON)
│   └── Chain Settings
│       ├── Global Enable/Disable (pause all chains)
│       ├── Notification Preferences (how to alert when chain triggers)
│       ├── Error Handling (what happens if chain fails)
│       └── Chain Limits (max chains per family: 10 on Free, unlimited on Premium)
│
├── 👥 Family & Settings
│   ├── Family Members
│   │   ├── View All Members
│   │   │   ├── Profile Photo, Name, Role
│   │   │   ├── Permissions Level (Admin, Co-Parent, Teen, Child, Extended, Guest)
│   │   │   ├── Last Active (timestamp)
│   │   │   └── Quick Actions (Edit, Remove, Message)
│   │   ├── Invite New Member
│   │   │   ├── Email Invitation
│   │   │   ├── SMS Invitation (Phase 2+)
│   │   │   ├── Share Invite Link
│   │   │   └── Child Account Creation (COPPA compliant)
│   │   ├── Member Details
│   │   │   ├── Profile Information (name, email, photo, birthdate)
│   │   │   ├── Role & Permissions
│   │   │   ├── Assigned Tasks (current, completed)
│   │   │   ├── Calendar Events (upcoming)
│   │   │   ├── Points & Badges (if child)
│   │   │   └── Activity Log (recent actions)
│   │   ├── Manage Roles & Permissions
│   │   │   ├── Change Role (Parent → Co-Parent, Teen → Parent)
│   │   │   ├── Custom Permissions (advanced: can edit event chains, can view budget)
│   │   │   └── Temporary Access (guest babysitter: 24-hour access)
│   │   └── Remove Member
│   │       ├── Remove from Family (soft delete: data retained for 30 days)
│   │       ├── Transfer Ownership (if removing primary admin)
│   │       └── Revoke Access (immediate: guest/babysitter)
│   ├── Family Settings
│   │   ├── Family Name & Photo
│   │   ├── Family Time Zone
│   │   ├── First Day of Week (Sunday or Monday)
│   │   ├── Default Calendar View (Month, Week, Day)
│   │   └── Family Visibility (who can see what)
│   ├── Notifications
│   │   ├── Notification Preferences
│   │   │   ├── Push Notifications (enabled/disabled)
│   │   │   ├── Email Notifications (enabled/disabled)
│   │   │   ├── SMS Notifications (Phase 2+)
│   │   │   └── Notification Schedule (quiet hours: 10pm-7am)
│   │   ├── Notification Types
│   │   │   ├── Event Reminders (15 min, 1 hour, 1 day before)
│   │   │   ├── Task Assignments (when assigned, when due)
│   │   │   ├── Event Chain Triggers ("Your meal plan shopping list is ready!")
│   │   │   ├── Family Activity (someone added event, completed task)
│   │   │   └── System Updates (new features, maintenance)
│   │   ├── Digest Mode
│   │   │   ├── Daily Digest (7:00 AM summary of today's schedule)
│   │   │   ├── Weekly Digest (Sunday evening: upcoming week preview)
│   │   │   └── Custom Digest (configure frequency, content)
│   │   └── Notification History
│   │       ├── View All Notifications
│   │       ├── Mark as Read/Unread
│   │       └── Clear Notifications
│   ├── Privacy & Security
│   │   ├── Data Privacy
│   │   │   ├── Privacy Policy (plain language)
│   │   │   ├── Data We Collect (minimal: email, family events, tasks)
│   │   │   ├── Data We DON'T Collect (no location tracking, no ad targeting)
│   │   │   ├── Data Sharing (opt-out by default, never sold to third parties)
│   │   │   └── COPPA Compliance (children under 13)
│   │   ├── Visibility Settings
│   │   │   ├── Who Can See My Events (All Family, Parents Only, Just Me)
│   │   │   ├── Who Can Assign Me Tasks (All Family, Parents Only)
│   │   │   └── Who Can View My Profile (All Family, Admins Only)
│   │   ├── Data Management
│   │   │   ├── Export My Data (JSON, CSV)
│   │   │   ├── Download Family Data (admin only)
│   │   │   ├── Delete My Account (30-day grace period)
│   │   │   └── Delete Family Data (admin only, irreversible after 30 days)
│   │   ├── Security
│   │   │   ├── Change Password
│   │   │   ├── Two-Factor Authentication (SMS, authenticator app)
│   │   │   ├── Active Sessions (view devices logged in)
│   │   │   └── Revoke Sessions (log out all devices)
│   │   └── Audit Log (Admin Only)
│   │       ├── View Family Activity Log (who did what, when)
│   │       ├── Filter by Member
│   │       ├── Filter by Action Type (created, edited, deleted)
│   │       └── Export Audit Log (CSV)
│   ├── Account Settings
│   │   ├── Profile
│   │   │   ├── Name, Email, Profile Photo
│   │   │   ├── Birthdate (optional, for kid birthdays)
│   │   │   ├── Phone Number (optional, for SMS notifications)
│   │   │   └── Bio (optional)
│   │   ├── Email & Password
│   │   │   ├── Change Email
│   │   │   ├── Change Password
│   │   │   └── Verify Email (if changed)
│   │   ├── Subscription & Billing (Premium Users)
│   │   │   ├── Current Plan (Free, Premium $9.99/mo, Family $14.99/mo)
│   │   │   ├── Payment Method (credit card, PayPal)
│   │   │   ├── Billing History (invoices, receipts)
│   │   │   ├── Upgrade Plan
│   │   │   ├── Downgrade Plan
│   │   │   └── Cancel Subscription
│   │   └── Account Deletion
│   │       ├── Delete My Account (30-day grace period)
│   │       └── Confirm Deletion (requires password)
│   ├── Preferences
│   │   ├── Theme
│   │   │   ├── Light Mode
│   │   │   ├── Dark Mode
│   │   │   └── Auto (system default)
│   │   ├── Language (English, Spanish, French - Phase 2+)
│   │   ├── Time Zone
│   │   ├── Date Format (MM/DD/YYYY or DD/MM/YYYY)
│   │   ├── Time Format (12-hour or 24-hour)
│   │   └── First Day of Week (Sunday or Monday)
│   └── Help & Support
│       ├── Help Center (FAQs)
│       │   ├── Getting Started
│       │   ├── Event Chains (how to use)
│       │   ├── Gamification (points, badges, rewards)
│       │   ├── Privacy & Security
│       │   └── Troubleshooting
│       ├── Tutorial & Onboarding
│       │   ├── Replay Onboarding Tour
│       │   ├── Feature Tutorials (video guides)
│       │   └── What's New (feature announcements)
│       ├── Contact Support
│       │   ├── Email Support (Premium users: priority)
│       │   ├── Live Chat (Phase 2+, Premium users)
│       │   └── Submit Bug Report
│       ├── Community
│       │   ├── User Forums (Phase 2+)
│       │   ├── Feature Requests (vote on roadmap)
│       │   └── Release Notes (changelog)
│       └── About
│           ├── Version Number
│           ├── Privacy Policy
│           ├── Terms of Service
│           └── Open Source Licenses
│
└── 🔍 Search (Global)
    ├── Search Bar (always visible in top nav)
    ├── Search Filters
    │   ├── Search Events (by title, location, attendees)
    │   ├── Search Tasks (by title, assignee, description)
    │   ├── Search Lists (by list name, item name)
    │   ├── Search Family Members (by name, role)
    │   └── Search Event Chains (by name, trigger)
    ├── Recent Searches (cached locally)
    ├── Search Suggestions (autocomplete)
    └── Advanced Search (Phase 2+)
        ├── Date Range Filter
        ├── Person Filter
        └── Status Filter (completed, pending, overdue)
```

---

## Navigation Structure

### Desktop Navigation (Top Nav + Sidebar)

**Top Navigation Bar** (persistent across all pages):

- **Left**: Family Hub logo (click → Dashboard)
- **Center**: Global search bar
- **Right**:
  - Notifications bell icon (badge for unread count)
  - Profile menu (avatar with dropdown)
    - My Profile
    - Account Settings
    - Switch Family (if user in multiple families)
    - Help & Support
    - Log Out

**Left Sidebar** (collapsible):

- 📊 Dashboard
- 📅 Calendar
- 📝 Lists
- ✅ Tasks & Chores
- ⚡ Event Chains ⭐ (badge: "NEW" for first 30 days)
- 👥 Family & Settings

**Responsive Behavior**:

- **Desktop (> 1024px)**: Sidebar expanded by default
- **Tablet (640px - 1024px)**: Sidebar collapsed to icons only
- **Mobile (< 640px)**: Sidebar hidden, bottom navigation visible

---

### Mobile Navigation (Bottom Nav)

**Bottom Navigation Bar** (5 tabs maximum per Nielsen Norman Group best practice):

1. **Home** (Dashboard)
   - Icon: 🏠
   - Always selected on app launch

2. **Calendar**
   - Icon: 📅
   - Badge: Shows count of today's events

3. **Lists**
   - Icon: 📝
   - Badge: Shows count of active shopping lists

4. **Tasks**
   - Icon: ✅
   - Badge: Shows count of tasks due today

5. **More**
   - Icon: ≡ (three horizontal lines)
   - Overflow menu containing:
     - ⚡ Event Chains
     - 👥 Family & Settings
     - 🔍 Search
     - 🔔 Notifications
     - ⚙️ Preferences

**Design**:

- Fixed position (always visible, doesn't scroll away)
- Active tab highlighted with brand color
- Icons + labels (for clarity)
- Haptic feedback on tap (iOS)

---

### Role-Based Navigation Differences

#### Parent/Admin Navigation

**Full Access** (sees all 6 top-level sections):

- 📊 Dashboard
- 📅 Calendar
- 📝 Lists
- ✅ Tasks & Chores
- ⚡ Event Chains (can create, edit, delete chains)
- 👥 Family & Settings (can manage family members, roles, billing)

**Additional Features**:

- Budget section (Phase 2+)
- Family management (invite/remove members, change roles)
- Event chain builder (advanced drag-drop)
- Audit log (view family activity)

---

#### Co-Parent Navigation

**Full Access** (same as Parent/Admin):

- All 6 sections visible
- Can create event chains
- Can manage family (except billing)

**Restrictions**:

- Cannot delete primary admin
- Cannot downgrade/cancel subscription (only primary admin)

---

#### Teen Navigation (13-17 years old)

**Limited Access** (simplified UI):

**Visible Sections** (4 top-level):

- 📊 Dashboard (teen-optimized: shows assigned tasks, events, points)
- 📅 Calendar (can see all family events, can create own events)
- 📝 Lists (can add items to shared lists, can create personal lists)
- ✅ Tasks & Chores (sees assigned tasks, can mark complete, earn points)

**Hidden Sections**:

- ⚡ Event Chains (view-only: can see active chains affecting them, cannot create/edit)
- 💰 Budget (Phase 2+: cannot view family budget)

**Additional Restrictions**:

- Cannot invite new family members
- Cannot change family settings
- Cannot delete events created by parents (can only delete own events)

**Gamification Emphasis**:

- Dashboard shows points balance prominently
- Badge showcase (unlocked achievements)
- Leaderboard (friendly competition with siblings)

---

#### Child Navigation (Under 13 years old)

**Heavily Restricted** (age-appropriate, simplified UI):

**Visible Sections** (3 top-level):

- 🏠 Home (child dashboard: today's tasks, upcoming events, points balance)
- ✅ My Tasks (assigned chores, with icons and points)
- 🎮 My Rewards (points, badges, streaks, rewards store)

**Hidden Sections**:

- 📅 Calendar (cannot view full family calendar, only sees own events on dashboard)
- 📝 Lists (cannot access shopping/to-do lists)
- ⚡ Event Chains (invisible to children)
- 💰 Budget (invisible)
- 👥 Family Settings (invisible)

**UI Simplifications**:

- Large text (reading level: 2nd grade)
- Visual icons for every task (🐕 "Feed dog", 🦷 "Brush teeth")
- Minimal text (no long descriptions)
- Gamification front and center (points, badges, confetti animations)

**Parental Controls**:

- Parents can view Noah's activity (tasks completed, points earned)
- Parents configure rewards (100 points = $10, 50 points = extra screen time)

---

#### Extended Family (Grandparent) Navigation

**Guest-Level Access** (view-only for most features):

**Visible Sections** (3 top-level):

- 📊 Dashboard (limited: shows babysitting schedule, meal plans)
- 📅 Calendar (can view events they're invited to, cannot create/edit)
- 📝 Lists (can add items to shared shopping lists, cannot delete lists)

**Hidden Sections**:

- ✅ Tasks & Chores (cannot assign tasks, can see tasks assigned to grandparent)
- ⚡ Event Chains (invisible)
- 💰 Budget (invisible)
- 👥 Family Settings (invisible, except own profile)

**Accessibility**:

- Large text mode (150% font size)
- High contrast mode (light theme default)
- Simplified navigation (no hamburger menus, clear labels)

---

#### Guest (Babysitter/Temporary Access) Navigation

**Temporary, Scoped Access** (expires after 30 days or manual revocation):

**Visible Sections** (2 top-level):

- 🏠 Babysitting Info (custom dashboard showing only relevant info)
  - Tonight's schedule (Emma swim practice 4pm, Noah bedtime 8pm)
  - Emergency contacts (Sarah, Mike, 911)
  - Important notes (allergies, medications, house rules)
  - Meal plan for tonight
- 📋 Check-In/Out (log arrival/departure time)

**Hidden Sections**:

- All other sections invisible
- Cannot view full family calendar
- Cannot view budget or family settings

**Privacy**:

- Guest cannot see private family events
- Guest cannot modify family data (read-only except check-in/out)
- Guest access logged in audit trail (admin can see what guest viewed)

---

## Content Organization

### Dashboard Design Philosophy

**Widget-Based, Customizable Layout**:

- Drag-and-drop widget reordering (desktop)
- Hide/show widgets (personal ization)
- Smart defaults based on role (parent sees budget widget, teen sees gamification widget)

**Parent Dashboard Widgets**:

1. **Today's Schedule** (next 3 events, "See all" link)
2. **My Tasks** (top 5 tasks due today/this week)
3. **Active Shopping Lists** (groceries, household items)
4. **Active Event Chains** (currently running chains, status)
5. **Family Activity Feed** (recent updates: "Emma completed 'Clean room' +10 points")
6. **Meal Plan This Week** (Mon-Sun dinners)
7. **Budget Summary** (Phase 2+: monthly spending, budget vs. actual)

**Teen Dashboard Widgets**:

1. **My Day** (today's events and tasks)
2. **My Tasks** (assigned chores, homework)
3. **Points & Badges** (gamification emphasis)
4. **Family Events** (next 3 family events)
5. **Shopping Lists** (can add items)

**Child Dashboard Widgets**:

1. **My Chores Today** (3-5 visual task cards with icons)
2. **My Points** (progress bar, confetti animation when milestone reached)
3. **My Badges** (showcase unlocked achievements)
4. **Fun Events Coming Up** (birthday party, zoo trip, movie night)

---

### Feature Grouping Logic

**Time-Based Features** (Calendar, Events, Reminders):

- Grouped under "Calendar" section
- Related features: Recurring events, event chains (auto-create tasks from events)

**Task-Based Features** (Tasks, Chores, Lists):

- Grouped under "Tasks & Chores" and "Lists"
- Related features: Gamification (points, badges), assignments

**Planning Features** (Meals, Budget, Shopping):

- Grouped under "Lists" (meal plans, shopping lists)
- Budget in separate section (Phase 2+)

**Automation Features** (Event Chains):

- Standalone section (primary differentiator, cannot be buried)
- Related features: Chain templates, chain builder, chain history

**Social Features** (Family, Notifications, Messages - Phase 2+):

- Grouped under "Family & Settings"
- Related features: Family activity feed, member profiles

---

### Settings Hierarchy

**Logical Grouping** (5 top-level categories):

1. **Account**
   - Profile (name, email, photo)
   - Email & Password
   - Two-Factor Authentication

2. **Family**
   - Members (invite, edit, remove)
   - Invitations (pending invites)
   - Roles & Permissions (admin, co-parent, teen, child)
   - Family Name/Photo

3. **Notifications**
   - Push Notifications
   - Email Notifications
   - SMS Notifications (Phase 2+)
   - Quiet Hours (10pm-7am default)
   - Digest Mode (daily, weekly)

4. **Privacy & Security**
   - Data Privacy (what we collect, what we don't)
   - Visibility Settings (who can see my events, tasks)
   - Data Export (JSON, CSV)
   - Delete Account (30-day grace period)

5. **Preferences**
   - Theme (Light, Dark, Auto)
   - Language (English, Spanish, French - Phase 2+)
   - Time Zone
   - First Day of Week (Sunday or Monday)

---

### Search and Discovery Patterns

**Global Search** (always accessible in top nav):

- Search scope: Current family only (privacy)
- Search across: Events, tasks, lists, family members, event chains
- Filters: By type (event, task, list), by person, by date range
- Recent searches: Cached locally (last 10 searches)
- Search suggestions: Autocomplete based on existing data

**Contextual Search** (within each section):

- Calendar: Search events by title, location, attendees
- Tasks: Search tasks by title, assignee, status
- Lists: Search list items by name, category
- Event Chains: Search chains by name, trigger type

**Advanced Search** (Phase 2+):

- Date range filter (events/tasks between dates)
- Person filter (show only items assigned to Emma)
- Status filter (completed, pending, overdue)
- Combine filters (Emma's overdue tasks)

---

## Multi-Role Experience

### Permission Matrix

Comprehensive table showing what each role can do:

| Feature | Parent | Co-Parent | Teen (13-17) | Child (<13) | Extended (Grandparent) | Guest (Babysitter) |
|---------|--------|-----------|--------------|-------------|-----------------------|-------------------|
| **Calendar** | | | | | | |
| View all family events | ✅ | ✅ | ✅ | ❌ (own events only) | ✅ (invited events only) | ❌ (relevant events only) |
| Create events | ✅ | ✅ | ✅ | ❌ (parent approval) | ✅ (limited) | ❌ |
| Edit own events | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| Edit others' events | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Delete events | ✅ | ✅ | ✅ (own only) | ❌ | ✅ (own only) | ❌ |
| **Tasks & Chores** | | | | | | |
| View all tasks | ✅ | ✅ | ✅ (assigned to them) | ✅ (assigned to them) | ✅ (assigned to them) | ❌ |
| Create tasks | ✅ | ✅ | ❌ (can suggest) | ❌ | ✅ (limited) | ❌ |
| Assign tasks | ✅ | ✅ | ❌ | ❌ | ✅ (limited) | ❌ |
| Mark tasks complete | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| Earn points (gamification) | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ |
| **Lists** | | | | | | |
| View shopping lists | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| Add items to lists | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| Check off items | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| Delete lists | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Event Chains** | | | | | | |
| View active chains | ✅ | ✅ | ✅ (view only) | ❌ | ❌ | ❌ |
| Create event chains | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Edit event chains | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Delete event chains | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Budget** (Phase 2+) | | | | | | |
| View budget | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Add expenses | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Edit budget categories | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Family Management** | | | | | | |
| Invite new members | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Edit member roles | ✅ | ✅ (except primary admin) | ❌ | ❌ | ❌ | ❌ |
| Remove members | ✅ | ✅ (except primary admin) | ❌ | ❌ | ❌ | ❌ |
| View audit log | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Manage subscription/billing | ✅ (primary admin only) | ❌ | ❌ | ❌ | ❌ | ❌ |

---

### Role Switching Mechanism

**No Explicit Role Switching Needed**:

- UI adapts automatically based on logged-in user's role
- Permissions enforced server-side (cannot bypass with client manipulation)

**Admin Preview Mode** (optional feature for testing):

- Parent can "Preview as Teen" to see Emma's dashboard
- Parent can "Preview as Child" to see Noah's simplified UI
- Useful for: Understanding what kids see, testing gamification features
- Clear banner: "Previewing as Emma (Teen). Click here to return to your view."

**Role Change Notification**:

- If admin changes user's role (Emma: Teen → Parent), user receives notification
- Next login: UI automatically adapts to new role
- Explanation modal: "Your role has changed from Teen to Parent. You now have full access to Family Hub."

---

### Age-Appropriate Content Filtering

**Children Under 13 (COPPA Compliance)**:

- **Automatic filtering**: Cannot see budget, cannot see adult events (doctor appointments marked "private")
- **Simplified language**: Notifications use kid-friendly language ("Hooray! You earned 10 points for feeding the dog!")
- **No marketing**: Cannot receive promotional emails or notifications
- **Parental oversight**: Sarah can view Noah's activity, points earned, badges unlocked

**Teens (13-17)**:

- **Age-appropriate**: Can see most family events, cannot see budget
- **Privacy controls**: Emma can mark events as "Private" (hidden from child siblings)
- **Gamification optional**: Teens can opt-out of gamification if they find it childish

**Adults (18+)**:

- **Full access**: No content filtering
- **Privacy controls**: Can mark events/tasks as "Private" (hidden from teens/children)

---

## Deep-Linking Strategy

**URL Pattern**: `/family/{familyId}/{section}/{item}/{id}`

### Examples

1. **Calendar Event**:
   - URL: `/family/abc123/calendar/event/evt456`
   - Behavior: Opens Family Hub → Calendar → Event Details modal for event `evt456`
   - Use case: Sarah shares link with Mike: "Here's Emma's swim practice schedule"

2. **Task**:
   - URL: `/family/abc123/tasks/task/tsk789`
   - Behavior: Opens Family Hub → Tasks → Task Details modal for task `tsk789`
   - Use case: Sarah assigns task to Emma via shared link

3. **Event Chain Template**:
   - URL: `/family/abc123/chains/template/doctor-appt`
   - Behavior: Opens Family Hub → Event Chains → Template Gallery → Doctor Appointment Template
   - Use case: Onboarding tutorial: "Try this event chain template!"

4. **Shopping List**:
   - URL: `/family/abc123/lists/shopping/lst456`
   - Behavior: Opens Family Hub → Lists → "Groceries for Tonight" list
   - Use case: Sarah shares shopping list with Mike: "Can you pick these up on your way home?"

5. **Event Chain Builder (Pre-Populated)**:
   - URL: `/family/abc123/chains/builder?template=meal-planning`
   - Behavior: Opens Event Chain Builder with Meal Planning template pre-loaded
   - Use case: Contextual suggestion: "Want to automate your meal planning?"

### Sharing Links

**Share Button** (available on events, tasks, lists):

- Copy link to clipboard
- Share via email (opens email client with pre-filled message)
- Share via SMS (Phase 2+)
- QR code (Phase 2+, for in-person sharing)

**Link Permissions**:

- Family-only: Link only works if recipient is a member of `familyId`
- Guest access: Link grants temporary view-only access (expires in 24 hours)
- Public sharing: Not supported (privacy-first approach)

---

## Responsive Breakpoints

**Mobile**: `< 640px` (sm)
**Tablet**: `640px - 1024px` (md - lg)
**Desktop**: `> 1024px` (xl)
**Large Desktop**: `> 1920px` (2xl)

### Responsive Behavior

**Navigation**:

- Mobile: Bottom nav (5 tabs)
- Tablet: Sidebar (collapsed to icons)
- Desktop: Sidebar (expanded)

**Dashboard**:

- Mobile: Single column, widgets stacked
- Tablet: 2 columns
- Desktop: 3 columns
- Large Desktop: 4 columns (or 3 columns with wider widgets)

**Calendar**:

- Mobile: Day view default (Month view available, swipe to navigate)
- Tablet: Week view default
- Desktop: Month view default

**Event Chain Builder**:

- Mobile: Linear step-by-step flow (not drag-drop)
- Tablet: Drag-drop available (with touch support)
- Desktop: Full drag-drop visual builder

---

## Next Steps

1. **Validate IA with User Testing**: Test navigation structure with 5 families (parent, teen, child)
2. **Create Low-Fidelity Wireframes**: Paper prototypes for key flows (onboarding, event chain discovery)
3. **Test Role-Based Navigation**: Ensure teens/children understand their limited UI
4. **Refine Event Chain Discovery**: A/B test contextual suggestions vs. onboarding tour
5. **Accessibility Audit**: Ensure WCAG 2.1 AA compliance (see accessibility-strategy.md)

---

**Document Status**: Final
**Last Updated**: 2025-12-19
**Next Review**: After MVP user testing
