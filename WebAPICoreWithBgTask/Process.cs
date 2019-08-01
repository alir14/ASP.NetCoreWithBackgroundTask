using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPICoreWithBgTask.BackgroundTasks;

namespace WebAPICoreWithBgTask
{
    public class FakeDB
    {
        public List<string> DB { get; set; } = new List<string>();
    }
}
