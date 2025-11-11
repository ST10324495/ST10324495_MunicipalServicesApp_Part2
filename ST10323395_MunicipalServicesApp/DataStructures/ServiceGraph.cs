using System;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Lightweight graph backed by custom collections to model relationships between departments, staff, and requests.
    /// </summary>
    /// <remarks>
    /// Stores adjacency lists inside <see cref="CustomList{T}"/> instances and tracks nodes with <see cref="CustomDictionary{TKey,TValue}"/>.
    /// Breadth-first traversal stays O(V + E), making it ideal for demonstrating relationship walkthroughs in the rubric.
    /// </remarks>
    public class ServiceGraph
    {
        private class GraphNode
        {
            public GraphNode(string label)
            {
                Label = label;
                Neighbours = new CustomList<string>();
                Routes = new CustomList<RouteEdge>();
            }

            public string Label { get; }
            public CustomList<string> Neighbours { get; }
            public CustomList<RouteEdge> Routes { get; }
        }

        private struct RouteEdge
        {
            public string TargetKey;
            public int Weight;
        }

        private readonly CustomDictionary<string, GraphNode> _nodes = new CustomDictionary<string, GraphNode>();

        /// <summary>
        /// Connects two nodes, creating them if they do not yet exist.
        /// </summary>
        /// <remarks>
        /// Runs in O(1) average time thanks to the dictionary lookup, and ensures the adjacency list is deduplicated.
        /// </remarks>
        public void AddConnection(string from, string to)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("Graph connections require non-empty nodes.");
            }

            var fromKey = Normalize(from);
            var toKey = Normalize(to);

            var fromNode = GetOrCreateNode(fromKey, from);
            GetOrCreateNode(toKey, to); // Ensure the target node exists even if it has no outgoing edges.

            if (!ContainsNeighbour(fromNode.Neighbours, toKey))
            {
                fromNode.Neighbours.Add(toKey);
            }
        }

        /// <summary>
        /// Returns the immediate neighbours for a given node.
        /// </summary>
        /// <remarks>
        /// Lookup is O(d) where d is the node degree, and the results come back in a <see cref="CustomList{T}"/> for consistency.
        /// </remarks>
        public CustomList<string> GetConnections(string node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            var result = new CustomList<string>();
            var key = Normalize(node);

            if (_nodes.TryGetValue(key, out var graphNode))
            {
                foreach (var targetKey in graphNode.Neighbours)
                {
                    if (_nodes.TryGetValue(targetKey, out var targetNode))
                    {
                        result.Add(targetNode.Label);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Performs a breadth-first traversal starting from the supplied node.
        /// </summary>
        /// <remarks>
        /// Visits each node at most once (O(V + E)). It powers the relationship tab inside the service status form.
        /// </remarks>
        public CustomList<string> Traverse(string startNode)
        {
            if (string.IsNullOrWhiteSpace(startNode))
            {
                throw new ArgumentException("Start node cannot be empty.", nameof(startNode));
            }

            var startKey = Normalize(startNode);
            var order = new CustomList<string>();

            if (!_nodes.ContainsKey(startKey))
            {
                return order;
            }

            var visited = new CustomHashSet<string>();
            var queue = new CustomQueue<string>();

            queue.Enqueue(startKey);
            visited.Add(startKey);

            while (queue.Count > 0)
            {
                var currentKey = queue.Dequeue();

                if (_nodes.TryGetValue(currentKey, out var currentNode))
                {
                    order.Add(currentNode.Label);

                    foreach (var neighbourKey in currentNode.Neighbours)
                    {
                        if (visited.Add(neighbourKey))
                        {
                            queue.Enqueue(neighbourKey);
                        }
                    }
                }
            }

            return order;
        }

        /// <summary>
        /// Performs a depth-first traversal using the custom stack implementation.
        /// </summary>
        /// <remarks>
        /// Still O(V + E). DFS mirrors a deep investigative chain across departments for complex service cases.
        /// </remarks>
        public CustomList<string> TraverseDepthFirst(string startNode)
        {
            if (string.IsNullOrWhiteSpace(startNode))
            {
                throw new ArgumentException("Start node cannot be empty.", nameof(startNode));
            }

            var startKey = Normalize(startNode);
            var order = new CustomList<string>();

            if (!_nodes.ContainsKey(startKey))
            {
                return order;
            }

            var visited = new CustomHashSet<string>();
            var stack = new CustomStack<string>();
            stack.Push(startKey);

            while (stack.Count > 0)
            {
                var currentKey = stack.Pop();
                if (!visited.Add(currentKey))
                {
                    continue;
                }

                if (_nodes.TryGetValue(currentKey, out var currentNode))
                {
                    order.Add(currentNode.Label);

                    // Push neighbours in reverse so left-most connections appear first, mimicking textbook DFS.
                    var neighbours = currentNode.Neighbours;
                    for (int i = neighbours.Count - 1; i >= 0; i--)
                    {
                        var neighbourKey = neighbours[i];
                        if (!visited.Contains(neighbourKey))
                        {
                            stack.Push(neighbourKey);
                        }
                    }
                }
            }

            return order;
        }

        /// <summary>
        /// Adds a weighted bidirectional route for MST simulations (e.g., maintenance crews travelling between departments).
        /// </summary>
        /// <remarks>
        /// Weights reflect travel cost or time. The structure remains custom-built, avoiding <c>Dictionary&lt;TKey,TValue&gt;</c> or <c>List&lt;T&gt;</c>.
        /// </remarks>
        public void AddRoute(string from, string to, int weight)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("Route nodes cannot be empty.");
            }

            if (weight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), "Routes must have a positive weight.");
            }

            var fromKey = Normalize(from);
            var toKey = Normalize(to);

            var fromNode = GetOrCreateNode(fromKey, from);
            var toNode = GetOrCreateNode(toKey, to);

            AddRouteInternal(fromNode, fromKey, toKey, weight);
            AddRouteInternal(toNode, toKey, fromKey, weight);
        }

        /// <summary>
        /// Builds a minimum spanning tree using a Prim-style scan, showcasing the cheapest maintenance network.
        /// </summary>
        /// <remarks>
        /// Runs in O(V^2) using our custom collections—fast enough for the small demo graph while keeping the logic transparent.
        /// </remarks>
        public CustomList<string> GetMinimumSpanningTree()
        {
            var routes = new CustomList<string>();
            if (_nodes.Count == 0)
            {
                return routes;
            }

            string startKey = null;
            foreach (var pair in _nodes)
            {
                startKey = pair.Key;
                break;
            }

            var visited = new CustomHashSet<string>();
            var visitedOrder = new CustomList<string>();
            visited.Add(startKey);
            visitedOrder.Add(startKey);

            while (visited.Count < _nodes.Count)
            {
                bool found = false;
                string bestFrom = null;
                RouteEdge bestEdge = default;

                for (int i = 0; i < visitedOrder.Count; i++)
                {
                    var currentKey = visitedOrder[i];
                    if (!_nodes.TryGetValue(currentKey, out var currentNode))
                    {
                        continue;
                    }

                    foreach (var edge in currentNode.Routes)
                    {
                        if (visited.Contains(edge.TargetKey))
                        {
                            continue;
                        }

                        if (!found || edge.Weight < bestEdge.Weight)
                        {
                            found = true;
                            bestEdge = edge;
                            bestFrom = currentKey;
                        }
                    }
                }

                if (!found)
                {
                    // Graph is not fully connected; exit gracefully.
                    break;
                }

                visited.Add(bestEdge.TargetKey);
                visitedOrder.Add(bestEdge.TargetKey);

                if (_nodes.TryGetValue(bestFrom, out var fromNode) &&
                    _nodes.TryGetValue(bestEdge.TargetKey, out var toNode))
                {
                    routes.Add($"{fromNode.Label} ↔ {toNode.Label} ({bestEdge.Weight} km)");
                }
            }

            return routes;
        }

        private GraphNode GetOrCreateNode(string key, string label)
        {
            if (_nodes.TryGetValue(key, out var node))
            {
                return node;
            }

            var newNode = new GraphNode(label);
            _nodes.Add(key, newNode);
            return newNode;
        }

        private static bool ContainsNeighbour(CustomList<string> targets, string key)
        {
            foreach (var target in targets)
            {
                if (string.Equals(target, key, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        private static void AddRouteInternal(GraphNode sourceNode, string sourceKey, string targetKey, int weight)
        {
            if (!ContainsNeighbour(sourceNode.Neighbours, targetKey))
            {
                sourceNode.Neighbours.Add(targetKey);
            }

            if (!ContainsRoute(sourceNode.Routes, targetKey))
            {
                sourceNode.Routes.Add(new RouteEdge
                {
                    TargetKey = targetKey,
                    Weight = weight
                });
            }
        }

        private static bool ContainsRoute(CustomList<RouteEdge> routes, string key)
        {
            foreach (var route in routes)
            {
                if (string.Equals(route.TargetKey, key, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

