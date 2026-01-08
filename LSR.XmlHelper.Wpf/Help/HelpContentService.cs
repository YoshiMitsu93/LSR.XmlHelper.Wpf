using LSR.XmlHelper.Wpf.Models;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Services.Help
{
    public sealed class HelpContentService
    {
        public IReadOnlyList<HelpTopic> GetTopics()
        {
            return new List<HelpTopic>
            {
                new HelpTopic(
                    1,
                    "Getting Started",
                    1,
                    "What this app is for",
                    "A beginner-friendly overview of what LSR XML Helper does and when to use it.",
                    "LSR XML Helper is built to help you safely edit Los Santos RED (LSR) XML files.\n\n" +
                    "It gives you two main editing styles:\n" +
                    "1) Raw XML editing (advanced, but precise)\n" +
                    "2) Friendly View editing (guided, safer, and easier)\n\n" +
                    "The app also protects you with backups, validation, and saved edit history.\n\n" +
                    "If you are new:\n" +
                    "- Prefer Friendly View whenever it’s available\n" +
                    "- Use Validate often\n" +
                    "- Use Restore from Backup if something breaks",
                    "overview", "beginner", "raw", "friendly", "backup", "validate"
                ),

                new HelpTopic(
                    1,
                    "Getting Started",
                    2,
                    "Open Folder and loading XML files",
                    "How folder scanning works, what 'Include subfolders' means, and what gets loaded.",
                    "Open Folder scans the folder you choose and finds XML files.\n\n" +
                    "Steps:\n" +
                    "1) File > Open Folder… (Ctrl+O)\n" +
                    "2) Choose the folder containing your LSR XML files\n" +
                    "3) Click an XML in the left list to load it\n\n" +
                    "Include subfolders:\n" +
                    "- OFF: only scans the folder you selected\n" +
                    "- ON: scans all nested folders too\n\n" +
                    "Tip:\n" +
                    "If you see 'no files found', double-check you selected the folder that actually contains the XMLs (not the game folder root).",
                    "open folder", "scan", "include subfolders", "load"
                ),

                new HelpTopic(
                    2,
                    "Views",
                    1,
                    "Folder view vs Flat view",
                    "Two ways to browse your XML files depending on how your folders are organized.",
                    "Folder view:\n" +
                    "- Shows the folder structure\n" +
                    "Flat view:\n" +
                    "- Shows every XML file in a single list\n" +
                    "- Best when you want quick searching + scrolling\n\n" +
                    "Switching views does not change files on disk. It’s only how the list is displayed.",
                    "folder view", "flat view", "browse"
                ),

                new HelpTopic(
                    3,
                    "Editing (Raw XML)",
                    1,
                    "Format (Ctrl+K)",
                    "Pretty-prints XML so it’s easier to read and reduces accidental editing mistakes.",
                    "What it does:\n" +
                    "- Re-indents and reflows the XML into a clean readable layout\n" +
                    "- Does not change the meaning of the XML\n\n" +
                    "When to use it:\n" +
                    "- After pasting chunks of XML\n" +
                    "- After making many edits\n" +
                    "- Before comparing changes\n\n" +
                    "Example:\n" +
                    "Before:\n" +
                    "<Root><Item><Name>Test</Name></Item></Root>\n\n" +
                    "After:\n" +
                    "<Root>\n" +
                    "  <Item>\n" +
                    "    <Name>Test</Name>\n" +
                    "  </Item>\n" +
                    "</Root>\n\n" +
                    "Tip:\n" +
                    "If your XML is broken (missing closing tags), Format might fail or produce unexpected results. Validate first if you’re unsure.",
                    "format", "pretty", "indent", "ctrl+k"
                ),

                new HelpTopic(
                    3,
                    "Editing (Raw XML)",
                    2,
                    "Validate (Ctrl+Shift+V)",
                    "Checks that the XML is well-formed so the game/mod won’t crash from broken structure.",
                    "What it does:\n" +
                    "- Verifies the XML is well-formed (proper nesting, closing tags, valid characters)\n" +
                    "- It does not guarantee the mod will accept the data logically, but it catches the most common 'XML is broken' problems\n\n" +
                    "When to use it:\n" +
                    "- After any manual editing\n" +
                    "- Before saving when you made large changes\n" +
                    "- When the game/mod crashes after edits\n\n" +
                    "Common errors it catches:\n" +
                    "- Missing closing tag: <Item> ... (no </Item>)\n" +
                    "- Wrong nesting: closing tags in the wrong order\n" +
                    "- Invalid characters (rare, but can happen if you paste from weird sources)\n\n" +
                    "Beginner rule:\n" +
                    "If you only remember one shortcut: Validate before you Save.",
                    "validate", "well-formed", "ctrl+shift+v", "error"
                ),

                new HelpTopic(
                    3,
                    "Editing (Raw XML)",
                    3,
                    "Save, Save As, and backups",
                    "Saving creates backups automatically so you can recover if something goes wrong.",
                    "Save:\n" +
                    "- Writes your changes to the current file\n" +
                    "- Creates a backup copy automatically (so you can restore later)\n\n" +
                    "Save As:\n" +
                    "- Saves your edited XML to a different file path\n" +
                    "- Useful for testing changes without overwriting the original\n\n" +
                    "Backups:\n" +
                    "- The app keeps backups so you can roll back\n" +
                    "- If you break something, restore a backup instead of trying to manually undo everything\n\n" +
                    "Tip:\n" +
                    "If you’re experimenting, use Save As to create a 'test' version first.",
                    "save", "save as", "backup", "restore"
                ),

                new HelpTopic(
                    3,
                    "Editing (Raw XML)",
                    4,
                    "Replace (Ctrl+H)",
                    "Replace text in Raw XML, including Replace All.",
                    "Replace lets you quickly swap text in the Raw XML editor.\n\n" +
                    "Open Replace:\n" +
                    "- Press Ctrl+H\n" +
                    "- Or use Edit > Replace...\n\n" +
                    "Replace workflow:\n" +
                    "1) Type the text you want to find\n" +
                    "2) Type the text to replace it with\n" +
                    "3) Use Find Next to step through matches\n" +
                    "4) Use Replace to replace the current match\n" +
                    "5) Use Replace All to replace every match in the current Raw XML\n\n" +
                    "Options:\n" +
                    "- Match case: case-sensitive matching\n" +
                    "- Wrap around: continues from the top after reaching the end\n\n" +
                    "Notes:\n" +
                    "- Replace works only in Raw XML\n" +
                    "- Friendly View has bulk editing for field values instead\n\n" +
                    "Tip:\n" +
                    "If you are unsure about a Replace All, use Find Next and Replace first to confirm you are hitting the right text.",
                    "replace", "ctrl+h", "replace all", "raw xml", "search", "find"
                ),

                new HelpTopic(
                    4,
                    "Friendly View",
                    1,
                    "What Friendly View is",
                    "A structured editor that reduces raw XML mistakes by guiding you through fields.",
                    "Friendly View is a structured editor for supported XML formats.\n\n" +
                    "Instead of manually editing raw tags, you:\n" +
                    "- Pick a category/group in the middle pane\n" +
                    "- Pick an entry\n" +
                    "- Edit fields in the right pane\n\n" +
                    "Why it’s safer:\n" +
                    "- It reduces the chance you break tag nesting\n" +
                    "- It focuses you on editable values\n\n" +
                    "If Friendly View is available, beginners should use it whenever possible.",
                    "friendly", "structured", "pane 2", "pane 3"
                ),

                new HelpTopic(
                    4,
                    "Friendly View",
                    2,
                    "Add / Delete entries and sub-entries",
                    "How the right-click actions work, and what to watch out for when deleting.",
                    "Right-click actions exist for both entries and sub-entries.\n\n" +
                    "Add:\n" +
                    "- Adds a new entry/sub-entry in the correct structure\n\n" +
                    "Delete:\n" +
                    "- Prompts for confirmation\n" +
                    "- Uses safety steps (including backups/edit history depending on your flow)\n\n" +
                    "Beginner tip:\n" +
                    "If you’re unsure, duplicate first, then modify the duplicate. That way you preserve a working example.",
                    "add entry", "delete entry", "sub-entry", "context menu"
                ),

                new HelpTopic(
                    4,
                    "Friendly View",
                    3,
                    "Duplicate (Ctrl+D) and Duplicate Block (Ctrl+Shift+D)",
                    "The fastest safe workflow: duplicate something that works, then edit it.",
                    "Duplicate Entry (Ctrl+D):\n" +
                    "- Duplicates the selected Friendly entry\n\n" +
                    "Duplicate Block / Item (Ctrl+Shift+D):\n" +
                    "- Duplicates a selected list item/block in places where that applies\n\n" +
                    "Why duplication is powerful:\n" +
                    "- You keep the correct structure\n" +
                    "- You only change values you understand\n" +
                    "- It reduces the chance of missing required fields\n\n" +
                    "Beginner workflow:\n" +
                    "Duplicate → Rename/Adjust values → Validate → Save",
                    "duplicate", "ctrl+d", "ctrl+shift+d"
                ),

                new HelpTopic(
                    4,
                    "Friendly View",
                    4,
                    "Bulk edit fields (multi-select)",
                    "Edit multiple field values at the same time in Friendly View.",
                    "Bulk editing lets you change the same field value across multiple entries at once.\n\n" +
                    "How to use it:\n" +
                    "1) Go to Friendly View\n" +
                    "2) Select an entry so fields appear in the right pane\n" +
                    "3) Hold Ctrl and click multiple fields with the same name\n" +
                    "4) Click into the Value column and start typing\n\n" +
                    "What happens:\n" +
                    "- All selected fields update live as you type\n" +
                    "- You do not need to press Enter to see changes\n\n" +
                    "Cancel behavior:\n" +
                    "- Press Escape to revert all selected fields to their original values\n\n" +
                    "Important notes:\n" +
                    "- Bulk edit only applies to the Value column\n" +
                    "- Fields must be compatible (same field type)\n" +
                    "- Bulk editing does not change structure, only values\n\n" +
                    "Tip:\n" +
                    "This is ideal for renaming, adjusting repeated values, or syncing parameters across entries quickly.",
                    "bulk edit", "multi select", "ctrl click", "friendly view", "pane 3"
                ),

                new HelpTopic(
                    5,
                    "Search",
                    1,
                    "Find in current XML (Ctrl+F)",
                    "Search inside the current XML in Raw XML or Friendly View.",
                    "Use Ctrl+F to search within the current XML.\n\n" +
                    "It works in:\n" +
                    "- Raw XML\n" +
                    "- Friendly View\n\n" +
                    "Good for:\n" +
                    "- Finding a specific tag, field, or value\n" +
                    "- Jumping to a known identifier\n\n" +
                    "Tip:\n" +
                    "If you don’t know which file contains something, use Global Search instead.",
                    "find", "ctrl+f", "local", "friendly", "raw"
                ),

                new HelpTopic(
                    5,
                    "Search",
                    2,
                    "Global Search (Ctrl+Shift+F)",
                    "Search Raw XML, Friendly View, or Both across your loaded folder.",
                    "Global Search scans XML files in your opened folder.\n\n" +
                    "Search mode:\n" +
                    "- Raw XML: scans text only (fast)\n" +
                    "- Friendly View: parses fields (slower)\n" +
                    "- Both: runs both scans (slowest)\n\n" +
                    "Include subfolders:\n" +
                    "- OFF: only scans the selected folder\n" +
                    "- ON: scans nested folders too (slower on huge trees)\n\n" +
                    "Friendly View only: Use parallel processing:\n" +
                    "- ON: faster, but current-file label shows the last started file\n" +
                    "- OFF: slower, but current-file label stays exact\n\n" +
                    "Double-click a result to open and jump directly to it.",
                    "global search", "ctrl+shift+f", "search all", "friendly", "raw", "parallel", "include subfolders"
                ),

                new HelpTopic(
                    6,
                    "Compare",
                    1,
                    "XML Compare",
                    "Compare two XML files and selectively import exactly what you want.",
                    "XML Compare lets you load your folder, then browse or drag and drop an XML from anywhere to compare.\n\n" +
                    "It breaks differences into a clean list so you can cherry pick changes.\n\n" +
                    "You can:\n" +
                    "- Import selected changes into Saved Edits so everything is tracked\n" +
                    "- Apply selected changes directly to your XML instantly\n" +
                    "- Merge older configs into one clean setup\n" +
                    "- Export config packs containing multiple XML configs, alongside any other changes you choose\n\n" +
                    "This makes migrating old configs painless and gives you full visibility and control over what actually changed.",
                    "compare", "xml compare", "merge", "import", "saved edits", "config pack", "export"
                ),

                new HelpTopic(
                    6,
                    "Import / Export",
                    1,
                    "Shared Config Packs (Share XML configs safely)",
                    "Package up your LSR XML changes so other people can import them easily and safely.",
                    "Shared Config Packs are for sharing LSR configs with other people.\n\n" +
                    "If you make configs:\n" +
                    "- You can export a pack and send it to others\n" +
                    "- They can import it without manually editing raw XML\n" +
                    "- It keeps things consistent and reduces mistakes\n\n" +
                    "Why this is a game changer:\n" +
                    "- LSR is huge and configs can take ages to build\n" +
                    "- Sharing raw XML instructions is error-prone\n" +
                    "- Packs make distribution simple: export → share → import\n\n" +
                    "Best practice for creators:\n" +
                    "1) Make your changes\n" +
                    "2) Validate and Save\n" +
                    "3) Export a Shared Config Pack\n" +
                    "4) Include a short description of what it changes (so users know what they’re importing)\n\n" +
                    "For people importing:\n" +
                    "- Import the pack\n" +
                    "- Validate after import if you’re unsure\n" +
                    "- If something goes wrong, Restore from Backup\n\n" +
                    "Note:\n" +
                    "Appearance settings can also be included, but the main value is sharing XML configs and saved edits safely.",
                    "shared config", "pack", "import", "export", "share", "config", "xml"
                ),

                new HelpTopic(
                    6,
                    "Import / Export",
                    2,
                    "Edit Summary Export",
                    "Export readable changelogs so you can track what you changed and why.",
                    "Edit Summary Export creates a readable summary of changes.\n\n" +
                    "Good for:\n" +
                    "- Remembering what you edited\n" +
                    "- Sharing changes with others\n" +
                    "- Keeping notes before/after mod updates\n\n" +
                    "Beginner habit:\n" +
                    "Export summaries after big sessions. Future-you will thank you.",
                    "edit summary", "export", "changelog"
                ),

                new HelpTopic(
                    7,
                    "Saved Edits & History",
                    1,
                    "Saved Edits: what it is",
                    "A system to track edits and re-apply them later (especially after updates or clean installs).",
                    "Saved Edits stores changes you’ve made in a way you can review and apply later.\n\n" +
                    "Why it exists:\n" +
                    "- You can keep a list of changes you want, even if the original files update\n" +
                    "- You can apply changes selectively instead of redoing everything manually\n\n" +
                    "Window:\n" +
                    "- Edit: Saved Edits…\n" +
                    "- Shows pending vs committed edits\n\n" +
                    "Beginner mindset:\n" +
                    "Think of it like a checklist of modifications you can reuse.",
                    "saved edits", "history", "pending", "committed"
                ),

                new HelpTopic(
                    8,
                    "Backups",
                    1,
                    "Restore from Backup",
                    "Safely restore an older version of an XML file (with safety backup before overwrite).",
                    "How it works:\n" +
                    "- File > Restore from Backup\n" +
                    "- You pick a backup XML\n" +
                    "- The app makes a safety backup of your current file before overwriting it\n" +
                    "- Then it restores the chosen backup and reloads it\n\n" +
                    "When to use it:\n" +
                    "- You saved a broken edit\n" +
                    "- The game/mod crashes after changes\n" +
                    "- You want to undo a set of edits fast\n\n" +
                    "Beginner tip:\n" +
                    "Restoring is often faster and safer than trying to manually fix raw XML mistakes.",
                    "restore", "backup browser", "revert"
                ),

                new HelpTopic(
                    9,
                    "Updates",
                    1,
                    "Check for Updates",
                    "Checks GitHub for the latest release so you can stay current.",
                    "Check for Updates:\n" +
                    "- Queries the GitHub releases for this app\n" +
                    "- Tells you if a newer version is available\n\n" +
                    "Beginner tip:\n" +
                    "If something behaves differently after an update, review Saved Edits and this documentation first.",
                    "update", "github", "release"
                ),

                new HelpTopic(
                    10,
                    "Troubleshooting",
                    1,
                    "If the game/mod crashes after edits",
                    "A simple recovery checklist for beginners.",
                    "Recovery checklist:\n" +
                    "1) Restore from Backup (fastest)\n" +
                    "2) Validate the XML\n" +
                    "3) If you used Raw editing, Format then Validate\n" +
                    "4) Try Save As to test changes separately\n\n" +
                    "Beginner tip:\n" +
                    "Most crashes caused by editing are from broken XML structure or missing required values. Restore + reapply changes more carefully.",
                    "crash", "broken", "restore", "validate"
                )
            };
        }
    }
}
