### Core Application
- WPF desktop application for editing LSR XML files
- Folder based workflow with automatic XML discovery
- Supports working with multiple XML files in a single session
- Clear separation between UI, XML parsing, and save logic

### File Management
- Open a folder and automatically load supported LSR XML files
- Folder View to browse XML files by filename
- Flat View to browse all entries across all loaded XMLs
- Switch between XML files without restarting the app
- Save
- Save As
- Automatic timestamped backups before every save
- Restore any XML from backup history

### XML Editing
- Raw XML editor
  - Syntax highlighted XML view
  - Manual editing support
  - Format XML (Ctrl+K)
  - Validate XML structure (Ctrl+Shift+V)
- Friendly View editor
  - Structured editing without touching raw XML
  - Edit attributes and values safely
  - Add new entries
  - Delete entries
  - Duplicate entries
  - Duplicate entire blocks
  - Supports nested and repeating elements
- Changes preserve original XML structure and formatting

### Editing Tools
- Duplicate selected entry (Ctrl+D)
- Duplicate entire block including child nodes (Ctrl+Shift+D)
- Edit tracking for modified entries
- Saved Edits list to review what has been changed
- Edit Summary export for review or sharing

### Search
- Find within the currently open XML (Ctrl+F)
- Global Search across all loaded XML files (Ctrl+Shift+F)
- Jump directly to matching entries from search results

### Import and Export
- Import Shared Config Packs
- Export Shared Config Packs
- Export Edit Summary for documentation or review

### Safety and Validation
- XML validation before saving
- Prevents saving malformed XML
- Automatic backups to avoid data loss
- Clear error messages for invalid edits

### Updates and Maintenance
- Built in Check for Updates using GitHub Releases
