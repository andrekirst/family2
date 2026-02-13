import { TestBed } from '@angular/core/testing';
import { Router, NavigationStart } from '@angular/router';
import { TemplateRef } from '@angular/core';
import { Subject } from 'rxjs';
import { ContextPanelService } from './context-panel.service';

describe('ContextPanelService', () => {
  let service: ContextPanelService;
  let routerEvents$: Subject<NavigationStart>;

  beforeEach(() => {
    routerEvents$ = new Subject();

    TestBed.configureTestingModule({
      providers: [
        ContextPanelService,
        {
          provide: Router,
          useValue: { events: routerEvents$.asObservable() },
        },
      ],
    });

    service = TestBed.inject(ContextPanelService);
  });

  function createFakeTemplate(): TemplateRef<unknown> {
    return {} as TemplateRef<unknown>;
  }

  it('should start closed with null template and itemId', () => {
    expect(service.isOpen()).toBe(false);
    expect(service.template()).toBeNull();
    expect(service.itemId()).toBeNull();
    expect(service.mode()).toBe('view');
  });

  it('should open with template and itemId', () => {
    const tpl = createFakeTemplate();
    service.open(tpl, 'event-1');

    expect(service.isOpen()).toBe(true);
    expect(service.template()).toBe(tpl);
    expect(service.itemId()).toBe('event-1');
    expect(service.mode()).toBe('view');
  });

  it('should close and reset all state', () => {
    const tpl = createFakeTemplate();
    service.open(tpl, 'event-1');
    service.close();

    expect(service.isOpen()).toBe(false);
    expect(service.template()).toBeNull();
    expect(service.itemId()).toBeNull();
    expect(service.mode()).toBe('view');
  });

  it('should toggle closed when opening with same itemId', () => {
    const tpl = createFakeTemplate();
    service.open(tpl, 'event-1');
    expect(service.isOpen()).toBe(true);

    service.open(tpl, 'event-1');
    expect(service.isOpen()).toBe(false);
    expect(service.template()).toBeNull();
    expect(service.itemId()).toBeNull();
  });

  it('should replace content when opening with different itemId', () => {
    const tpl1 = createFakeTemplate();
    const tpl2 = createFakeTemplate();

    service.open(tpl1, 'event-1');
    service.open(tpl2, 'event-2');

    expect(service.isOpen()).toBe(true);
    expect(service.template()).toBe(tpl2);
    expect(service.itemId()).toBe('event-2');
  });

  it('should close on NavigationStart', () => {
    const tpl = createFakeTemplate();
    service.open(tpl, 'event-1');
    expect(service.isOpen()).toBe(true);

    routerEvents$.next(new NavigationStart(1, '/other'));

    expect(service.isOpen()).toBe(false);
    expect(service.template()).toBeNull();
    expect(service.itemId()).toBeNull();
  });

  // Create mode tests
  it('should open in create mode when itemId is omitted', () => {
    const tpl = createFakeTemplate();
    service.open(tpl);

    expect(service.isOpen()).toBe(true);
    expect(service.mode()).toBe('create');
    expect(service.itemId()).toBeNull();
    expect(service.template()).toBe(tpl);
  });

  it('should not toggle when opening without itemId consecutively', () => {
    const tpl = createFakeTemplate();

    service.open(tpl);
    expect(service.isOpen()).toBe(true);

    service.open(tpl);
    expect(service.isOpen()).toBe(true);
    expect(service.mode()).toBe('create');
  });

  it('should transition from create to view via setItemId', () => {
    const tpl = createFakeTemplate();
    service.open(tpl);

    expect(service.mode()).toBe('create');
    expect(service.itemId()).toBeNull();

    service.setItemId('new-event-1');

    expect(service.mode()).toBe('view');
    expect(service.itemId()).toBe('new-event-1');
    expect(service.isOpen()).toBe(true);
  });

  it('should reset mode on close', () => {
    const tpl = createFakeTemplate();
    service.open(tpl);
    expect(service.mode()).toBe('create');

    service.close();
    expect(service.mode()).toBe('view');
  });

  it('should open in view mode when itemId is provided', () => {
    const tpl = createFakeTemplate();
    service.open(tpl, 'event-1');

    expect(service.mode()).toBe('view');
  });
});
