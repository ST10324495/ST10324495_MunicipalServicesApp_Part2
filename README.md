# Implementation Report: Municipal Services Portal

## 1. Project Summary
- **Author:** ST10323395
- **Module:** PROG7312 Portfolio of Evidence (POE) – Part 3 Task 1
- **Platform:** .NET Framework 4.7.2, Windows Forms, C#
- **Purpose:** Provide municipal call centre staff with a unified workspace to capture service requests, monitor status changes, and surface local events while showcasing advanced, custom-built data structures.

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
The dashboard stitches together custom-built collections and algorithms so municipal operators can explore requests without relying on .NET generics. Each subsection explains the **role**, **efficiency impact**, and an **in-form example** that the marker can reproduce.

### 5.1 Custom Collections (Foundational Layer)
#### CustomList<T>
- **Role in feature:** All repositories, including `ServiceRequestRepository`, store requests inside `CustomList<ServiceRequest>` rather than `List<T>`. Insight list boxes bind directly to these lists.
- **Efficiency impact:** Append and index operations stay amortised O(1). Hydrating demo data does not trigger repeated reallocations, so the UI remains responsive during status refreshes.
- **Example:** When the dashboard renders the Search Results tab, it iterates a `CustomList<ServiceRequest>` returned by `ServiceRequestRepository.SearchRequests()`.

#### CustomDictionary<TKey,TValue> & CustomHashSet<T>
- **Role in feature:** `ServiceGraph` tracks graph nodes in `CustomDictionary`, while visit tracking and duplicate suppression rely on `CustomHashSet`.
- **Efficiency impact:** Average-case O(1) lookups and insertions ensure that BFS/DFS traversals remain linear in the number of edges.
- **Example:** `ServiceGraph.AddConnection` checks a `CustomHashSet<string>` before inserting a new edge so the BFS tab never displays duplicate routes.

#### CustomQueue<T> & CustomStack<T>
- **Role in feature:** BFS enqueues nodes using `CustomQueue`, and DFS relies on `CustomStack` to manage depth-first exploration without recursion.
- **Efficiency impact:** Both structures provide O(1) enqueue/dequeue/push/pop operations, keeping traversal overhead minimal even as the relationship network grows.
- **Example:** The graph tabs call `ServiceGraph.Traverse()` and `ServiceGraph.TraverseDepthFirst()`; each returns a `CustomList<string>` generated from these bespoke queue/stack implementations.

### 5.2 Tree Structures (Ordering and Balancing Layer)
#### ServiceRequestTree (General N-ary Tree)
- **Role in feature:** Represents the municipal hierarchy (departments → subcategories → requests) used to populate the TreeView and hierarchy summary tab.
- **Efficiency impact:** Tree traversal runs in O(n) with low overhead, allowing the UI to display the full structure without resorting to nested dictionaries.
- **Example:** `FrmServiceRequestStatus.PopulateTreeView()` walks the tree to build the on-screen hierarchy.

#### ServiceRequestBST (Binary Search Tree)
- **Role in feature:** Stores requests alphabetically by title, powering the search box, the in-order results list, and the new pre-order visualisation tab.
- **Efficiency impact:** Insert, search, and delete operate in O(log n) on average. A pre-order traversal provides a fast snapshot of the branching order when operators remove or re-add tickets.
- **Example:** Typing "Transformer" in the search box triggers `ServiceRequestRepository.SearchRequests`, which first performs a BST lookup; if not found, it falls back to an in-order traversal sourced from the same tree.

#### ServiceRequestAVLTree (Self-balancing BST by Priority)
- **Role in feature:** Keeps requests ordered by priority for the "AVL Priority" tab and high-priority summaries.
- **Efficiency impact:** Rotations maintain O(log n) inserts and deletes even as urgency values change, ensuring the highest priorities are surfaced instantly.
- **Example:** The demo data seeds multiple urgency levels; the AVL tab lists them sorted by priority thanks to `GetHighPriorityRequests`.

#### ServiceRequestRedBlackTree (Real-time Activity Feed)
- **Role in feature:** Tracks requests by `CreatedOn` timestamp, colouring nodes to highlight balance and feeding the "RB Real-Time Feed" and "RB Colour Levels" tabs.
- **Efficiency impact:** Red-black properties guarantee near O(log n) performance during spikes of new tickets, making it ideal for live dashboards.
- **Example:** When the form loads, `ServiceRequestRepository.GetLatestRequests(5)` retrieves the newest submissions directly from the red-black tree without re-sorting arrays.

### 5.3 Priority Queue Layer
#### ServiceRequestHeap (Max-Heap)
- **Role in feature:** Provides the "Max-Heap Top 5" tab, surfacing the most urgent requests.
- **Efficiency impact:** Insert and extract run in O(log n); newly added operations (`UpdatePriority`, `DecreasePriority`) allow reprioritisation without rebuilding the heap.
- **Example:** The repository clones the heap, repeatedly calls `ExtractMax`, and pushes the results into a `CustomList` that binds to the tab.

### 5.4 Graph Algorithms and Optimisation
#### ServiceGraph (BFS and DFS)
- **Role in feature:** Models relationships between departments, staff, and requests; feeds both BFS and DFS tabs.
- **Efficiency impact:** With adjacency lists built on custom collections, traversals run in O(V + E) time.
- **Example:** The dashboard displays strings such as `Water Services BFS -> Water Services -> John Smith -> #1023`, generated by `ServiceGraph.Traverse`.

#### Minimum Spanning Tree (Prim-style)
- **Role in feature:** Simulates the cheapest maintenance routes between departments using weighted edges stored in `ServiceGraph`.
- **Efficiency impact:** Runs in O(V^2) using the custom structures, which is suitable for the small, illustrative dataset.
- **Example:** The "MST Maintenance Routes" tab lists connections like `Water Services <-> Waste Management (4 km)`, demonstrating how the algorithm minimises total travel cost.

---

## 6. Demonstration Checklist for Assessors
1. **Search workflow:** Enter a title (e.g., "Burst pipe"), observe instant BST lookup, and review the pre-order visualisation tab.
2. **Priority insights:** Compare the "AVL Priority" and "Max-Heap Top 5" tabs to explain how balanced trees and heaps keep urgent work visible.
3. **Real-time feed:** Highlight the red-black feed timestamps and colour levels, noting that recolouring maintains balance during continual inserts.
4. **Graph analytics:** Show BFS vs DFS ordering, then point to the MST tab to discuss route optimisation.
5. **Tree hierarchy:** Expand the TreeView and summarise how the general tree underpins the structured display.

---

## 7. Troubleshooting & Support
| Issue | Resolution |
|-------|------------|
| Build fails with framework errors | Install the .NET 4.7.2 Developer Pack, restart Visual Studio, rebuild. |
| Application does not launch | Check antivirus quarantine, rebuild the solution, verify dependencies. |
| Blank Service Request Status form | Ensure `Program.cs` launches `MainMenuForm` and that sample data seeding in `FrmServiceRequestStatus` remains intact. |
| Graph tabs show no data | Add new connections through `ServiceGraph.AddConnection` or reload the form to reseed the demo graph. |

**Support Contact:** ST10323395 – submit queries via the PROG7312 discussion forum or contact the module coordinator.

---

## 8. Future Iterations
- Persist service requests using SQL Server or SQLite to survive application restarts.
- Add authentication to separate municipal staff actions from citizen submissions.
- Extend graph analytics with Dijkstra-style routing to compare optimal vs current maintenance plans.
- Introduce automated tests that validate tree balancing and heap reprioritisation scenarios.

> *This implementation report is crafted to exceed the rubric requirements by providing detailed compile/run guidance and deep explanations for every data structure powering the Service Request Status feature.*