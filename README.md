### Core Application V1.4.2

- WPF desktop application for editing LSR XML files
- Folder based workflow with automatic XML discovery
- Supports working with multiple XML files in a single session
- Clear separation between UI, XML parsing, and save logic
- Community LSR XML Guides: users can create and bundle guides

### File Management
- Open a folder and automatically load supported LSR XML files
- Folder View to browse XML files by filename
- Flat View to browse all entries across all loaded XMLs
- Automatic timestamped backups before every save
- Restore any XML from backup history

### XML Editing
- Raw XML editor for manual editing
  - Syntax highlighted XML view
  - Format XML 
  - Validate XML structure 
- Raw XML editor upgrades
  - Inline error highlighting with tooltips
  - Hierarchy visuals (nested shading, indent guides, active region highlight)
  - Folding
  - Breadcrumb navigation
  - Outline tree panel
- Friendly View editor
  - Structured editing without touching raw XML
  - Edit attributes and values safely
  - Bulk editing support (hold Ctrl and click multiple values)
  - Supports nested and repeating elements
- Changes preserve original XML structure and formatting

### Editing Tools
- Duplicate selected entry (Ctrl+D)
- Duplicate entire block including child nodes (Ctrl+Shift+D)
- Edit tracking for modified entries
- Saved Edits list to review what has been changed (updates live while open)
- Asynchronous status evaluation to prevent UI freezes
- Clear status indicators while edits are being checked
- Edit Summary export for review or sharing

### Search
- Find within the currently open XML (Ctrl+F)
- Global Search across all loaded XML files (Ctrl+Shift+F)
- Works in Raw XML, Friendly View, or both
- Include subfolders toggle to control folder scan depth
- Optional parallel processing for Friendly View searches
  - Faster multi-worker search mode
  - Accurate single-threaded mode for precise current file tracking
- Jump directly to matching entries from search results


### XML Compare
- Compare two XML files side by side
- Drag and drop an XML from anywhere to compare against loaded files
- Clean, readable list of detected differences
- Detect differences and cherry pick exactly which changes to apply
- Apply selected changes directly to XML instantly
- Import selected changes into Saved Edits for tracking
- Merge older configs into a clean updated setup
- Export config packs containing multiple XML configs and edits
- Export multi XML config packs created via XML Compare

### Import and Export
- Import Shared Config Packs
- Export Shared Config Packs
- Export multi XML config packs created via XML Compare
- Export Edit Summary for documentation or review

### Safety and Validation
- XML validation before saving
- Prevents saving malformed XML
- Automatic backups to avoid data loss
- Clear error messages for invalid edits

### Updates and Maintenance
- Automatic update check on startup


