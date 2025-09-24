# Centralized Styles Documentation

This document explains how to use the centralized styling system in the Viadex Device Portal frontend.

## Overview

All reusable styles have been moved to `/src/assets/styles/` and are automatically imported globally. This provides:

- **Consistency** across all views
- **Maintainability** through centralized design tokens
- **Reusability** of common UI patterns
- **Performance** by reducing style duplication

## File Structure

```
src/assets/styles/
├── index.css           # Main import file (imported in main.ts)
├── variables.css       # CSS custom properties (design tokens)
├── layout.css          # Layout patterns and grids
├── components.css      # Buttons, forms, modals, tables
├── status-badges.css   # Status indicators for all entity types
├── states.css          # Loading, empty, error states
└── stats.css           # Statistics cards and pagination
```

## Design Tokens (CSS Variables)

### Colors
```css
/* Primary colors */
--color-primary-500: #3b82f6
--color-primary-600: #2563eb

/* Status colors */
--color-success-500: #22c55e
--color-warning-500: #f59e0b
--color-danger-500: #ef4444

/* Gray scale */
--color-gray-50: #f9fafb
--color-gray-500: #6b7280
--color-gray-900: #111827
```

### Spacing
```css
--spacing-xs: 0.25rem    /* 4px */
--spacing-sm: 0.5rem     /* 8px */
--spacing-lg: 1rem       /* 16px */
--spacing-xl: 1.5rem     /* 24px */
--spacing-2xl: 2rem      /* 32px */
```

### Typography
```css
--font-size-xs: 0.75rem
--font-size-sm: 0.875rem
--font-size-lg: 1.125rem
--font-weight-medium: 500
--font-weight-bold: 700
```

## Common Layout Classes

### Page Structure
```html
<div class="page-view">
  <div class="page-header">
    <h1 class="page-title">Page Title</h1>
    <p class="page-subtitle">Description</p>
  </div>
  
  <div class="content-section">
    <!-- Content -->
  </div>
</div>
```

### Grids and Flexbox
```html
<!-- Statistics grid -->
<div class="stats-grid">
  <div class="stat-card">...</div>
</div>

<!-- Flex layouts -->
<div class="flex items-center gap-lg">
  <div>Item 1</div>
  <div>Item 2</div>
</div>

<!-- Filter layouts -->
<div class="filters-section">
  <div class="filters-row">
    <div class="filter-group">...</div>
  </div>
</div>
```

## Button Classes

```html
<!-- Primary actions -->
<button class="btn btn-primary">Save</button>

<!-- Secondary actions -->
<button class="btn btn-secondary">Cancel</button>

<!-- Outline buttons -->
<button class="btn btn-outline">Edit</button>

<!-- Danger actions -->
<button class="btn btn-danger">Delete</button>

<!-- Small buttons -->
<button class="btn btn-small btn-outline">View</button>
```

## Form Components

```html
<div class="form-group">
  <label class="form-label">Field Label</label>
  <input class="form-input" type="text" />
</div>

<div class="filter-group">
  <label class="filter-label">Filter</label>
  <select class="filter-select">...</select>
</div>
```

## Status Badges

### Device Statuses
```html
<span class="status-badge status-active">Active</span>
<span class="status-badge status-retired">Retired</span>
<span class="status-badge status-repair">Under Repair</span>
```

### Shipment Statuses
```html
<span class="status-badge status-processing">Processing</span>
<span class="status-badge status-in-transit">In Transit</span>
<span class="status-badge status-delivered">Delivered</span>
<span class="status-badge status-delayed">Delayed</span>
```

### Quote/Support Tier Statuses
```html
<span class="status-badge status-draft">Draft</span>
<span class="status-badge tier-basic">Basic</span>
<span class="status-badge tier-premium">Premium</span>
```

## State Components

### Loading States
```html
<div class="loading-state">
  <div class="loading-spinner"></div>
  <p>Loading...</p>
</div>
```

### Empty States
```html
<div class="empty-state">
  <div class="empty-icon">📱</div>
  <h3>No items found</h3>
  <p>Description of empty state</p>
  <button class="btn btn-primary">Add Item</button>
</div>
```

### Error States
```html
<div class="error-banner">
  <div class="error-content">
    <span class="error-icon">⚠️</span>
    <span class="error-message">Error message</span>
    <button class="error-retry">Retry</button>
  </div>
</div>
```

## Tables

```html
<div class="table-container">
  <table class="data-table">
    <thead>
      <tr>
        <th>Column 1</th>
        <th>Column 2</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td class="cell-name">Primary text</td>
        <td class="cell-secondary">Secondary text</td>
        <td class="cell-price">£99.99</td>
        <td class="cell-actions">
          <button class="btn btn-small btn-secondary">View</button>
        </td>
      </tr>
    </tbody>
  </table>
</div>
```

## Statistics Cards

```html
<div class="stats-grid">
  <div class="stat-card">
    <div class="stat-value">42</div>
    <div class="stat-label">Total Items</div>
  </div>
  
  <div class="stat-card stat-card-success">
    <div class="stat-value">38</div>
    <div class="stat-label">Active</div>
  </div>
</div>
```

## Modals

```html
<div class="modal-overlay">
  <div class="modal-content">
    <div class="modal-header">
      <h2>Modal Title</h2>
      <button class="modal-close">&times;</button>
    </div>
    
    <div class="modal-form">
      <!-- Form content -->
      
      <div class="modal-actions">
        <button class="btn btn-outline">Cancel</button>
        <button class="btn btn-primary">Save</button>
      </div>
    </div>
  </div>
</div>
```

## Pagination

```html
<div class="pagination-section">
  <div class="pagination-info">
    Showing 1-20 of 100 items
  </div>
  
  <div class="pagination-controls">
    <button class="btn btn-outline btn-small">Previous</button>
    
    <div class="page-numbers">
      <button class="page-number active">1</button>
      <button class="page-number">2</button>
      <button class="page-number">3</button>
    </div>
    
    <button class="btn btn-outline btn-small">Next</button>
  </div>
</div>
```

## Utility Classes

### Spacing
```html
<div class="gap-lg">        <!-- gap: 1rem -->
<div class="section-spacing"> <!-- margin-bottom: 2rem -->
```

### Text
```html
<p class="text-secondary">   <!-- Secondary text color -->
<p class="text-center">      <!-- Center alignment -->
<p class="font-medium">      <!-- Medium font weight -->
```

### Layout
```html
<div class="flex items-center justify-between">
<div class="w-full">
<div class="overflow-x-auto">
```

## Migration Guide

When migrating existing components:

1. **Replace layout classes**: `devices-view` → `page-view`, `header` → `page-header`
2. **Update table classes**: `devices-table` → `data-table`, `device-name` → `cell-name`
3. **Use status badges**: Replace custom status classes with standard badge classes
4. **Remove duplicate styles**: Delete styles that now exist globally
5. **Keep view-specific styles**: Only keep styles that are truly unique to that view

## Best Practices

1. **Use design tokens**: Always use CSS variables instead of hardcoded values
2. **Prefer utility classes**: Use existing utilities before creating custom styles
3. **Follow naming conventions**: Use semantic class names (e.g., `cell-name` not `bold-text`)
4. **Keep views minimal**: Only add scoped styles for truly view-specific behaviors
5. **Test responsiveness**: All global styles include mobile breakpoints
6. **Document new patterns**: Update this README when adding new reusable components

## Examples in Use

See the updated `Devices.vue` and `Shipments.vue` files for examples of how to use these centralized styles effectively.