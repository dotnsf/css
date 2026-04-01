# Implementation Roadmap

This document provides a step-by-step guide for implementing the CSS (Command-Line Spreadsheet) application.

## Phase 1: Project Setup & Core Models

### Step 1.1: Create .NET Project
```bash
cd css
dotnet new console -n CssApp -f net10.0
cd CssApp
```

### Step 1.2: Configure Project File
Edit `CssApp.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <PublishSingleFile>true</PublishSingleFile>
    <AssemblyName>css</AssemblyName>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
    <PublishTrimmed>false</PublishTrimmed>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### Step 1.3: Create Directory Structure
```
CssApp/
├── Program.cs
├── Models/
│   ├── Cell.cs
│   ├── CellAddress.cs
│   ├── Worksheet.cs
│   └── ColumnInfo.cs
├── Core/
│   ├── FormulaParser.cs
│   ├── FormulaEvaluator.cs
│   ├── FunctionLibrary.cs
│   └── DependencyTracker.cs
├── UI/
│   ├── ConsoleRenderer.cs
│   ├── Viewport.cs
│   ├── InputHandler.cs
│   └── StatusBar.cs
├── IO/
│   ├── CsvReader.cs
│   ├── CsvWriter.cs
│   └── CommandParser.cs
└── Utils/
    └── ColumnHelper.cs
```

### Step 1.4: Implement Core Models

**Priority Order:**
1. `CellAddress.cs` - Cell reference parsing (A1, B2, etc.)
2. `ColumnHelper.cs` - Column name conversion (A-ZZ ↔ 0-701)
3. `Cell.cs` - Cell data structure
4. `Worksheet.cs` - Grid management
5. `ColumnInfo.cs` - Column width settings

**Key Features:**
- CellAddress: Parse "A1", "ZZ1024", validate ranges
- Cell: Store raw value, formula, evaluated result, error state
- Worksheet: Dictionary-based storage for sparse grid
- ColumnInfo: Default width = 10, adjustable per column

## Phase 2: Formula Engine

### Step 2.1: Tokenizer
Create token types:
- Number: `123`, `45.67`
- Operator: `+`, `-`, `*`, `/`, `(`, `)`
- CellReference: `A1`, `B2`
- Function: `SUM`, `AVERAGE`, etc.
- Range: `A1:B10`
- String: `"text"`

### Step 2.2: Parser
Build Abstract Syntax Tree (AST):
- Respect operator precedence: `*`, `/` before `+`, `-`
- Handle parentheses
- Parse function calls with arguments
- Parse cell ranges

### Step 2.3: Evaluator
Evaluate AST:
- Resolve cell references from worksheet
- Execute arithmetic operations
- Call functions from FunctionLibrary
- Handle errors gracefully

### Step 2.4: Function Library
Implement functions:
1. `SUM(range)` - Sum all numeric values
2. `AVERAGE(range)` - Average of numeric values
3. `MIN(range)` - Minimum value
4. `MAX(range)` - Maximum value
5. `IF(condition, trueValue, falseValue)` - Conditional

### Step 2.5: Dependency Tracking
- Build dependency graph
- Detect circular references
- Implement topological sort for evaluation order

## Phase 3: Console UI

### Step 3.1: Basic Rendering
- Clear screen
- Draw grid borders
- Display cell values
- Show column headers (A, B, C, ...)
- Show row numbers (1, 2, 3, ...)

### Step 3.2: Viewport Management
- Calculate visible area based on terminal size
- Implement scrolling (horizontal and vertical)
- Keep headers fixed while scrolling
- Track current cell position

### Step 3.3: Cell Highlighting
- Highlight active cell with different background
- Show cursor position in edit mode
- Display cell coordinates in status bar

### Step 3.4: Status Bar
Display:
- Current cell address (e.g., "A1")
- Current mode (Navigation/Edit/Command)
- Current file name
- Error messages

## Phase 4: Input Handling

### Step 4.1: Navigation Mode
Handle keys:
- Arrow keys: Move between cells
- Page Up/Down: Scroll viewport
- Home/End: Jump to row start/end
- Enter: Switch to edit mode
- `:`: Switch to command mode

### Step 4.2: Edit Mode
Handle keys:
- Character input: Update cell content
- Backspace: Delete character
- Enter: Confirm and save
- ESC: Cancel and restore original value

### Step 4.3: Command Mode
Handle keys:
- Character input: Build command string
- Backspace: Delete character
- Enter: Execute command
- ESC: Cancel command mode

## Phase 5: File Operations

### Step 5.1: CSV Reader
- Parse CSV format
- Handle quoted fields
- Detect formulas (starting with `=`)
- Support UTF-8 encoding
- Handle empty cells

### Step 5.2: CSV Writer
- Format cells as CSV
- Quote fields containing commas
- Preserve formulas
- Use UTF-8 encoding
- Handle special characters

### Step 5.3: Command Parser
Implement commands:
- `:w [filename]` - Save
- `:o filename` - Open
- `:n` - New
- `:width N` - Set column width
- `:q` - Quit
- `:wq [filename]` - Save and quit

### Step 5.4: Command-line Arguments
- Parse `css.exe filename.csv`
- Load file on startup
- Handle file not found errors

## Phase 6: Integration & Testing

### Step 6.1: Main Application Loop
```
Initialize → Load File (if specified) → Main Loop → Save (if needed) → Exit
                                           ↓
                                    Render → Input → Update → Evaluate
```

### Step 6.2: Error Handling
- File I/O errors
- Formula parsing errors
- Circular reference detection
- Division by zero
- Invalid cell references

### Step 6.3: Testing Scenarios

**Formula Tests:**
```
=1+2*3 → 7
=A1+B1 → (depends on A1, B1)
=SUM(A1:A10) → (sum of range)
=IF(A1>10, "High", "Low") → (conditional)
```

**Navigation Tests:**
- Move to all corners of grid
- Scroll to edge cases
- Jump between distant cells

**File Tests:**
- Save and reload
- Handle large files
- Handle special characters
- Handle formulas in CSV

**Edge Cases:**
- Empty spreadsheet
- Single cell
- Maximum grid size (ZZ1024)
- Very long formulas
- Deeply nested functions

## Phase 7: Polish & Optimization

### Step 7.1: Performance
- Lazy evaluation (only recalculate changed cells)
- Efficient rendering (only redraw changed areas)
- Memory optimization (sparse storage)

### Step 7.2: User Experience
- Smooth scrolling
- Clear error messages
- Helpful status messages
- Responsive input handling

### Step 7.3: Build Configuration
```bash
# Development
dotnet build

# Release (self-contained)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

### Step 7.4: Documentation
- Update README with examples
- Add inline code comments
- Create user guide
- Document known limitations

## Implementation Order Summary

1. ✅ **Setup** (30 min)
   - Create project
   - Set up directory structure
   - Configure build settings

2. ✅ **Core Models** (2 hours)
   - CellAddress, ColumnHelper
   - Cell, Worksheet
   - Basic data structures

3. ✅ **Formula Engine** (4 hours)
   - Tokenizer and Parser
   - Evaluator
   - Function library
   - Dependency tracking

4. ✅ **Console UI** (3 hours)
   - Basic rendering
   - Viewport management
   - Status bar
   - Cell highlighting

5. ✅ **Input Handling** (2 hours)
   - Navigation mode
   - Edit mode
   - Command mode

6. ✅ **File Operations** (2 hours)
   - CSV reader/writer
   - Command parser
   - Command-line args

7. ✅ **Integration** (2 hours)
   - Main loop
   - Error handling
   - Testing

8. ✅ **Polish** (1 hour)
   - Performance tuning
   - Documentation
   - Final testing

**Total Estimated Time: 16-18 hours**

## Development Tips

### Debugging
- Use `Console.WriteLine` for debugging (output to separate file)
- Test each component independently
- Use unit tests for formula engine
- Test with sample CSV files

### Common Pitfalls
- Off-by-one errors in cell indexing
- Incorrect operator precedence
- Circular reference infinite loops
- Terminal size changes during runtime
- Unicode handling in CSV files

### Best Practices
- Keep functions small and focused
- Use meaningful variable names
- Add comments for complex logic
- Handle all error cases
- Test edge cases thoroughly

## Next Steps

Once planning is complete, switch to **Code mode** to begin implementation following this roadmap.