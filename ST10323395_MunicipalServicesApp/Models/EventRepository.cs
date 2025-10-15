using System;
using System.Collections.Generic;
using System.Linq;

namespace ST10323395_MunicipalServicesApp.Models
{
    // Manages local events using various data structures
    public static class EventRepository
    {
        #region Data Structures

        // Upcoming events sorted by date
        public static readonly SortedList<DateTime, List<Event>> UpcomingEvents = new SortedList<DateTime, List<Event>>();

        // Recently viewed events (stack)
        public static readonly Stack<Event> RecentlyViewedEvents = new Stack<Event>();

        // Events organized by category
        public static readonly SortedDictionary<string, List<Event>> EventsByCategory = new SortedDictionary<string, List<Event>>();

        // Unique event categories
        public static readonly HashSet<string> UniqueCategories = new HashSet<string>();

        // Unique event dates
        public static readonly HashSet<DateTime> UniqueEventDates = new HashSet<DateTime>();

        // Recent search terms (queue)
        public static readonly Queue<string> RecentSearches = new Queue<string>();

        // Related events for recommendations
        public static readonly Dictionary<string, List<Event>> RelatedEvents = new Dictionary<string, List<Event>>();

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
                UpcomingEvents[eventDate] = new List<Event>();
            }
            UpcomingEvents[eventDate].Add(eventItem);

            // Add to category dictionary
            if (!EventsByCategory.ContainsKey(eventItem.Category))
            {
                EventsByCategory[eventItem.Category] = new List<Event>();
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
            var tempStack = new Stack<Event>();
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
                var limitedStack = new Stack<Event>();
                for (int i = 0; i < 10; i++)
                {
                    limitedStack.Push(RecentlyViewedEvents.Pop());
                }
                RecentlyViewedEvents.Clear();
                while (limitedStack.Count > 0)
                {
                    RecentlyViewedEvents.Push(limitedStack.Pop());
                }
            }
        }

        public static void AddSearchTerm(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return;

            // Remove if already exists to avoid duplicates
            var tempQueue = new Queue<string>();
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
                var limitedQueue = new Queue<string>();
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

        public static List<Event> GetAllEvents()
        {
            var events = new List<Event>();

            // Get all events from the sorted list (already ordered by date)
            foreach (var eventList in UpcomingEvents.Values)
            {
                events.AddRange(eventList);
            }

            return events.OrderBy(e => e.EventDate).ToList();
        }

        public static List<Event> GetEventsByCategory(string category)
        {
            if (EventsByCategory.ContainsKey(category))
            {
                return EventsByCategory[category].OrderBy(e => e.EventDate).ToList();
            }
            return new List<Event>();
        }

        public static List<Event> GetRecentlyViewedEvents(int count = 3)
        {
            var recentEvents = new List<Event>();
            var tempStack = new Stack<Event>();

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

        public static List<string> GetRecentSearches(int count = 5)
        {
            var searches = new List<string>();
            var tempQueue = new Queue<string>();

            // Get the most recent searches
            var allSearches = RecentSearches.ToArray();
            int startIndex = Math.Max(0, allSearches.Length - count);
            
            for (int i = startIndex; i < allSearches.Length; i++)
            {
                searches.Add(allSearches[i]);
            }

            return searches;
        }

        public static List<Event> GetRecommendedEvents()
        {
            var recommendations = new List<Event>();
            var recentSearches = GetRecentSearches(10);
            var recentlyViewed = GetRecentlyViewedEvents(5);

            // Analyze search patterns and find related events
            foreach (var search in recentSearches)
            {
                if (RelatedEvents.ContainsKey(search.ToLower()))
                {
                    recommendations.AddRange(RelatedEvents[search.ToLower()]);
                }

                // Also check for category matches
                foreach (var category in UniqueCategories)
                {
                    if (category.ToLower().Contains(search.ToLower()) || search.ToLower().Contains(category.ToLower()))
                    {
                        recommendations.AddRange(GetEventsByCategory(category));
                    }
                }
            }

            // Add recommendations based on recently viewed events
            foreach (var viewedEvent in recentlyViewed)
            {
                // Find events in the same category as recently viewed events
                var sameCategoryEvents = GetEventsByCategory(viewedEvent.Category)
                    .Where(e => e.Id != viewedEvent.Id && e.IsActive && e.EventDate >= DateTime.Now)
                    .ToList();
                recommendations.AddRange(sameCategoryEvents);

                // Find events with similar keywords
                var keywords = ExtractKeywords(viewedEvent);
                foreach (var keyword in keywords)
                {
                    if (RelatedEvents.ContainsKey(keyword))
                    {
                        var relatedEvents = RelatedEvents[keyword]
                            .Where(e => e.Id != viewedEvent.Id && e.IsActive && e.EventDate >= DateTime.Now)
                            .ToList();
                        recommendations.AddRange(relatedEvents);
                    }
                }
            }

            // If no recommendations from searches or recent views, provide popular events as defaults
            if (recommendations.Count == 0)
            {
                var allEvents = GetAllEvents()
                    .Where(e => e.IsActive && e.EventDate >= DateTime.Now)
                    .OrderBy(e => e.EventDate)
                    .Take(5)
                    .ToList();
                recommendations.AddRange(allEvents);
            }

            // Remove duplicates and return top recommendations
            var finalRecommendations = recommendations
                .Where(e => e.IsActive && e.EventDate >= DateTime.Now)
                .GroupBy(e => e.Id)
                .Select(g => new { Event = g.First(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Event.EventDate)
                .Select(x => x.Event)
                .Take(10)
                .OrderBy(x => (x.Id + DateTime.Now.Minute) % 10)
                .Take(5)
                .ToList();
                
            return finalRecommendations;
        }

        public static List<Event> SearchEvents(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllEvents();

            var searchLower = searchTerm.ToLower();
            var results = new List<Event>();

            foreach (var eventItem in GetAllEvents())
            {
                if (eventItem.Title.ToLower().Contains(searchLower) ||
                    eventItem.Description.ToLower().Contains(searchLower) ||
                    eventItem.Category.ToLower().Contains(searchLower) ||
                    eventItem.Location.ToLower().Contains(searchLower))
                {
                    results.Add(eventItem);
                }
            }

            // Add search term to recent searches
            AddSearchTerm(searchTerm);

            return results.OrderBy(e => e.EventDate).ToList();
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
                    RelatedEvents[keyword] = new List<Event>();
                }
                
                if (!RelatedEvents[keyword].Any(e => e.Id == eventItem.Id))
                {
                    RelatedEvents[keyword].Add(eventItem);
                }
            }
        }

        private static List<string> ExtractKeywords(Event eventItem)
        {
            var keywords = new List<string>();
            
            // Add category as keyword
            keywords.Add(eventItem.Category.ToLower());
            
            // Extract words from title and description
            var text = $"{eventItem.Title} {eventItem.Description}".ToLower();
            var words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in words)
            {
                if (word.Length > 3) // Only consider words longer than 3 characters
                {
                    keywords.Add(word);
                }
            }
            
            return keywords.Distinct().ToList();
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
            var sampleEvents = new List<Event>
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
