import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container mx-auto px-4 py-8">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-4xl font-bold">Event Chains - Intelligent Automation</h1>
        <p class="text-lg text-gray-600 mt-2">
          The feature that makes Family Hub different from every other family app
        </p>
        <div class="badge badge-primary badge-lg mt-4">COMING SOON - MVP MOCKUP</div>
      </div>

      <!-- What are Event Chains? -->
      <div class="card bg-base-100 shadow-xl mb-8">
        <div class="card-body">
          <h2 class="card-title text-2xl">What are Event Chains?</h2>
          <p class="text-gray-700">
            Event Chains are intelligent automation workflows that connect different family
            activities. When one thing happens, Family Hub automatically creates related tasks,
            sends reminders, and coordinates family members — all without you lifting a finger.
          </p>
          <div class="alert alert-info mt-4">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              class="stroke-current shrink-0 w-6 h-6"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              ></path>
            </svg>
            <span
              ><strong>Example:</strong> Add your child's birthday → Family Hub automatically
              reminds you 1 week before, creates a shopping task, assigns it to a parent, and sends
              day-of notifications. Zero manual work.</span
            >
          </div>
        </div>
      </div>

      <!-- Example Event Chains -->
      <h2 class="text-3xl font-bold mb-6">Example Event Chains</h2>

      <div class="space-y-6">
        <!-- Birthday Chain -->
        <div class="card bg-gradient-to-r from-primary/10 to-secondary/10 shadow-xl">
          <div class="card-body">
            <h3 class="card-title text-xl">🎂 Birthday Event Chain</h3>
            <ul class="timeline timeline-vertical timeline-compact">
              <li>
                <div class="timeline-start timeline-box">
                  <strong>Trigger:</strong> Birthday added to calendar
                </div>
                <div class="timeline-middle">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                    class="w-5 h-5"
                  >
                    <path
                      fill-rule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                      clip-rule="evenodd"
                    />
                  </svg>
                </div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-middle">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                    class="w-5 h-5"
                  >
                    <path
                      fill-rule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                      clip-rule="evenodd"
                    />
                  </svg>
                </div>
                <div class="timeline-end timeline-box">
                  <strong>Day -7:</strong> Send reminder to all parents
                </div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-start timeline-box">
                  <strong>Day -7:</strong> Create "Buy birthday gift" shopping task
                </div>
                <div class="timeline-middle">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                    class="w-5 h-5"
                  >
                    <path
                      fill-rule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                      clip-rule="evenodd"
                    />
                  </svg>
                </div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-middle">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                    class="w-5 h-5"
                  >
                    <path
                      fill-rule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                      clip-rule="evenodd"
                    />
                  </svg>
                </div>
                <div class="timeline-end timeline-box">
                  <strong>Day -7:</strong> Auto-assign task to parent with lightest workload
                </div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-start timeline-box">
                  <strong>Day -1:</strong> Final reminder to all family members
                </div>
                <div class="timeline-middle">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                    class="w-5 h-5"
                  >
                    <path
                      fill-rule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                      clip-rule="evenodd"
                    />
                  </svg>
                </div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-middle">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                    class="w-5 h-5 text-success"
                  >
                    <path
                      fill-rule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                      clip-rule="evenodd"
                    />
                  </svg>
                </div>
                <div class="timeline-end timeline-box"><strong>Day 0:</strong> Celebrate! 🎉</div>
              </li>
            </ul>
            <div class="text-sm text-gray-500 mt-4 italic">
              All of this happens automatically. You just add the birthday once.
            </div>
          </div>
        </div>

        <!-- School Event Chain -->
        <div class="card bg-gradient-to-r from-accent/10 to-primary/10 shadow-xl">
          <div class="card-body">
            <h3 class="card-title text-xl">📚 School Event Chain</h3>
            <ul class="timeline timeline-vertical timeline-compact">
              <li>
                <div class="timeline-start timeline-box">
                  <strong>Trigger:</strong> School event added (e.g., Parent-Teacher Conference)
                </div>
                <div class="timeline-middle">➜</div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-middle">➜</div>
                <div class="timeline-end timeline-box">
                  <strong>Day -3:</strong> Remind both parents
                </div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-start timeline-box">
                  <strong>Day -1:</strong> Send calendar invite with location
                </div>
                <div class="timeline-middle">➜</div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-middle">➜</div>
                <div class="timeline-end timeline-box text-success">
                  <strong>Day 0:</strong> Attend together
                </div>
              </li>
            </ul>
          </div>
        </div>

        <!-- Grocery Shopping Chain -->
        <div class="card bg-gradient-to-r from-secondary/10 to-accent/10 shadow-xl">
          <div class="card-body">
            <h3 class="card-title text-xl">🛒 Grocery Shopping Chain</h3>
            <ul class="timeline timeline-vertical timeline-compact">
              <li>
                <div class="timeline-start timeline-box">
                  <strong>Trigger:</strong> Item added to shopping list by anyone
                </div>
                <div class="timeline-middle">➜</div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-middle">➜</div>
                <div class="timeline-end timeline-box">
                  <strong>Immediate:</strong> Notify family member who usually shops
                </div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-start timeline-box">
                  <strong>Smart:</strong> Group items by store (Aldi, Rewe, etc.)
                </div>
                <div class="timeline-middle">➜</div>
                <hr />
              </li>
              <li>
                <hr />
                <div class="timeline-middle">➜</div>
                <div class="timeline-end timeline-box text-success">
                  <strong>Result:</strong> Optimized shopping trip
                </div>
              </li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Why This Matters -->
      <div class="card bg-primary text-primary-content shadow-xl mt-8">
        <div class="card-body">
          <h2 class="card-title text-2xl">Why Event Chains Are Game-Changing</h2>
          <div class="grid md:grid-cols-2 gap-6 mt-4">
            <div>
              <h3 class="font-bold mb-2">❌ Without Event Chains (Competitors)</h3>
              <ul class="space-y-2 text-sm">
                <li>• Manually create reminders for every birthday</li>
                <li>• Manually assign shopping tasks</li>
                <li>• Manually send notifications to family</li>
                <li>• Forget important events</li>
                <li>• Uneven task distribution</li>
              </ul>
            </div>
            <div>
              <h3 class="font-bold mb-2">✅ With Event Chains (Family Hub)</h3>
              <ul class="space-y-2 text-sm">
                <li>• Set it once, automates forever</li>
                <li>• Smart task assignment (fair distribution)</li>
                <li>• Everyone stays informed automatically</li>
                <li>• Never miss important moments</li>
                <li>• More time for family, less time coordinating</li>
              </ul>
            </div>
          </div>
        </div>
      </div>

      <!-- Call to Action -->
      <div class="text-center mt-8">
        <p class="text-xl mb-4">Interested in this feature?</p>
        <p class="text-gray-600 mb-6">
          We're building Event Chains based on real user feedback. Tell us what you think!
        </p>
        <button class="btn btn-primary btn-lg">Provide Feedback (Coming Soon)</button>
      </div>
    </div>
  `,
})
export class EventsComponent {}
