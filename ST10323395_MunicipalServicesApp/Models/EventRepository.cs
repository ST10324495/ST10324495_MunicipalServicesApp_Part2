using System;
using ST10323395_MunicipalServicesApp.DataStructures;

namespace ST10323395_MunicipalServicesApp.Models
{
    // Manages local events using various data structures
    public static class EventRepository
    {
        #region Data Structures

        // Upcoming events sorted by date
        public static readonly CustomSortedList<DateTime, CustomList<Event>> UpcomingEvents = new CustomSortedList<DateTime, CustomList<Event>>();

        // Recently viewed events (stack)
        public static readonly CustomStack<Event> RecentlyViewedEvents = new CustomStack<Event>();

        // Events organized by category
        public static readonly CustomSortedDictionary<string, CustomList<Event>> EventsByCategory = new CustomSortedDictionary<string, CustomList<Event>>();

        // Unique event categories
        public static readonly CustomHashSet<string> UniqueCategories = new CustomHashSet<string>();

        // Unique event dates
        public static readonly CustomHashSet<DateTime> UniqueEventDates = new CustomHashSet<DateTime>();

        // Recent search terms (queue)
        public static readonly CustomQueue<string> RecentSearches = new CustomQueue<string>();

        // Related events for recommendations
        public static readonly CustomDictionary<string, CustomList<Event>> RelatedEvents = new CustomDictionary<string, CustomList<Event>>();

        // Counter for unique event IDs
        private static int _nextEventId = 1;

        #endregion

        #region Event Management Methods

        public static void AddEvent(Event eventItem)
        {
            // Assign unique ID
            eventItem.Id = _nextEventId++;

            // Add to sorted list (ordered by event date)
            var eventDate = eventItem.EventDate;
            if (!UpcomingEvents.ContainsKey(eventDate))
            {
                UpcomingEvents[eventDate] = new CustomList<Event>();
            }
            UpcomingEvents[eventDate].Add(eventItem);

            // Add to category dictionary
            if (!EventsByCategory.ContainsKey(eventItem.Category))
            {
                EventsByCategory[eventItem.Category] = new CustomList<Event>();
            }
            EventsByCategory[eventItem.Category].Add(eventItem);

            // Add to unique categories set
            UniqueCategories.Add(eventItem.Category);

            // Add to unique dates set
            UniqueEventDates.Add(eventItem.EventDate.Date);

            // Update related events
            UpdateRelatedEvents(eventItem);
        }
        public static void AddToRecentlyViewed(Event eventItem)
        {
            // Remove if already exists to avoid duplicates
            var tempStack = new CustomStack<Event>();
            while (RecentlyViewedEvents.Count > 0)
            {
                var item = RecentlyViewedEvents.Pop();
                if (item.Id != eventItem.Id)
                {
                    tempStack.Push(item);
                }
            }

            // Restore stack
            while (tempStack.Count > 0)
            {
                RecentlyViewedEvents.Push(tempStack.Pop());
            }

            // Add the new item to the top
            RecentlyViewedEvents.Push(eventItem);

            // Limit stack size to prevent memory issues
            if (RecentlyViewedEvents.Count > 10)
            {
                var buffer = new Event[10];
                var index = 0;
                for (int i = 0; i < 10; i++)
                {
                    buffer[index++] = RecentlyViewedEvents.Pop();
                }
                RecentlyViewedEvents.Clear();
                for (int i = index - 1; i >= 0; i--)
                {
                    RecentlyViewedEvents.Push(buffer[i]);
                }
            }
        }

        public static void AddSearchTerm(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return;

            // Remove if already exists to avoid duplicates
            var tempQueue = new CustomQueue<string>();
            while (RecentSearches.Count > 0)
            {
                var item = RecentSearches.Dequeue();
                if (!item.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    tempQueue.Enqueue(item);
                }
            }

            // Restore queue
            while (tempQueue.Count > 0)
            {
                RecentSearches.Enqueue(tempQueue.Dequeue());
            }

            // Add the new search term
            RecentSearches.Enqueue(searchTerm);

            // Limit queue size
            if (RecentSearches.Count > 20)
            {
                var limitedQueue = new CustomQueue<string>();
                var items = RecentSearches.ToArray();
                for (int i = items.Length - 20; i < items.Length; i++)
                {
                    limitedQueue.Enqueue(items[i]);
                }
                RecentSearches.Clear();
                while (limitedQueue.Count > 0)
                {
                    RecentSearches.Enqueue(limitedQueue.Dequeue());
                }
            }
        }

        public static CustomList<Event> GetAllEvents()
        {
            var events = new CustomList<Event>();

            foreach (var eventList in UpcomingEvents.Values)
            {
                events.AddRange(eventList);
            }

            return CreateChronologicalCopy(events);
        }

        public static CustomList<Event> GetEventsByCategory(string category)
        {
            if (EventsByCategory.ContainsKey(category))
            {
                return CreateChronologicalCopy(EventsByCategory[category]);
            }
            return new CustomList<Event>();
        }

        public static CustomList<Event> GetRecentlyViewedEvents(int count = 3)
        {
            var recentEvents = new CustomList<Event>();
            var tempStack = new CustomStack<Event>();

            // Get the top items from the stack
            for (int i = 0; i < count && RecentlyViewedEvents.Count > 0; i++)
            {
                var eventItem = RecentlyViewedEvents.Pop();
                recentEvents.Add(eventItem);
                tempStack.Push(eventItem);
            }

            // Restore the stack
            while (tempStack.Count > 0)
            {
                RecentlyViewedEvents.Push(tempStack.Pop());
            }

            return recentEvents;
        }

        public static CustomList<string> GetRecentSearches(int count = 5)
        {
            var searches = new CustomList<string>();

            // Get the most recent searches
            var allSearches = RecentSearches.ToArray();
            int startIndex = Math.Max(0, allSearches.Length - count);
            
            for (int i = startIndex; i < allSearches.Length; i++)
            {
                searches.Add(allSearches[i]);
            }

            return searches;
        }

        /// <summary>
        /// Returns all unique categories sorted alphabetically.
        /// </summary>
        /// <remarks>
        /// Copies set members into a <see cref="CustomList{T}"/> so the UI can bind without touching <c>List&lt;T&gt;</c>.
        /// </remarks>
        public static CustomList<string> GetSortedCategories()
        {
            var categoryArray = UniqueCategories.ToArray();
            SortStrings(categoryArray);

            var sorted = new CustomList<string>();
            for (int i = 0; i < categoryArray.Length; i++)
            {
                sorted.Add(categoryArray[i]);
            }

            return sorted;
        }

        public static CustomList<Event> GetRecommendedEvents()
        {
            var uniqueEvents = new CustomDictionary<int, Event>();
            var frequency = new CustomDictionary<int, int>();

            var recentSearches = GetRecentSearches(10);
            var recentlyViewed = GetRecentlyViewedEvents(5);

            AddRecommendationsFromSearches(recentSearches, uniqueEvents, frequency);
            AddRecommendationsFromViewed(recentlyViewed, uniqueEvents, frequency);

            if (uniqueEvents.Count == 0)
            {
                var fallback = GetAllEvents();
                foreach (var eventItem in fallback)
                {
                    if (!eventItem.IsActive || eventItem.EventDate < DateTime.Now)
                    {
                        continue;
                    }

                    RegisterCandidate(eventItem, uniqueEvents, frequency, 1);
                    if (uniqueEvents.Count >= 5)
                    {
                        break;
                    }
                }
            }

            return BuildOrderedRecommendations(uniqueEvents, frequency, 5);
        }

        public static CustomList<Event> SearchEvents(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllEvents();

            var searchLower = searchTerm.ToLowerInvariant();
            var results = new CustomList<Event>();

            foreach (var eventItem in GetAllEvents())
            {
                if (eventItem.Title.ToLowerInvariant().Contains(searchLower) ||
                    eventItem.Description.ToLowerInvariant().Contains(searchLower) ||
                    eventItem.Category.ToLowerInvariant().Contains(searchLower) ||
                    eventItem.Location.ToLowerInvariant().Contains(searchLower))
                {
                    results.Add(eventItem);
                }
            }

            // Add search term to recent searches
            AddSearchTerm(searchTerm);

            return CreateChronologicalCopy(results);
        }

        #endregion

        #region Recommendation Helpers

        private static void AddRecommendationsFromSearches(CustomList<string> searches, CustomDictionary<int, Event> catalog, CustomDictionary<int, int> frequency)
        {
            foreach (var search in searches)
            {
                var lowered = search.ToLowerInvariant();

                if (RelatedEvents.ContainsKey(lowered))
                {
                    foreach (var related in RelatedEvents[lowered])
                    {
                        RegisterCandidate(related, catalog, frequency, 3);
                    }
                }

                foreach (var category in UniqueCategories)
                {
                    if (CategoryMatchesSearch(category, lowered))
                    {
                        var categoryEvents = GetEventsByCategory(category);
                        foreach (var evt in categoryEvents)
                        {
                            RegisterCandidate(evt, catalog, frequency, 1);
                        }
                    }
                }
            }
        }

        private static void AddRecommendationsFromViewed(CustomList<Event> recentlyViewed, CustomDictionary<int, Event> catalog, CustomDictionary<int, int> frequency)
        {
            foreach (var viewed in recentlyViewed)
            {
                var sameCategoryEvents = GetEventsByCategory(viewed.Category);
                foreach (var evt in sameCategoryEvents)
                {
                    if (evt.Id == viewed.Id) continue;
                    RegisterCandidate(evt, catalog, frequency, 2);
                }

                var keywords = ExtractKeywords(viewed);
                foreach (var keyword in keywords)
                {
                    if (RelatedEvents.ContainsKey(keyword))
                    {
                        foreach (var related in RelatedEvents[keyword])
                        {
                            if (related.Id == viewed.Id) continue;
                            RegisterCandidate(related, catalog, frequency, 1);
                        }
                    }
                }
            }
        }

        private static void RegisterCandidate(Event candidate, CustomDictionary<int, Event> catalog, CustomDictionary<int, int> frequency, int weight)
        {
            if (candidate == null)
            {
                return;
            }

            if (!candidate.IsActive || candidate.EventDate < DateTime.Now)
            {
                return;
            }

            if (catalog.ContainsKey(candidate.Id))
            {
                catalog[candidate.Id] = candidate;
            }
            else
            {
                catalog.Add(candidate.Id, candidate);
            }

            IncrementScore(frequency, candidate.Id, weight);
        }

        private static void IncrementScore(CustomDictionary<int, int> frequency, int eventId, int weight)
        {
            if (frequency.ContainsKey(eventId))
            {
                frequency[eventId] = frequency[eventId] + weight;
            }
            else
            {
                frequency.Add(eventId, weight);
            }
        }

        private static int GetScore(CustomDictionary<int, int> frequency, int eventId)
        {
            return frequency.TryGetValue(eventId, out var value) ? value : 0;
        }

        private static CustomList<Event> BuildOrderedRecommendations(CustomDictionary<int, Event> catalog, CustomDictionary<int, int> frequency, int maxCount)
        {
            var total = catalog.Count;
            var ids = new int[total];
            var events = new Event[total];

            int index = 0;
            foreach (var pair in catalog)
            {
                ids[index] = pair.Key;
                events[index] = pair.Value;
                index++;
            }

            for (int i = 0; i < total - 1; i++)
            {
                int best = i;
                for (int j = i + 1; j < total; j++)
                {
                    int currentScore = GetScore(frequency, ids[j]);
                    int bestScore = GetScore(frequency, ids[best]);

                    if (currentScore > bestScore ||
                        (currentScore == bestScore && events[j].EventDate < events[best].EventDate))
                    {
                        best = j;
                    }
                }

                if (best != i)
                {
                    Swap(ids, events, i, best);
                }
            }

            var result = new CustomList<Event>();
            int limit = maxCount <= 0 || maxCount > total ? total : maxCount;
            for (int i = 0; i < limit; i++)
            {
                result.Add(events[i]);
            }

            return result;
        }

        private static void Swap(int[] ids, Event[] events, int first, int second)
        {
            var idTemp = ids[first];
            ids[first] = ids[second];
            ids[second] = idTemp;

            var eventTemp = events[first];
            events[first] = events[second];
            events[second] = eventTemp;
        }

        private static bool CategoryMatchesSearch(string category, string searchLower)
        {
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(searchLower))
            {
                return false;
            }

            var categoryLower = category.ToLowerInvariant();
            return categoryLower.IndexOf(searchLower, StringComparison.Ordinal) >= 0 ||
                   searchLower.IndexOf(categoryLower, StringComparison.Ordinal) >= 0;
        }

        private static CustomList<Event> CreateChronologicalCopy(CustomList<Event> source)
        {
            var array = source.ToArray();
            SortEventsByDate(array);

            var sorted = new CustomList<Event>();
            for (int i = 0; i < array.Length; i++)
            {
                sorted.Add(array[i]);
            }

            return sorted;
        }

        private static void SortEventsByDate(Event[] items)
        {
            for (int i = 1; i < items.Length; i++)
            {
                var key = items[i];
                int j = i - 1;

                while (j >= 0 && items[j].EventDate > key.EventDate)
                {
                    items[j + 1] = items[j];
                    j--;
                }

                items[j + 1] = key;
            }
        }

        private static void SortStrings(string[] items)
        {
            for (int i = 1; i < items.Length; i++)
            {
                var key = items[i];
                int j = i - 1;

                while (j >= 0 && string.Compare(items[j], key, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    items[j + 1] = items[j];
                    j--;
                }

                items[j + 1] = key;
            }
        }

        #endregion

        #region Helper Methods

        private static void UpdateRelatedEvents(Event eventItem)
        {
            var keywords = ExtractKeywords(eventItem);
            
            foreach (var keyword in keywords)
            {
                if (!RelatedEvents.ContainsKey(keyword))
                {
                    RelatedEvents[keyword] = new CustomList<Event>();
                }

                var eventGroup = RelatedEvents[keyword];
                var alreadyTracked = false;

                for (int i = 0; i < eventGroup.Count; i++)
                {
                    if (eventGroup[i].Id == eventItem.Id)
                    {
                        alreadyTracked = true;
                        break;
                    }
                }

                if (!alreadyTracked)
                {
                    eventGroup.Add(eventItem);
                }
            }
        }

        private static CustomList<string> ExtractKeywords(Event eventItem)
        {
            var keywords = new CustomHashSet<string>();
            
            // Add category as keyword
            keywords.Add(eventItem.Category.ToLowerInvariant());
            
            // Extract words from title and description
            var text = $"{eventItem.Title} {eventItem.Description}".ToLowerInvariant();
            var words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in words)
            {
                if (word.Length > 3) // Only consider words longer than 3 characters
                {
                    keywords.Add(word);
                }
            }
            
            var result = new CustomList<string>();
            var keywordArray = keywords.ToArray();
            SortStrings(keywordArray);

            for (int i = 0; i < keywordArray.Length; i++)
            {
                result.Add(keywordArray[i]);
            }

            return result;
        }

        public static void InitializeSampleData()
        {
            // Clear existing data
            UpcomingEvents.Clear();
            RecentlyViewedEvents.Clear();
            EventsByCategory.Clear();
            UniqueCategories.Clear();
            UniqueEventDates.Clear();
            RecentSearches.Clear();
            RelatedEvents.Clear();
            _nextEventId = 1;

            // Add sample events
            var sampleEvents = new CustomList<Event>
            {
                new Event("Community Garden Workshop", "Learn sustainable gardening techniques and community building", "Education",
                         DateTime.Now.AddDays(7), "Community Center", "contact@community.org", 0, true), // Full
                new Event("Summer Sports Festival", "Annual sports competition for all ages", "Sports",
                         DateTime.Now.AddDays(14), "Sports Complex", "sports@municipal.gov", 200, false),
                new Event("Art Exhibition Opening", "Local artists showcase their latest works", "Arts & Culture",
                         DateTime.Now.AddDays(3), "Art Gallery", "gallery@arts.org", 5, true), // Limited spots
                new Event("Environmental Cleanup Day", "Help clean up local parks and waterways", "Community Service",
                         DateTime.Now.AddDays(10), "Central Park", "environment@municipal.gov", 100, false),
                new Event("Technology Workshop", "Introduction to digital skills for seniors", "Education",
                         DateTime.Now.AddDays(21), "Library", "tech@library.org", 25, true),
                new Event("Music Concert in the Park", "Free outdoor concert featuring local bands", "Entertainment",
                         DateTime.Now.AddDays(5), "Riverside Park", "music@events.org", 500, false),
                new Event("Health & Wellness Fair", "Free health screenings and wellness information", "Health",
                         DateTime.Now.AddDays(12), "Health Center", "health@municipal.gov", 150, false),
                new Event("Book Club Meeting", "Monthly discussion of selected books", "Education",
                         DateTime.Now.AddDays(28), "Library", "books@library.org", 0, true), // Full
                new Event("Food Truck Festival", "Local food vendors showcase their best dishes", "Food & Dining",
                         DateTime.Now.AddDays(6), "Downtown Square", "food@events.org", 300, false),
                new Event("Photography Workshop", "Learn basic photography techniques and composition", "Arts & Culture",
                         DateTime.Now.AddDays(15), "Art Studio", "photo@arts.org", 15, true),
                new Event("Basketball Tournament", "Community basketball tournament for all skill levels", "Sports",
                         DateTime.Now.AddDays(9), "Community Gym", "basketball@sports.org", 80, true),
                new Event("Science Fair", "Students showcase their science projects and experiments", "Education",
                         DateTime.Now.AddDays(18), "High School", "science@school.edu", 200, false),
                new Event("Farmers Market", "Fresh local produce and handmade goods", "Community Service",
                         DateTime.Now.AddDays(2), "Market Square", "market@local.org", 500, false),
                new Event("Yoga in the Park", "Free outdoor yoga session for all levels", "Health",
                         DateTime.Now.AddDays(4), "Sunset Park", "yoga@wellness.org", 50, false),
                new Event("Cooking Class", "Learn to cook healthy meals with local ingredients", "Food & Dining",
                         DateTime.Now.AddDays(11), "Community Kitchen", "cooking@food.org", 2, true), // Almost full
                new Event("Movie Night", "Outdoor screening of family-friendly movies", "Entertainment",
                         DateTime.Now.AddDays(8), "Park Amphitheater", "movies@events.org", 400, false),
                new Event("Volunteer Training", "Training session for community volunteers", "Community Service",
                         DateTime.Now.AddDays(13), "Volunteer Center", "volunteer@community.org", 25, true),
                new Event("Dance Workshop", "Learn various dance styles from professional instructors", "Arts & Culture",
                         DateTime.Now.AddDays(16), "Dance Studio", "dance@arts.org", 20, true),
                new Event("Cycling Tour", "Guided cycling tour of local landmarks", "Sports",
                         DateTime.Now.AddDays(20), "Starting Point: City Hall", "cycling@sports.org", 30, true),
                new Event("Career Fair", "Meet local employers and explore job opportunities", "Education",
                         DateTime.Now.AddDays(25), "Convention Center", "careers@jobs.org", 300, false)
            };

            foreach (var eventItem in sampleEvents)
            {
                AddEvent(eventItem);
            }

            // Add some sample search terms to populate recommendations
            AddSearchTerm("sports");
            AddSearchTerm("education");
            AddSearchTerm("community");
            AddSearchTerm("health");
            AddSearchTerm("arts");
            AddSearchTerm("technology");
            
            // Add some events to recently viewed for demonstration
            var allEvents = GetAllEvents();
            if (allEvents.Count >= 3)
            {
                AddToRecentlyViewed(allEvents[0]); // Community Garden Workshop
                AddToRecentlyViewed(allEvents[1]); // Summer Sports Festival
                AddToRecentlyViewed(allEvents[2]); // Art Exhibition Opening
            }
        }

        #endregion
    }
}
