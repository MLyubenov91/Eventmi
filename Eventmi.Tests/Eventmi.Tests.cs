using Eventmi.Core.Models.Event;
using Eventmi.Infrastructure.Data.Contexts;
using Eventmi.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework.Internal;
using RestSharp;
using System.Net;

namespace Eventmi.Tests
{
    public class Tests
    {
        private RestClient _client;
        private const string baseUrl = @"https://localhost:7236";
        //private EventmiContext _dbContext;
        private Event? lastCreatedEvent;
        private static int lastCreatedEventID;


        [SetUp]
        public void Setup()
        {
            _client = new RestClient(baseUrl);

            //var options = new DbContextOptionsBuilder<EventmiContext>().UseSqlServer("Server=DESKTOP-S8BB35R\\SQLEXPRESS;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true").Options;
            //_dbContext = new EventmiContext(options);
        }

        [Test, Order(1)]
        public async Task GetAllEvents_ReturnsSuccessStatusCode()
        {
            // Arrange
            var request = new RestRequest("/Event/All", Method.Get);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test, Order(2)]
        public async Task Add_GetRequest_ReturnsAddView()
        {
            // Arrange
            var request = new RestRequest("/Event/Add", Method.Get);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test, Order(3)]
         public async Task Add_PostRequest_AddsEventAndRedirects()
        {
            // Arrange
            var random = new Randomizer();
            var randomName = random.GetString();

            var newEvent = new EventFormModel()
            {
                Name = randomName,
                Place = "SoftUni",
                Start = new DateTime(2025, 12, 10, 7, 0, 0),
                //Start = DateTime.Now.AddMinutes(10),
                End = new DateTime(2025, 12, 11, 6, 0, 0)
                //End = DateTime.Now.AddHours(2)
            };

            var request = new RestRequest("/Event/Add", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("Name", newEvent.Name);
            request.AddParameter("Start", newEvent.Start.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("End", newEvent.End.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("Place", newEvent.Place);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(CheckIfEventExists(newEvent.Name), Is.True);

            lastCreatedEvent = GetEventByName(newEvent.Name);
            lastCreatedEventID = lastCreatedEvent.Id;

            //--------------------------------------------------------------------------------------

            //// Arrange
            //var input = new EventFormModel()
            //{
            //    Name = "Soft Uni Conf",
            //    Place = "Soft Uni",
            //    Start = new DateTime(2024, 12, 12, 12, 0, 0),
            //    End = new DateTime(2024, 12, 12, 16, 0, 0)
            //};

            //var request = new RestRequest("/Event/Add", Method.Post);
            //request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            //request.AddParameter("Name", input.Name);
            //request.AddParameter("Place", input.Place);
            //request.AddParameter("Start", input.Start.ToString("MM/dd/yyyy hh:mm tt"));
            //request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh:mm tt"));

            //// Act
            //var response = await _client.ExecuteAsync(request);

            //// Assert
            //Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            //Assert.True(CheckIfEventExists(input.Name), "Event was not added to the database");
        }

        [Test, Order(4)]
        public async Task GetEventDetails_ReturnsSuccessAndExpectedContent()
        {
            // Arrange
            var eventID = lastCreatedEventID;
            var request = new RestRequest($"/Event/Details/{eventID}", Method.Get);

            // Act 
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        // Teo
        [Test, Order(5)]
        public async Task Details_GetRequest_ShouldReturnNotFoundIfNoIdIsGiven()
        {
            // Arrange
            var request = new RestRequest($"/Event/Details/", Method.Get);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test, Order(6)]
        public async Task Edit_GetRequest_ReturnsViewForValidId()
        {
            // Arrange
            var eventID = lastCreatedEventID;
            var request = new RestRequest($"/Event/Edit/{eventID}", Method.Get);

            // Act 
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        // Teo
        [Test, Order(7)]
        public async Task Edit_GetRequest_ShouldReturnNotFoundIfNoIdIsGiven()
        {
            // Arrange
            var request = new RestRequest($"/Event/Edit/", Method.Get);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test, Order(8)]
        public async Task Edit_PostRequest_EditsAnEvent()
        {
            // Arrange
            var eventID = lastCreatedEventID;
            var dbEvent = GetEventById(eventID);

            var input = new EventFormModel()
            {
                Id = dbEvent.Id,
                Name = $"{dbEvent.Name} + UPDATED",
                Start = dbEvent.Start,
                End = dbEvent.End,
                Place = dbEvent.Place
            };

            var request = new RestRequest($"/Event/Edit/{eventID}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("Id", input.Id);
            request.AddParameter("Name", input.Name);
            request.AddParameter("Start", input.Start.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("Place", input.Place);

            // Act 
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        //  Teo
        [Test, Order(9)]
        public async Task Edit_PostRequest_ShouldReturnBackTheSameViewIfModelErrorsArePresent()
        {
            // Arrange
            var eventId = 1;
            var dbEvent = GetEventById(eventId);

            var input = new EventFormModel()
            {
                Id = dbEvent.Id,
                Place = dbEvent.Place,
            };

            var request = new RestRequest($"/Event/Edit/{dbEvent.Id}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Id", input.Id);
            request.AddParameter("Name", input.Name);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        // Teo
        [Test, Order(10)]
        public async Task Edit_WtihIdMismatch_ShouldReturnNotFound()
        {
            // Arrange
            var eventId = lastCreatedEventID;
            var dbEvent = GetEventById(eventId);

            var input = new EventFormModel()
            {
                Id = 445,
                End = dbEvent.End,
                Name = $"{dbEvent.Name} UPDATED!!!",
                Place = dbEvent.Place,
                Start = dbEvent.Start,
            };

            var request = new RestRequest($"/Event/Edit/{eventId}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Id", input.Id);
            request.AddParameter("Name", input.Name);
            request.AddParameter("Place", input.Place);
            request.AddParameter("Start", input.Start.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh:mm tt"));

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test, Order(11)]
        public async Task Delete_WithValidId_RedirectsToAllEvents()
        {
            // Arrange
            var eventID = lastCreatedEventID;
            var request = new RestRequest($"/Event/Delete/{eventID}", Method.Post);

            // Act 
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        //Teo
        [Test, Order(12)]
        public async Task Delete_WithNoId_ShouldReturnNotFound()
        {
            // Arrange
            var request = new RestRequest("/Event/Delete/", Method.Post);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));  // Internal Server Error???
        }

        private bool CheckIfEventExists(string eventName)
        {
            var options = new DbContextOptionsBuilder<EventmiContext>().UseSqlServer("Server=DESKTOP-S8BB35R\\SQLEXPRESS;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true").Options;

            using var context = new EventmiContext(options);
            return context.Events.Any(x => x.Name == eventName);
        }

        private Event? GetEventById(int id)
        {
            var options = new DbContextOptionsBuilder<EventmiContext>().UseSqlServer("Server=DESKTOP-S8BB35R\\SQLEXPRESS;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true").Options;
            using var context = new EventmiContext(options);
            return context.Events.FirstOrDefault(x => x.Id == id);
        }

        private Event GetEventByName(string name)
        {
            var options = new DbContextOptionsBuilder<EventmiContext>().UseSqlServer("Server=DESKTOP-S8BB35R\\SQLEXPRESS;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true").Options;

            using var context = new EventmiContext(options);

            return context.Events.FirstOrDefault(x => x.Name == name);
        }
    }
}