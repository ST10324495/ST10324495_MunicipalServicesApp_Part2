# Implementation Report: Municipal Services Portal

## 1. Project Summary
- **Author:** ST10323395
- **Module:** PROG7312 Portfolio of Evidence (POE) 

The application emphasises the **Service Request Status** dashboard, which integrates binary trees, balanced trees, heaps, and graph algorithms to keep municipal data organised, searchable, and responsive during live operations.

---

## 2. Prerequisites
| Requirement | Minimum | Recommended |
|-------------|---------|-------------|
| Operating system | Windows 10 | Windows 11 |
| .NET Framework | 4.7.2 Developer Pack | 4.8 Developer Pack |
| IDE | Visual Studio 2017 | Visual Studio 2022 |
| Hardware | 4 GB RAM, 1.5 GB free disk space | 8 GB RAM, 4 GB free disk space |

> **Tip:** Install the .NET 4.7.2 Developer Pack before opening the solution. Restart Visual Studio after installing prerequisites to avoid reference-load issues.

---

## 3. How to Compile and Run the Application

### 3.1 Visual Studio Workflow (Preferred)
1. **Clone or copy the project** to a local directory, for example `C:\Users\<you>\source\repos\ST10323395_MunicipalServicesApp`.
2. **Open the solution:** `File` → `Open` → `Project/Solution` → select `ST10323395_MunicipalServicesApp.sln`.
3. **Restore dependencies:** Visual Studio restores .NET references automatically. If prompted, accept the restore dialog.
4. **Build:** Press `Ctrl+Shift+B` or choose `Build → Build Solution`. Wait for the `Build succeeded` message in the Output window.
5. **Run / Debug:** 
   - Press `F5` to launch with debugging, or `Ctrl+F5` to run without the debugger.
   - The **MainMenuForm** appears. All feature forms (Report Issues, Service Request Status, Local Events) open within the main window.

### 3.2 Command Line Build
1. Open a **Developer Command Prompt for VS** (ships with Visual Studio).
2. Navigate to the solution root:
   ```
   cd C:\Users\<you>\source\repos\ST10323395_MunicipalServicesApp
   ```
3. Compile the Debug build:
   ```
   msbuild ST10323395_MunicipalServicesApp.sln /p:Configuration=Debug
   ```
4. Run the executable:
   ```
   ST10323395_MunicipalServicesApp\bin\Debug\ST10323395_MunicipalServicesApp.exe
   ```

### 3.3 Verifying the Build
- Ensure `ST10323395_MunicipalServicesApp.exe` appears in `bin\Debug` (or `bin\Release`).
- If the application fails to launch, confirm that antivirus tools have not quarantined the output and that no pending Windows updates require a restart.
- For missing framework errors, reinstall the .NET 4.7.2 Developer Pack and rebuild.

---

## 4. Navigating and Using the Application

1. **Launch:** Start the executable; the **MainMenuForm** displays navigation buttons for each feature.
2. **Report Issues Form:**
   - Enter location, category, and description details.
   - Submit to log the item inside `IssueRepository.Items`. A progress bar confirms success.
3. **View Issues Form:** Read-only grid that lists all captured issues with timestamps and attachment status.
4. **Service Request Status Dashboard (core POE artefact):**
   - TreeView visualises departments → subcategories → requests using `ServiceRequestTree`.
   - Search bar performs O(log n) BST lookups by request title.
   - Insight tabs showcase BST, AVL, Red-Black tree, Heap, and Graph analytics (detailed in Section 5).
   - Buttons refresh data, and the tab descriptions explain the complexity benefits to the marker.
5. **Local Events & Announcements:**
   - Uses additional custom collections to surface event recommendations and search history.
   - Demonstrates Part 2 structures outside the Service Request Status scope.

> **Demonstration hint:** Launch the Service Request Status form immediately after running the app to show seeded data powering each advanced structure.

---

## 5. Data Structures Driving "Service Request Status"

This section is written for the marker who wants to understand *why* each structure exists and *how* it actively improves the dashboard. I’m keeping the technical bits, but I’ve added context so it reads like a walkthrough rather than a textbook. If something already does the job, I’ve left it alone; the focus is on the parts where the richer story helps you see the value.

### 5.1 Custom Collections (Foundational Layer)
#### CustomList<T>
- **Role in feature:** All repositories, including `ServiceRequestRepository`, store requests inside `CustomList<ServiceRequest>` rather than `List<T>`. The insight tabs bind straight to these lists without intermediate conversion.
- **Why it matters:** The list grows dynamically, so when call-centre staff log another ticket the UI doesn’t stutter or pause. Append, index, and iterate stay at an amortised O(1), which keeps the dashboard flicker-free even when a flood of demo data loads on startup.
- **The lived example:** When the Search Results tab refreshes, it simply loops over a `CustomList<ServiceRequest>` and paints the items. There’s no hidden translation step that could get out of sync.

#### CustomDictionary<TKey,TValue> & CustomHashSet<T>
- **Role in feature:** `ServiceGraph` keeps its adjacency lists inside a `CustomDictionary`, and both BFS/DFS traversals rely on a `CustomHashSet` to remember what’s been visited.
- **Why it matters:** Call centre managers get instant feedback when they click the BFS tab because lookups stay O(1) on average. No duplicate edges sneak into the graph, so the traversal output reads cleanly.
- **The lived example:** When I add “Water Services ↔ John Smith” to the graph, the hash set spots any duplicate before the UI redraws. The BFS tab then streams a tidy sentence without repeated names.

#### CustomQueue<T> & CustomStack<T>
- **Role in feature:** The BFS sweep leans on `CustomQueue`, while the DFS trail uses `CustomStack` to avoid recursion limits.
- **Why it matters:** Each push/pop or enqueue/dequeue is O(1). Even if another department is added during marking, both traversals still feel instant.
- **The lived example:** Trigger the DFS tab and you’ll see paths like `Waste Management DFS → Naledi Jacobs → #3051`. That output is generated by repeatedly popping from `CustomStack` until the traversal is complete.

### 5.2 Tree Structures (Ordering and Balancing Layer)
#### ServiceRequestTree (General N-ary Tree)
- **Role in feature:** Holds the municipal hierarchy (departments → subcategories → requests) so the TreeView and summary tab can reuse a single source of truth.
- **Why it matters:** Traversal is plain O(n) with hardly any overhead, which suits a hierarchy that’s usually small but needs to be refreshed often.
- **The lived example:** `PopulateTreeView()` simply walks the `ServiceRequestTree`, and the on-screen indenting comes directly from that pass—there’s no fragile manual wiring.

#### ServiceRequestBST (Binary Search Tree)
- **Role in feature:** Indexes every request title alphabetically so both the search box and the “BST Pre-Order Map” tab can respond immediately.
- **Why it matters:** With O(log n) inserts and lookups, operators feel like they’re searching a well-organised cabinet instead of a messy file stack. When a title isn’t a direct hit, the fallback in-order traversal still runs quickly.
- **The lived example:** Type “Transformer” into the search bar. The repository hits the BST first; if it doesn’t find a perfect match, it traverses alphabetically and still shows the closest entries.

#### ServiceRequestAVLTree (Self-balancing BST by Priority)
- **Role in feature:** Stores requests by urgency so the “AVL Priority” tab and any escalated-work summaries can pull the hottest items first.
- **Why it matters:** Rotations keep the tree height balanced, so even if you reclassify several tickets as “critical” during a demo, inserts stay O(log n) and the top list refreshes without delay.
- **The lived example:** The tab showing “Priority 5 – Transformer failure” comes straight out of `GetHighPriorityRequests`, which traverses the AVL tree in-order.

#### ServiceRequestRedBlackTree (Real-time Activity Feed)
- **Role in feature:** Keeps a chronological record of activity so the “RB Real-Time Feed” and colour visualiser can tell the story of what just happened.
- **Why it matters:** During a live demo you might insert a burst of requests; the red-black balancing keeps each insert close to O(log n), so the feed updates instantly and the colour tab shows the balancing mechanics in action.
- **The lived example:** `GetLatestRequests(5)` doesn’t sort anything—it walks the red-black tree from the newest node backward, which is why the timestamps appear in order the moment the form loads.

### 5.3 Priority Queue Layer
#### ServiceRequestHeap (Max-Heap)
- **Role in feature:** Powers the “Max-Heap Top 5” tab by always bubbling the most urgent request to the top.
- **Why it matters:** Operators often need a quick “what’s burning?” list. Both insert and extract stay O(log n), and helper operations (`UpdatePriority`, `DecreasePriority`) let us shuffle priorities mid-demo without rebuilding from scratch.
- **The lived example:** When the tab refreshes, the code clones the heap, calls `ExtractMax` five times, and binds straight to the list box. You’ll see the most urgent ticket first, every time.

### 5.4 Graph Algorithms and Optimisation
#### ServiceGraph (BFS and DFS)
- **Role in feature:** Describes who touches a request—departments, staff members, even linked tickets—so the BFS and DFS tabs can reveal collaboration paths.
- **Why it matters:** Traversals run in O(V + E) thanks to the custom adjacency lists. During marking, expanding the graph still returns results in a single pass without redundant visits.
- **The lived example:** Hit the BFS tab and you’ll read a sentence like `Water Services BFS → John Smith → #1023`. That sentence is generated on the fly by taking the traversal order and formatting it for humans.

#### Minimum Spanning Tree (Prim-style)
- **Role in feature:** Demonstrates how departments could minimise travel time when coordinating maintenance visits, using the weighted edges stored inside `ServiceGraph`.
- **Why it matters:** Even though the dataset is small, running a Prim-style MST gives the dashboard a concrete optimisation story. Complexity is O(V^2), which is ideal for classrooms and easy to reason about during a demo.
- **The lived example:** The "MST Maintenance Routes" tab shows lines such as `Water Services ↔ Waste Management (4 km)`. That output isn't pre-written; it's produced by the MST routine and appended to the list box as the algorithm runs.

---

## 6. Updates Based on Feedback: Incorporation of Feedback

Feedback from Part 1 and Part 2 of the Portfolio of Evidence led to significant improvements across the application's user interface and technical architecture.

### Part 1 Feedback and Response
- **Feedback received:** The main concerns focused on UI consistency and the fact that forms did not resize correctly on different screen sizes, creating a poor user experience across different display configurations.
- **Implementation changes:** This was addressed by restructuring the layout of all main forms, improving docking and anchoring properties of controls, and ensuring that all forms scale dynamically to accommodate various screen resolutions. The improvements ensure a consistent visual experience regardless of the user's display setup.

### Part 2 Feedback and Response
- **Feedback received:** The lecturer pointed out that the implementation relied heavily on built-in .NET data structures (such as `List<T>`, `Dictionary<TKey,TValue>`, and `HashSet<T>`) instead of implementing custom data structures from scratch, which was a key requirement of the assignment.
- **Implementation changes:** For Part 3, all major data structures were completely rebuilt as **custom implementations from scratch**, following object-oriented design principles. The rebuilt structures—`ServiceRequestTree`, `ServiceRequestBST`, `ServiceRequestAVLTree`, `ServiceRequestRedBlackTree`, `ServiceRequestHeap`, and `ServiceGraph`—are now fully custom-coded and integrated with the Service Request Status dashboard, demonstrating proper understanding of data structure internals and algorithms.

These improvements have resulted in both enhanced performance characteristics and full compliance with the assignment rubric, delivering a cleaner, more consistent, and technically accurate project that showcases mastery of custom data structure implementation.
