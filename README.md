# Redliner - WPF Engineering Annotator

A C# WPF application for PDF, AutoCAD DXF, and DWG file annotation and redlining software.

## Project Structure

### Architecture
- **Framework**: C# WPF (.NET 8.0)
- **Pattern**: MVVM (Model-View-ViewModel)
- **Database**: SQLite with Entity Framework Core
- **UI Framework**: WPF with CommunityToolkit.Mvvm

### Folder Structure
```
Redliner/
├── Models/                 # Data models and entities
│   ├── Document.cs        # Document entity
│   ├── Annotation.cs      # Annotation entity
│   ├── UserPreference.cs  # User settings entity
│   └── Enums.cs          # Shared enumerations
├── ViewModels/            # MVVM ViewModels
│   ├── ViewModelBase.cs   # Base ViewModel class
│   └── MainViewModel.cs   # Main window ViewModel
├── Views/                 # WPF Views/Windows
├── Services/              # Business logic services
│   ├── DocumentService.cs # Document management
│   ├── AnnotationService.cs # Annotation operations
│   └── FileService.cs     # File handling
├── Data/                  # Database context and configurations
│   └── RedlinerDbContext.cs # EF Core context
├── Converters/            # Value converters for data binding
│   └── InvertedBooleanToVisibilityConverter.cs
└── Commands/              # Custom commands (future)
```

## Features Implemented

### Core Infrastructure
- ✅ MVVM architecture with CommunityToolkit.Mvvm
- ✅ SQLite database with Entity Framework Core
- ✅ Basic WPF UI layout with menu, toolbar, and panels
- ✅ Document management service
- ✅ Annotation management service
- ✅ File handling service foundation

### Database Schema
- **Documents**: Stores document metadata and file information
- **Annotations**: Stores annotation data with position, styling, and content
- **UserPreferences**: Stores application settings and user preferences

### UI Components
- Menu bar with File, Edit, Annotations, and View menus
- Toolbar with common annotation tools
- Three-panel layout:
  - Left: Document tree/navigation
  - Center: Document viewer with annotation canvas
  - Right: Annotation properties panel
- Status bar for application feedback

## Supported File Types
- PDF files (.pdf)
- AutoCAD DXF files (.dxf)
- AutoCAD DWG files (.dwg)

## Annotation Types
- Text annotations
- Rectangle shapes
- Circle shapes
- Arrow annotations
- Highlighting
- Freehand drawing (planned)
- Measurement tools (planned)

## Development Notes

### Building on Windows
This project requires the WPF workload which is only available on Windows. To build and run:

1. Install Visual Studio 2022 with the .NET desktop development workload
2. Or install .NET 8.0 SDK with Windows Desktop workload
3. Run `dotnet build` and `dotnet run`

### Database
The SQLite database is automatically created in the user's AppData folder:
- Location: `%APPDATA%\Redliner\redliner.db`
- Schema is created automatically on first run using EF Core migrations

### Dependencies
- **CommunityToolkit.Mvvm**: MVVM framework with code generators
- **Microsoft.EntityFrameworkCore.Sqlite**: Database ORM
- **PdfSharp**: PDF manipulation library
- **System.Data.SQLite**: Additional SQLite support

## Future Enhancements
- PDF rendering and display
- DXF/DWG file parsing and rendering
- Advanced annotation tools
- Layer management system
- Export/import functionality
- Undo/redo system
- Zooming and panning
- Print functionality
- Multi-document tabs