### Core Application V1.0.1
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
- Asynchronous status evaluation to prevent UI freezes
- Clear status indicators while edits are being checked
- Edit Summary export for review or sharing

### Search
- Find within the currently open XML (Ctrl+F)
- Global Search across all loaded XML files (Ctrl+Shift+F)
- Works in Raw XML, Friendly View, or both
- Include subfolders toggle to control folder scan depth
- Optional parallel processing for Friendly View searches
  - Faster multi worker search mode
  - Accurate single threaded mode for precise current file tracking
- Jump directly to matching entries from search results
- Tooltips explain performance versus accuracy tradeoffs

### XML Compare
- Compare two XML files side by side
- Drag and drop an XML from anywhere to compare against loaded files
- Clean, readable list of detected differences
- Cherry pick exactly which changes to apply
- Import selected changes into Saved Edits for tracking
- Apply selected changes directly to XML instantly
- Merge older configs into a clean updated setup
- Export config packs containing multiple XML configs and edits

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
- Built in Check for Updates using GitHub Releases

Release/EXE: https://github.com/YoshiMitsu93/LSR.XmlHelper.Wpf/releases/tag/v1.0.1  

VirusTotal: https://www.virustotal.com/gui/file-analysis/ODU0MzBjNjdmMDdlMDUxMmYwMjYyZTlkNzUzYWE2NTc6MTc2Nzc1NDM1MQ==
