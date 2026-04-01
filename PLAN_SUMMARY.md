# CSS Project Plan Summary

## Project Overview

**Goal**: Create a command-line spreadsheet application (css.exe) for Windows 64-bit that runs in Command Prompt with embedded .NET Runtime.

## Key Requirements Met

✅ .NET SDK 10.0 compatible  
✅ Self-contained with embedded .NET Runtime  
✅ Executable named `css.exe`  
✅ Command-line argument support: `css.exe filename.csv`  
✅ Grid: A-ZZ columns (702) × 1-1024 rows  
✅ Scrollable viewport with fixed headers  
✅ Formula support: arithmetic operations + cell references  
✅ Functions: SUM, AVERAGE, MIN, MAX, IF  
✅ Navigation: Arrow keys  
✅ Editing: Enter to edit, Enter to confirm, ESC to cancel  
✅ File operations: CSV read/write/new  
✅ Column width adjustment  
✅ Vim-style command mode (`:w`, `:o`, `:n`, `:width`, `:q`, `:wq`)

## Architecture Highlights

### Component Structure
```
Models → Core (Formula Engine) → UI (Console Renderer) → IO (File Operations)
   ↓           ↓                      ↓                        ↓
  Cell    Parser/Evaluator      Viewport/Input           CSV Reader/Writer
```

### Key Technologies
- **Language**: C# with .NET 10.0
- **UI**: Console-based with ANSI escape codes
- **Data Structure**: Dictionary-based sparse grid
- **Formula Engine**: Tokenizer → Parser → AST → Evaluator
- **File Format**: CSV with UTF-8 encoding

### Design Patterns
- **MVC-like**: Separation of data (Models), logic (Core), and presentation (UI)
- **Observer**: Dependency tracking for formula recalculation
- **Command**: Command mode parser for user actions
- **Strategy**: Different input handlers for different modes

## Implementation Phases

### Phase 1: Foundation (2.5 hours)
- Project setup with .NET 10.0
- Core data models (Cell, Worksheet, CellAddress)
- Column helper utilities

### Phase 2: Formula Engine (4 hours)
- Tokenizer and parser
- Expression evaluator
- Function library (SUM, AVERAGE, MIN, MAX, IF)
- Circular reference detection

### Phase 3: User Interface (3 hours)
- Console rendering with grid
- Viewport and scrolling
- Status bar
- Cell highlighting

### Phase 4: Input System (2 hours)
- Navigation mode (arrow keys)
- Edit mode (Enter/ESC)
- Command mode (colon commands)

### Phase 5: File Operations (2 hours)
- CSV reader/writer
- Command parser
- Command-line argument handling

### Phase 6: Integration & Testing (2 hours)
- Main application loop
- Error handling
- End-to-end testing

### Phase 7: Polish (1 hour)
- Performance optimization
- Documentation
- Build configuration

**Total Estimated Time**: 16-18 hours

## File Structure

```
css/
├── README.md                    # User documentation (Japanese)
├── TECHNICAL_SPEC.md           # Technical specifications
├── ARCHITECTURE.md             # Architecture details
├── IMPLEMENTATION_ROADMAP.md   # Step-by-step guide
├── PLAN_SUMMARY.md            # This file
└── CssApp/                    # Source code (to be created)
    ├── CssApp.csproj
    ├── Program.cs
    ├── Models/
    ├── Core/
    ├── UI/
    ├── IO/
    └── Utils/
```

## Key Features

### 1. Formula System
```
=A1+B2*3          # Arithmetic with precedence
=SUM(A1:A10)      # Range functions
=IF(A1>10,"H","L") # Conditional logic
=(A1+B1)/2        # Parentheses
```

### 2. User Interface
```
┌─────┬──────────┬──────────┬──────────┐
│     │    A     │    B     │    C     │  ← Fixed header
├─────┼──────────┼──────────┼──────────┤
│  1  │   100    │   200    │   300    │  ← Data rows
│  2  │   =A1+B1 │   =B1*2  │   =C1/3  │
└─────┴──────────┴──────────┴──────────┘
Status: A1 | Navigation Mode | data.csv
```

### 3. Command Mode
```
:w data.csv    # Save to file
:o data.csv    # Open file
:n             # New spreadsheet
:width 15      # Set column width
:wq            # Save and quit
```

## Technical Decisions

### Why Dictionary-based Storage?
- Memory efficient for sparse grids
- Fast random access
- Only stores non-empty cells

### Why AST for Formulas?
- Proper operator precedence
- Easy to extend with new operators/functions
- Clear error handling

### Why Vim-style Commands?
- Familiar to power users
- No need for complex menu system
- Efficient keyboard-only operation

### Why Self-contained Deployment?
- No .NET Runtime installation required
- Single executable file
- Easy distribution

## Success Criteria

- [ ] Application builds successfully
- [ ] Executable is self-contained (includes .NET Runtime)
- [ ] Can open/save CSV files
- [ ] All formulas evaluate correctly
- [ ] All functions work (SUM, AVERAGE, MIN, MAX, IF)
- [ ] Navigation works smoothly
- [ ] Edit mode functions properly
- [ ] Command mode executes all commands
- [ ] Column width adjustment works
- [ ] No crashes on edge cases
- [ ] Performance is acceptable (< 1s for typical operations)

## Risk Mitigation

### Potential Issues
1. **Terminal compatibility**: Different terminals may render differently
   - *Mitigation*: Test on Command Prompt and Windows Terminal
   
2. **Formula complexity**: Complex nested formulas may be slow
   - *Mitigation*: Implement caching and lazy evaluation
   
3. **Large files**: 1024 rows × 702 columns may be memory-intensive
   - *Mitigation*: Use sparse storage, only load visible cells
   
4. **Circular references**: May cause infinite loops
   - *Mitigation*: Implement dependency tracking and detection

## Next Steps

1. **Review this plan** - Ensure all requirements are covered
2. **Switch to Code mode** - Begin implementation
3. **Follow roadmap** - Implement phase by phase
4. **Test incrementally** - Test each component as it's built
5. **Build and deploy** - Create self-contained executable

## Questions for Review

1. Is the command mode interface (`:w`, `:o`, etc.) acceptable?
2. Are the included functions (SUM, AVERAGE, MIN, MAX, IF) sufficient?
3. Is the column width adjustment method (`:width N`) satisfactory?
4. Any additional features needed before implementation?

---

**Ready to proceed?** If this plan looks good, we can switch to Code mode and start building!