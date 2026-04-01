# Command-Line Spreadsheet (CSS) - Technical Specification

## Project Overview
A terminal-based spreadsheet application for Windows 64-bit that runs in Command Prompt, built with .NET 10.0.

## Requirements Summary

### 1. Platform & Runtime
- **Target**: .NET 10.0 SDK
- **Deployment**: Self-contained with embedded .NET Runtime
- **Platform**: Windows 64-bit
- **Executable**: `css.exe`

### 2. Grid Specifications
- **Columns**: A to ZZ (702 columns)
- **Rows**: 1 to 1024
- **Display**: Scrollable viewport with fixed column/row headers
- **Default Column Width**: 10 characters (adjustable per column)

### 3. Cell Features
- **Data Types**: Text, Numbers, Formulas
- **Formula Prefix**: `=` (e.g., `=A1+B2`)
- **Arithmetic Operations**: `+`, `-`, `*`, `/`, `()`
- **Functions**: 
  - `SUM(range)` - Sum of cells
  - `AVERAGE(range)` - Average of cells
  - `MIN(range)` - Minimum value
  - `MAX(range)` - Maximum value
  - `IF(condition, true_value, false_value)` - Conditional logic
- **Cell References**: Single cells (A1) and ranges (A1:B10)

### 4. User Interface

#### Navigation Mode (Default)
- **Arrow Keys**: Move between cells (Up, Down, Left, Right)
- **Page Up/Down**: Scroll viewport vertically
- **Home/End**: Jump to first/last column in current row
- **Enter**: Enter edit mode for current cell
- **:**: Enter command mode

#### Edit Mode
- **Enter**: Confirm and save cell content
- **ESC**: Cancel editing and restore original value
- **Typing**: Edit cell content

#### Command Mode
Commands are entered by typing `:` followed by the command:
- `:w [filename]` - Save to CSV file (current file if no name specified)
- `:o filename` - Open CSV file
- `:n` - Create new spreadsheet
- `:width N` - Set width of current column to N characters
- `:q` - Quit application
- `:wq [filename]` - Save and quit

### 5. File Operations
- **Format**: CSV (Comma-Separated Values)
- **Encoding**: UTF-8
- **Formula Storage**: Store formulas as-is in CSV (e.g., `=A1+B2`)
- **Command-line**: `css.exe [filename.csv]` - Open file on startup

## Architecture Design

### Component Structure

```
css.exe
├── Program.cs (Entry point, main loop)
├── Models/
│   ├── Cell.cs (Cell data: value, formula, display)
│   ├── Worksheet.cs (Grid management, cell collection)
│   └── ColumnInfo.cs (Column width settings)
├── Core/
│   ├── FormulaParser.cs (Parse formula strings)
│   ├── FormulaEvaluator.cs (Evaluate formulas)
│   └── FunctionLibrary.cs (SUM, AVERAGE, etc.)
├── UI/
│   ├── ConsoleRenderer.cs (Draw grid to console)
│   ├── InputHandler.cs (Keyboard input processing)
│   └── Viewport.cs (Scrolling and visible area)
├── IO/
│   ├── CsvReader.cs (Load CSV files)
│   ├── CsvWriter.cs (Save CSV files)
│   └── CommandParser.cs (Parse colon commands)
└── Utils/
    ├── CellReference.cs (Parse A1, B2:C10 notation)
    └── ColumnHelper.cs (Convert A-ZZ to index)
```

### Data Flow

```
User Input → InputHandler → State Update → FormulaEvaluator → ConsoleRenderer
                                ↓
                          Worksheet (Data Model)
                                ↓
                          CsvReader/Writer
```

### Formula Evaluation Strategy

1. **Dependency Graph**: Track cell dependencies to detect circular references
2. **Lazy Evaluation**: Recalculate only affected cells when a cell changes
3. **Error Handling**: Display `#ERROR`, `#REF!`, `#DIV/0!` for invalid formulas

### Console Rendering Strategy

```
┌─────┬──────────┬──────────┬──────────┐
│     │    A     │    B     │    C     │  ← Fixed header row
├─────┼──────────┼──────────┼──────────┤
│  1  │   100    │   200    │   300    │  ← Data rows
│  2  │   =A1+B1 │   =B1*2  │   =C1/3  │
│  3  │          │          │          │
└─────┴──────────┴──────────┴──────────┘
  ↑
Fixed column
```

- **Active Cell**: Highlighted with different background color
- **Edit Mode**: Show cursor and current input
- **Status Bar**: Display current cell, mode, and file name

## Implementation Phases

### Phase 1: Core Infrastructure
- Project setup with .NET 10.0
- Basic data models (Cell, Worksheet)
- Console rendering framework

### Phase 2: UI & Navigation
- Grid display with fixed headers
- Viewport and scrolling
- Keyboard navigation

### Phase 3: Cell Editing
- Edit mode implementation
- Input handling (Enter/ESC)
- Display updates

### Phase 4: Formula Engine
- Formula parser (tokenizer, AST)
- Expression evaluator
- Cell reference resolution

### Phase 5: Functions
- Implement SUM, AVERAGE, MIN, MAX
- Implement IF function
- Range parsing (A1:B10)

### Phase 6: File Operations
- CSV reader/writer
- Command mode parser
- Command-line argument handling

### Phase 7: Polish & Deployment
- Column width adjustment
- Error handling
- Self-contained build configuration
- Testing and documentation

## Build Configuration

### Project File (.csproj)
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
  </PropertyGroup>
</Project>
```

### Build Command
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Testing Strategy

### Unit Tests
- Formula parser correctness
- Function implementations
- Cell reference resolution
- CSV read/write operations

### Integration Tests
- End-to-end formula evaluation
- File operations with sample CSV files
- Command parsing

### Manual Tests
- UI rendering in different terminal sizes
- Keyboard navigation
- Edit mode behavior
- Large spreadsheet performance (1024 rows)

## Performance Considerations

- **Lazy Rendering**: Only render visible cells in viewport
- **Incremental Updates**: Redraw only changed portions
- **Formula Caching**: Cache evaluated results until dependencies change
- **Memory Management**: Use efficient data structures for 702×1024 grid

## Error Handling

- **File Errors**: Display error message, don't crash
- **Formula Errors**: Show error in cell, continue operation
- **Invalid Commands**: Show error message in status bar
- **Circular References**: Detect and display `#CIRCULAR!`

## Future Enhancements (Out of Scope)

- Undo/Redo functionality
- Copy/Paste operations
- Cell formatting (bold, colors)
- Additional functions (VLOOKUP, etc.)
- Multi-sheet support
- Mouse support