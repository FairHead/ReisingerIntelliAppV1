# Visual Verification Guide for Device Positioning Mode

## Before/After Visual States

### Normal State (Default)
- Device control has standard size (100%)
- Background color: Semi-transparent blue (#992020A0)
- Positioning button (📍) background: Gray
- Device can be long-pressed to activate positioning mode

### Positioning Mode Active
- Device control scales to 105% (5% larger)
- Background color: Semi-transparent gray (#99808080)
- Positioning button (📍) background: Orange
- Device can be dragged to new position

## UI Layout Changes

### Button Layout
The positioning button is now the first button in the vertical stack:
```
[Large circular door toggle button]  [📍 Positioning Button]
                                    [⚙ Settings Button]
                                    [🗑 Delete Button]
```

### Visual Feedback Hierarchy
1. **Button Color**: Orange (active) vs Gray (inactive)
2. **Device Scale**: 105% (active) vs 100% (inactive)
3. **Background Color**: Gray (active) vs Blue (inactive)

## User Interaction Flow

### Positioning Mode Activation
1. User clicks positioning button (📍)
2. Button turns orange
3. Device scales to 105%
4. Device background changes to gray
5. Device becomes draggable

### Device Repositioning
1. User drags device to new location
2. Device follows touch/mouse input
3. Visual feedback shows device moving
4. Release completes the repositioning

### Positioning Mode Deactivation
1. Occurs automatically after successful drag
2. Button returns to gray
3. Device scales back to 100%
4. Background returns to blue
5. New position is saved

## Testing Checklist

### Visual Verification
- [ ] Positioning button is visible and properly styled
- [ ] Button changes color when clicked (gray → orange)
- [ ] Device scales to 105% when positioning mode is active
- [ ] Device background changes to gray when positioning mode is active
- [ ] All visual changes revert when positioning mode is deactivated

### Functional Verification
- [ ] Positioning button toggles positioning mode on/off
- [ ] Device can be dragged when positioning mode is active
- [ ] Device cannot be dragged when positioning mode is inactive
- [ ] Floor plan zoom/pan is disabled when any device is in positioning mode
- [ ] Device position is saved after successful repositioning
- [ ] Long-press still works for backward compatibility

### Integration Verification
- [ ] Multiple devices can be positioned independently
- [ ] Positioning works at different zoom levels
- [ ] Positioning works with different screen sizes
- [ ] No interference with other UI elements
- [ ] Smooth animation transitions

## Common Issues to Watch For

### Visual Issues
- Button not changing color
- Device not scaling properly
- Background color not updating
- Positioning button not visible

### Functional Issues
- Device not becoming draggable
- Positioning mode not toggling
- Device position not saving
- Interference with zoom/pan

### Integration Issues
- Multiple devices in positioning mode simultaneously
- Positioning mode not properly resetting
- Pan/pinch container conflicts
- Memory leaks from event handlers

## Manual Testing Steps

1. **Setup**: Open floor plan manager with placed devices
2. **Activation Test**: Click positioning button, verify visual changes
3. **Drag Test**: Drag device to new position, verify movement
4. **Completion Test**: Release device, verify position save and mode exit
5. **Integration Test**: Test zoom/pan functionality
6. **Reset Test**: Verify all states return to normal