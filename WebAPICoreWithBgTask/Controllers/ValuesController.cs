using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using WebAPICoreWithBgTask.BackgroundTasks;

namespace WebAPICoreWithBgTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly FakeDB _fakeSB;
        public IBackGroundTaskQueue Queue { get; }

        public ValuesController(ILogger<ValuesController> logger, FakeDB fakeDB,
            IServiceScopeFactory serviceScopeFactory, IBackGroundTaskQueue queue)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            Queue = queue;
            _fakeSB = fakeDB;
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            Queue.QueueBackGroundWorkItem(async token =>
            {
                var guid = Guid.NewGuid().ToString();

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<FakeDB>();

                    for (int i = 0; i < 4; i++)
                    {
                        try
                        {
                            db.DB.Add($"Queued Background Task {guid} has written a step. {i}/3");

                            _fakeSB.DB.Add($"Queued Background Task {guid} has written a step. {i}/3");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"An error occurred writing to the database. Error: {ex.Message}");
                        }
                    }
                }

                _logger.LogInformation($"Queue Background task {guid} is complete. 3/3");

            });

            return Ok(id);
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        //// GET api/values/5
        //[HttpGet("{id}")]
        //public ActionResult<string> Get(int id)
        //{
        //    return "value";
        //}

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
